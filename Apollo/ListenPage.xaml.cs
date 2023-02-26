using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace Apollo;

public partial class ListenPage : Page
{
    private Stack<string> LastPlayed { get; } // Stores songs that have been played
    private Queue<string> Playlist { get; } // Stores songs that are upcoming 
    private string? CurrentlyPlaying { get; set; } // Stores path to media which is currently playing 
    
    public ListenPage()
    {
        InitializeComponent();
        LastPlayed = new Stack<string>();
        Playlist = new Queue<string>();
        CurrentlyPlaying = null;
    }

    #region MediaElement Events
    /// <summary>
    /// Event which is called when the MediaElement has opened and loaded a piece of media
    /// </summary>
    private void OnMediaOpened(object sender, RoutedEventArgs e)
    {
        
    }

    /// <summary>
    /// Event which is called when the MediaElement has finished playing a piece of media
    /// </summary>
    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        // Push what was played to the last played stack
        LastPlayed.Push(CurrentlyPlaying);
        
        // Update currently playing from queue
    }
    #endregion
}

internal class Stack<T>
{
    private List<T> _contents;
    private int _pointer; 

    public Stack()
    {
        _contents = new List<T>();
        _pointer = -1;
    }

    public void Push(T item)
    {
        _contents.Add(item);
        _pointer++;
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
}

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
}