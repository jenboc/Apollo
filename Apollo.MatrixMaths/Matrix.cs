using System;
using System.IO;
using System.Threading.Tasks;

namespace Apollo.MatrixMaths;

// Generic Matrix 
public class Matrix
{
    /// <summary>
    /// Generate matrix full of zeros 
    /// </summary>
    /// <param name="rows">Number of rows in matrix</param>
    /// <param name="columns">Number of columns in matrix</param>
    public Matrix(int rows, int columns)
    {
        Contents = new float[rows, columns];
    }

    /// <summary>
    /// Create a matrix with already defined data
    /// </summary>
    /// <param name="defaultData">Data contained in the matrix</param>
    public Matrix(float[,] defaultData)
    {
        Contents = defaultData;
    }

    /// <summary>
    /// Create a matrix of random values
    /// </summary>
    /// <param name="rows">Number of rows in the matrix</param>
    /// <param name="columns">Number of columns in the matrix</param>
    /// <param name="r">Instance of Random</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    public Matrix(int rows, int columns, Random r, int min = -2, int max = 2)
    {
        Contents = new float[rows, columns];
        IterateContent(value => r.Next(min, max) + (float)r.NextDouble());
    }

    public float[,] Contents { get; private set; }

    public float this[int i, int j]
    {
        get => Contents[i, j];
        set => Contents[i, j] = value;
    }

    public int Rows => Contents.GetLength(0);
    public int Columns => Contents.GetLength(1);
    
    /// <summary>
    /// Create a new matrix with the same shape as another matrix
    /// </summary>
    /// <param name="matrix">Matrix to copy the shape of</param>
    public static Matrix Like(Matrix matrix)
    {
        return new Matrix(matrix.Rows, matrix.Columns);
    }

    /// <summary>
    /// Apply a function over each element in the matrix (used for tanh, sqrt, etc.)
    /// </summary>
    /// <param name="contentAction">Function to apply to every value in the matrix</param>
    public void IterateContent(Func<float, float> contentAction)
    {
        Parallel.For(0, Rows, i =>
        {
            for (var j = 0; j < Columns; j++)
                Contents[i, j] = contentAction(Contents[i, j]);
        });
    }

    /// <summary>
    /// Apply the hyperbolic tangent to each element in the matrix
    /// </summary>
    public void Tanh()
    {
        IterateContent(ActivationFuncs.Tanh);
    }
    public static Matrix Tanh(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.Tanh();
        return returnMat;
    }

    /// <summary>
    /// Apply the derivative of the hyperbolic tangent to each element in the matrix
    /// </summary>
    public void DTanh()
    {
        IterateContent(ActivationFuncs.DTanh);
    }
    public static Matrix DTanh(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.DTanh();
        return returnMat;
    }

    /// <summary>
    /// Apply the sigmoid activation function to each element in the matrix
    /// </summary>
    public void Sigmoid()
    {
        IterateContent(ActivationFuncs.Sigmoid);
    }
    public static Matrix Sigmoid(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.Sigmoid();
        return returnMat;
    }
    
    /// <summary>
    /// Apply the derivative of the sigmoid function to each element in the matrix
    /// </summary>
    public void DSigmoid()
    {
        IterateContent(ActivationFuncs.DSigmoid);
    }
    public static Matrix DSigmoid(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.DSigmoid();
        return returnMat;
    }

    /// <summary>
    /// Square root each element in the matrix
    /// </summary>
    public void Sqrt()
    {
        IterateContent(MathF.Sqrt);
    }
    public static Matrix Sqrt(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.Sqrt();
        return returnMat;
    }

    /// <summary>
    /// Put euler's number to the power of each element in the matrix
    /// </summary>
    public void Exp()
    {
        IterateContent(MathF.Exp);
    }
    public static Matrix Exp(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.Exp();
        return returnMat;
    }

    /// <summary>
    /// Raise each element to a power
    /// </summary>
    /// <param name="power">The power to raise each element to</param>
    public void Power(float power)
    {
        IterateContent(value => MathF.Pow(value, power));
    }
    public static Matrix Power(Matrix mat, float power)
    {
        var returnMat = mat.Clone();
        returnMat.Power(power);
        return returnMat;
    }
    
