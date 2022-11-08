using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Weight
{
    private Matrix WeightMat { get; set; }
    private Matrix Gradient { get; set; }

    public Weight(int rows, int columns)
    {
        // Initialise weight matrix randomly 
        WeightMat = Matrix.Random(rows, columns);
        
        // Initialise a zero matrix the same size as the weight matrix
        Gradient = new Matrix(rows, columns); 
    }
}