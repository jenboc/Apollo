using Apollo.MatrixMaths;
using Apollo.MIDI;
using Apollo.NeuralNet;

var alpha = 0.001f;
var beta1 = 0.9f;
var beta2 = 0.999f;
var epsilon = 1e-8f;
var hyperparameters = new AdamParameters(alpha, beta1, beta2, epsilon);

var r = new Random();

static void DisplayMat(Matrix mat)
{
    for (var i = 0; i < mat.Rows; i++)
    {
        for (var j = 0; j < mat.Columns; j++) Console.Write($"{mat[i, j]}\t");
        Console.WriteLine();
    }
}

const string PATH = @"file.mid";

var fred = MidiManager.ReadFile(PATH);
MidiManager.WriteFile(fred, "testwrite.mid");

Console.WriteLine(fred);

var vocab = new Vocab(fred);
var trainingData = vocab.PrepareTrainingData(fred);

Console.WriteLine($"Vocab Size: {vocab.Size}");
var rnn = new Rnn("rnn.state", vocab.Size, 10, 32, 50, hyperparameters, r);

rnn.Train(trainingData, 1000, 200, r);

var initialInput = new Matrix[32];
for (var i = 0; i < 32; i++) initialInput[i] = trainingData[i];

var outputs = rnn.Forward(Matrix.StackArray(initialInput));

foreach (var output in outputs)
    for (var i = 0; i < output.Rows; i++)
    {
        var rowContent = new float[1, output.Columns];

        for (var j = 0; j < output.Columns; j++) rowContent[0, j] = output[i, j];

        var mat = new Matrix(rowContent);
        Console.Write(vocab.InterpretOneHot(mat));
    }