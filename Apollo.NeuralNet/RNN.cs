using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Rnn
{
    // General Parameters
    private int InputSize { get; }
    private int OutputSize { get; }
    private int RecurrenceAmount { get; }
    private double LearningRate { get; }
    
    // Storing state of the LSTM gates at each recurrence for backprop
    private Matrix InputGates { get; set; }
    private Matrix OutputGates { get; set; }
    private Matrix CellStates { get; set; }
    private Matrix HiddenStates { get; set; }
    
    // Weight for the RNN itself
    private Weight NetWeight { get; set; }
    
    // LSTM Cell 
    private Lstm LstmCell { get; }

    public Rnn(int inputSize, int outputSize, int recurrenceAmount, double learningRate)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        NetWeight = new Weight(OutputSize, OutputSize);

        InputGates = new Matrix(RecurrenceAmount + 1, InputSize);
        OutputGates = new Matrix(RecurrenceAmount + 1, OutputSize);
        CellStates = new Matrix(RecurrenceAmount + 1, OutputSize);
        HiddenStates = new Matrix(RecurrenceAmount + 1, OutputSize);

        LstmCell = new Lstm(InputSize, OutputSize, RecurrenceAmount, LearningRate);
    }

    public Matrix Forward()
    {
        throw new NotImplementedException();
    }

    public double Backprop()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}