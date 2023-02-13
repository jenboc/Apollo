namespace Apollo.MatrixMaths.Tests;

public class MatrixTests
{
    /// <summary>
    /// Test data used in the majority of the unit tests
    /// </summary>
    private readonly float[,] defaultData =
    {
        { 0, 1, 4 },
        { 9, 18, 99 },
        { 1000, 2500, 1990 }
    };

    private readonly float[,] defaultData2 =
    {
        { 24, 3, 12 },
        { 1, 0, -5 },
        { -25, 1, 22 }
    };

    [Fact]
    // Tests that the power operation is correctly performed
    // i.e. each item in the matrix is taken and put to the specified power separately 
    public void Power()
    {
        float[,] expectedData =
        {
            { 0, 1, 16 },
            { 81, 324, 9801 },
            { 1000000, 6250000, 3960100 }
        };

        var actualMatrix = new Matrix(defaultData);
        actualMatrix.Power(2);
        var actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that the square root operation is correctly performed
    // i.e. the square root of each item in the matrix is found separately
    public void Sqrt()
    {
        var expectedData = new[,]
        {
            { 0, 1, 2 },
            { 3, 4.2426406871192848f, 9.9498743710662f },
            { 31.622776601683793f, 50, 44.609416046390926f }
        };

        var actualMatrix = new Matrix(defaultData);
        actualMatrix.Sqrt();
        var actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that the exp operation is correctly performed
    // i.e. Euler's number is put to the power of each item in the matrix separately 
    public void Exp()
    {
        var expectedData = new[,]
        {
            { 1, 2.71828175f, 54.5981483f },
            { 8103.08398f, 65659968, float.PositiveInfinity },
            { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity }
        };

        var actualMatrix = new Matrix(defaultData);
        actualMatrix.Exp();
        var actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that the tanh operation is correctly performed
    // i.e. tanh is applied to each item in the matrix separately 
    public void Tanh()
    {
        var expectedData = new[,]
        {
            { 0, 0.761594176f, 0.999329329f },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        var actualMatrix = new Matrix(defaultData);
        actualMatrix.Tanh();
        var actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that standard matrix multiplication is correctly applied to matrices of the same shape
    public void SameShapeMatmul()
    {
        var expectedData = new float[,]
        {
            { -99, 4, 83 },
            { -2241, 126, 2196 },
            { -23250, 4990, 43280 }
        };

        var defaultMat1 = new Matrix(defaultData);
        var defaultMat2 = new Matrix(defaultData2);
        defaultMat1.Multiply(defaultMat2);
        var actualData = defaultMat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that standard matrix multiplication is correctly applied to matrices of different but valid shapes
    public void DiffShapeMatmul()
    {
        var expectedData = new[,]
        {
            { -7f },
            { -1f }
        };

        var mat1Data = new[,]
        {
            { 1f, -2f },
            { 3f, 4f }
        };

        var mat2Data = new[,]
        {
            { -3f },
            { 2f }
        };

        var mat1 = new Matrix(mat1Data);
        var mat2 = new Matrix(mat2Data);
        var result = Matrix.Multiply(mat1, mat2);
        var actualData = result.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that multiplying a matrix by a scalar works as expected
    // i.e. each item in the matrix is independently multiplied by the scalar quantity 
    public void ScalarMul()
    {
        var scalar = 2;
        var expectedData = new float[,]
        {
            { 0, 2, 8 },
            { 18, 36, 198 },
            { 2000, 5000, 3980 }
        };

        var defaultMat = new Matrix(defaultData);
        defaultMat.Multiply(scalar);
        var actualData = defaultMat.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Checks that summing the elements in the matrix works as expected
    public void Sum()
    {
        var expectedData = 5621.0;

        var defaultMat = new Matrix(defaultData);
        var actualData = defaultMat.Sum();

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Checks that the transpose operation works as expected
    // i.e. the first row becomes the first column, and the second row becomes the second column, etc.
    public void Transpose()
    {
        var startingData = new float[,]
        {
            { 0, 1, 4 },
            { 9, 18, 99 }
        };

        var expectedData = new float[,]
        {
            { 0, 9 },
            { 1, 18 },
            { 4, 99 }
        };

        var startMat = new Matrix(startingData);
        startMat.Transpose();
        var actualData = startMat.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Checks that the clamp operation works as expected
    // i.e. all the elements in the matrix after the operation should be in the interval [min, max]
    public void Clamp()
    {
        var min = 2;
        var max = 10;

        var expectedData = new float[,]
        {
            { 2, 2, 4 },
            { 9, 10, 10 },
            { 10, 10, 10 }
        };

        var defaultMat = new Matrix(defaultData);
        defaultMat.Clamp(min, max);
        var actualData = defaultMat.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Checks that matrices can be stacked horizontally
    // i.e. pushed next to one another 
    public void HorizontalStack()
    {
        var expectedData = new float[,]
        {
            { 0, 1, 4, 24, 3, 12 },
            { 9, 18, 99, 1, 0, -5 },
            { 1000, 2500, 1990, -25, 1, 22 }
        };

        var defaultMat1 = new Matrix(defaultData);
        var defaultMat2 = new Matrix(defaultData2);
        defaultMat1.HorizontalStack(defaultMat2);
        var actualData = defaultMat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Checks that matrices can be stacked vertically
    // i.e. stacked one on top of the other 
    public void VerticalStack()
    {
        var expectedData = new float[,]
        {
            { 0, 1, 4 },
            { 9, 18, 99 },
            { 1000, 2500, 1990 },
            { 24, 3, 12 },
            { 1, 0, -5 },
            { -25, 1, 22 }
        };

        var defaultMat1 = new Matrix(defaultData);
        var defaultMat2 = new Matrix(defaultData2);
        defaultMat1.VerticalStack(defaultMat2);
        var actualData = defaultMat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that matrices can be randomly generated 
    public void Random()
    {
        var mat1R = new Random(5);
        var mat2R = new Random(120);

        var mat1 = new Matrix(5, 5, mat1R);
        var mat2 = new Matrix(5, 5, mat2R);

        Assert.NotEqual(mat1.Contents, mat2.Contents);
    }

    [Fact]
    // Tests that two matrices can be added together correctly
    // (Addition is element-wise) 
    public void Add()
    {
        var expectedData = new float[,]
        {
            { 24, 4, 16 },
            { 10, 18, 94 },
            { 975, 2501, 2012 }
        };

        var mat1 = new Matrix(defaultData);
        var mat2 = new Matrix(defaultData2);
        mat1.Add(mat2);
        var actualData = mat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that two matrices can be subtracted correctly
    // (Subtraction is element-wise)
    public void Subtract()
    {
        var expectedData = new float[,]
        {
            { -24, -2, -8 },
            { 8, 18, 104 },
            { 1025, 2499, 1968 }
        };

        var mat1 = new Matrix(defaultData);
        var mat2 = new Matrix(defaultData2);
        mat1.Subtract(mat2);
        var actualData = mat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests that the sigmoid function is correctly applied to the matrix
    // i.e. the sigmoid function is carried out on each item in the matrix separately 
    public void Sigmoid()
    {
        var expectedData = new[,]
        {
            { 0.5f, 0.731058598f, 0.982013762f },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        var defaultMatrix = new Matrix(defaultData);
        defaultMatrix.Sigmoid();
        var actualData = defaultMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    // Tests element-wise multiplication is correctly carried out
    public void HadamardProd()
    {
        var expectedOutput = new[,]
        {
            { 3f, 2f, 25f },
            { 54f, 130f, 36f },
            { 16f, 352f, 132f }
        };

        var matContent1 = new[,]
        {
            { 1f, 0.5f, 5f },
            { 2f, 13f, 12f },
            { 4f, 22f, 11f }
        };

        var matContent2 = new[,]
        {
            { 3f, 4f, 5f },
            { 27f, 10f, 3f },
            { 4f, 16f, 12f }
        };

        var mat1 = new Matrix(matContent1);
        var mat2 = new Matrix(matContent2);
        var mat1x2 = Matrix.HadamardProd(mat1, mat2);
        var mat2x1 = Matrix.HadamardProd(mat2, mat1);

        Assert.Equal(expectedOutput, mat1x2.Contents); // Check if right answer obtained 
        Assert.Equal(expectedOutput, mat2x1.Contents); // Check if order matters
    }

    [Fact]
    // Tests the softmax activation function is correctly carried out on the matrix
    public void Softmax()
    {
        var data = new[,]
        {
            { 0.59474933f, 0.6371081f, 0.62671316f, 0.7615537f }
        };

        var expectedOutput = new[,]
        {
            { 0.23489222f, 0.2450557f, 0.24252155f, 0.27753058f }
        };

        var matrix = new Matrix(data);
        matrix.Softmax();
        var actualOutput = matrix.Contents;

        Assert.Equal(expectedOutput, actualOutput);
    }

    [Fact]
    // Tests logarithms are correctly performed on the matrix
    // i.e. the logarithm should be carried out on each element separately 
    public void Log()
    {
        var logBase = 5;

        var data = new[,]
        {
            { 2f, 5.5f },
            { 25f, 125.7556f }
        };
        var expectedOutput = new[,]
        {
            { 0.43067655f, 1.0592195f },
            { 2f, 3.003744539f }
        };

        var matrix = new Matrix(data);
        matrix.Log(logBase);
        var actualOutput = matrix.Contents;

        Assert.Equal(expectedOutput, actualOutput);
    }

    [Fact]
    // Tests that the matrix can be cloned properly 
    // i.e. the clone should not be affected when the original is changed 
    public void Clone()
    {
        var mat1 = new Matrix(defaultData);
        var mat2 = mat1.Clone();

        mat1.Add(mat2);

        Assert.False(mat1.Contents == mat2.Contents);
    }
}