using System.IO;
namespace Apollo.MIDI;

public static class MidiReader
{
    public static void Read(string path)
    {
        
    }

    public static void ReadDir(string path)
    {
        
    }

    private static bool PathIsValid(string path)
    {
        var attributes = File.GetAttributes(path);
        
        return attributes switch
        {
            FileAttributes.Directory => Directory.Exists(path),
            _ => File.Exists(path)
        };
    }
}