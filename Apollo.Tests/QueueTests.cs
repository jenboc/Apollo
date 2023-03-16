namespace Apollo.Tests;

public class QueueTests
{
    // Helper subroutine to create queue for testing 
    public Queue<int> MakeQueue(int numItems)
    {
        var queue = new Queue<int>();
        
        for (var i = 0; i < numItems; i++) 
            queue.Enqueue(i);

        return queue;
    }

    [Fact]
    // Test that items can be added to the queue
    public void Enqueue()
    {
        var queue = new Queue<int>();
        
        queue.Enqueue(1);
        queue.Enqueue(2); 
        queue.Enqueue(3);

        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
    }

    [Fact]
    // Test that the next item to be dequeued is returned from peek
    public void Peek()
    {
        var queue = MakeQueue(5);
        
        Assert.Equal(0, queue.Peek());
        queue.Dequeue();
        Assert.Equal(1, queue.Peek());
        queue.Dequeue();
        Assert.Equal(2, queue.Peek());
    }

    [Fact]
    // Test that the first item to be added is returned from dequeue
    public void Dequeue()
    {
        var queue = MakeQueue(5);
        
        Assert.Equal(0, queue.Dequeue());
        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
        Assert.Equal(4, queue.Dequeue());
    }

    [Fact]
    // Test that queue correctly identifies whether or not it is empty 
    public void IsEmpty()
    {
        var queue = new Queue<int>();
        Assert.True(queue.IsEmpty());
        queue = MakeQueue(5);
        Assert.False(queue.IsEmpty());
    }

    [Fact]
    // Test that the queue is correctly cleared 
    public void Clear()
    {
        var queue = MakeQueue(5);
        Assert.False(queue.IsEmpty());
        queue.Clear();
        Assert.True(queue.IsEmpty());
    }
}