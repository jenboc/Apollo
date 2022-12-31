using Apollo.MIDI;
using Apollo.NeuralNet;
using Apollo.MatrixMaths;

const string PATH = @"bach_846.mid";

static void DisplayVector(Matrix vector)
{
    for (var row = 0; row < vector.Rows; row++)
    {
        Console.WriteLine(vector[row, 0]);
    }
}

var vocab = new Vocab();
var midiString = string.Join('\n', MidiManager.ReadFile(PATH));

var trainingData = vocab.PrepareTrainingData(midiString);

var rnn = new Rnn(vocab.Size, 1, 10e-5f);

foreach (var data in trainingData)
{
    var outputs = rnn.Forward(data);

    foreach (var output in outputs)
    {
        var result = vocab.InterpretOneHot(output);
        Console.Write(result);
    }
}
