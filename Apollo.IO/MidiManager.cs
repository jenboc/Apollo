using NAudio.Midi;

namespace Apollo.IO;

/// <summary>
/// Static class that handles anything to do with MidiFiles or music
/// </summary>
public static class MidiManager
{
    #region Reading Files

    private const int READ_LIMIT = 10000;
    
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
        var numRead = 0;
        var reachedMax = false;
        for (var trackNum = 0; trackNum < file.Tracks && !reachedMax; trackNum++)
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
                numRead++;

                reachedMax = numRead > READ_LIMIT;
            }
        }

        return data;
    }
    
    /// <summary>
    /// Reads all midi files in a given directory
    /// </summary>
    /// <param name="path">Path to the directory to read from</param>
    /// <returns>List containing the string representation of every midi file in the directory</returns>
    public static List<string> ReadDir(string path)
    {
        // Check path is a directory 
        if (GetPathType(path) != 'd')
            throw new DirectoryNotFoundException($"{path} is not a valid directory");

        var dirData = new List<string>();

        // Iterate through all files in directory
        // Call ReadFile with the path 
        // Add the result to a list
        var dirInfo = new DirectoryInfo(path);
        var dirFiles = dirInfo.GetFiles();

        foreach (var fileInfo in dirFiles)
        {
            // Check that the file is a midi file
            if (!fileInfo.Name.EndsWith(".mid") && !fileInfo.Name.EndsWith(".midi"))
                continue;
            
            var filePath = fileInfo.FullName;
            var fileData = ReadFile(filePath);
            dirData.Add(fileData);
        }

        return dirData;
    }


    #endregion

    #region Writing Files

    // There are 12 semitones in an octave
    private const int SEMITONES_IN_OCTAVE = 12;

    // MIDI file parameters
    private const int TICKS_PER_QUARTER_NOTE = 480;
    private const int VELOCITY = 100;
    
    // How each note offsets the pitch
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
    // How sharps/flats affects the pitch
    private static readonly Dictionary<char, int> _pitchModifiers = new Dictionary<char, int>()
    {
        { '#', 1 },
        { 'b', -1 }
    };
    
    /// <summary>
    /// Writes a string representation to a MIDI file 
    /// </summary>
    /// <param name="data">The string representation of the file</param>
    /// <param name="path">The path of the file to write it to</param>
    /// <param name="beatsPerMinute">Music's beat per minute</param>
    public static void WriteFile(string data, string path, int beatsPerMinute)
    {
        // Magic number, change later
        var collection = new MidiEventCollection(0, TICKS_PER_QUARTER_NOTE);

        // Notes last for 3/4 of a MIDI tick
        var noteDur = (3 / 4) * TICKS_PER_QUARTER_NOTE;

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
                currentNote.Clear();
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

            var onEvent = new NoteOnEvent(absoluteTime, 1, pitch, VELOCITY, noteDur);
            var offEvent = new NoteEvent(absoluteTime + noteDur, 1, MidiCommandCode.NoteOff, pitch, 0);
            
            collection.AddEvent(onEvent, 1);
            collection.AddEvent(offEvent, 1);

            currentNote.Clear();
        }
        
        collection.PrepareForExport();
        MidiFile.Export(path, collection);
    }


    #endregion

    /// <summary>
    /// Delete all MIDI files in a directory
    /// </summary>
    /// <param name="dirPath">The directory to delete from</param>
    public static void PurgeDir(string dirPath)
    {
        foreach (var filePath in Directory.GetFiles(dirPath))
        {
            if (filePath.EndsWith(".mid") || filePath.EndsWith(".midi"))
                File.Delete(filePath);
        }
    }

    /// <summary>
    /// Add a variable number of spaces to a string
    /// </summary>
    /// <param name="data">A reference to the string to add spaces to</param>
    /// <param name="numSpaces">The number of spaces</param>
    private static void AddSpaces(ref string data, int numSpaces)
    {
        var spaces = "";
        Parallel.For(0, numSpaces, i => { spaces += " "; });
        data += spaces;
    }



    /// <summary>
    /// Calculate microseconds per quarter note from beats per minute
    /// </summary>
    /// <param name="bpm">Beats per minute</param>
    /// <returns>Microseconds per quarter note</returns>
    private static int CalculateMicrosecondsPerQuarterNote(int bpm)
    {
        // Number of microseconds in a minute ÷ bpm 
        // 60 seconds * 1000 => milliseconds * 1000 => microseconds
        return 60 * 1000 * 1000 / bpm;
    }

    /// <summary>
    /// Creates a MIDI value for passed in note
    /// </summary>
    /// <param name="note">String representation of note</param>
    /// <returns>Integer of the MIDI representation of the note</returns>
    private static int ParseNote(Note note)
    {
        // Start at the offsetted pitch for the letter (if there is one) 
        var noteValue = (_pitchOffsets.ContainsKey(note.NoteName)) ? _pitchOffsets[note.NoteName] : 0;

        // Pitch modifiers can only be applied to notes w/ letter
        if (_pitchModifiers.ContainsKey(note.Modifier) && noteValue != 0)
            noteValue += _pitchModifiers[note.Modifier];

        // -1 is used to state that there is no octave in the Note struct
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