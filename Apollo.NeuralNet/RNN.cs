using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    /// <param name="ioSize">Input/Output size</param>
    /// <param name="recurrenceAmount">How many recurrences to do on a single pass through</param>
    /// <param name="learningRate">Rate of change of the parameters of the network</param>
    public Rnn(int ioSize, int recurrenceAmount, float learningRate)
    {
        IoSize = ioSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        LstmCell = new Lstm(IoSize, LearningRate);
    }

    // General Parameters
    private int IoSize { get; }
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
        
        for (var i = 0; i < RecurrenceAmount; i++)
        {
            var lstmOutput = LstmCell.Forward(lstmInput);
            lstmInput = lstmOutput;
            
            // Interpret output 
            outputs[i] = lstmOutput;
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