    /// <summary>
    /// Multiply two matrices together using standard matrix multiplication
    /// </summary>
    /// <param name="otherMat">The other operand</param>
    public void Multiply(Matrix otherMat)
    {
        // This matrix is multiplicatively conformable to otherMat if and only if:
        // this.columns = otherMat.rows 
        if (Columns != otherMat.Rows)
            throw new InvalidShapeException("First matrix isn't multiplicatively conformable to the other",
                this, otherMat);

        var newContents = new float[Rows, otherMat.Columns];

        Parallel.For(0, Rows, row =>
        {
            for (var col = 0; col < otherMat.Columns; col++)
            {
                // Row in A * Col in B 
                // A has as many columns as B has rows therefore A's columns has the same amount of numbers as B's rows 
                float sum = 0;

                for (var i = 0; i < Columns; i++)
                    sum += Contents[row, i] * otherMat.Contents[i, col];

                newContents[row, col] = sum;
            }
        });
        
        Contents = newContents;
    }
    public static Matrix Multiply(Matrix mat, Matrix otherMat)
    {
        var returnMat = mat.Clone();
        returnMat.Multiply(otherMat);
        return returnMat;
    }

    /// <summary>
    /// Calculate the sum of every element in the matrix
    /// </summary>
    /// <returns>The sum of every element in the matrix</returns>
    public float Sum()
    {
        float sum = 0;

        for (var i = 0; i < Contents.GetLength(0); i++)
        for (var j = 0; j < Contents.GetLength(1); j++)
            sum += Contents[i, j];

        return sum;
    }

    /// <summary>
    /// Transpose the matrix
    /// </summary>
    public void Transpose()
    {
        var newContents = new float[Columns, Rows];

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            newContents[j, i] = Contents[i, j];

        Contents = newContents;
    }
    public static Matrix Transpose(Matrix mat)
    {
        var returnMatrix = mat.Clone();
        returnMatrix.Transpose();
        return returnMatrix;
    }

    /// <summary>
    /// Clamp each element of the matrix between two values
    /// </summary>
    /// <param name="min">The minimum possible value in the matrix</param>
    /// <param name="max">The maximum possible value in the matrix</param>
    public void Clamp(float min, float max)
    {
        IterateContent(value => Math.Clamp(value, min, max));
    }
    public static Matrix Clamp(Matrix mat, int min, int max)
    {
        var returnMat = mat.Clone();
        returnMat.Clamp(min, max);
        return returnMat;
    }
    
    /// <summary>
    /// Concatenate two matrices horizontally 
    /// </summary>
    /// <param name="otherMat">The matrix to concatenate to this one</param>
    public void HorizontalStack(Matrix otherMat)
    {
        // Must have same amount of rows 
        if (otherMat.Rows != Rows)
            throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows",
                this, otherMat);

        // New shape = (same rows, sum of columns)
        var newContents = new float[Rows, Columns + otherMat.Columns];

        for (var i = 0; i < newContents.GetLength(0); i++)
        for (var j = 0; j < newContents.GetLength(1); j++)
        {
            var insertData = j >= Columns ? otherMat.Contents[i, j - Columns] : Contents[i, j];
            newContents[i, j] = insertData;
        }

