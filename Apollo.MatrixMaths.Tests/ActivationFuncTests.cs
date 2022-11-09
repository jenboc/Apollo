namespace Apollo.MatrixMaths.Tests;

public class ActivationFuncTests
{
    private readonly float _testData = 3;
    private readonly float _tanhBoundary = 4.0001f;
    private readonly float _sigmoidBoundary = 6.0001f;
    
    
    [Fact]
    public void Tanh()
    {
        var expectedData = 0.9950547537f; // Expected data for "testData"
        var actualData = ActivationFuncs.Tanh(_testData); // Actual data for "testData"
        Assert.Equal(expectedData, actualData);
        
        // Boundary tests 
        Assert.Equal(1, ActivationFuncs.Tanh(_tanhBoundary));
        Assert.Equal(-1, ActivationFuncs.Tanh(-_tanhBoundary));
        
    }

    [Fact]
    public void DTanh()
    {
        var expectedData = 0.009866037165f;
        var actualData = ActivationFuncs.DTanh(_testData);
        Assert.Equal(expectedData, actualData);
        
        // Boundary tests 
        Assert.Equal(0, ActivationFuncs.DTanh(_tanhBoundary));
        Assert.Equal(0, ActivationFuncs.DTanh(-_tanhBoundary));
    }

    [Fact]
    public void Sigmoid()
    {
        var expectedData = 0.9525741268f;
        var actualData = ActivationFuncs.Sigmoid(_testData);
        Assert.Equal(expectedData, actualData);
        
        // Boundary Tests 
        Assert.Equal(1, ActivationFuncs.Sigmoid(_sigmoidBoundary));
        Assert.Equal(0 , ActivationFuncs.Sigmoid(-_sigmoidBoundary));
    }

    [Fact]
    public void DSigmoid()
    {
        var expectedData = 0.045176655f;
        var actualData = ActivationFuncs.DSigmoid(_testData);
        Assert.Equal(expectedData, actualData);
        
        // Boundary Tests 
        Assert.Equal(0, ActivationFuncs.DSigmoid(_sigmoidBoundary));
        Assert.Equal(0 , ActivationFuncs.DSigmoid(-_sigmoidBoundary));
    }
}