using Apollo;

namespace Apollo.Tests;

public class StackTests
{
    // Helper function to create stack to use for testing
    private Stack<int> MakeStack(int maxItems)
    {
        // Stack will contain incrementing numbers 
        var stack = new Stack<int>(maxItems);

        for (var i = 0; i < maxItems; i++)
            stack.Push(i);

        return stack;
    }

    [Fact]
    // Test items can be pushed to the stack
    public void Push()
    {
        var stack = new Stack<int>(3);

        stack.Push(1);
        Assert.Equal(1, stack.Peek());

        stack.Push(2);
        Assert.Equal(2, stack.Peek());

        stack.Push(3);
        Assert.Equal(3, stack.Peek());
    }

    [Fact]
    // Test the top item is returned by peek
    public void Peek()
    {
        var stack = MakeStack(5);
        var expected = 4;
        Assert.Equal(expected, stack.Peek());
    }
    
    [Fact]
    // Test items can be removed from the stack
    public void Pop()
    {
        var stack = MakeStack(5);
        var expected = 4;
        Assert.Equal(expected, stack.Pop());
        Assert.NotEqual(expected, stack.Peek());
    }

    [Fact]
    // Check that the stack correctly identifies if it is empty 
    public void IsEmpty()
    {
        var stack = new Stack<int>(2);
        Assert.True(stack.IsEmpty());
        
        stack = MakeStack(5);
        Assert.False(stack.IsEmpty());
    }

    [Fact]
    // Check that the contents of the stack can be cleared
    public void Clear()
    {
        var stack = MakeStack(5); 
        Assert.False(stack.IsEmpty());
        stack.Clear();
        Assert.True(stack.IsEmpty());
    }

    [Fact]
    // Test that only a certain amount of items can be added to the stack
    public void TestLimit()
    {
        var stack = MakeStack(10);

        for (var i = 0; i < 10; i++)
            stack.Push(i);
        
        stack.Push(10);
        
        Assert.NotEqual(10, stack.Peek());
    }
}