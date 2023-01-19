using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
/// A matrix representing a weight in the network, can be optimised
/// </summary>
public class Weight : Matrix
{
    public Matrix Gradient { get; set; }
    
    #region ADAM Matrices
    private Matrix MomentVector { get; set; }
    private Matrix InfinityNorm { get; set; }
    #endregion 
    
    /// <param name="rows">Number of Rows in the weight matrix</param>
    /// <param name="columns">Number of Columns in the weight matrix</param>
    /// <param name="r">Random Instance</param> 
    public Weight(int rows, int columns, Random r) : base(rows, columns, r)
    {
        // Create gradient matrix of the same size
        Gradient = new Matrix(rows, columns);
        
        // Create ADAM matrices with same size
        MomentVector = new Matrix(rows, columns);
        InfinityNorm = new Matrix(rows, columns); 
    }

    /// <summary>
    /// Optimise the weight using the Adaptive Moment Estimation Algorithm (ADAM)
    /// </summary>
    /// <param name="alpha">Learning Rate</param>
    /// <param name="beta1">Exponential Decay for first moment estimates</param>
    /// <param name="beta2">Exponential Decay for second moment estimates</param>
    /// <param name="epsilon">Very small number to prevent division by 0</param>
    public void Adam(float alpha, float beta1, float beta2, float epsilon)
    {
        // Change ADAM matrices
        MomentVector = beta1 * MomentVector + (1 - beta1) * Gradient;
        InfinityNorm = beta2 * InfinityNorm + (1 - beta2) * Power(Gradient, 2);
        
        // Calculate m_hat and v_hat 
        var mHat = MomentVector / (1 - beta1);
        var vHat = InfinityNorm / (1 - beta2);
        
        // Update weight 
        var update = HadamardDiv(alpha * mHat, Sqrt(vHat) + epsilon);
        Subtract(update);
    }
}