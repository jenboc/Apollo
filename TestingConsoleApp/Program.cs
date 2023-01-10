using Apollo.MatrixMaths;
using Apollo.MIDI;
using Apollo.NeuralNet;

const string PATH = @"bach_846.mid";

var bob = MidiManager.ReadFile(PATH);
var fred = string.Join('\n', bob);

Console.WriteLine(fred);

var vocab = new Vocab(fred);
var trainingData = vocab.PrepareTrainingData(fred);

Console.WriteLine($"Vocab Size: {vocab.Size}");
var rnn = new Rnn(vocab.Size, 10, 32, 50, 10e-5f);

rnn.Train(trainingData, 10);