using Apollo.IO;
using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    /// <param name="profile">Profile object containing state data for the network</param>
    /// <param name="vocabSize">The amount of words in the vocabulary list</param>
    /// <param name="hiddenSize">Size which depicts shape of hidden layer weights</param>
    /// <param name="batchSize">The amount of words (from the vocab) passed in a single input</param>
    /// <param name="r">Random Instance to instantiate weights</param>
    public Rnn(int vocabSize, int hiddenSize,
        int batchSize, Random r)
    {
        VocabSize = vocabSize;
        HiddenSize = hiddenSize;
        BatchSize = batchSize;

        LstmCell = new Lstm(VocabSize, HiddenSize, BatchSize, r);

        SoftmaxWeight = new Weight(HiddenSize, VocabSize, r);
    }

    /// <summary>
    ///     Create an RNN object from a state file
    /// </summary>
    /// <param name="stateFileToLoad">Path to the state file to load from</param>
    public Rnn(string stateFileToLoad)
    {
        LoadState(stateFileToLoad);
    }

    // General Parameters
    private int VocabSize { get; set; }
    public int BatchSize { get; private set; } // Accessed when generating seeds for forward prop
    private int HiddenSize { get; set; }
    private float LearningRate { get; set; }

    // LSTM Cell 
    private Lstm LstmCell { get; set; }

    // Softmax layer weight 
    private Weight SoftmaxWeight { get; set; }

    /// <summary>
    ///     Save network into a binary file
    /// </summary>
    public void LoadState(string name)
    {
        using (var stream = File.Open(name, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream))
            {
                VocabSize = reader.ReadInt32();
                BatchSize = reader.ReadInt32();
                HiddenSize = reader.ReadInt32();
                LearningRate = reader.ReadSingle();

                LstmCell = new Lstm(VocabSize, HiddenSize, BatchSize, reader);

                SoftmaxWeight = Weight.ReadFromFile(reader, HiddenSize, VocabSize);
            }
        }
    }

    /// <summary>
    ///     Save the network to a binary file
    /// </summary>
    private void SaveState(string name)
    {
        using (var stream = File.Open(name, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(VocabSize);
                writer.Write(BatchSize);
                writer.Write(HiddenSize);
                writer.Write(LearningRate);
                LstmCell.WriteToFile(writer);
                SoftmaxWeight.WriteToFile(writer);
            }
        }
    }

    /// <summary>
    ///     Complete a full pass of the neural network, with the correct number of recurrences.
    /// </summary>
    /// <returns> An array containing the output from each recurrence of the network </returns>
    public Matrix[] Forward(Matrix initialInput, int recurrenceAmount)
    {
        // Array to store outputs at each timestep
        var outputs = new Matrix[recurrenceAmount];

        // HiddenState (lstm output) = matrix of 0s when t=0 
        var lstmInput = initialInput;
        var hiddenState = new Matrix(BatchSize, HiddenSize);

        for (var i = 0; i < recurrenceAmount; i++)
        {
            hiddenState = LstmCell.Forward(lstmInput, hiddenState);
            outputs[i] = hiddenState.Clone();

            // Softmax layer
            // Apply softmax to LSTM output and interpret
            outputs[i] *= SoftmaxWeight;
            outputs[i].Softmax();
            outputs[i] = InterpretOutput(outputs[i]);

            // Output = next input
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
        var r = new Random();
        // Create matrix of 0s the same shape as softmax matrix
        var interpreted = Matrix.Like(softmax);

        // Find the index of the highest number on each row, and set the corresponding index in the interpreted
        // matrix to 1
        for (var i = 0; i < softmax.Rows; i++)
        {
            var highestProb = float.MinValue;
            var highestProbIndex = -1;

            var highestSucceed = float.MinValue;
            var highestSucceedIndex = -1;

            for (var j = 0; j < softmax.Columns; j++)
            {
                if (softmax[i, j] > highestProb)
                    highestProbIndex = j;
                var rollQuota = (int)(softmax[i, j] * 100);
                var diceRoll = r.Next(100);

                if (diceRoll <= rollQuota && softmax[i, j] > highestSucceed)
                    highestSucceedIndex = j;
            }

            // The 1 goes into the slot with the highest probability which succeeded the dice roll 
            // Or alternatively (if every probability failed), the highest probability in general
            if (highestSucceedIndex == -1)
                interpreted[i, highestProbIndex] = 1;
            else
                interpreted[i, highestSucceedIndex] = 1;
        }

        return interpreted;
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
    private void Backprop(List<Matrix> forgetGates, List<Matrix> candidateStates, List<Matrix> cellStates,
        List<Matrix> inputGates, List<Matrix> outputGates, List<Matrix> inputs, List<Matrix> lstmOutputs,
        List<Matrix> predictedOutputs, List<Matrix> expectedOutputs)
    {
        // t represents timestep
        // Go until t > 1 since there is no timestep -1 (t-1 when t = 0) 
        for (var t = inputs.Count - 1; t > 1; t--)
        {
            // dL/dh(t) = (y_hat - y)V^T
            var dH = (predictedOutputs[t] - expectedOutputs[t]) * Matrix.Transpose(SoftmaxWeight);

            // dL/dc(t) = dL/dh(t) x o(t)tanh'(c_t) 
            var dC = Matrix.HadamardProd(dH, Matrix.HadamardProd(outputGates[t], Matrix.DTanh(cellStates[t])));

            // dL/dg(t) = dL/dc(t) x i(t) 
            var dG = Matrix.HadamardProd(dC, inputGates[t]);

            // dL/do(t) = dL/dh x tanh(c(t)) 
            var dO = Matrix.HadamardProd(dH, Matrix.Tanh(cellStates[t]));

            // dL/di(t) = dL/dc(t) x g(t)
            var dI = Matrix.HadamardProd(dC, candidateStates[t]);

            // dL/df(t) = dL/dc(t) x c(t-1) 
            var dF = Matrix.HadamardProd(dC, cellStates[t - 1]);

            // Increment gradient for weights 
            SoftmaxWeight.Gradient += Matrix.Transpose(lstmOutputs[t]) * (predictedOutputs[t] - expectedOutputs[t]);

            // Backprop through LSTM cell
            LstmCell.Backprop(inputs[t], dF, forgetGates[t], dI, inputGates[t], dO, outputGates[t],
                dG, candidateStates[t], lstmOutputs[t - 1]);

            // Update network parameters
            Update(t);
        }
    }

    /// <summary>
    ///     Update the parameters of the neural network
    /// </summary>
    private void Update(int t)
    {
        SoftmaxWeight.Adam(t);
        LstmCell.Update(t);
    }

    /// <summary>
    ///     Calculate the loss using categorical cross-entropy
    /// </summary>
    /// <param name="expected">The expected/desired output from the LSTM</param>
    /// <param name="actual">The actual output of the LSTM</param>
    /// <returns>A matrix representing the loss of the neural network</returns>
    private float CalculateLoss(Matrix expected, Matrix actual)
    {
        var lossMatrix = -1 * Matrix.HadamardProd(expected, Matrix.Log(actual, MathF.E));
        return lossMatrix.Sum();
    }

    /// <summary>
    ///     Train the neural network on a single file
    /// </summary>
    /// <param name="trainingData">The one-hot vector representation of the file</param>
    /// <param name="minimumEpochs">The minimum number of epochs to perform</param>
    /// <param name="maximumEpochs">The maximum number of epochs to perform before stopping</param>
    /// <param name="maximumError">The maximum error, training will stop early if the average error is below this</param>
    /// <param name="batchesPerEpoch">The amount of batches per epoch, do not put to high or risk NaNs</param>
    /// <param name="beforeStatePath">The path at which to write the before-training state file</param>
    /// <param name="afterStatePath">The path at which to write the after-training state file</param>
    /// <param name="r">An instance of random, to randomly select batches</param>
    public void Train(Matrix[] trainingData, int minimumEpochs, int maximumEpochs, float maximumError,
        int batchesPerEpoch, string beforeStatePath, string afterStatePath, Random r)
    {
        // Save before state
        SaveState(beforeStatePath);

        var (inputData, expectedOutputs) = CreateBatches(trainingData);
        LogManager.WriteLine($"Training input data length: {inputData.Count}");

        // Previous gate/state values to be used in backpropagation
        var usedInputs = new List<Matrix>();
        var usedOutputs = new List<Matrix>();
        var forgetGateValues = new List<Matrix>();
        var candidateStateValues = new List<Matrix>();
        var cellStateValues = new List<Matrix>();
        var inputGateValues = new List<Matrix>();
        var outputGateValues = new List<Matrix>();
        var hiddenStateValues = new List<Matrix>();
        var actualOutputValues = new List<Matrix>();

        float totalLoss;
        for (var epoch = 0; epoch < maximumEpochs; epoch++)
        {
            SaveState("temp_state.state");
            if (batchesPerEpoch <= 0) // Stop if training with 0 batches per epoch
                break;

            // Clear stored values on each epoch 
            usedInputs.Clear();
            usedOutputs.Clear();
            forgetGateValues.Clear();
            candidateStateValues.Clear();
            cellStateValues.Clear();
            inputGateValues.Clear();
            outputGateValues.Clear();
            hiddenStateValues.Clear();
            actualOutputValues.Clear();

            LstmCell.Clear();

            // Create new matrix to store the hidden state (LSTM output) 
            // Instantiated as 0s since there is no outputted hidden state at t=0
            var hiddenState = new Matrix(BatchSize, HiddenSize);
            totalLoss = 0f;

            // Simulate a forward pass of the network using a randomly selected training batch
            var start = r.Next(inputData.Count - batchesPerEpoch);
            var end = start + batchesPerEpoch;
            for (var i = start; i < end; i++)
            {
                var input = inputData[i];
                var expected = expectedOutputs[i];

                usedInputs.Add(input);
                usedOutputs.Add(expected);

                hiddenState = LstmCell.Forward(input, hiddenState);

                var actualOutput = hiddenState.Clone();
                actualOutput *= SoftmaxWeight;
                actualOutput.Softmax();
                actualOutputValues.Add(actualOutput);

                hiddenStateValues.Add(hiddenState);

                var gateValues = LstmCell.GetGateValues();
                forgetGateValues.Add(gateValues[0]);
                inputGateValues.Add(gateValues[1]);
                outputGateValues.Add(gateValues[2]);

                var stateValues = LstmCell.GetStateValues();
                cellStateValues.Add(stateValues[0]);
                candidateStateValues.Add(stateValues[1]);

                totalLoss += CalculateLoss(expected, actualOutput);
            }

            // Work out and log the average loss at this point 
            var averageLoss = totalLoss / batchesPerEpoch;
            LogManager.WriteLine($"Epoch: {epoch}\nLoss: {averageLoss}");

            if (float.IsNaN(averageLoss))
            {
                batchesPerEpoch -= 5;
                LoadState(beforeStatePath);
                continue;
            }

            // Break if training can stop
            // i.e. the loss is lower than the maximum error, and we've done the minimum amount of epochs
            if (averageLoss < maximumError && epoch >= minimumEpochs)
                break;

            // Backprop to reduce error
            Backprop(forgetGateValues, candidateStateValues, cellStateValues, inputGateValues, outputGateValues,
                usedInputs, hiddenStateValues, actualOutputValues, usedOutputs);

            File.Delete("temp_state.state"); // Delete temp state file
        }

        // Save after state
        SaveState(afterStatePath);
    }

    /// <summary>
    ///     Create valid batches from the training data
    /// </summary>
    /// <param name="trainingData">The training data to make the batches from</param>
    /// <returns>A tuple in the form of (inputBatches, expectedBatches)</returns>
    private Tuple<List<Matrix>, List<Matrix>> CreateBatches(Matrix[] trainingData)
    {
        var inputData = new List<Matrix>();
        var expectedOutputs = new List<Matrix>();

        for (var i = 0; i < trainingData.Length - BatchSize - 1; i++)
        {
            var input = trainingData.Skip(i).Take(BatchSize).ToArray();
            var expected = trainingData.Skip(i + 1).Take(BatchSize).ToArray();

            inputData.Add(Matrix.StackArray(input));
            expectedOutputs.Add(Matrix.StackArray(expected));
        }

        return new Tuple<List<Matrix>, List<Matrix>>(inputData, expectedOutputs);
    }
}