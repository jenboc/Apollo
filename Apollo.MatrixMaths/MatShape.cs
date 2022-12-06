﻿namespace Apollo.MatrixMaths
{
    public struct MatShape
    {
        public int Rows;
        public int Columns;

        public int Size { get => Rows * Columns; }

        public MatShape(int numRows, int numCols)
        {
            Rows = numRows;
            Columns = numCols;
        }
    }
}