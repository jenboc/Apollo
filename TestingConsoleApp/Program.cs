using Apollo.MatrixMaths;
using Apollo.MIDI;
using Apollo.NeuralNet;

const string PATH = @"bach_846.mid";

var bob = MidiManager.ReadFile(PATH);
var fred = string.Join('\n', bob);

Console.WriteLine(fred);

var vocab = new Vocab(fred);
var trainingData = vocab.PrepareTrainingData(fred);

var rnn = new Rnn(vocab.Size, 2, 1, 50, 0.001f);
var initInput = trainingData[0];
var output = rnn.Forward(initInput);

foreach (var o in output)
{
    Console.WriteLine();

    for (var c = 0; c < o.Columns; c++)
    {
        Console.Write($"{o[0, c]}\t");
    }
}