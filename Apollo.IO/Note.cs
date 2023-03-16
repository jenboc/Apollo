namespace Apollo.IO;

/// <summary>
///     Struct to hold data about a single note
/// </summary>
internal struct Note
{
    public int Octave;
    public char Modifier;
    public char NoteName;

    public override string ToString()
    {
        return $"{NoteName}{Octave}{Modifier}";
    }

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
        // A note is "incomplete" if either the note name or the octave does not have an assigned value 
        return Octave == -1 || NoteName == ' ';
    }
}