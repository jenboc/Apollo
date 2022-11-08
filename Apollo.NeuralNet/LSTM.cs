using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Lstm
{
    // General Parameters
    private int InputSize { get; }
    private int OutputSize { get; }
    private double LearningRate { get; }

    // Gate weight matrices
    private Weight ForgetWeight { get; set; }
    private Weight InputWeight { get; set; }
    private Matrix MemoryCellState { get; set; }
    private Weight CellWeight { get; set; }
    private Weight OutputWeight { get; set; }
    
    
    public Lstm(int inputSize, int outputSize, double learningRate)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
        LearningRate = learningRate;

        // Init weight matrices
        MemoryCellState = new Matrix(OutputSize, OutputSize);
        
        ForgetWeight = new Weight(OutputSize, InputSize + OutputSize);
        InputWeight = new Weight(OutputSize, InputSize + OutputSize);
        CellWeight = new Weight(OutputSize, InputSize + OutputSize);
        OutputWeight = new Weight(OutputSize, InputSize + OutputSize);
    }

    public Matrix[] Forward(Matrix input)
    {
        var forgetGate = Matrix.Multiply(ForgetWeight, input);
        forgetGate.Sigmoid();
        MemoryCellState.Multiply(forgetGate);

        var inputGate = Matrix.Multiply(InputWeight, input);
        inputGate.Sigmoid();
        var cell = Matrix.Multiply(CellWeight, input);
        cell.Tanh();
        MemoryCellState.Add(Matrix.Multiply(inputGate, cell));

        var outputGate = Matrix.Multiply(OutputWeight, input);
        outputGate.Sigmoid();
        var output = Matrix.Multiply(outputGate, Matrix.Tanh(MemoryCellState));
        
        var returnValues = new Matrix[] { MemoryCellState, output, forgetGate, inputGate, cell, outputGate };
        return returnValues;
    }

    public Matrix[] Backprop(Matrix input, Matrix cellState, Matrix error, Matrix cellStates, Matrix forgetGate, 
        Matrix inputGate, Matrix cell, Matrix outputGate, Matrix dfcs, Matrix dfhs)
    {
        error = Matrix.Clamp(error + dfhs, -6, 6);
        
        // Calculate outputUpdate amount 
        var outputDerivative = Matrix.Tanh(cellState) * error;
        var outputUpdate = Matrix.Multiply(Matrix.Transpose(outputDerivative * Matrix.DTanh(outputGate)),
            input);
        
        // Calculate cellUpdate amount 
        var cellStatePDerivative = Matrix.Clip(error * outputGate * Matrix.DTanh(outputGate) + dfcs, -6, 6);
        var cellDerivative = cellStatePDerivative * inputGate;
        var cellUpdate = Matrix.Multiply(Matrix.Transpose(cellDerivative * Matrix.DTanh(cell)), input);
        
        // Calculate inputUpdate amount 
        var inputDerivative = cellStatePDerivative * cellStates;
        var inputUpdate = Matrix.Multiply(Matrix.Transpose(inputDerivative * Matrix.DSigmoid(inputGate)),
            input);
        
        // Calculate forgetUpdate amount 
        var forgetDerivative = cellStatePDerivative * cellStates;
        var forgetUpdate = Matrix.Multiply(Matrix.Transpose(forgetDerivative * Matrix.DSigmoid(forgetGate)),
            input);
        
        // Calculate full cell state derivative, and full hidden state derivative 
        var cellStateDerivative = cellStatePDerivative * forgetGate;

        var sliceString = $":{OutputSize}";
        var hiddenStateDerivative =
            Matrix.Multiply(cellDerivative, CellWeight)[sliceString] *
            Matrix.Multiply(outputDerivative, OutputWeight)[sliceString] *
            Matrix.Multiply(inputDerivative, InputWeight)[sliceString] *
            Matrix.Multiply(forgetDerivative, ForgetWeight)[sliceString];

        var returnValues = new Matrix[]
        {
            forgetUpdate, inputUpdate, cellUpdate,
            outputUpdate, cellStateDerivative, hiddenStateDerivative
        };
        return returnValues;
    }

    // Updating the LSTM cell is managed by the RNN "parent" network 
    // Adjusts variables of the cell 
    public void Update(Matrix forgetUpdate, Matrix inputUpdate, Matrix cellUpdate, Matrix outputUpdate)
    {
        // Adjust gradients 
        ForgetWeight.Update(forgetUpdate, LearningRate);
        InputWeight.Update(inputUpdate, LearningRate);
        CellWeight.Update(cellUpdate, LearningRate);
        OutputWeight.Update(outputUpdate, LearningRate);
    }
}