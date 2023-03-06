using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
///     Gate class responsible for optimising associated weight matrices and calculations in the LSTM cell
/// </summary>
public class Gate
{
    /// <summary>
    ///     Create an LSTM gate, with correctly sized weights and bias.
    /// </summary>
    public Gate(int vocabSize, int hiddenSize, int batchSize, Random r)
    {
        Value = new Matrix(batchSize, hiddenSize);

        InputWeight = new Weight(vocabSize, hiddenSize, r);
        PrevOutputWeight = new Weight(hiddenSize, hiddenSize, r);

        Bias = new Matrix(batchSize, hiddenSize, r);
    }

    /// <summary>
    ///     Create an LSTM gate using the state file
    /// </summary>
    /// <param name="vocabSize">Vocab size (already read)</param>
    /// <param name="hiddenSize">Hidden size (already read)</param>
    /// <param name="batchSize">Batch size (already read)</param>
    /// <param name="reader">Stream to the state file to read from</param>
    public Gate(int vocabSize, int hiddenSize, int batchSize, BinaryReader reader)
    {
        Value = new Matrix(batchSize, hiddenSize);

        InputWeight = Weight.ReadFromFile(reader, vocabSize, hiddenSize);
        PrevOutputWeight = Weight.ReadFromFile(reader, hiddenSize, hiddenSize);
        Bias = Matrix.ReadFromFile(reader, batchSize, hiddenSize);
    }

    public Matrix Value { get; set; } // Attribute to store the value of the gate 
    public Weight InputWeight { get; set; }
    public Weight PrevOutputWeight { get; set; }
    private Matrix Bias { get; }

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

    /// <summary>
    ///     Update the gate's weights
    /// </summary>
    /// <param name="t">Backpropagation timestep</param>
    public void Update(int t)
    {
        InputWeight.Adam(t);
        PrevOutputWeight.Adam(t);
    }

    /// <summary>
    ///     Writes the values of the gate's weights to the binary file
    /// </summary>
    /// <param name="writer">Instance of BinaryWriter to use for writing</param>
    public void WriteToFile(BinaryWriter writer)
    {
        InputWeight.WriteToFile(writer);
        PrevOutputWeight.WriteToFile(writer);
        Bias.WriteToFile(writer);
    }
}