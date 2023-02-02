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
    { }

    public InvalidShapeException(string message) : base(message)
    { }

    public InvalidShapeException(string message, Exception inner) : base(message, inner)
    { }
}

internal class InvalidSliceException : Exception
{
    public InvalidSliceException()
    { }

    public InvalidSliceException(string message) : base(message)
    { }

    public InvalidSliceException(string message, Exception inner) : base(message, inner)
    { }
}