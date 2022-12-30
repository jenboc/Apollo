namespace Apollo.MIDI;

public static class MidiReader
{
    public static void Read(string path)
    {
        
    }

    public static void ReadDir(string path)
    {
        
    }
    
    /// <summary>
    /// Checks whether a path is valid 
    /// </summary>
    /// <param name="path">The path to validate</param>
    /// <returns>A character flag (d => directory, f => file, n => invalid path)</returns>
    private static char PathIsValid(string path)
    {
        var attributes = File.GetAttributes(path);

        if (attributes == FileAttributes.Directory && Directory.Exists(path))
            return 'd';

        return File.Exists(path) ? 'f' : 'n';
    }
}