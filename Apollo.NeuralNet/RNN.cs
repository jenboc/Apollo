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
    private int RecurrenceAmount { get; }
    private float LearningRate { get; }

    // LSTM Cell 
    private Lstm LstmCell { get; }

    // Softmax layer weight 
    private Matrix Weight { get; set; }

    /// <summary>
    ///     Complete a full pass of the neural network, with the correct number of recurrences.
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
    ///     Interprets the softmaxed LSTM output as a one-hot vector
    /// </summary>
    /// <param name="softmax">The result after using the softmax function on the LSTM output</param>
    /// <returns>A one-hot vector representing the LSTM's predictions</returns>
    private Matrix InterpretOutput(Matrix softmax)
    {
        var highest = float.MinValue;
        var highestIndex = -1;

        for (var row = 0; row < softmax.Rows; row++)
            if (softmax[row, 0] > highest)
            {
                highest = softmax[row, 0];
                highestIndex = row;
            }

        softmax *= 0; // Set everything to 0 
        softmax.Contents[highestIndex, 0] = 1; // Set index of highest value to 1 

        return softmax;
    }

    /// <summary>
    ///     Perform the backpropagation algorithm on the neural network to optimise its parameters.
    /// </summary>
    /// <param name="forgetGates">The values of the forget gate at each timestep during training</param>
    /// <param name="candidateStates">The values of the candidate state at each timestep during training</param>
    /// <param name="cellStates">The values of the cell state at each timestep during training</param>
    /// <param name="inputGates">The values of the input gate at each timestep during training</param>
    /// <param name="outputGates">The values of the output gate at each timestep during training</param>
    /// <param name="inputs">The inputs to the LSTM cell at each timestep during training</param>
    /// <param name="lstmOutputs">The outputs straight from the LSTM at each timestep during training</param>
    /// <param name="predictedOutputs">
    ///     The output from the overall network (after softmax layer) at each timestep
    ///     during training
    /// </param>
    /// <param name="expectedOutputs">The training data</param>
    private void Backprop(Matrix[] forgetGates, Matrix[] candidateStates, Matrix[] cellStates,
        Matrix[] inputGates, Matrix[] outputGates, Matrix[] inputs, Matrix[] lstmOutputs, Matrix[] predictedOutputs,
        Matrix[] expectedOutputs)
    {
        // Derivatives for tunable parameters are cumulative => define outside loop 
        var dV = Matrix.Like(Weight); // Gradient for softmax layer weight 

        // Gates require 2 derivatives (one for the weight applied to the input, and one for the weight applied to 
        // the previous output) 
        // [0] => derivative for input weight 
        // [1] => derivative for previous output weight 
        var dWF = new[] { new(VocabSize, VocabSize), new Matrix(VocabSize, VocabSize) };
        var dWI = new[] { new(VocabSize, VocabSize), new Matrix(VocabSize, VocabSize) };
        var dWO = new[] { new(VocabSize, VocabSize), new Matrix(VocabSize, VocabSize) };
        var dWG = new[] { new(VocabSize, VocabSize), new Matrix(VocabSize, VocabSize) };

        // t represents timestep
        for (var t = 1; t < inputs.Length; t++)
        {
            // dL/dh(t) = (y_hat - y)V^T
            var dH = (predictedOutputs[t] - expectedOutputs[t]) * Matrix.Transpose(Weight);

            // dL/dc(t) = dL/dh(t) x o(t)tanh'(c_t) 
            var dC = Matrix.Hadamard(dH, Matrix.Hadamard(outputGates[t], Matrix.DTanh(cellStates[t])));

            // dL/dg(t) = dL/dc(t) x i(t) 
            var dG = Matrix.Hadamard(dC, inputGates[t]);

            // dL/do(t) = dL/dh x tanh(c(t)) 
            var dO = Matrix.Hadamard(dH, Matrix.Tanh(cellStates[t]));

            // dL/di(t) = dL/dc(t) x g(t)
            var dI = Matrix.Hadamard(dC, candidateStates[t]);

            // dL/df(t) = dL/dc(t) x c(t-1) 
            var dF = Matrix.Hadamard(dC, cellStates[t - 1]);

            // Increment gradient for weights 
            dV += Matrix.Transpose(lstmOutputs[t]) * (predictedOutputs[t] - expectedOutputs[t]);

            dWF[0] += Matrix.Hadamard(Matrix.Transpose(inputs[t]) * dF, Matrix.DSigmoid(forgetGates[t]));
            dWF[1] += Matrix.Transpose(dF) * Matrix.Hadamard(Matrix.DSigmoid(forgetGates[t]), lstmOutputs[t - 1]);

            dWI[0] += Matrix.Hadamard(Matrix.Transpose(inputs[t]) * dI, Matrix.DSigmoid(inputGates[t]));
            dWI[1] += Matrix.Transpose(dI) * Matrix.Hadamard(Matrix.DSigmoid(inputGates[t]), lstmOutputs[t - 1]);

            dWO[0] += Matrix.Hadamard(Matrix.Transpose(inputs[t]) * dO, Matrix.DSigmoid(outputGates[t]));
            dWO[1] += Matrix.Transpose(dO) * Matrix.Hadamard(Matrix.DSigmoid(outputGates[t]), lstmOutputs[t - 1]);

            dWG[0] += Matrix.Hadamard(Matrix.Transpose(inputs[t]) * dG, Matrix.DTanh(candidateStates[t]));
            dWG[1] += Matrix.Transpose(dG) * Matrix.Hadamard(Matrix.DTanh(candidateStates[t]), lstmOutputs[t - 1]);
        }
    }

    /// <summary>
    ///     Update the parameters of the neural network
    /// </summary>
    private void Update(Matrix dV, Matrix[] dWF, Matrix[] dWI, Matrix[] dWO, Matrix[] dWG)
    {
        Weight = Weight - dV * LearningRate;

        LstmCell.Update(dWF, dWI, dWO, dWG);
    }

    /// <summary>
    ///     Calculate the loss using categorical cross-entropy
    /// </summary>
    /// <param name="expected">The expected/desired output from the LSTM</param>
    /// <param name="actual">The actual output of the LSTM</param>
    /// <returns>A matrix representing the loss of the neural network</returns>
    private Matrix CalculateLoss(Matrix expected, Matrix actual)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Train the neural network on a single file
    /// </summary>
    /// <param name="trainingData">An array of one-hot vectors representing a single MIDI file</param>
    public void Train(Matrix[] trainingData)
    {
        LstmCell.Clear();

        var gateValues = LstmCell.GetGateValues();
        var stateValues = LstmCell.GetStateValues();

        // Data required to perform backpropagation 
        var previousForgetGates = new Matrix[trainingData.Length];
        previousForgetGates[0] = gateValues[0].Clone();

        var previousCandidateStates = new Matrix[trainingData.Length];
        previousCandidateStates[0] = stateValues[1].Clone();

        var previousCellStates = new Matrix[trainingData.Length];
        previousCellStates[0] = stateValues[0].Clone();

        var previousInputGates = new Matrix[trainingData.Length];
        previousInputGates[0] = gateValues[1].Clone();

        var previousOutputGates = new Matrix[trainingData.Length];
        previousOutputGates[0] = gateValues[2].Clone();

        var previousInputs = new Matrix[trainingData.Length];

        var previousLstmOutputs = new Matrix[trainingData.Length];
        previousLstmOutputs[0] = new Matrix(VocabSize, 1);

        var previousFinalOutputs = new Matrix[trainingData.Length];

        var totalLoss = new Matrix(VocabSize, VocabSize);

        // Start at 1 so that previousInputs[i] is supposed to produce trainingData[i]
        for (var i = 1; i < trainingData.Length; i++)
        {
            var input = trainingData[i - 1];
            var previousOutput = i == 0 ? Matrix.Like(input) : previousLstmOutputs[i - 1];

            var expectedOutput = trainingData[i];

            var actualOutput = LstmCell.Forward(input, previousOutput);
            previousLstmOutputs[i] = actualOutput.Clone();

            actualOutput = Matrix.Multiply(Weight, actualOutput);
            actualOutput.Softmax();
            previousFinalOutputs[i] = actualOutput.Clone();

            totalLoss += CalculateLoss(expectedOutput, actualOutput);

            previousInputs[i] = input;

            gateValues = LstmCell.GetGateValues();
            previousForgetGates[i] = gateValues[0].Clone();
            previousInputGates[i] = gateValues[1].Clone();
            previousOutputGates[i] = gateValues[2].Clone();

            stateValues = LstmCell.GetStateValues();
            previousCellStates[i] = stateValues[0].Clone();
            previousCandidateStates[i] = stateValues[1].Clone();
        }

        Backprop(previousForgetGates, previousCandidateStates, previousCellStates, previousInputGates,
            previousOutputGates, previousInputs, previousLstmOutputs, previousFinalOutputs,
            trainingData);
    }
}