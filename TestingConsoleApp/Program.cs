using Apollo.MatrixMaths;
using Apollo.NeuralNet;
using System.Text.Json;

var expectedOutput = new[,]
{
    { 3f, 2f, 25f },
    { 54f, 130f, 36f },
    { 16f, 352f, 132f }
};
        
var matContent1 = new[,]
{
    { 1f, 0.5f, 5f },
    { 2f, 13f, 12f },
    { 4f, 22f, 11f }
};

var matContent2 = new[,]
{
    { 3f, 4f, 5f },
    { 27f, 10f, 3f },
    { 4f, 16f, 12f }
};

var mat1 = new Matrix(matContent1);
var mat2 = new Matrix(matContent2);

Console.WriteLine(mat1.Contents[0,0]);

var mat1x2 = Matrix.Hadamard(mat1, mat2);
var mat2x1 = Matrix.Hadamard(mat2, mat1);

Console.WriteLine(mat1.Contents[0,0]);