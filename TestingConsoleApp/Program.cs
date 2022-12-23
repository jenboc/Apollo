using Apollo.MatrixMaths;
using Apollo.NeuralNet;
using System.Text.Json;

var mat1 = Matrix.Random(1, 5);
var mat2 = Matrix.Random(5, 5);

var result = Matrix.Multiply(mat1, mat2);