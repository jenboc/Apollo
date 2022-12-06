using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    public Rnn(int inputSize, int outputSize, int recurrenceAmount, float learningRate)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        LstmCell = new Lstm(InputSize, OutputSize, LearningRate);
    }

    // General Parameters
    private int InputSize { get; }
    private int OutputSize { get; }
    private int RecurrenceAmount { get; }
    private float LearningRate { get; }

    // LSTM Cell 
    private Lstm LstmCell { get; }

    public Matrix Forward()
    {
        throw new NotImplementedException();
    }

    public float Backprop()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}