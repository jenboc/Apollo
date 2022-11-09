using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

// Gate class responsible for optimising associated weight matrices and calculations in the LSTM cell
public class Gate
{
    // All gate calculations take the form of 
    // Some activation function (W_x * x + W_h * h)
    // Where
    // input = x 
    // prev_output = h

    public Matrix Value { get; set; }
    private Matrix Weight { get; set; }
    private Matrix Bias { get; set; }
    private Matrix WeightGradient { get; set; } // Gradient for adjusting input weight
    private Matrix BiasGradient { get; set; }

    public Gate(MatShape inputShape, MatShape biasShape)
    {
        Value = new Matrix(inputShape);
        
        Weight = Matrix.Random(inputShape);
        Bias = Matrix.Random(biasShape);

        WeightGradient = new Matrix(inputShape);
        BiasGradient = new Matrix(biasShape);
    }

    public void CalcUnactivated(Matrix input)
    {
        Value = Matrix.Multiply(Weight, input) + Bias;
    }
}