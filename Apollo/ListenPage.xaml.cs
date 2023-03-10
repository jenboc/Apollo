using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Apollo;

public partial class ListenPage : Page
{
    public ListenPage()
    {
        InitializeComponent();
        LastPlayed = new Stack<string>(15);
        Playlist = new Queue<string>();
        CurrentlyPlaying = null;
    }

    private Stack<string> LastPlayed { get; } // Stores last 15 songs that have been played
    private Queue<string> Playlist { get; } // Stores songs that are upcoming 
    private string? CurrentlyPlaying { get; set; } // Stores path to media which is currently playing 
    private DispatcherTimer? ProgressUpdateTimer { get; set; }

    private const string PLAY_IMAGE = "Images/Play.png";
    private const string PAUSE_IMAGE = "Images/Pause.png";

    /// <summary>
    ///     Update the UI elements associated with the playlist queue
    ///     (The Playlist Stack Panel and the Next Song Label)
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

        // Nothing to update if the length of the queue at least 1 more than the number of children
        if (Playlist.Length >= PlaylistPanel.Children.Count + 1)
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
        PlayPauseImage.Source = new BitmapImage(new Uri(PAUSE_IMAGE, UriKind.Relative));
    }

    /// <summary>
    ///     Begin playing the next song 3
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
    ///     Adds a song to the stack panel
    /// </summary>
    private void AddSongToStackPanel(string filePath)
    {
        // Create label to add to stack panel
        var label = new Label();
        label.Content = Path.GetFileName(filePath);
        label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eaf205"));
        label.FontFamily = new FontFamily("file:///Fonts/Roboto");
        label.FontWeight = FontWeights.Bold;
        label.FontSize = 25;
        label.HorizontalAlignment = HorizontalAlignment.Left;

        // Add label to stack panel
        PlaylistPanel.Children.Add(label);
    }

    /// <summary>
    ///     Opens a File Dialog for the user to select a file to play
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

    #region Playlist Events

    /// <summary>
    ///     Event which is called when the "Add to Playlist" button is pressed
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

    #region Playback Button Events

    /// <summary>
    ///     Event which is called when previous song button is pressed
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
    ///     Event which is called when rewind 5 seconds button is pressed
    /// </summary>
    private void OnBack5ButtonPress(object sender, RoutedEventArgs e)
    {
        var timespanChange = TimeSpan.FromSeconds(-5);
        var newPos = timespanChange.Add(MusicPlayer.Position);
        MusicPlayer.Position = newPos;
    }

    /// <summary>
    ///     Event which is called when the play/pause button is pressed
    /// </summary>
    private void OnPlayPauseButtonPress(object sender, RoutedEventArgs e)
    {
        // Do not handle event if nothing is playing
        if (string.IsNullOrEmpty(CurrentlyPlaying))
            return;

        var displayedImageUri = (PlayPauseImage.Source as BitmapImage).UriSource.OriginalString;
        Console.WriteLine(displayedImageUri);

        // If the button says play, play
        if (displayedImageUri == PLAY_IMAGE)
        {
            PlayPauseImage.Source = new BitmapImage(new Uri(PAUSE_IMAGE, UriKind.Relative));
            MusicPlayer.Play();
        }
        // Otherwise pause
        else
        {
            PlayPauseImage.Source = new BitmapImage(new Uri(PLAY_IMAGE, UriKind.Relative));
            MusicPlayer.Pause();
        }
    }

    /// <summary>
    ///     Event which is called when stop playback button is pressed
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
    ///     Event which is called when the fast forward 5 seconds button is pressed
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
    ///     Event which is called when the next song button is pressed
    /// </summary>
    private void OnNextButtonPress(object sender, RoutedEventArgs e)
    {
        MusicPlayer.Stop();
        PlayNextSong();

        if (Playlist.IsEmpty())
            CurrentSongLabel.Content = "";

        UpdateQueueUI();
    }

    #endregion

    #region MediaElement Events

    /// <summary>
    ///     Event which is called when the MediaElement has opened and loaded a piece of media
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
        ProgressUpdateTimer.Tick += UpdateBarEvent;
        ProgressUpdateTimer.Start();
    }

    /// <summary>
    ///     Event which is called when the MediaElement has finished playing a piece of media
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
    ///     Event which is called by the DispatcherTimer to update the progress bar
    /// </summary>
    private void UpdateBarEvent(object? sender, EventArgs e)
    {
        if (CurrentlyPlaying == null)
        {
            ProgressUpdateTimer.Stop();
            MediaProgressBar.Value = 0;
            return;
        }

        MediaProgressBar.Value = MusicPlayer.Position.TotalMilliseconds;
    }

    #endregion
}