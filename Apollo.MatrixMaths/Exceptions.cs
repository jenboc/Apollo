using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.MatrixMaths
{
    class MatrixArithmeticException : Exception 
    {
        public MatrixArithmeticException() : base() { }
        public MatrixArithmeticException(string message) : base(message) { }
        public MatrixArithmeticException(string message, Exception inner) : base(message, inner) { }
    }

    class InvalidShapeException : Exception
    {
        public InvalidShapeException() : base() { }
        public InvalidShapeException(string message) : base(message) { }
        public InvalidShapeException(string message, Exception inner) : base(message, inner) { }
    }

    class InvalidSliceException : Exception
    {
        public InvalidSliceException() : base() { }
        public InvalidSliceException(string message) : base(message) { }
        public InvalidSliceException(string message, Exception inner) : base(message, inner) { }
    }
}