        Contents = newContents;
    }
    public static Matrix HorizontalStack(Matrix mat, Matrix otherMat)
    {
        var returnMat = mat.Clone();
        returnMat.HorizontalStack(otherMat);
        return returnMat;
    }

    /// <summary>
    /// Concatenate two matrices vertically
    /// </summary>
    /// <param name="otherMat">The matrix to concatenate to this one</param>
    public void VerticalStack(Matrix otherMat)
    {
        // Must have same amount of columns 
        if (otherMat.Columns != Columns)
            throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows",
                this, otherMat);

        // New shape = (same rows, sum of columns)
        var newContents = new float[Rows + otherMat.Rows, Columns];

        for (var i = 0; i < newContents.GetLength(0); i++)
        for (var j = 0; j < newContents.GetLength(1); j++)
        {
            var insertData = i >= Rows ? otherMat.Contents[i - Rows, j] : Contents[i, j];
            newContents[i, j] = insertData;
        }

        Contents = newContents;
    }
    public static Matrix VerticalStack(Matrix mat, Matrix otherMat)
    {
        var returnMat = mat.Clone();
        returnMat.VerticalStack(otherMat);
        return returnMat;
    }

    /// <summary>
    /// Add two matrices together
    /// </summary>
    /// <param name="otherMat">The matrix to add to this one</param>
    public void Add(Matrix otherMat)
    {
        // Shapes must be the same 
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new MatrixArithmeticException("Matrices aren't additively applicable (not the same shape)");

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] += otherMat.Contents[i, j];
    }
    public static Matrix Add(Matrix mat, Matrix otherMat)
    {
        var returnMat = mat.Clone();
        returnMat.Add(otherMat);
        return returnMat;
    }

    /// <summary>
    /// Subtract two matrices
    /// </summary>
    /// <param name="otherMat">The matrix to take away from this one</param>
    public void Subtract(Matrix otherMat)
    {
        // Shapes must be the same 
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new MatrixArithmeticException("Matrices aren't additively applicable (not the same shape)");

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] -= otherMat.Contents[i, j];
    }
    public static Matrix Subtract(Matrix mat, Matrix otherMat)
    {
        var returnMat = mat.Clone();
        returnMat.Subtract(otherMat);
        return returnMat;
    }
    
    /// <summary>
    /// Subtract a scalar quantity from each element in the matrix 
    /// </summary>
    /// <param name="scalar">Quantity to subtract</param>
    public void Subtract(float scalar)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] -= scalar;
    }
    public static Matrix Subtract(Matrix mat, float scalar)
    {
        var returnMat = mat.Clone();
        returnMat.Subtract(scalar);
        return returnMat;
    }

    /// <summary>
    /// Add a scalar quantity to each element in the matrix
    /// </summary>
    /// <param name="scalar">Quantity to add</param>
    public void Add(float scalar)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] += scalar;
    }
    public static Matrix Add(Matrix mat, float scalar)
    {
        var returnMat = mat.Clone();
        returnMat.Add(scalar);
        return returnMat;
    }

    /// <summary>
    /// Multiply each element in the matrix by a scalar quantity
    /// </summary>
    /// <param name="scalar">Quantity to multiply by</param>
    public void Multiply(float scalar)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] *= scalar;
    }
    public static Matrix Multiply(Matrix mat, float scalar)
    {
        var returnMat = mat.Clone();
        returnMat.Multiply(scalar);
        return returnMat;
    }

    /// <summary>
    /// Element wise multiplication between two matrices
    /// </summary>
    /// <param name="otherMat">Matrix to multiply by</param>
    public void HadamardProd(Matrix otherMat)
    {
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new InvalidShapeException("Matrices must be the same shape for Hadamard multiplication",
                this, otherMat);

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] *= otherMat.Contents[i, j];
    }
    public static Matrix HadamardProd(Matrix mat1, Matrix mat2)
    {
        var returnMat = mat1.Clone();
        returnMat.HadamardProd(mat2);
        return returnMat;
    }

    /// <summary>
    /// Element wise division between two matrices
    /// </summary>
    /// <param name="otherMat">Matrix to divide by</param>
    public void HadamardDiv(Matrix otherMat)
    {
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new InvalidShapeException("Matrices must be the same shape for Hadamard division", this, otherMat);

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] /= otherMat.Contents[i, j];
    }
    public static Matrix HadamardDiv(Matrix mat1, Matrix mat2)
    {
        var returnMat = mat1.Clone();
        returnMat.HadamardDiv(mat2);
        return returnMat;
    }

    public void Softmax()
    {
        // If not implemented properly, this function could produce NaNs 
        // This is because e^x is very large when x isn't that big, thus could produce an overflow error 
        // To counter-act this, we subtract the highest number in the matrix from every matrix element, and then 
        // calculate the softmax since this operation does not change the result 

        // Softmaxed matrices are considered row by row, so only apply softmax to individual rows then recompile the matrix

        var matRows = new Matrix[Rows];

        for (var i = 0; i < Rows; i++)
        {
            var rowContent = new float[1, Columns];
            for (var j = 0; j < Columns; j++) rowContent[0, j] = Contents[i, j];

            matRows[i] = new Matrix(rowContent);

            var highestNumber = matRows[i].Max();
            matRows[i].IterateContent(value => value - highestNumber);
            matRows[i].Exp();
            var sum = matRows[i].Sum();
            matRows[i].Multiply(1 / sum);
        }

        // Compile the softmax rows into one matrix
        var compiled = StackArray(matRows);
        Contents = compiled.Contents;
    }
    public static Matrix Softmax(Matrix matrix)
    {
        var returnMat = matrix.Clone();
        returnMat.Softmax();
        return returnMat;
    }

    /// <summary>
    /// Get the highest value in the matrix
    /// </summary>
    public float Max()
    {
        var max = float.MinValue;
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            if (Contents[i, j] > max)
                max = Contents[i, j];

        return max;
    }

    /// <summary>
    /// Apply a logarithm to each element in the matrix
    /// </summary>
    /// <param name="logBase">The base of the logarithm</param>
    public void Log(float logBase)
    {
        IterateContent(value => MathF.Log(value, logBase));
    }
    public static Matrix Log(Matrix matrix, float logBase)
    {
        var returnMat = matrix.Clone();
        returnMat.Log(logBase);
        return returnMat;
    }
    
    #region Operator overloading
    
    public static bool operator ==(Matrix a, Matrix b)
    {
        return a.Contents == b.Contents;
    }

    public static bool operator !=(Matrix a, Matrix b)
    {
        return a.Contents != b.Contents;
    }

    public static Matrix operator +(Matrix a, Matrix b)
    {
        return Add(a, b);
    }

    public static Matrix operator +(Matrix a, float b)
    {
        return Add(a, b);
    }

    public static Matrix operator -(Matrix a, Matrix b)
    {
        return Subtract(a, b);
    }

    public static Matrix operator -(Matrix a, float b)
    {
        return Subtract(a, b);
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        return Multiply(a, b);
    }

    public static Matrix operator *(Matrix mat, float scalar)
    {
        return Multiply(mat, scalar);
    }

    public static Matrix operator *(float scalar, Matrix mat)
    {
        return Multiply(mat, scalar);
    }

    public static Matrix operator /(Matrix mat, float scalar)
    {
        return Multiply(mat, 1 / scalar);
    }

    public static Matrix operator /(float scalar, Matrix mat)
    {
        return Multiply(mat, 1 / scalar);
    }

    #endregion
    
    /// <summary>
    /// Stack a list of matrices into a single matrix
    /// </summary>
    /// <param name="matrices">List of matrices to stack</param>
    /// <param name="vertically">Flag variable specificing whether to stack vertically</param>
    /// <returns>A compiled matrix</returns>
    public static Matrix StackArray(Matrix[] matrices, bool vertically = true)
    {
        var mat = matrices[0].Clone();

        for (var i = 1; i < matrices.Length; i++)
            if (vertically)
                mat.VerticalStack(matrices[i]);
            else
                mat.HorizontalStack(matrices[i]);

        return mat;
    }

    /// <summary>
    /// Creates a copy of the matrix
    /// </summary>
    public Matrix Clone()
    {
        return new Matrix((float[,])Contents.Clone());
    }

    /// <summary>
    /// Write the matrix to a binary file
    /// </summary>
    /// <param name="writer">Instance of BinaryWriter to use for writing</param>
    public void WriteToFile(BinaryWriter writer)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            writer.Write(Contents[i, j]);
    }

    /// <summary>
    /// Read a matrix from a binary file
    /// </summary>
    /// <param name="reader">Instance of BinaryReader to use for reading</param>
    /// <param name="rows">Number of rows in the matrix</param>
    /// <param name="columns">Number of columns in the matrix</param>
    /// <returns></returns>
    public static Matrix ReadFromFile(BinaryReader reader, int rows, int columns)
    {
        var readMat = new Matrix(rows, columns);

        for (var i = 0; i < rows; i++)
        for (var j = 0; j < columns; j++)
            readMat.Contents[i, j] = reader.ReadSingle();

        return readMat;
    }
}