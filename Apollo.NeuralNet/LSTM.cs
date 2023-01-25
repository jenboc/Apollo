using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Lstm
{
    public Lstm(int vocabSize, int hiddenSize, int batchSize, Random r)
    {
        Forget = new Gate(vocabSize, hiddenSize, batchSize, r);
        Input = new Gate(vocabSize, hiddenSize, batchSize, r);
        CandidateState = new Gate(vocabSize, hiddenSize, batchSize, r);
        Output = new Gate(vocabSize, hiddenSize, batchSize, r);

        CellState = new Matrix(batchSize, hiddenSize);
    }

    public Lstm(int vocabSize, int hiddenSize, int batchSize, BinaryReader reader)
    {
        Forget = new Gate(vocabSize, hiddenSize, batchSize, reader);
        Input = new Gate(vocabSize, hiddenSize, batchSize, reader);
        CandidateState = new Gate(vocabSize, hiddenSize, batchSize, reader);
        Output = new Gate(vocabSize, hiddenSize, batchSize, reader);

        CellState = new Matrix(batchSize, hiddenSize);
    }

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
        CellState = Matrix.HadamardProd(Forget.Value, CellState) +
                    Matrix.HadamardProd(Input.Value, CandidateState.Value);

        // Calculate output gate
        Output.CalcUnactivated(input, previousOutput);
        Output.Value.Sigmoid();

        // return output 
        return Matrix.HadamardProd(Output.Value, Matrix.Tanh(CellState));
    }

    /// <summary>
    ///     Perform backprop for a single timestep
    /// </summary>
    public void Backprop(Matrix input, Matrix dF, Matrix forgetValue, Matrix dI, Matrix inputGateValue, Matrix dO,
        Matrix outputGateValue,
        Matrix dG, Matrix candidateStateValue, Matrix lstmOutput)
    {
        Forget.InputWeight.Gradient += Matrix.Transpose(input) * Matrix.HadamardProd(dF, Matrix.DSigmoid(forgetValue));
        Forget.PrevOutputWeight.Gradient +=
            Matrix.Transpose(dF) * Matrix.HadamardProd(Matrix.DSigmoid(forgetValue), lstmOutput);

        Input.InputWeight.Gradient +=
            Matrix.Transpose(input) * Matrix.HadamardProd(dI, Matrix.DSigmoid(inputGateValue));
        Input.PrevOutputWeight.Gradient +=
            Matrix.Transpose(dI) * Matrix.HadamardProd(Matrix.DSigmoid(inputGateValue), lstmOutput);

        Output.InputWeight.Gradient +=
            Matrix.Transpose(input) * Matrix.HadamardProd(dO, Matrix.DSigmoid(outputGateValue));
        Output.PrevOutputWeight.Gradient +=
            Matrix.Transpose(dO) * Matrix.HadamardProd(Matrix.DSigmoid(outputGateValue), lstmOutput);

        CandidateState.InputWeight.Gradient +=
            Matrix.Transpose(input) * Matrix.HadamardProd(dG, Matrix.DTanh(candidateStateValue));
        CandidateState.PrevOutputWeight.Gradient +=
            Matrix.Transpose(dG) * Matrix.HadamardProd(Matrix.DTanh(candidateStateValue), lstmOutput);
    }

    public void Update(AdamParameters hyperparameters, int t)
    {
        Forget.Update(hyperparameters, t);
        Input.Update(hyperparameters, t);
        Output.Update(hyperparameters, t);
        CandidateState.Update(hyperparameters, t);
    }

    /// <summary>
    ///     Returns the values of the forget, input and output gates
    /// </summary>
    /// <returns>An array containing the values. Index 0 is forget, 1 is input and 2 is output</returns>
    public Matrix[] GetGateValues()
    {
        return new[] { Forget.Value.Clone(), Input.Value.Clone(), Output.Value.Clone() };
    }

    /// <summary>
    ///     Returns the values of the cell state, and candidate state
    /// </summary>
    /// <returns>An array containing the values. Index 0 is cell state, 1 is candidate state</returns>
    public Matrix[] GetStateValues()
    {
        return new[] { CellState.Clone(), CandidateState.Value.Clone() };
    }

    /// <summary>
    ///     Clears the value of all states and gates in the LSTM
    /// </summary>
    public void Clear()
    {
        Forget.Clear();
        Input.Clear();
        Output.Clear();
        CandidateState.Clear();
        CellState *= 0;
    }

    /// <summary>
    ///     Write the parameters of the LSTM cell to a binary file
    /// </summary>
    /// <param name="writer">Instance of BinaryWriter to use for writing</param>
    public void WriteToFile(BinaryWriter writer)
    {
        Forget.WriteToFile(writer);
        Input.WriteToFile(writer);
        CandidateState.WriteToFile(writer);
        Output.WriteToFile(writer);
    }
}