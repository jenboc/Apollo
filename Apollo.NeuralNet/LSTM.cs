using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Lstm
{
    public Lstm(int vocabSize, float learningRate)
    {
        VocabSize = vocabSize; // The amount of different characters present in the training data
        LearningRate = learningRate;

        var weightShape = new int[] { VocabSize, VocabSize };
        var biasShape = new int[] { VocabSize, 1 };
        Forget = new Gate(weightShape, biasShape);
        Input = new Gate(weightShape, biasShape);
        NewInfo = new Gate(weightShape, biasShape);
        Output = new Gate(weightShape, biasShape);

        CellState = new Matrix(VocabSize, 1);
    }

    // General Parameters
    private int VocabSize { get; }
    private float LearningRate { get; }

    // Gates 
    private Gate Forget { get; }
    private Gate Input { get; }
    private Gate NewInfo { get; }
    private Gate Output { get; }

    // Cell state 
    private Matrix CellState { get; set; }

    /// <summary>
    ///     Complete one pass through of the LSTM cell, given an input
    /// </summary>
    /// <param name="input">Matrix representing the input into the LSTM</param>
    /// <param name="previousOutput">LSTM's previous output</param> 
    public Matrix Forward(Matrix input, Matrix? previousOutput=null)
    {
        if (previousOutput == null)
        {
            // Create matrix of 0s with same size as output weight of gates
            previousOutput = new Matrix(Forget.WeightRows, Forget.WeightColumns);  
        }
        
        // Calculate forget gate value 
        Forget.CalcUnactivated(input, previousOutput);
        Forget.Value.Sigmoid();

        // Calculate input gate value
        Input.CalcUnactivated(input, previousOutput);
        Input.Value.Sigmoid();

        // Calculate new info value 
        NewInfo.CalcUnactivated(input, previousOutput);
        NewInfo.Value.Tanh();

        // Calculate cell state
        CellState = (Forget.Value * CellState) + (Input.Value * NewInfo.Value);

        // Calculate output gate
        Output.CalcUnactivated(input, previousOutput);
        Output.Value.Sigmoid();

        // return output 
        return Output.Value * Matrix.Tanh(CellState);
    }

    public Matrix[] Backprop(Matrix input, Matrix cellState, Matrix error, Matrix cellStates, Matrix forgetGate,
        Matrix inputGate, Matrix cell, Matrix outputGate, Matrix dfcs, Matrix dfhs)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjusts the variables of the LSTM relative to update parameters
    /// </summary>
    /// <param name="forgetUpdate">Matrix to update the forget gate weight relative to</param>
    /// <param name="inputUpdate">Matrix to update the input gate weight relative to</param>
    /// <param name="newInfoUpdate">Matrix to update the new info gate weight relative to</param>
    /// <param name="outputUpdate">Matrix to update the output gate weight relative to</param>
    public void Update(Matrix forgetUpdate, Matrix inputUpdate, Matrix newInfoUpdate, Matrix outputUpdate)
    {
    }
}