namespace Apollo.MatrixMaths.Tests;

public class MatrixTests
{
    float[,] defaultData = new float[,]
    {
        { 0, 1, 4 },
        { 9, 18, 99 },
        { 1000, 2500, 1990 }
    };

    float[,] defaultData2 = new float[,]
    {
        { 24, 3, 12 },
        { 1, 0, -5 },
        { -25, 1, 22},
    };

    [Fact]
    public void Power()
    {
        float[,] expectedData = new float[,]
        {
            { 0, 1, 16 },
            { 81, 324, 9801 },
            { 1000000, 6250000, 3960100 }
        };

        var actualMatrix = new Matrix(defaultData);
        actualMatrix.Power(2);
        float[,] actualData = actualMatrix.Contents;
                        
        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void Sqrt()
    {
        var expectedData = new float[,]
        {
            { 0, 1, 2 },
            { 3, 4.2426406871192848f, 9.9498743710662f },
            { 31.622776601683793f, 50, 44.609416046390926f }
        };

        Matrix actualMatrix = new Matrix(defaultData);
        actualMatrix.Sqrt();
        float[,] actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void Exp()
    {
        var expectedData = new float[,]
        {
            { 1, 2.71828175f, 54.5981483f },
            { 8103.08398f, 65659968, float.PositiveInfinity },
            { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity}
        };

        Matrix actualMatrix = new Matrix(defaultData);
        actualMatrix.Exp();
        float[,] actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void Tanh()
    {
        var expectedData = new float[,]
        {
            { 0, 0.761594176f, 0.999329329f },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        Matrix actualMatrix = new Matrix(defaultData);
        actualMatrix.Tanh();
        float[,] actualData = actualMatrix.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void MatMul()
    {
        var expectedData = new float[,]
        {
            { -99, 4, 83},
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
    public void Sum()
    {
        var expectedData = 5621.0;

        var defaultMat = new Matrix(defaultData);
        var actualData = defaultMat.Sum();

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void Ravel()
    {
        var expectedData = new float[,]
        {
            { 0, 1, 4, 9, 18, 99, 1000, 2500, 1990 }
        };

        var defaultMat = new Matrix(defaultData);
        defaultMat.Ravel();
        var actualData = defaultMat.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
    public void Reshape()
    {
        MatShape targetShape = new(9, 1);
        var expectedData = new float[,]
        {
            { 0 },
            { 1 },
            { 4 },
            { 9 },
            { 18 },
            { 99 },
            { 1000 },
            { 2500 },
            { 1990 }
        };

        var defaultMat = new Matrix(defaultData);
        defaultMat.Reshape(targetShape);
        var actualData = defaultMat.Contents;

        Assert.Equal(expectedData, actualData);
    }

    [Fact]
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
     public void VerticalStack() 
     {
        var expectedData = new float[,]
        {
            { 0, 1, 4 },
            { 9, 18, 99 },
            { 1000, 2500, 1990 },
            { 24, 3, 12 },
            { 1, 0, -5 },
            { -25, 1, 22},
        };

        var defaultMat1 = new Matrix(defaultData);
        var defaultMat2 = new Matrix(defaultData2);
        defaultMat1.VerticalStack(defaultMat2);
        var actualData = defaultMat1.Contents;

        Assert.Equal(expectedData, actualData);
    }

     [Fact] 
     public void Random()
     {
        var seeded1 = Matrix.Random(5, 5, 2);
        var seeded2 = Matrix.Random(5, 5, 2);

         var unseeded = Matrix.Random(5, 5);

         Assert.Equal(seeded1.Contents, seeded2.Contents);
         Assert.NotEqual(seeded1.Contents, unseeded.Contents);
    }

     [Fact]
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
     public void Slicing()
     {
        var expectedData = new float[,]
        {
            { 0, 1 },
            { 9, 18 }
        };
        var slice = ":2,:2";

        var defaultMatrix = new Matrix(defaultData);
        var slicedMat = defaultMatrix[slice];

        var actualData = slicedMat.Contents;

        Assert.Equal(expectedData, actualData);
     }

     [Fact]
     public void Sigmoid()
     {
        var expectedData = new float[,]
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
}