using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Weight : Matrix 
{
    private Matrix Gradient { get; set; }

    // Call Matrix constructor to create Weight matrix
    public Weight(int rows, int columns) : base(rows, columns)
    {
        // Initialise a zero matrix the same size as the weight matrix
        Gradient = new Matrix(rows, columns);
    }
    
    // Update weight + gradient using RMSProp
    public void Update(Matrix updateAmount, float learningRate)
    {
        // Adjust Gradient
        Gradient = 0.9f * Gradient + 0.1f * Power(updateAmount, 2); // Uses static method to create new matrix
        
        // Adjust itself
        Subtract(learningRate / Sqrt(Gradient + 1e-8f) * updateAmount); // Uses non-static method to adjust
    }
}