using Apollo.MatrixMaths;
using Apollo.IO; 
using Apollo.NeuralNet;


const int BATCH_SIZE = 4;
const int HIDDEN_SIZE = 32;
const int RECURRENCE_AMOUNT = 10000;
const int NUM_EPOCHS = 500;
const int NUM_TIMESTEPS = 200;

static Matrix CreateGenSeed(Vocab vocab, Random r)
{
    var rows = new Matrix[BATCH_SIZE];
    for (var i = 0; i < BATCH_SIZE; i++)
    {
        var id = r.Next(vocab.Size);
        rows[i] = vocab.CreateOneHot(vocab[id]);
    }

    return Matrix.StackArray(rows);
}

static string Generate(Rnn rnn, Matrix genSeed, Vocab vocab)
{
    var outputs = rnn.Forward(genSeed);
    var generated = "";
    foreach (var output in outputs)
    {
        var rowContent = new float[1, output.Columns];
        for (var j = 0; j < output.Columns; j++) rowContent[0, j] = output[output.Rows-1, j];
        var mat = new Matrix(rowContent);
        generated += vocab.InterpretOneHot(mat);
    }

    return generated;
}


var alpha = 0.001f;
var beta1 = 0.9f;
var beta2 = 0.999f;
var epsilon = 1e-8f;
var hyperparameters = new AdamParameters(alpha, beta1, beta2, epsilon);

var r = new Random();

const string PATH = @"file.mid";

var fred = MidiManager.ReadFile(PATH);

var vocab = new Vocab(fred);
var trainingData = vocab.PrepareTrainingData(fred);

var rnn = new Rnn("rnn.state", vocab.Size, HIDDEN_SIZE, BATCH_SIZE, RECURRENCE_AMOUNT, hyperparameters, r);

// rnn.Train(trainingData, NUM_EPOCHS, NUM_TIMESTEPS, r);

for (var i = 1; i <= 10; i++)
{
    var fileName = $"generated/Generated{i}.mid";
    var seed = CreateGenSeed(vocab, r);
    var generatedString = Generate(rnn, seed, vocab); 
    MidiManager.WriteFile(generatedString, fileName, 30);
}