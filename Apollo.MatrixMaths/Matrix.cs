using System;

namespace Apollo.MatrixMaths;

// Generic Matrix 
public class Matrix
{
    public float[,] Contents {get; private set;}

    public int Rows
    {
        get 
        {
            return Contents.GetLength(0);
        }
    }

    public int Columns 
    {
        get 
        {
            return Contents.GetLength(1);
        }
    }

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


    public Matrix this[string slice]
    {
        get
        {
            slice = slice.Replace(" ", "");
            var splitSlice = slice.Split(',');

            // Default slices such that the difference between item 0 and item 1 is the same as the shape of the matrix 
            int[] rowSlice = { 0, Rows };
            int[] colSlice = { 0, Columns };

            // Scenario A: only the row slice is provided (first character in slice is not a comma, and the length of the split slice is 1) 
            if (splitSlice[1] == "")
            {
                InterpretSlice(splitSlice[0], ref rowSlice);
            }
            // Scenario B: only the col slice is provided (first character in slice IS a comma, and the length of the split slice is 1) 
            else if (splitSlice[0] == "")
            {
                InterpretSlice(splitSlice[1], ref colSlice);
            }
            // Scenario C: both are provided 
            else
            {
                InterpretSlice(splitSlice[0], ref rowSlice);
                InterpretSlice(splitSlice[1], ref colSlice);
            }

            var numRows = rowSlice[1] - rowSlice[0];
            var numCols = colSlice[1] - colSlice[0];

            // Slices will be invalid if the number of rows or columns is negative, or 0 
            if (numRows <= 0 || numCols <= 0)
                throw new InvalidSliceException("The inputted slice resulted in a number of rows or columns <= 0");

            var sliceContent = new float[numRows, numCols];

            // Start is inclusive, end is exclusive
            for (var i = rowSlice[0]; i < rowSlice[1]; i++)
            {
                var sliceI = i - rowSlice[0];
                for (var j = colSlice[0]; j < colSlice[1]; j++)
                {
                    var sliceJ = j - colSlice[0];
                    sliceContent[sliceI, sliceJ] = Contents[i, j];
                }
            }

            return new Matrix(sliceContent);
        }
    }


    // Create a matrix with the same shape of another matrix 
    public static Matrix Like(Matrix matrix)
    {
        return new Matrix(matrix.Rows, matrix.Columns);
    }

    // Creates matrix of random values (between 0 and 1 inclusive) given a shape 
    public static Matrix Random(int rows, int columns, int seed = -1)
    {
        var r = seed == -1 ? new Random() : new Random(seed);

        var returnMatrix = new Matrix(rows, columns);
        returnMatrix.IterateContent(value => (float)r.NextDouble());
        return returnMatrix;
    }

    // Apply a function over each element in the matrix (used for tanh, sqrt, etc.)
    public void IterateContent(Func<float, float> contentAction)
    {
        for (var i = 0; i < Contents.GetLength(0); i++)
        for (var j = 0; j < Contents.GetLength(1); j++)
            Contents[i, j] = contentAction(Contents[i, j]);
    }

    // Apply tanh() to each element
    public void Tanh()
    {
        IterateContent(ActivationFuncs.Tanh);
    }

    public static Matrix Tanh(Matrix mat)
    {
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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

        for (var row = 0; row < newContents.GetLength(0); row++)
        {
            for (var col = 0; col < newContents.GetLength(1); col++)
            {
                // Row in A * Col in B 
                // A has as many columns as B has rows therefore A's columns has the same amount of numbers as B's rows 
                float sum = 0;

                for (var i = 0; i < Columns; i++)
                    sum += Contents[row, i] * otherMat.Contents[i, col];

                newContents[row, col] = sum;
            }
        }

        Contents = newContents;
    }

    public static Matrix Multiply(Matrix mat, Matrix otherMat)
    {
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMatrix = (Matrix)mat.MemberwiseClone();
        returnMatrix.Reshape(targetRows, targetColumns);
        return returnMatrix;
    }

    // Change the matrix shape to (1,)
    public void Ravel()
    {
        var newContents = new float[1, Rows * Columns];

        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                var newJ = i * Rows + j;
                newContents[0, newJ] = Contents[i, j];
            }
        }

        Contents = newContents;
    }

    public static Matrix Ravel(Matrix mat)
    {
        var returnMatrix = (Matrix)mat.MemberwiseClone();
        returnMatrix.Ravel();
        return returnMatrix;
    }

    // Transpose the matrix
    public void Transpose()
    {
        var newContents = new float[Columns, Rows];

        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
                newContents[j, i] = Contents[i, j];
        }

        Contents = newContents;
    }

    public static Matrix Transpose(Matrix mat)
    {
        var returnMatrix = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
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
        var returnMat = (Matrix)mat.MemberwiseClone();
        returnMat.Add(scalar);
        return returnMat;
    }

    /// <summary>
    /// Matrix multiplication by scalar, changing the matrix it is performed upon 
    /// </summary>
    public void Multiply(float scalar)
    {
        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Columns; j++)
            Contents[i, j] *= scalar;
    }
    
    /// <summary>
    /// Matrix multiplication by scalar, creating a new matrix for its output
    /// </summary>
    public static Matrix Multiply(Matrix mat, float scalar)
    {
        var returnMat = (Matrix)mat.MemberwiseClone();
        returnMat.Multiply(scalar);
        return returnMat;
    }

    /// <summary>
    /// Element-Wise multiplication, changing the matrix it is performed upon 
    /// </summary>
    public void Hadamard(Matrix otherMat)
    {
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                Contents[i, j] *= otherMat.Contents[i, j];
            }
        }
    }

    /// <summary>
    /// Element-Wise multiplication, creating a new matrix for the output 
    /// </summary>
    public static Matrix Hadamard(Matrix mat1, Matrix mat2)
    {
        var returnMat = (Matrix)mat1.MemberwiseClone();
        returnMat.Hadamard(mat2);
        return returnMat;
    }

    public void Clip(float min, float max)
    {
        IterateContent(value => Math.Clamp(value, min, max));
    }

    public static Matrix Clip(Matrix mat, float min, float max)
    {
        var returnMat = (Matrix)mat.MemberwiseClone();
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

    private void InterpretSlice(string slice, ref int[] intSlice)
    {
        // The first part of the split slice tells us what rows to take the second half from
        // The second half behaves like a regular python slice in the format START(inc.):END(ex.)  
        // Essentially, the entire slice directs us to what rows and columns to carve out from the original matrix

        var splitSlice = slice.Split(':');

        if (splitSlice[1] == "") // No number after : => slice start only 
        {
            intSlice[0] = Convert.ToInt32(splitSlice[0]);
        }
        else if (splitSlice[0] == "") // No numberbefore : => slice end only 
        {
            intSlice[1] = Convert.ToInt32(splitSlice[1]);
        }
        else // One is before and one is after => first is the slice start, second is the slice end 
        {
            Console.WriteLine(splitSlice[0]);
            intSlice[0] = Convert.ToInt32(splitSlice[0]);
            intSlice[1] = Convert.ToInt32(splitSlice[1]);
        }
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