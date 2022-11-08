using System;

namespace MatrixMaths
{
    // Generic Matrix 
    public class Matrix
    {
        protected double[,] _contents;

        public double[,] Contents { get => _contents; }

        public MatShape Shape { get => new MatShape(_contents.GetLength(0), _contents.GetLength(1)); }


        // Generate matrix full of zeros/default values 
        public Matrix(int rows, int columns)
        {
            _contents = new double[rows, columns];
        }
        public Matrix(MatShape shape) : this(shape.Rows, shape.Columns)
        { }
        

        // Create a matrix with already defined data 
        public Matrix(double[,] defaultData)
        {
            _contents = defaultData;
        }


        // Create a matrix with the same shape of another matrix 
        public static Matrix Like(Matrix matrix)
        {
            return new Matrix(matrix.Shape);
        }

        // Creates matrix of random values (between 0 and 1 inclusive) given a shape 
        public static Matrix Random(int rows, int columns, int seed=-1)
        {
            Random r = (seed == -1) ? new Random() : new Random(seed);

            var returnMatrix = new Matrix(rows, columns);
            returnMatrix.IterateContent((value) => r.NextDouble());
            return returnMatrix;
        }

        // Apply a function over each element in the matrix (used for tanh, sqrt, etc.)
        public void IterateContent(Func<double, double> contentAction)
        {
            for (int i = 0; i < _contents.GetLength(0); i++)
            {
                for (int j = 0; j < _contents.GetLength(1); j++)
                {
                    _contents[i, j] = contentAction(_contents[i, j]);
                }
            }
        }

        // Apply tanh() to each element
        public void Tanh()
        { 
            IterateContent(Math.Tanh);
        }
        public static Matrix Tanh(Matrix mat)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Tanh();
            return returnMat;
        }
    
