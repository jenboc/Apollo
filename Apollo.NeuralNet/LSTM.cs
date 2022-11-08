using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Lstm
{
    // General Parameters
    private int InputSize { get; }
    private int OutputSize { get; }
    private int RecurrenceAmount { get; }
    private double LearningRate { get; }

    // Gate weight matrices
    private Weight ForgetGate { get; set; }
    private Weight InputGate { get; set; }
    private Weight MemoryCellMatrix { get; set; }
    private Weight CellState { get; set; }
    private Weight OutputGate { get; set; }
    
    
    public Lstm(int inputSize, int outputSize, int recurrenceAmount, double learningRate)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        RecurrenceAmount = recurrenceAmount;
        LearningRate = learningRate;

        // Init weight matrices
        MemoryCellMatrix = new Weight(OutputSize, OutputSize);
        
        ForgetGate = new Weight(OutputSize, InputSize + OutputSize);
        InputGate = new Weight(OutputSize, InputSize + OutputSize);
        CellState = new Weight(OutputSize, InputSize + OutputSize);
        OutputGate = new Weight(OutputSize, InputSize + OutputSize);
    }

    public Matrix[] Forward()
    {
        throw new NotImplementedException();
    }

    public Matrix[] Backprop()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}