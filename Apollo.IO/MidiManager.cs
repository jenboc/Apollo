using System.ComponentModel;
using NAudio.Midi;

namespace Apollo.IO;

public static class MidiManager
{
    private const int SEMITONES_IN_OCTAVE = 12;
    
    private static Dictionary<char, int> _pitchOffsets = new Dictionary<char, int>()
    {
        { 'c', 0 },
        { 'd', 2 },
        { 'e', 4 },
        { 'f', 5 },
        { 'g', 7 },
        { 'a', 9 },
        { 'b', 11 }
    };

    private static Dictionary<char, int> _pitchModifiers = new Dictionary<char, int>()
    {
        { '#', 1 },
        { 'b', -1 }
    };
    
    public static string ReadFile(string path)
    {
        if (GetPathType(path) != 'f')
            throw new FileNotFoundException($"{path} is not a valid file path");

        var file = new MidiFile(path, false);
        var data = ""; 
        
        Console.WriteLine(file.DeltaTicksPerQuarterNote);
        
        var prevAbsoluteTime = 0L; 
        for (var trackNum = 0; trackNum < file.Tracks; trackNum++)
        {
            // foreach (var e in file.Events[trackNum])
            //     if (e.GetType() == typeof(NoteOnEvent))
            //     {
            //         try
            //         {
            //             var onEvent = (NoteOnEvent)e;
            //             var offEvent = onEvent.OffEvent;
            //
            //             var noteNumber = onEvent.NoteNumber;
            //             var onTime = onEvent.AbsoluteTime;
            //             var offTime = offEvent.AbsoluteTime;
            //             var noteLength = onEvent.NoteLength;
            //             var velocity = onEvent.Velocity;
            //
            //             data.Add($"{noteNumber},{onTime},{offTime},{noteLength},{velocity}");
            //         }
            //         catch
            //         {
            //             // Log 
            //         }
            //     }
            //     else if (e.GetType() == typeof(TempoEvent))
            //     {
            //         var tempoEvent = (TempoEvent)e;
            //         data.Add($"{tempoEvent.MicrosecondsPerQuarterNote}, {tempoEvent.AbsoluteTime}");
            //     }
            
            foreach (var e in file.Events[trackNum])
            {
                if (e.GetType() != typeof(NoteOnEvent))
                    continue;

                var midiEvent = (NoteOnEvent)e;
                var note = midiEvent.NoteName;
                var absoluteTime = midiEvent.AbsoluteTime;

                var timeDiff = Convert.ToInt32(absoluteTime - prevAbsoluteTime);
                AddSpaces(ref data, timeDiff);
                data += note;

                prevAbsoluteTime = absoluteTime;
            }
        }

        return data;
    }

    private static void AddSpaces(ref string data, int numSpaces)
    {
        for (var i = 0; i < numSpaces; i++)
            data += " "; 
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
        // var lines = data.Split('\n');
        // var dTicks = int.Parse(lines[0]);
        // var collection = new MidiEventCollection(0, dTicks);
        //
        // for (var i = 1; i < lines.Length; i++)
        // {
        //     var eventType = DetermineEventType(lines[i]);
        //     var eventData = lines[i].Split(',');
        //
        //     switch (eventType)
        //     {
        //         case EventType.NoteEvent:
        //             var noteNumber = int.Parse(eventData[0]);
        //             var onEventTime = long.Parse(eventData[1]);
        //             var offEventTime = long.Parse(eventData[2]);
        //             var duration = int.Parse(eventData[3]);
        //             var velocity = int.Parse(eventData[4]);
        //             
        //             collection.AddEvent(new NoteOnEvent(onEventTime, 1, noteNumber, velocity, duration), 1);
        //             collection.AddEvent(new NoteEvent(offEventTime, 1, MidiCommandCode.NoteOff, noteNumber, velocity),
        //                 1);
        //             break;
        //         case EventType.TempoEvent:
        //             var microsecondsPerQuarterNote = int.Parse(eventData[0]);
        //             var absoluteTime = long.Parse(eventData[1]);
        //             collection.AddEvent(new TempoEvent(microsecondsPerQuarterNote, absoluteTime), 1);
        //             break;
        //         case EventType.InvalidEvent:
        //             break;
        //     }
        // }

        // Magic number, change later
        var collection = new MidiEventCollection(0, 480);

        var velocity = 100;
        var noteDur = 3 * 480 / 4;
        var noteSpace = 480;

        var currentNote = ""; 
        var numSpaces = 0L;

        var bpm = 60;
        var tempoEvent = new TempoEvent(CalculateMicrosecondsPerQuarterNote(bpm), numSpaces);
        collection.AddEvent(tempoEvent, 1);
        
        foreach (var c in data)
        {
            // Increment num spaces (represents absolute time) 
            if (c == ' ')
            {
                numSpaces++;
                continue; 
            }
            
            // Add to stack if it is empty (or is related to the same note) 
            if ((char.IsLetter(c) && char.IsUpper(c) && currentNote.Length == 0) || !(char.IsLetter(c) && char.IsUpper(c)))
            {
                currentNote += c; 
                continue;
            }
            
            // Gets here if it reaches a new note 
            // Add note to collection 
            var pitch = ParseNote(currentNote);

            var onEvent = new NoteOnEvent(numSpaces, 1, pitch, velocity, noteDur);
            var offEvent = new NoteEvent(numSpaces + noteDur, 1, MidiCommandCode.NoteOff, pitch, 0);
            
            collection.AddEvent(onEvent, 1);
            collection.AddEvent(offEvent, 1);

            currentNote = "";
        }
        
        collection.PrepareForExport();
        MidiFile.Export(path, collection);
    }

    private static int CalculateMicrosecondsPerQuarterNote(int bpm)
    {
        return 60 * 1000 * 1000 / bpm;
    }

    /// <summary>
    /// Creates a MIDI value for passed in note
    /// </summary>
    /// <param name="note">String representation of note</param>
    /// <returns>Integer of the MIDI representation of the note</returns>
    private static int ParseNote(string note)
    {
        note = note.ToLower();
        var noteName = note[0];
        
        char modifier;
        int octave;
        if (note.Length == 3) // == 3 if it has both modifier and octave
        {
            modifier = note[1];
            octave = (int)char.GetNumericValue(note[2]);
        }
        else
        {
            modifier = '_'; // Placeholder for empty modifier

            try
            {
                octave = (int)char.GetNumericValue(note[1]);
            }
            catch
            {
                octave = (int)char.GetNumericValue(note[0]);
            }
        }

        var noteValue = (_pitchOffsets.ContainsKey(noteName)) ? _pitchOffsets[noteName] : 0;

        if (modifier != '_')
            noteValue += _pitchModifiers[modifier];

        noteValue += SEMITONES_IN_OCTAVE * octave;
        
        return noteValue;
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