using NAudio.Midi;

namespace Apollo.MIDI;

public static class MidiManager
{
    public static string ReadFile(string path)
    {
        if (GetPathType(path) != 'f')
            throw new FileNotFoundException($"{path} is not a valid file path");

        var file = new MidiFile(path, false);
        var data = new List<string>();

        data.Add(file.DeltaTicksPerQuarterNote.ToString());

        for (var trackNum = 0; trackNum < file.Tracks; trackNum++)
            foreach (var e in file.Events[trackNum])
                if (e.GetType() == typeof(NoteOnEvent))
                {
                    try
                    {
                        var onEvent = (NoteOnEvent)e;
                        var offEvent = onEvent.OffEvent;

                        var noteNumber = onEvent.NoteNumber;
                        var onTime = onEvent.AbsoluteTime;
                        var offTime = offEvent.AbsoluteTime;
                        var noteLength = onEvent.NoteLength;
                        var velocity = onEvent.Velocity;

                        data.Add($"{noteNumber},{onTime},{offTime},{noteLength},{velocity}");
                    }
                    catch
                    {
                        // Log 
                    }
                }
                else if (e.GetType() == typeof(TempoEvent))
                {
                    var tempoEvent = (TempoEvent)e;
                    data.Add($"{tempoEvent.MicrosecondsPerQuarterNote}, {tempoEvent.AbsoluteTime}");
                }

        return string.Join('\n', data);
    }

    public static List<string> ReadDir(string path)
    {
        if (GetPathType(path) != 'd')
            throw new DirectoryNotFoundException($"{path} is not a valid directory");

        var dirData = new List<string>();

        var dirInfo = new DirectoryInfo(path);
        var dirFiles = dirInfo.GetFiles();

        foreach (var fileInfo in dirFiles)
        {
            if (!fileInfo.Name.EndsWith(".mid") && !fileInfo.Name.EndsWith(".midi"))
                continue;

            var filePath = fileInfo.FullName;
            var fileData = ReadFile(filePath);
            dirData.Add(fileData);
        }

        return dirData;
    }
    
    /// <summary>
    /// Used to determine what type of midi event the string represents
    /// </summary>
    /// <param name="e">The string representation of the event</param>
    /// <returns>An enum denoting the type of event represented by the string</returns>
    private static EventType DetermineEventType(string e)
    {
        var eventData = e.Split(',');

        switch (eventData.Length)
        {
            case 4:
                return EventType.NoteEvent; 
            case 2:
                return EventType.TempoEvent;
            default:
                return EventType.InvalidEvent;
        }
    }
    
    /// <summary>
    /// Writes a string representation to a MIDI file 
    /// </summary>
    /// <param name="data">The string representation of the file</param>
    /// <param name="path">The path of the file to write it to</param>
    public static void WriteFile(string data, string path)
    {
        var lines = data.Split('\n');
        var dTicks = int.Parse(lines[0]);
        var collection = new MidiEventCollection(0, dTicks);
        
        for (var i = 1; i < lines.Length; i++)
        {
            var eventType = DetermineEventType(lines[i]);
            var eventData = lines[i].Split(',');

            switch (eventType)
            {
                case EventType.NoteEvent:
                    var noteNumber = int.Parse(eventData[0]);
                    var onEventTime = long.Parse(eventData[1]);
                    var offEventTime = long.Parse(eventData[2]);
                    var duration = int.Parse(eventData[3]);
                    var velocity = int.Parse(eventData[4]);
                    
                    collection.AddEvent(new NoteOnEvent(onEventTime, 1, noteNumber, velocity, duration), 1);
                    collection.AddEvent(new NoteEvent(offEventTime, 1, MidiCommandCode.NoteOff, noteNumber, velocity),
                        1);
                    break;
                case EventType.TempoEvent:
                    var microsecondsPerQuarterNote = int.Parse(eventData[0]);
                    var absoluteTime = long.Parse(eventData[1]);
                    collection.AddEvent(new TempoEvent(microsecondsPerQuarterNote, absoluteTime), 1);
                    break;
                case EventType.InvalidEvent:
                    break;
            }
        }

        MidiFile.Export(path, collection);
    }

    /// <summary>
    ///     Checks whether a path is valid
    /// </summary>
    /// <param name="path">The path to validate</param>
    /// <returns>A character flag (d => directory, f => file, n => invalid path)</returns>
    private static char GetPathType(string path)
    {
        var attributes = File.GetAttributes(path);

        if (attributes == FileAttributes.Directory && Directory.Exists(path))
            return 'd';

        return File.Exists(path) ? 'f' : 'n';
    }
}