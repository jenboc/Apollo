using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using Microsoft.Win32;

namespace Apollo;

public partial class ListenPage : Page
{
    private Stack<string> LastPlayed { get; } // Stores last 15 songs that have been played
    private Queue<string> Playlist { get; } // Stores songs that are upcoming 
    private string? CurrentlyPlaying { get; set; } // Stores path to media which is currently playing 
    
    public ListenPage()
    {
        InitializeComponent();
        LastPlayed = new Stack<string>(15);
        Playlist = new Queue<string>();
        CurrentlyPlaying = null;
    }

    /// <summary>
    /// Update the UI elements associated with the playlist queue
    /// (The Playlist Stack Panel and the Next Song Label)
    /// </summary>
    private void UpdateQueueUI()
    {
        // If there is nothing in the playlist then wipe the UI and display appropriate message
        if (Playlist.IsEmpty())
        {
            NextSongLabel.Content = "No song in playlist";
            PlaylistPanel.Children.Clear();
        }
        
        // If the next song label is already correct, then nothing needs to be changed
        if (Path.GetFileName(Playlist.Peek()) == NextSongLabel.Content.ToString())
            return;
        
        // Otherwise remove top child of stack panel and change next song label 
        PlaylistPanel.Children.RemoveAt(0);
        NextSongLabel.Content = Path.GetFileName(Playlist.Peek());
    }

    /// <summary>
    /// Begin playing the next song 
    /// </summary>
    private void PlayNextSong()
    {
        // Return if there is nothing to play
        if (Playlist.IsEmpty())
            return;
        
        // Set music player source to the stored path
        var nextSong = Playlist.Dequeue();
        MusicPlayer.Source = new Uri(nextSong);
        MusicPlayer.Play();
        CurrentSongLabel.Content = Path.GetFileName(nextSong);
        
        // Ensure play/pause button says pause
        PlayPauseButton.Content = "Pause";
    }

    /// <summary>
    /// Adds a song to the stack panel 
    /// </summary>
    private void AddSongToStackPanel(string filePath)
    {
        // Create label to add to stack panel
        var label = new Label();
        label.Content = Path.GetFileName(filePath);
        // label.Foreground = yellow 
        // label.FontFamily = Roboto
        label.FontWeight = FontWeights.Bold;
        label.FontSize = 25;
        label.HorizontalAlignment = HorizontalAlignment.Left;

        // Add label to stack panel
        PlaylistPanel.Children.Add(label);
    }

    /// <summary>
    /// Opens a File Dialog for the user to select a file to play
    /// </summary>
    /// <returns>The path to the file to play</returns>
    private string GetFile()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "MIDI File|*.mid;*.midi";
        dialog.Title = "Select a file to add to the playlist";

        if (dialog.ShowDialog() == true)
            return dialog.FileName;

        return string.Empty;
    }
    
    #region Playback Button Events

    /// <summary>
    /// Event which is called when previous song button is pressed
    /// </summary>
    private void OnPreviousButtonPress(object sender, RoutedEventArgs e)
    {
        if (LastPlayed.IsEmpty())
        {
            MessageBox.Show("You haven't played any songs recently");
            return;
        }

        MusicPlayer.Stop();
        
        var song = LastPlayed.Pop();
        MusicPlayer.Source = new Uri(song);
        MusicPlayer.Play();
        CurrentSongLabel.Content = Path.GetFileName(song);
        PlayPauseButton.Content = "Pause";
        
        // Do not need to update queue UI as nothing was taken from there
    }
    
    /// <summary>
    /// Event which is called when rewind 5 seconds button is pressed
    /// </summary>
    private void OnBack5ButtonPress(object sender, RoutedEventArgs e)
    {
        var timespanChange = TimeSpan.FromSeconds(-5);
        var newPos = timespanChange.Add(MusicPlayer.Position);
        MusicPlayer.Position = newPos;
    }
    
    /// <summary>
    /// Event which is called when the play/pause button is pressed
    /// </summary>
    private void OnPlayPauseButtonPress(object sender, RoutedEventArgs e)
    {
        // Do not handle event if nothing is playing
        if (string.IsNullOrEmpty(CurrentlyPlaying))
            return;
        
        var button = (Button)sender;
        
        // If the button says play, play
        if (button.Content.ToString() == "Play")
        {
            button.Content = "Pause";
            MusicPlayer.Play();
        }
        // Otherwise pause
        else
        {
            button.Content = "Play";
            MusicPlayer.Pause();
        }
    }
    
    /// <summary>
    /// Event which is called when stop playback button is pressed
    /// </summary>
    private void OnStopButtonPress(object sender, RoutedEventArgs e)
    {
        // Stop playback
        MusicPlayer.Stop(); 
        
        // Wipe playlist 
        Playlist.Clear(); 
        UpdateQueueUI();
    }
    
    /// <summary>
    /// Event which is called when the fast forward 5 seconds button is pressed 
    /// </summary>
    private void OnSkip5ButtonPress(object sender, RoutedEventArgs e)
    {
        var timespanChange = TimeSpan.FromSeconds(5);
        var newPos = timespanChange.Add(MusicPlayer.Position);
        MusicPlayer.Position = newPos;
    }
    
    /// <summary>
    /// Event which is called when the next song button is pressed
    /// </summary>
    private void OnNextButtonPress(object sender, RoutedEventArgs e)
    {
        MusicPlayer.Stop();
        PlayNextSong();
        UpdateQueueUI();
    }
    #endregion

    #region MediaElement Events
    /// <summary>
    /// Event which is called when the MediaElement has opened and loaded a piece of media
    /// </summary>
    private void OnMediaOpened(object sender, RoutedEventArgs e)
    {
        // Update currently playing variable and label
        CurrentlyPlaying = MusicPlayer.Source.ToString();
        CurrentSongLabel.Content = CurrentlyPlaying;
    }

    /// <summary>
    /// Event which is called when the MediaElement has finished playing a piece of media
    /// </summary>
    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        // Push what was played to the last played stack
        LastPlayed.Push(CurrentlyPlaying);
        
        // Start playing next song and update the UI 
        PlayNextSong();
        UpdateQueueUI();
    }
    #endregion
    
    #region Playlist Events

    /// <summary>
    /// Event which is called when the "Add to Playlist" button is pressed
    /// </summary>
    private void OnAddButtonPress(object sender, RoutedEventArgs e)
    {
        var filePath = GetFile();

        // Do not continue if no file is provided
        if (string.IsNullOrEmpty(filePath))
            return; 
        
        // Add to Playlist Queue 
        Playlist.Enqueue(filePath);
        
        // Add file name to stack panel
        AddSongToStackPanel(filePath);
        
        // Play if nothing is playing 
        if (string.IsNullOrEmpty(CurrentlyPlaying))
        {
            PlayNextSong();
            UpdateQueueUI();
        }
    }
    #endregion
}

internal class Stack<T>
{
    private List<T> _contents;
    private int _pointer;
    private int _maxItems;

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
        if (_contents.Count > _maxItems)
        {
            _contents.RemoveAt(0);
        }
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