using System;

namespace MatrixMaths
{
    // Class made specifically to be used for the weights of the neural network 
    public class WeightMatrix : Matrix 
    {
        public Matrix Gradient { get; set; } // Another matrix to store the change in weight 

        public WeightMatrix(MatShape shape) : base(shape)
        {
            Gradient = new Matrix(shape); 
        }

        public void Optimise()
        {
            throw new NotImplementedException();
        }
    }
}