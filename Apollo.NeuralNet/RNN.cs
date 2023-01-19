﻿using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    /// <param name="vocabSize">The amount of words in the vocabulary list</param>
    /// <param name="hiddenSize">Size which depicts shape of hidden layer weights</param>
    /// <param name="batchSize">The amount of words (from the vocab) passed in a single input</param>
    /// <param name="recurrenceAmount">How many recurrences to do on a single pass through</param>
    /// <param name="learningRate">Rate of change of the parameters of the network</param>
    /// <param name="r">Random Instance to instantiate weights</param> 
    public Rnn(int vocabSize, int hiddenSize, int batchSize, int recurrenceAmount, float learningRate, Random r)
    {
        VocabSize = vocabSize;
        HiddenSize = hiddenSize;
        BatchSize = batchSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        LstmCell = new Lstm(VocabSize, HiddenSize, BatchSize, LearningRate, r);

        Weight = new Matrix(HiddenSize, VocabSize, r);
    }

    // General Parameters
    private int VocabSize { get; }
    private int BatchSize { get; }
    private int HiddenSize { get; }
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
        var hiddenState = new Matrix(BatchSize, HiddenSize);

        for (var i = 0; i < RecurrenceAmount; i++)
        {
            hiddenState = LstmCell.Forward(lstmInput, hiddenState);
            outputs[i] = hiddenState.Clone();

            outputs[i] *= Weight;
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
        // var highest = float.MinValue;
        // var highestIndex = -1;
        //
        // for (var row = 0; row < softmax.Rows; row++)
        //     if (softmax[row, 0] > highest)
        //     {
        //         highest = softmax[row, 0];
        //         highestIndex = row;
        //     }
        //
        // softmax *= 0; // Set everything to 0 
        // softmax.Contents[highestIndex, 0] = 1; // Set index of highest value to 1 

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
        // Derivatives for tunable parameters are cumulative => define outside loop 
        var dV = Matrix.Like(Weight); // Gradient for softmax layer weight 

        // Gates require 2 derivatives (one for the weight applied to the input, and one for the weight applied to 
        // the previous output) 
        // [0] => derivative for input weight 
        // [1] => derivative for previous output weight 
        var dWF = new[] { new(VocabSize, HiddenSize), new Matrix(HiddenSize, HiddenSize) };
        var dWI = new[] { new(VocabSize, HiddenSize), new Matrix(HiddenSize, HiddenSize) };
        var dWO = new[] { new(VocabSize, HiddenSize), new Matrix(HiddenSize, HiddenSize) };
        var dWG = new[] { new(VocabSize, HiddenSize), new Matrix(HiddenSize, HiddenSize) };

        // t represents timestep
        // Go until t > 1 since there is no timestep -1 (t-1 when t = 0) 
        for (var t = inputs.Count - 1; t > 1; t--)
        {
            // dL/dh(t) = (y_hat - y)V^T
            var dH = (predictedOutputs[t] - expectedOutputs[t]) * Matrix.Transpose(Weight);

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
            dV += Matrix.Transpose(lstmOutputs[t]) * (predictedOutputs[t] - expectedOutputs[t]);

            //dWF[0] += Matrix.HadamardProd(Matrix.Transpose(inputs[t]) * dF, Matrix.DSigmoid(forgetGates[t]));
            dWF[0] += Matrix.Transpose(inputs[t]) * Matrix.HadamardProd(dF, Matrix.DSigmoid(forgetGates[t]));
            dWF[1] += Matrix.Transpose(dF) * Matrix.HadamardProd(Matrix.DSigmoid(forgetGates[t]), lstmOutputs[t - 1]);

            //dWI[0] += Matrix.HadamardProd(Matrix.Transpose(inputs[t]) * dI, Matrix.DSigmoid(inputGates[t]));
            dWI[0] += Matrix.Transpose(inputs[t]) * Matrix.HadamardProd(dI, Matrix.DSigmoid(inputGates[t]));
            dWI[1] += Matrix.Transpose(dI) * Matrix.HadamardProd(Matrix.DSigmoid(inputGates[t]), lstmOutputs[t - 1]);

            dWO[0] += Matrix.Transpose(inputs[t]) * Matrix.HadamardProd(dO, Matrix.DSigmoid(outputGates[t]));
            dWO[1] += Matrix.Transpose(dO) * Matrix.HadamardProd(Matrix.DSigmoid(outputGates[t]), lstmOutputs[t - 1]);

            dWG[0] += Matrix.Transpose(inputs[t]) * Matrix.HadamardProd(dG, Matrix.DTanh(candidateStates[t]));
            dWG[1] += Matrix.Transpose(dG) * Matrix.HadamardProd(Matrix.DTanh(candidateStates[t]), lstmOutputs[t - 1]);
        }
        
        Update(dV, dWF, dWI, dWO, dWG);
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
    public void Train(Matrix[] trainingData, int maxError)
    {
        var (inputData, expectedOutputs) = CreateBatches(trainingData);

        // Previous gate/state values to be used in backpropagation
        var forgetGateValues = new List<Matrix>();
        var candidateStateValues = new List<Matrix>();
        var cellStateValues = new List<Matrix>();
        var inputGateValues = new List<Matrix>();
        var outputGateValues = new List<Matrix>();
        var hiddenStateValues = new List<Matrix>();
        var actualOutputValues = new List<Matrix>();

        var epoch = 1;

        float totalLoss;       
        do
        {
            LstmCell.Clear();

            Console.WriteLine($"Epoch {epoch}");

            var hiddenState = new Matrix(BatchSize, HiddenSize);
            totalLoss = 0f;

            for (var i = 0; i < inputData.Count; i++)
            {
                var input = inputData[i];
                var expected = expectedOutputs[i];

                hiddenState = LstmCell.Forward(input, hiddenState);

                var actualOutput = hiddenState.Clone();
                actualOutput *= Weight;
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

            Console.WriteLine($"Loss: {totalLoss / inputData.Count}");

            Backprop(forgetGateValues, candidateStateValues, cellStateValues, inputGateValues, outputGateValues,
                inputData, hiddenStateValues, actualOutputValues, expectedOutputs);

            epoch++;
        } while (totalLoss > maxError);
    }
    
    /// <summary>
    /// Create valid batches from the training data
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
            
            inputData.Add(Matrix.StackArray(input, true));
            expectedOutputs.Add(Matrix.StackArray(expected, true));
        }
        
        return new Tuple<List<Matrix>, List<Matrix>> (inputData, expectedOutputs);
    }
}