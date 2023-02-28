using System.Collections.Generic;

namespace Apollo;

internal class Queue<T>
{
    private List<T> _contents;

    public Queue()
    {
        _contents = new List<T>();
    }

    public void Enqueue(T item)
    {
        _contents.Add(item);
    }

    public T Peek()
    {
        if (IsEmpty())
            return default;

        return _contents[0];
    }

    public T Dequeue()
    {
        if (IsEmpty())
            return default;

        var dequeued = Peek();
        _contents.RemoveAt(0);
        return dequeued;
    }

    public bool IsEmpty()
    {
        return _contents.Count == 0;
    }

    public void Clear()
    {
        _contents.Clear();
    }
}