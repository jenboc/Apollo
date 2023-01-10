using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

// Gate class responsible for optimising associated weight matrices and calculations in the LSTM cell
public class Gate
{
    /// <summary>
    /// Create an LSTM gate, with correctly sized weights and bias.
    /// </summary>
    /// <param name="vocabSize"></param>
    /// <param name="hiddenSize"></param>
    /// <param name="batchSize"></param>
    public Gate(int vocabSize, int hiddenSize, int batchSize)
    {
        Value = new Matrix(batchSize, hiddenSize);

        InputWeight = Matrix.Random(vocabSize, hiddenSize);
        PrevOutputWeight = Matrix.Random(hiddenSize, hiddenSize);
        Bias = Matrix.Random(batchSize, hiddenSize);

        InputWeightGradient = Matrix.Like(InputWeight);
        PrevOutputWeightGradient = Matrix.Like(PrevOutputWeight);
        BiasGradient = Matrix.Like(Bias);
    }

    public Matrix Value { get; set; } // Attribute to store the value of the gate 
    private Matrix InputWeight { get; set; }
    private Matrix PrevOutputWeight { get; set; }
    private Matrix Bias { get; }
    private Matrix InputWeightGradient { get; } // Gradient for adjusting the input weight
    private Matrix PrevOutputWeightGradient { get; } // Gradient for adjusting the previous output weight
    private Matrix BiasGradient { get; } // Gradient for adjusting bias 

    /// <summary>
    ///     Calculate the value of the gate with a given input
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

        Value = Matrix.Multiply(input, InputWeight) 
                + Matrix.Multiply(prevOutput, PrevOutputWeight) + Bias;
    }

    /// <summary>
    ///     Set the value of the gate to 0
    /// </summary>
    public void Clear()
    {
        Value *= 0;
    }

    public void Update(Matrix inputGradient, Matrix prevOutputGradient, float learningRate)
    {
        InputWeight = InputWeight - inputGradient * learningRate;
        PrevOutputWeight = PrevOutputWeight - prevOutputGradient * learningRate;
    }
}