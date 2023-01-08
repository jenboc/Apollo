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
        CandidateState = new Gate(weightShape, biasShape);
        Output = new Gate(weightShape, biasShape);

        CellState = new Matrix(VocabSize, 1);
    }

    // General Parameters
    private int VocabSize { get; }
    private float LearningRate { get; }

    // Gates 
    private Gate Forget { get; }
    private Gate Input { get; }
    private Gate Output { get; }
    
    // States 
    private Matrix CellState { get; set; }
    
    // Candidate state uses Gate class since it carries out the same mathematical operations 
    private Gate CandidateState { get; } 

    /// <summary>
    ///     Complete one pass through of the LSTM cell, given an input
    /// </summary>
    /// <param name="input">Column one-hot vector representing the input into the LSTM</param>
    /// <param name="previousOutput">LSTM's previous output</param> 
    public Matrix Forward(Matrix input, Matrix previousOutput)
    {
        // Validating arguments
        if (input.Rows != VocabSize && input.Columns > 1)
        {
            throw new LstmInputException("Input parameter must be a column vector with VocabSize rows");
        }
        
        // Calculate forget gate value 
        Forget.CalcUnactivated(input, previousOutput);
        Forget.Value.Sigmoid();

        // Calculate input gate value
        Input.CalcUnactivated(input, previousOutput);
        Input.Value.Sigmoid();

        // Calculate new info value 
        CandidateState.CalcUnactivated(input, previousOutput);
        CandidateState.Value.Tanh();

        // Calculate cell state
        // Forget_Gate x CellState + Input_Gate x New_Info_Gate (using element-wise multiplication) 
        CellState = Matrix.Hadamard(Forget.Value, CellState) + Matrix.Hadamard(Input.Value, CandidateState.Value);

        // Calculate output gate
        Output.CalcUnactivated(input, previousOutput);
        Output.Value.Sigmoid();

        // return output 
        return Matrix.Hadamard(Output.Value, Matrix.Tanh(CellState));
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

    /// <summary>
    /// Returns the values of the forget, input and output gates
    /// </summary>
    /// <returns>An array containing the values. Index 0 is forget, 1 is input and 2 is output</returns>
    public Matrix[] GetGateValues()
    {
        return new Matrix[] { Forget.Value, Input.Value, Output.Value };
    }
    
    /// <summary>
    /// Returns the values of the cell state, and candidate state
    /// </summary>
    /// <returns>An array containing the values. Index 0 is cell state, 1 is candidate state</returns>
    public Matrix[] GetStateValues()
    {
        return new Matrix[] { CellState, CandidateState.Value };
    }

    /// <summary>
    /// Clears the value of all states and gates in the LSTM
    /// </summary>
    public void Clear()
    {
        Forget.Clear();
        Input.Clear();
        Output.Clear();
        CandidateState.Clear();
        CellState *= 0;
    }
}