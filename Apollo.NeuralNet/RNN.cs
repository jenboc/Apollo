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
    }

    // General Parameters
    private int VocabSize { get; }
    private int RecurrenceAmount { get; }
    private float LearningRate { get; }

    // LSTM Cell 
    private Lstm LstmCell { get; }
    
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
            // Interpret outputs[i]
            lstmInput = outputs[i];
        }

        return outputs;
    }
    
    /// <summary>
    /// Perform the backpropagation algorithm on the neural network to optimise its parameters.
    /// </summary>
    /// <returns> A float representing the error of the network </returns>
    public float Backprop()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}