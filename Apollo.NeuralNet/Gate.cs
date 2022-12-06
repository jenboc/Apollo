using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

// Gate class responsible for optimising associated weight matrices and calculations in the LSTM cell
public class Gate
{
    public Gate(MatShape inputShape, MatShape biasShape)
    {
        Value = new Matrix(inputShape);

        Weight = Matrix.Random(inputShape);
        Bias = Matrix.Random(biasShape);

        WeightGradient = new Matrix(inputShape);
        BiasGradient = new Matrix(biasShape);
    }
    // All gate calculations take the form of 
    // Some activation function (W_x * x + W_h * h)
    // Where
    // input = x 
    // prev_output = h

    public Matrix Value { get; set; }
    private Matrix Weight { get; }
    private Matrix Bias { get; }
    private Matrix WeightGradient { get; } // Gradient for adjusting input weight
    private Matrix BiasGradient { get; }

    public void CalcUnactivated(Matrix input)
    {
        Value = Matrix.Multiply(Weight, input) + Bias;
    }
}