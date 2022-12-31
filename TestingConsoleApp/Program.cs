using Apollo.MIDI;
using Apollo.NeuralNet;

const string PATH = @"bach_846.mid";

var vocab = new Vocab();
var midiString = string.Join('\n', MidiManager.ReadFile(PATH));

var trainingData = vocab.PrepareTrainingData(midiString);

foreach (var data in trainingData)
{
    Console.WriteLine($"{data.Rows}x{data.Columns}");
}

for (var row = 0; row < trainingData[0].Rows; row++)
{
    Console.WriteLine(trainingData[0][row,0]);
}