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
    /// <param name="hyperparameters">The hyperparameters for the algorithm</param> 
    public void Adam(AdamParameters hyperparameters)
    {
        // Change ADAM matrices
        MomentVector = hyperparameters.Beta1 * MomentVector + (1 - hyperparameters.Beta1) * Gradient;
        InfinityNorm = hyperparameters.Beta2 * InfinityNorm + (1 - hyperparameters.Beta2) * Power(Gradient, 2);
        
        // Calculate m_hat and v_hat 
        var mHat = MomentVector / (1 - hyperparameters.Beta1);
        var vHat = InfinityNorm / (1 - hyperparameters.Beta2);
        
        // Update weight 
        var update = HadamardDiv(hyperparameters.Alpha * mHat, Sqrt(vHat) + hyperparameters.Epsilon);
        Subtract(update);
    }
}