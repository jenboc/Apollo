using System;
using System.IO;
using System.Threading.Tasks;

namespace Apollo.MatrixMaths;

// Generic Matrix 
public class Matrix
{
    // Generate matrix full of zeros/default values 
    public Matrix(int rows, int columns)
    {
        Contents = new float[rows, columns];
    }

    // Create a matrix with already defined data 
    public Matrix(float[,] defaultData)
    {
        Contents = defaultData;
    }

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

    // Create a matrix with the same shape of another matrix 
    public static Matrix Like(Matrix matrix)
    {
        return new Matrix(matrix.Rows, matrix.Columns);
    }

    // Apply a function over each element in the matrix (used for tanh, sqrt, etc.)
    public void IterateContent(Func<float, float> contentAction)
    {
        Parallel.For(0, Rows, i =>
        {
            for (var j = 0; j < Columns; j++)
                Contents[i, j] = contentAction(Contents[i, j]);
        });
    }

    // Apply tanh() to each element
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

    // Derivative of tanh() 
    // = sech^2(x) = 1 / cosh^2(x)
    public void DTanh()
    {
        // for (var i = 0; i < Rows; i++)
        // {
        //     for (var j = 0; j < Columns; j++)
        //     {
        //         Contents[i, j] = 1 / Math.Pow(Math.Cosh(Contents[i, j]), 2);
        //     }
        // }

        IterateContent(ActivationFuncs.DTanh);
    }

    public static Matrix DTanh(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.DTanh();
        return returnMat;
    }

    // Apply sigmoid function to each element 
    // sigmoid(x) = 1 / 1 + e^-x
    public void Sigmoid()
    {
        // for (int i = 0; i < Rows; i++)
        // {
        //     for (int j = 0; j < Columns; j++)
        //     {
        //         Contents[i, j] = 1 / (1 + Math.Exp(-Contents[i, j]));
        //     }
        // }

        IterateContent(ActivationFuncs.Sigmoid);
    }

    public static Matrix Sigmoid(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.Sigmoid();
        return returnMat;
    }

    // Derivative of the Sigmoid Function 
    // = e^(-x) / (1 + e^(-x))^2
    public void DSigmoid()
    {
        // for (var i = 0; i < Rows; i++)
        // {
        //     for (var j = 0; j < Columns; j++)
        //     {
        //         Contents[i, j] = Math.Exp(-Contents[i, j]) / Math.Pow(1 + Math.Exp(-Contents[i, j]), 2);
        //     }
        // }

        IterateContent(ActivationFuncs.DSigmoid);
    }

    public static Matrix DSigmoid(Matrix mat)
    {
        var returnMat = mat.Clone();
        returnMat.DSigmoid();
        return returnMat;
    }

    // Apply sqrt to each element
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

    // e ^ each element
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

    // Raise each element to a power
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