        // Apply sigmoid function to each element 
        // sigmoid(x) = 1 / 1 + e^-x
        public void Sigmoid()
        {
            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] = 1 / (1 + Math.Exp(-_contents[i, j]));
                }
            }
        }
        public static Matrix Sigmoid(Matrix mat)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Sigmoid();
            return returnMat;
        }

        // Apply sqrt to each element
        public void Sqrt()
        {
            IterateContent(Math.Sqrt);
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
            IterateContent(Math.Exp);
        }
        public static Matrix Exp(Matrix mat)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Exp();
            return returnMat;
        }

        // Raise each element to a power
        public void Power(double power)
        {
            IterateContent((value) => Math.Pow(value, power));
        }
        public static Matrix Power(Matrix mat, double power)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Power(power);
            return returnMat;
        }

        // Multiply 2 matrices
        public void Multiply(Matrix otherMat)
        {
            // This matrix is multiplicatively comformable to otherMat if and only if:
            // this.columns = otherMat.rows 
            int thisCol = Shape.Columns;
            int otherRow = otherMat.Shape.Rows;

            // WRITE PROPER MESSAGE
            if (thisCol != otherRow)
                throw new MatrixArithmeticException("First matrix isn't multiplicatively conformable to the other");

            double[,] newContents = new double[Shape.Rows,otherMat.Shape.Columns];
            
            for (int row = 0; row < newContents.GetLength(0); row++)
            {
                for (int col = 0; col < newContents.GetLength(1); col++)
                {
                    // Row in A * Col in B 
                    // A has as many rows as B has columns therefore A's row has the same amount of numbers as B's column 
                    double sum = 0;

                    for (int i = 0; i < Shape.Columns; i++)
                    {
                        sum += _contents[row, i] * otherMat.Contents[i, col];
                    }

                    newContents[row, col] = sum;
                }
            }

            _contents = newContents;
        }
        public static Matrix Multiply(Matrix mat, Matrix otherMat)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Multiply(otherMat);
            return returnMat;
        }

        // Find the sum of every element in the matrix
        public double Sum()
        {
            double sum = 0;

            for (int i = 0; i < _contents.GetLength(0); i++)
            {
                for (int j = 0; j < _contents.GetLength(1); j++)
                {
                    sum += _contents[i, j];
                }
            }

            return sum;
        }

        // Reshape the matrix 
        public void Reshape(MatShape targetShape)
        {
            if (targetShape.Size != Shape.Size)
                throw new InvalidShapeException("Reshaping invalid - new matrix won't be the same size");

            double[,] newContents = new double[targetShape.Rows, targetShape.Columns];

            int newI = 0;
            int newJ = 0;
            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    newContents[newI, newJ] = _contents[i, j];
                    newJ++;

                    if (newJ >= targetShape.Columns)
                    {
                        newJ = 0;
                        newI++;
                    }
                }
            }

            _contents = newContents;
        }
        public static Matrix Reshape(Matrix mat, MatShape shape)
        {
            var returnMatrix = (Matrix)mat.MemberwiseClone();
            returnMatrix.Reshape(shape);
            return returnMatrix;
        }

        // Change the matrix shape to (1,)
        public void Ravel()
        {
            double[,] newContents = new double[1, Shape.Size];
         
            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    var newJ = i * Shape.Rows + j;
                    newContents[0,newJ] = _contents[i, j];
                }
            }

            _contents = newContents;
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
            double[,] newContents = new double[Shape.Columns, Shape.Rows];

            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    newContents[j, i] = _contents[i, j];
                }
            }

            _contents = newContents;
        }
        public static Matrix Transpose(Matrix mat)
        {
            var returnMatrix = (Matrix)mat.MemberwiseClone();
            returnMatrix.Transpose();
            return returnMatrix;
        }

        // Clamp each element between 2 bounds
        public void Clamp(double min, double max)
        {
            IterateContent((value) => Math.Clamp(value, min, max));
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
            if (otherMat.Shape.Rows != Shape.Rows)
                throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows");

            // New shape = (same rows, sum of columns)
            double[,] newContents = new double[Shape.Rows, Shape.Columns + otherMat.Shape.Columns];
            
            for (int i = 0; i < newContents.GetLength(0); i++)
            {
                for (int j = 0; j < newContents.GetLength(1); j++)
                {
                    double insertData = (j >= Shape.Columns) ? otherMat.Contents[i, j - Shape.Columns] : _contents[i,j];
                    newContents[i, j] = insertData;
                }
            }

            _contents = newContents;
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
            if (otherMat.Shape.Columns != Shape.Columns)
                throw new InvalidShapeException("You can only horizontally stack matrices with the same amount of rows");

            // New shape = (same rows, sum of columns)
            double[,] newContents = new double[Shape.Rows + otherMat.Shape.Rows, Shape.Columns];

            for (int i = 0; i < newContents.GetLength(0); i++)
            {
                for (int j = 0; j < newContents.GetLength(1); j++)
                {
                    double insertData = (i >= Shape.Rows) ? otherMat.Contents[i - Shape.Rows, j] : _contents[i, j];
                    newContents[i, j] = insertData;
                }
            }

            _contents = newContents;
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
            if (otherMat.Shape.Rows != Shape.Rows || otherMat.Shape.Columns != Shape.Columns)
                throw new MatrixArithmeticException("Matrices aren't additively applicable (not the same shape)");

            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] += otherMat.Contents[i, j];
                }
            }
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
            if (otherMat.Shape.Rows != Shape.Rows || otherMat.Shape.Columns != Shape.Columns)
                throw new MatrixArithmeticException("Matrices aren't additively applicable (not the same shape)");

            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] -= otherMat.Contents[i, j];
                }
            }
        }
        public static Matrix Subtract(Matrix mat, Matrix otherMat)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Subtract(otherMat);
            return returnMat;
        }
        
        public void Subtract(double scalar)
        {
            for (var i = 0; i < Shape.Rows; i++)
            {
                for (var j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] -= scalar;
                }
            }
        }
        public static Matrix Subtract(Matrix mat, double scalar)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Subtract(scalar);
            return returnMat;
        }
        
        public void Add(double scalar)
        {
            for (var i = 0; i < Shape.Rows; i++)
            {
                for (var j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] += scalar;
                }
            }
        }
        public static Matrix Add(Matrix mat, double scalar)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Add(scalar);
            return returnMat;
        }


        public void Multiply(double scalar)
        {
            for (int i = 0; i < Shape.Rows; i++)
            {
                for (int j = 0; j < Shape.Columns; j++)
                {
                    _contents[i, j] *= scalar;
                }
            }
        }
        public static Matrix Multiply(Matrix mat, double scalar)
        {
            var returnMat = (Matrix)mat.MemberwiseClone();
            returnMat.Multiply(scalar);
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

        public static Matrix operator +(Matrix a, double b)
        {
            return Add(a, b);
        }
        
        public static Matrix operator -(Matrix a, Matrix b)
        {
            return Subtract(a, b);
        }

        public static Matrix operator -(Matrix a, double b)
        {
            return Subtract(a, b);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            return Multiply(a, b);
        }

        public static Matrix operator *(Matrix mat, double scalar)
        {
            return Multiply(mat, scalar);
        }
        public static Matrix operator *(double scalar, Matrix mat)
        {
            return Multiply(mat, scalar);
        }

        public static Matrix operator /(Matrix mat, double scalar)
        {
            return Multiply(mat, 1 / scalar);
        }

        public static Matrix operator /(double scalar, Matrix mat)
        {
            return Multiply(mat, 1 / scalar);
        }

        private void InterpretSlice(string slice, ref int[] intSlice) 
        {
            // The first part of the split slice tells us what rows to take the second half from
            // The second half behaves like a regular python slice in the format START(inc.):END(ex.)  
            // Essentially, the entire slice directs us to what rows and columns to carve out from the original matrix

            string[] splitSlice = slice.Split(':');

            if (splitSlice[1] == "") // No number after : => slice start only 
                intSlice[0] = Convert.ToInt32(splitSlice[0]);
            else if (splitSlice[0] == "") // No numberbefore : => slice end only 
                intSlice[1] = Convert.ToInt32(splitSlice[1]);
            else  // One is before and one is after => first is the slice start, second is the slice end 
            {
                Console.WriteLine(splitSlice[0]);
                intSlice[0] = Convert.ToInt32(splitSlice[0]);
                intSlice[1] = Convert.ToInt32(splitSlice[1]);
            }
        }

        public Matrix this[string slice]
        {
            get
            {
                slice = slice.Replace(" ", "");
                string[] splitSlice = slice.Split(',');

                // Default slices such that the difference between item 0 and item 1 is the same as the shape of the matrix 
                int[] rowSlice = { 0, Shape.Rows };
                int[] colSlice = { 0, Shape.Columns};

                // Scenario A: only the row slice is provided (first character in slice is not a comma, and the length of the split slice is 1) 
                if (splitSlice[1] == "")
                    InterpretSlice(splitSlice[0], ref rowSlice);
                // Scenario B: only the col slice is provided (first character in slice IS a comma, and the length of the split slice is 1) 
                else if (splitSlice[0] == "")
                    InterpretSlice(splitSlice[1], ref colSlice);
                // Scenario C: both are provided 
                else
                {
                    InterpretSlice(splitSlice[0], ref rowSlice);
                    InterpretSlice(splitSlice[1], ref colSlice);
                }

                int numRows = rowSlice[1] - rowSlice[0];
                int numCols = colSlice[1] - colSlice[0];

                // Slices will be invalid if the number of rows or columns is negative, or 0 
                if (numRows <= 0 || numCols <= 0)
                    throw new InvalidSliceException("The inputted slice resulted in a number of rows or columns <= 0");

                double[,] sliceContent = new double[numRows, numCols]; 

                // Start is inclusive, end is exclusive
                for (int i = rowSlice[0]; i < rowSlice[1]; i++)
                {
                    int sliceI = i - rowSlice[0];
                    for (int j = colSlice[0]; j < colSlice[1]; j++)
                    {
                        int sliceJ = j - colSlice[0];
                        sliceContent[sliceI, sliceJ] = _contents[i, j];
                    }
                }

                return new Matrix(sliceContent);
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
}
