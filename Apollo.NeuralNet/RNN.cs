using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    /// <param name="vocabSize">The amount of words in the vocabulary list</param>
    /// <param name="recurrenceAmount">How many recurrences to do on a single pass through</param>
    /// <param name="learningRate">Rate of change of the parameters of the network</param>
    public Rnn(int vocabSize, int recurrenceAmount, float learningRate)
    {
        VocabSize = vocabSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        LstmCell = new Lstm(VocabSize, LearningRate);

        Weight = Matrix.Random(VocabSize, VocabSize); 
    }

    // General Parameters
    private int VocabSize { get; }
    private int RecurrenceAmount { get; set;  }
    private float LearningRate { get; }

    // LSTM Cell 
    private Lstm LstmCell { get; }
    
    // Softmax layer weight 
    private Matrix Weight { get; }

    /// <summary>
    /// Complete a full pass of the neural network, with the correct number of recurrences.
    /// </summary>
    /// <returns> An array containing the output from each recurrence of the network </returns>
    public Matrix[] Forward(Matrix initialInput)
    {
        var outputs = new Matrix[RecurrenceAmount];

        var lstmInput = initialInput;
        var lstmOutput = Matrix.Like(initialInput);

        for (var i = 0; i < RecurrenceAmount; i++)
        {
            lstmOutput = LstmCell.Forward(lstmInput, lstmOutput);
            outputs[i] = lstmOutput;

            outputs[i] = Weight * outputs[i];
            outputs[i].Softmax();
            outputs[i] = InterpretOutput(outputs[i]);

            lstmInput = outputs[i];
        }

        return outputs;
    }
    
    /// <summary>
    /// Interprets the softmaxed LSTM output as a one-hot vector
    /// </summary>
    /// <param name="softmax">The result after using the softmax function on the LSTM output</param>
    /// <returns>A one-hot vector representing the LSTM's predictions</returns>
    private Matrix InterpretOutput(Matrix softmax)
    {
        var highest = float.MinValue;
        var highestIndex = -1;

        for (var row = 0; row < softmax.Rows; row++)
        {
            if (softmax[row, 0] > highest)
            {
                highest = softmax[row, 0];
                highestIndex = row;
            }
        }

        softmax *= 0; // Set everything to 0 
        softmax.Contents[highestIndex, 0] = 1; // Set index of highest value to 1 

        return softmax;
    }
    
    /// <summary>
    /// Perform the backpropagation algorithm on the neural network to optimise its parameters. 
    /// </summary>
    /// <param name="forgetGates">The values of the forget gate at each timestep during training</param>
    /// <param name="candidateStates">The values of the candidate state at each timestep during training</param>
    /// <param name="cellStates">The values of the cell state at each timestep during training</param>
    /// <param name="inputGates">The values of the input gate at each timestep during training</param>
    /// <param name="outputGates">The values of the output gate at each timestep during training</param>
    /// <param name="inputs">The inputs to the LSTM cell at each timestep during training</param>
    /// <param name="outputs">The outputs from the LSTM cell at each timestep during training</param>
    private void Backprop(List<Matrix> forgetGates, List<Matrix> candidateStates, List<Matrix> cellStates, 
        List<Matrix> inputGates, List<Matrix> outputGates, List<Matrix> inputs, List<Matrix> outputs)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Update the parameters of the neural network
    /// </summary>
    private void Update()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate the loss using categorical cross-entropy 
    /// </summary>
    /// <param name="expected">The expected/desired output from the LSTM</param>
    /// <param name="actual">The actual output of the LSTM</param>
    /// <returns>A matrix representing the loss of the neural network</returns>
    private Matrix CalculateLoss(Matrix expected, Matrix actual)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Train the neural network on a single file
    /// </summary>
    /// <param name="trainingData">An array of one-hot vectors representing a single MIDI file</param>
    public void Train(Matrix[] trainingData)
    {
        // Data required to perform backpropagation 
        var previousForgetGates = new List<Matrix>();
        var previousCandidateStates = new List<Matrix>();
        var previousCellStates = new List<Matrix>();
        var previousInputGates = new List<Matrix>();
        var previousOutputGates = new List<Matrix>();
        var previousInputs = new List<Matrix>();
        var previousLstmOutputs = new List<Matrix>();

        var totalLoss = new Matrix(VocabSize, VocabSize);

        for (var i = 0; i < trainingData.Length - 1; i++)
        {
            var input = trainingData[i];
            var previousOutput = i == 0 ? Matrix.Like(input) : previousLstmOutputs[i - 1];
            
            var expectedOutput = trainingData[i + 1];
            
            var actualOutput = LstmCell.Forward(input, previousOutput); 
            
            actualOutput = Matrix.Multiply(Weight, actualOutput);
            actualOutput.Softmax();

            totalLoss += CalculateLoss(expectedOutput, actualOutput); 

            previousInputs.Add(input);

            var gateValues = LstmCell.GetGateValues();
            previousForgetGates.Add(gateValues[0]);
            previousInputGates.Add(gateValues[1]);
            previousOutputGates.Add(gateValues[2]);

            var stateValues = LstmCell.GetStateValues();
            previousCellStates.Add(stateValues[0]); 
            previousCandidateStates.Add(stateValues[1]);

        }
        
        Backprop(previousForgetGates, previousCandidateStates, previousCellStates, previousInputGates, 
            previousOutputGates, previousInputs, previousLstmOutputs);
    }
}