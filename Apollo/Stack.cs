using System.Collections.Generic;

namespace Apollo;

internal class Stack<T>
{
    private readonly List<T> _contents;
    private readonly int _maxItems;
    private int _pointer;

    public Stack(int maxItems)
    {
        _contents = new List<T>();
        _pointer = -1;
        _maxItems = maxItems;
    }

    public void Push(T item)
    {
        _contents.Add(item);
        _pointer++;

        // If there is more than the maximum amount of items in the stack, remove the last item in the stack
        if (_contents.Count > _maxItems) _contents.RemoveAt(0);
    }

    public T Peek()
    {
        if (IsEmpty())
            return default;

        return _contents[_pointer];
    }

    public T Pop()
    {
        if (IsEmpty())
            return default;

        var popped = Peek();
        _contents.RemoveAt(_pointer);
        _pointer--;
        return popped;
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