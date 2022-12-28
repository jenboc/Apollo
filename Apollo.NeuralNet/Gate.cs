using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

// Gate class responsible for optimising associated weight matrices and calculations in the LSTM cell
public class Gate
{
    /// <param name="weightShape">Shape of gate's weight matrix</param>
    /// <param name="biasShape">Shape of gate's bias matrix </param>
    public Gate(int[] weightShape, int[] biasShape)
    {
        Value = new Matrix(weightShape[0], weightShape[1]);

        InputWeight = Matrix.Random(weightShape[0], weightShape[1]);
        PrevOutputWeight = Matrix.Random(weightShape[0], weightShape[1]);
        Bias = Matrix.Random(biasShape[0], biasShape[1]);

        WeightGradient = new Matrix(weightShape[0], weightShape[1]);
        BiasGradient = new Matrix(biasShape[0], biasShape[1]);

        WeightRows = weightShape[0];
        WeightColumns = weightShape[1];
        BiasRows = biasShape[0];
        BiasColumns = biasShape[1];
    }
    
    public Matrix Value { get; set; } // Attribute to store the value of the gate 
    private Matrix InputWeight { get; }
    private Matrix PrevOutputWeight { get; }
    private Matrix Bias { get; }
    private Matrix WeightGradient { get; } // Gradient for adjusting  weight
    private Matrix BiasGradient { get; } // Gradient for adjusting bias 

    public int WeightRows { get; }
    public int WeightColumns { get; }
    public int BiasRows { get; }
    public int BiasColumns { get; }

    /// <summary>
    /// Calculate the value of the gate with a given input 
    /// </summary>
    /// <param name="input">The input into the gate</param>
    /// <param name="prevOutput">The previous output of the LSTM</param>
    public void CalcUnactivated(Matrix input, Matrix prevOutput)
    {
        // All gate calculations take the form of 
        // Some activation function (W_x * x + W_h * h)
        // Where
        // input = x 
        // prev_output = h

        Value = Matrix.Multiply(InputWeight, input) + Matrix.Multiply(PrevOutputWeight, prevOutput) + Bias;
    }
}