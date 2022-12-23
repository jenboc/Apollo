﻿using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Lstm
{
    public Lstm(int vocabSize, float learningRate)
    {
        VocabSize = vocabSize; // The amount of different characters present in the training data
        LearningRate = learningRate;

        var gateShape = new int[] { VocabSize, VocabSize };
        Forget = new Gate(gateShape, gateShape);
        Input = new Gate(gateShape, gateShape);
        NewInfo = new Gate(gateShape, gateShape);
        Output = new Gate(gateShape, gateShape);

        CellState = new Matrix(vocabSize, vocabSize);
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
    public Matrix Forward(Matrix input)
    {
        // Calculate forget gate value 
        Forget.CalcUnactivated(input);
        Forget.Value.Sigmoid();

        // Calculate input gate value
        Input.CalcUnactivated(input);
        Input.Value.Sigmoid();

        // Calculate new info value 
        NewInfo.CalcUnactivated(input);
        NewInfo.Value.Tanh();

        // Calculate cell state
        CellState = Forget.Value * CellState + Input.Value * NewInfo.Value;

        // Calculate output gate
        Output.CalcUnactivated(input);
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