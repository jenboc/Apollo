using NAudio.Midi;
namespace Apollo.MIDI;

public static class MidiManager
{
    public static List<string> ReadFile(string path)
    {
        if (GetPathType(path) != 'f')
            throw new FileNotFoundException($"{path} is not a valid file path");
        
        var file = new MidiFile(path, false);
        var data = new List<string>();

        data.Add(file.DeltaTicksPerQuarterNote.ToString());

        for (var trackNum = 0; trackNum < file.Tracks; trackNum++)
        {
            foreach (var e in file.Events[trackNum])
            {
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

                        data.Add($"P{noteNumber}");
                        data.Add(onTime.ToString());
                        data.Add(offTime.ToString());
                        data.Add(noteLength.ToString());
                        data.Add(velocity.ToString());
                    }
                    catch
                    {
                        continue;
                    }
                }
                else if (e.GetType() == typeof(TempoEvent))
                {
                    var tempoEvent = (TempoEvent)e;
                    data.Add($"T{tempoEvent.MicrosecondsPerQuarterNote}");
                    data.Add(tempoEvent.AbsoluteTime.ToString());
                }
            }
        }

        return data;
    }

    public static void ReadDir(string path)
    {
        if (GetPathType(path) != 'd')
            throw new DirectoryNotFoundException($"{path} is not a valid directory");
    }

    public static void WriteFile(List<string> data)
    {
        
    }
    public static void WriteFile(string data)
    {
        WriteFile(data.Split('\n').ToList());
    }
    
    /// <summary>
    /// Checks whether a path is valid 
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