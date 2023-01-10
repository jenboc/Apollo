using Apollo.MatrixMaths;
using Apollo.MIDI;
using Apollo.NeuralNet;

const string PATH = @"bach_846.mid";

var bob = MidiManager.ReadFile(PATH);
var fred = string.Join('\n', bob);

Console.WriteLine(fred);

var vocab = new Vocab(fred);
var trainingData = vocab.PrepareTrainingData(fred);

var rnn = new Rnn(vocab.Size, 500, 0.001f);
var loss = rnn.Train(trainingData);

Console.WriteLine(loss); 