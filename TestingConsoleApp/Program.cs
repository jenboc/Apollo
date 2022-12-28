using Apollo.MatrixMaths;
using Apollo.NeuralNet;
using System.Text.Json;

var vocabSize = 3;
var recurrenceLength = 100;
var learningRate = 10e-5f;

var initialInput = Matrix.Random(vocabSize, 1);

var r = new Rnn(vocabSize, recurrenceLength, learningRate);

var outputs = r.Forward(initialInput);

foreach (var o in outputs) 
{
    for (var i = 0; i < o.Rows; i++)
    {
        for (var j = 0; j < o.Columns; j++)
        {
            Console.Write($"{o.Contents[i,j]}\t");
        }
        Console.WriteLine();
    }
    Console.WriteLine("\n\n\n\n");
}
