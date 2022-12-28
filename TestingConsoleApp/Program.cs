using Apollo.MatrixMaths;
using Apollo.NeuralNet;
using System.Text.Json;

var vocabSize = 3;
var recurrenceLength = 20;
var learningRate = 10e-5f;

var initialInput = Matrix.Random(vocabSize, 1);

var r = new Rnn(vocabSize, recurrenceLength, learningRate);

var outputs = r.Forward(initialInput);

Console.WriteLine(JsonSerializer.Serialize(outputs[0]));