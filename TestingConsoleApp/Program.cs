using Apollo.MatrixMaths;
using Apollo.NeuralNet;
using System.Text.Json;

var r = new Rnn(5, 10, 10e-5f);

var initialInput = Matrix.Random(5,1);
var outputs = r.Forward(initialInput);

Console.WriteLine(JsonSerializer.Serialize(outputs[0].Contents));