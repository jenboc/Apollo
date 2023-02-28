using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Apollo;

public partial class ListenPage : Page
{
    private Stack<string> LastPlayed { get; } // Stores last 15 songs that have been played
    private Queue<string> Playlist { get; } // Stores songs that are upcoming 
    private string? CurrentlyPlaying { get; set; } // Stores path to media which is currently playing 
    private DispatcherTimer? ProgressUpdateTimer { get; set; }
    
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
            return;
        }
        
        // If the next song label is already correct, then nothing needs to be changed
        if (Path.GetFileName(Playlist.Peek()) == NextSongLabel.Content.ToString())
            return;
        
        // Otherwise remove top child of stack panel and change next song label 
        PlaylistPanel.Children.RemoveAt(0);
        NextSongLabel.Content = Path.GetFileName(Playlist.Peek());
    }

    private void PlaySong(string songPath)
    {
        LastPlayed.Push(songPath);
        CurrentlyPlaying = songPath;
        
        // Set music player source to the stored path
        MusicPlayer.Source = new Uri(songPath);
        MusicPlayer.Play();

        // Ensure play/pause button says pause
        PlayPauseButton.Content = "Pause";
    }

    /// <summary>
    /// Begin playing the next song 3
    /// </summary>
    private void PlayNextSong()
    {
        // Return if there is nothing to play
        if (Playlist.IsEmpty())
            return;

        var nextSong = Playlist.Dequeue();
        PlaySong(nextSong);
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
        PlaySong(song);
        // Do not need to update queue UI as nothing was taken from the queue
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
        MusicPlayer.Close();

        CurrentlyPlaying = null;
        CurrentSongLabel.Content = "";
        
        // Wipe playlist 
        Playlist.Clear(); 
        UpdateQueueUI();
    }
    
    /// <summary>
    /// Event which is called when the fast forward 5 seconds button is pressed 
    /// </summary>
    private void OnSkip5ButtonPress(object sender, RoutedEventArgs e)
    {
        if (!MusicPlayer.NaturalDuration.HasTimeSpan)
            return;

        var secondsUntilEnd = MusicPlayer.NaturalDuration.TimeSpan.Subtract(MusicPlayer.Position).TotalSeconds;

        if (secondsUntilEnd > 5)
        {
            var timespanChange = TimeSpan.FromSeconds(5);
            var newPos = timespanChange.Add(MusicPlayer.Position);
            MusicPlayer.Position = newPos;
        }
        else
        {
            PlayNextSong();
            UpdateQueueUI();
        }
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
        CurrentSongLabel.Content = Path.GetFileName(CurrentlyPlaying);
        
        // Set up maximum 
        MediaProgressBar.Maximum = MusicPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        
        // Set up timer 
        ProgressUpdateTimer = new DispatcherTimer();
        ProgressUpdateTimer.Interval = TimeSpan.FromSeconds(1);
        ProgressUpdateTimer.Tick += new EventHandler(UpdateBarEvent);
        ProgressUpdateTimer.Start();
    }

    /// <summary>
    /// Event which is called when the MediaElement has finished playing a piece of media
    /// </summary>
    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        CurrentSongLabel.Content = "";
        CurrentlyPlaying = null;
        
        // Start playing next song and update the UI 
        PlayNextSong();
        UpdateQueueUI();
    }

    /// <summary>
    /// Event which is called by the DispatcherTimer to update the progress bar
    /// </summary>
    private void UpdateBarEvent(object? sender, EventArgs e)
    {
        if (CurrentlyPlaying == null)
        {
            ProgressUpdateTimer.Stop();
            return;
        }

        MediaProgressBar.Value = MusicPlayer.Position.TotalMilliseconds;
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
        }
        
        // Update the UI in case it needs updating
        UpdateQueueUI();
    }
    #endregion
}