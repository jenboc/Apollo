using System;

namespace Apollo.MatrixMaths;

internal class MatrixArithmeticException : Exception
{
    public MatrixArithmeticException()
    { }

    public MatrixArithmeticException(string message) : base(message)
    { }

    public MatrixArithmeticException(string message, Exception inner) : base(message, inner)
    { }
}

internal class InvalidShapeException : Exception
{
    public InvalidShapeException()
    {
    }

    public InvalidShapeException(string message, Matrix a, Matrix b) : base($"{message} " +
                                                                            $"\nMatrix A Shape: ({a.Rows}x{a.Columns})" +
                                                                            $"\nMatrix B Shape: ({b.Rows}x{b.Columns})")
    {
    }

    public InvalidShapeException(string message, Exception inner) : base(message, inner)
    {
    }
}