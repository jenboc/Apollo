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
