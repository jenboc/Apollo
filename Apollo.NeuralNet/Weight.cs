using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
/// A matrix representing a weight in the network, can be optimised
/// </summary>
public class Weight : Matrix
{
    private Matrix Gradient { get; set; }
    
    /// <param name="rows">Number of Rows in the weight matrix</param>
    /// <param name="columns">Number of Columns in the weight matrix</param>
    /// <param name="r">Random Instance</param> 
    public Weight(int rows, int columns, Random r) : base(rows, columns, r)
    {
        // Create gradient matrix of the same size
        Gradient = new Matrix(rows, columns); 
    }

    /// <summary>
    /// Optimise the weight using the Adaptive Moment Estimation Algorithm (ADAM) 
    /// </summary>
    public void Adam()
    {
        
    }
}