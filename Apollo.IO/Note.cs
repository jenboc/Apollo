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
        // A note is "incomplete" if it doesn't have a data value for all 3 parts of a note
        return Octave == -1 || NoteName == ' ';
    }
}