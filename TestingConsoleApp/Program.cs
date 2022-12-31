using Apollo.MIDI;

const string PATH = @"D:\Documents\0. Documents\Programming\Apollo\TestingConsoleApp\bin\Debug\net7.0";

var dirData = MidiManager.ReadDir(PATH);

Console.WriteLine(dirData.Count);

for (var i = 1; i <= dirData.Count; i++)
{
    var name = $"rewritten{i}.mid";
    MidiManager.WriteFile(dirData[i - 1], name.ToString());
}