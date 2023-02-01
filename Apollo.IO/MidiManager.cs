using NAudio.Midi;

namespace Apollo.IO;

/// <summary>
/// Struct to hold data about a single note
/// </summary>
struct Note
{
    public int Octave;
    public char Modifier;
    public char NoteName;

    public Note()
    {
        Clear();
    }
    
    public void Clear()
    {
        Octave = -1;
        Modifier = ' ';
        NoteName = ' ';
    }

    public bool IsIncomplete()
    {
        return Octave == -1 || Modifier == ' ' || NoteName == ' ';
    }
}

/// <summary>
/// Static class that handles anything to do with MidiFiles or music
/// </summary>
public static class MidiManager
{
    // Constants and dictionaries for creating MidiFile from a string 
    private const int SEMITONES_IN_OCTAVE = 12;
    private static readonly Dictionary<char, int> _pitchOffsets = new Dictionary<char, int>()
    {
        { 'C', 0 },
        { 'D', 2 },
        { 'E', 4 },
        { 'F', 5 },
        { 'G', 7 },
        { 'A', 9 },
        { 'B', 11 }
    };
    private static readonly Dictionary<char, int> _pitchModifiers = new Dictionary<char, int>()
    {
        { '#', 1 },
        { 'b', -1 }
    };
    
    /// <summary>
    /// Read a MidiFile at the specified path
    /// </summary>
    /// <param name="path">Path to the MIDI file</param>
    /// <returns>The string representation of the notes in the file</returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static string ReadFile(string path)
    {
        if (GetPathType(path) != 'f')
            throw new FileNotFoundException($"{path} is not a valid file path");

        var file = new MidiFile(path, false);
        var data = "";

        var prevAbsoluteTime = 0L; 
        for (var trackNum = 0; trackNum < file.Tracks; trackNum++)
        {
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
    
    /// <summary>
    /// Add a variable number of spaces to a string
    /// </summary>
    /// <param name="data">A reference to the string to add spaces to</param>
    /// <param name="numSpaces">The number of spaces</param>
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
    /// <param name="beatsPerMinute">Music's beat per minute</param>
    public static void WriteFile(string data, string path, int beatsPerMinute)
    {
        // Magic number, change later
        var collection = new MidiEventCollection(0, 480);

        // Magic Numbers, change later
        var velocity = 100;
        var noteDur = 3 * 480 / 4;

        var absoluteTime = 0L;
        
        // Create tempo event so that the MidiFile is valid
        var tempoEvent = new TempoEvent(CalculateMicrosecondsPerQuarterNote(beatsPerMinute), absoluteTime);
        collection.AddEvent(tempoEvent, 1);

        var currentNote = new Note(); 
        foreach (var c in data)
        {
            // Space = increment absolute time 
            if (c == ' ')
            {
                absoluteTime++;
                continue; 
            }
            
            // Add information to note
            if (char.IsUpper(c) && currentNote.NoteName == ' ')
                currentNote.NoteName = c;
            else if (_pitchModifiers.ContainsKey(c) && currentNote.Modifier == ' ')
                currentNote.Modifier = c;
            else if (char.IsNumber(c) && currentNote.Octave == -1)
                currentNote.Octave = (int)char.GetNumericValue(c);
            
            // If the information about the note isn't complete then continue
            if (currentNote.IsIncomplete())
                continue;

            // Gets here when enough information was gathered for a complete note
            // Add note to collection 
            var pitch = ParseNote(currentNote);

            var onEvent = new NoteOnEvent(absoluteTime, 1, pitch, velocity, noteDur);
            var offEvent = new NoteEvent(absoluteTime + noteDur, 1, MidiCommandCode.NoteOff, pitch, 0);
            
            collection.AddEvent(onEvent, 1);
            collection.AddEvent(offEvent, 1);

            currentNote.Clear();
        }
        
        collection.PrepareForExport();
        MidiFile.Export(path, collection);
    }

    /// <summary>
    /// Calculate microseconds per quarter note from beats per minute
    /// </summary>
    /// <param name="bpm">Beats per minute</param>
    /// <returns>Microseconds per quarter note</returns>
    private static int CalculateMicrosecondsPerQuarterNote(int bpm)
    {
        return 60 * 1000 * 1000 / bpm;
    }

    /// <summary>
    /// Creates a MIDI value for passed in note
    /// </summary>
    /// <param name="note">String representation of note</param>
    /// <returns>Integer of the MIDI representation of the note</returns>
    private static int ParseNote(Note note)
    {
        var noteValue = (_pitchOffsets.ContainsKey(note.NoteName)) ? _pitchOffsets[note.NoteName] : 0;

        if (_pitchModifiers.ContainsKey(note.Modifier) && noteValue != 0)
            noteValue += _pitchModifiers[note.Modifier];

        if (note.Octave != -1)
            noteValue += SEMITONES_IN_OCTAVE * note.Octave;
        
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