using System.Data.Common;
using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
///     A matrix representing a weight in the network, can be optimised
/// </summary>
public class Weight : Matrix
{
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

    public Weight(float[,] defaultData) : base(defaultData)
    {
        // Create gradient matrix of the same size
        Gradient = new Matrix(Rows, Columns);

        // Create ADAM matrices with same size
        MomentVector = new Matrix(Rows, Columns);
        InfinityNorm = new Matrix(Rows, Columns);
    }

    public Matrix Gradient { get; set; }

    /// <summary>
    ///     Optimise the weight using the Adaptive Moment Estimation Algorithm (ADAM)
    /// </summary>
    /// <param name="t">Backpropagation timestep</param>
    public void Adam(int t)
    {
        // Change ADAM matrices
        MomentVector = AdamParameters.BETA1 * MomentVector + (1 - AdamParameters.BETA1) * Gradient;
        InfinityNorm = AdamParameters.BETA2 * InfinityNorm + (1 - AdamParameters.BETA2) * Power(Gradient, 2);

        // Calculate m_hat and v_hat 
        var mHat = MomentVector / (1 - MathF.Pow(AdamParameters.BETA1, t));
        var vHat = InfinityNorm / (1 - MathF.Pow(AdamParameters.BETA2, t));

        // Update weight 
        var update = HadamardDiv(AdamParameters.ALPHA * mHat, Sqrt(vHat) + AdamParameters.EPSILON);
        Subtract(update);
    }

    public new static Weight ReadFromFile(BinaryReader reader, int rows, int columns)
    {
        var readData = Matrix.ReadFromFile(reader, rows, columns).Contents;
        return new Weight(readData);
    }

    #region ADAM Matrices

    private Matrix MomentVector { get; set; }
    private Matrix InfinityNorm { get; set; }

    #endregion
}