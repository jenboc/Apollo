using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    /// <param name="vocabSize">The amount of words in the vocabulary list</param>
    /// <param name="hiddenSize">Size which depicts shape of hidden layer weights</param>
    /// <param name="batchSize">The amount of words (from the vocab) passed in a single input</param>
    /// <param name="recurrenceAmount">How many recurrences to do on a single pass through</param>
    /// <param name="hyperparameters">Hyperparameters for the ADAM optimisation algorithm</param>
    /// <param name="r">Random Instance to instantiate weights</param>
    public Rnn(string statePath, int vocabSize, int hiddenSize, int batchSize, int recurrenceAmount,
        AdamParameters hyperparameters, Random r)
    {
        StatePath = statePath;
        VocabSize = vocabSize;
        HiddenSize = hiddenSize;
        BatchSize = batchSize;
        RecurrenceAmount = recurrenceAmount;
        Hyperparameters = hyperparameters;

        LstmCell = new Lstm(VocabSize, HiddenSize, BatchSize, r);

        SoftmaxWeight = new Weight(HiddenSize, VocabSize, r);
    }

    private string StatePath { get; }

    // General Parameters
    private int VocabSize { get; set; }
    private int BatchSize { get; set; }
    private int HiddenSize { get; set; }
    private int RecurrenceAmount { get; set; }
    private float LearningRate { get; set; }

    // LSTM Cell 
    private Lstm LstmCell { get; set; }

    // Softmax layer weight 
    private Weight SoftmaxWeight { get; set; }

    // Adam hyperparameters 
    private AdamParameters Hyperparameters { get; set; }

    /// <summary>
    ///     Save network into a binary file
    /// </summary>
    private void LoadState()
    {
        using (var stream = File.Open(StatePath, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream))
            {
                VocabSize = reader.ReadInt32();
                BatchSize = reader.ReadInt32();
                HiddenSize = reader.ReadInt32();
                RecurrenceAmount = reader.ReadInt32();
                LearningRate = (float)reader.ReadDecimal();

                LstmCell = new Lstm(VocabSize, HiddenSize, BatchSize, reader);

                Hyperparameters = AdamParameters.ReadFromFile(reader);
                SoftmaxWeight = Weight.ReadFromFile(reader, HiddenSize, VocabSize);
            }
        }
    }

    /// <summary>
    ///     Save the network to a binary file
    /// </summary>
    private void SaveState()
    {
        using (var stream = File.Open(StatePath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(VocabSize);
                writer.Write(BatchSize);
                writer.Write(HiddenSize);
                writer.Write(RecurrenceAmount);
                writer.Write(LearningRate);
                LstmCell.WriteToFile(writer);
                Hyperparameters.WriteToFile(writer);
                SoftmaxWeight.WriteToFile(writer);
            }
        }
    }

    /// <summary>
    ///     Complete a full pass of the neural network, with the correct number of recurrences.
    /// </summary>
    /// <returns> An array containing the output from each recurrence of the network </returns>
    public Matrix[] Forward(Matrix initialInput)
    {
        var outputs = new Matrix[RecurrenceAmount];

        var lstmInput = initialInput;
        var hiddenState = new Matrix(BatchSize, HiddenSize);

        for (var i = 0; i < RecurrenceAmount; i++)
        {
            hiddenState = LstmCell.Forward(lstmInput, hiddenState);
            outputs[i] = hiddenState.Clone();

            outputs[i] *= SoftmaxWeight;
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
        var r = new Random();
        var interpreted = Matrix.Like(softmax);

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

            LstmCell.Backprop(inputs[t], dF, forgetGates[t], dI, inputGates[t], dO, outputGates[t],
                dG, candidateStates[t], lstmOutputs[t - 1]);
            
            Update(t);
        }

    }

    /// <summary>
    ///     Update the parameters of the neural network
    /// </summary>
    private void Update(int t)
    {
        SoftmaxWeight.Adam(Hyperparameters, t);
        LstmCell.Update(Hyperparameters, t);
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
    /// <param name="trainingData">An array of one-hot vectors representing a single MIDI file</param>
    /// <param name="numEpochs">The amount of iterations you want to do over the training data</param>
    /// <param name="numTimesteps">The number of timesteps per epoch</param>
    public void Train(Matrix[] trainingData, int numEpochs, int numTimesteps, Random r)
    {
        var (inputData, expectedOutputs) = CreateBatches(trainingData);
        Console.WriteLine($"Input data length: {inputData.Count}");
        
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
        for (var epoch = 0; epoch < numEpochs; epoch++)
        {
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

            Console.WriteLine($"Epoch {epoch}");

            var hiddenState = new Matrix(BatchSize, HiddenSize);
            totalLoss = 0f;

            var start = r.Next(inputData.Count - numTimesteps);
            var end = start + numTimesteps;

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

            var averageLoss = totalLoss / numTimesteps;
            Console.WriteLine($"Loss: {averageLoss}");

            if (averageLoss < 0.5f)
                break;

            Backprop(forgetGateValues, candidateStateValues, cellStateValues, inputGateValues, outputGateValues,
                usedInputs, hiddenStateValues, actualOutputValues, usedOutputs);
        } 
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