    // Multiply 2 matrices
    public void Multiply(Matrix otherMat)
    {
        // This matrix is multiplicatively conformable to otherMat if and only if:
        // this.columns = otherMat.rows 
        if (Columns != otherMat.Rows)
            throw new MatrixArithmeticException("First matrix isn't multiplicatively conformable to the other");

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

    // Find the sum of every element in the matrix
    public float Sum()
    {
        float sum = 0;

        for (var i = 0; i < Contents.GetLength(0); i++)
        for (var j = 0; j < Contents.GetLength(1); j++)
            sum += Contents[i, j];

        return sum;
    }

    // Reshape the matrix 
    public void Reshape(int targetRows, int targetColumns)
    {
        if (targetRows * targetColumns != Rows * Columns)
            throw new InvalidShapeException("Reshaping invalid - new matrix won't be the same size");

        var newContents = new float[targetRows, targetColumns];

        var newI = 0;
        var newJ = 0;
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
        {
            newContents[newI, newJ] = Contents[i, j];
            newJ++;

            if (newJ >= targetColumns)
            {
                newJ = 0;
                newI++;
            }
        }

        Contents = newContents;
    }

    public static Matrix Reshape(Matrix mat, int targetRows, int targetColumns)
    {
        var returnMatrix = mat.Clone();
        returnMatrix.Reshape(targetRows, targetColumns);
        return returnMatrix;
    }

    // Change the matrix shape to (1,)
    public void Ravel()
    {
        var newContents = new float[1, Rows * Columns];

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
        {
            var newJ = i * Rows + j;
            newContents[0, newJ] = Contents[i, j];
        }

        Contents = newContents;
    }

    public static Matrix Ravel(Matrix mat)
    {
        var returnMatrix = mat.Clone();
        returnMatrix.Ravel();
        return returnMatrix;
    }

    // Transpose the matrix
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

    // Clamp each element between 2 bounds
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

    // Append 2 matrices horizontally 
    public void HorizontalStack(Matrix otherMat)
    {
        // Must have same amount of rows 
        if (otherMat.Rows != Rows)
            throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows");

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

    // Append 2 matrices vertically
    public void VerticalStack(Matrix otherMat)
    {
        // Must have same amount of columns 
        if (otherMat.Columns != Columns)
            throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows");

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
    ///     Matrix multiplication by scalar, changing the matrix it is performed upon
    /// </summary>
    public void Multiply(float scalar)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] *= scalar;
    }

    /// <summary>
    ///     Matrix multiplication by scalar, creating a new matrix for its output
    /// </summary>
    public static Matrix Multiply(Matrix mat, float scalar)
    {
        var returnMat = mat.Clone();
        returnMat.Multiply(scalar);
        return returnMat;
    }

    /// <summary>
    ///     Element-Wise multiplication, changing the matrix it is performed upon
    /// </summary>
    public void HadamardProd(Matrix otherMat)
    {
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new InvalidShapeException("Matrices must be the same shape for Hadamard multiplication " +
                                            $"one matrix is {Rows}x{Columns} and the other is {otherMat.Rows}x{otherMat.Columns}");

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] *= otherMat.Contents[i, j];
    }

    /// <summary>
    ///     Element-Wise multiplication, creating a new matrix for the output
    /// </summary>
    public static Matrix HadamardProd(Matrix mat1, Matrix mat2)
    {
        var returnMat = mat1.Clone();
        returnMat.HadamardProd(mat2);
        return returnMat;
    }

    /// <summary>
    ///     Element-wise division, changing the matrix it is performed upon
    /// </summary>
    public void HadamardDiv(Matrix otherMat)
    {
        if (otherMat.Rows != Rows || otherMat.Columns != Columns)
            throw new InvalidShapeException("Matrices must be the same shape for Hadamard division " +
                                            $"one matrix is {Rows}x{Columns} and the other is {otherMat.Rows}x{otherMat.Columns}");

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] /= otherMat.Contents[i, j];
    }

    /// <summary>
    ///     Element wise division, creating a new matrix for the output
    /// </summary>
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
    ///     Get the highest value in the matrix
    /// </summary>
    /// <returns>The highest value in the matrix</returns>
    public float Max()
    {
        var max = float.MinValue;
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            if (Contents[i, j] > max)
                max = Contents[i, j];

        return max;
    }

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

    public void Clip(float min, float max)
    {
        IterateContent(value => Math.Clamp(value, min, max));
    }

    public static Matrix Clip(Matrix mat, float min, float max)
    {
        var returnMat = mat.Clone();
        returnMat.Clip(min, max);
        return returnMat;
    }

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

    public Matrix Clone()
    {
        return new Matrix((float[,])Contents.Clone());
    }

    /// <summary>
    ///     Write the matrix to a binary file
    /// </summary>
    /// <param name="writer">Instance of BinaryWriter to use for writing</param>
    public void WriteToFile(BinaryWriter writer)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            writer.Write(Contents[i, j]);
    }

    /// <summary>
    ///     Read a matrix from a binary file
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
            readMat.Contents[i, j] = (float)reader.ReadDecimal();

        return readMat;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}