using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace Apollo;

public partial class ListenPage : Page
{
    private Queue<string> Playlist { get; set; }
    private SoundPlayer Player { get; set; }
    
    public ListenPage()
    {
        InitializeComponent();
        Playlist = new Queue<string>();
    }

    /// <summary>
    /// Checks if given path is valid (i.e. references a MIDI file)
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>Boolean depicting whether the path is true or not</returns>
    private bool IsValidPath(string path)
    {
        // The path must end with .mid or .midi
        // The path must actually exist
        return (path.EndsWith(".mid") || path.EndsWith(".midi")) && File.Exists(path); 
    }
    
    private void AddButtonPressed(object sender, RoutedEventArgs e)
    {
        var path = PathBox.Text;

        if (!IsValidPath(path))
        {
            MessageBox.Show("Entered path is not valid");
            return; 
        }
        
        Playlist.Enqueue(path);

        if (Playlist.Count == 1)
            ChangeSong(); 
    }

    private void ChangeSong()
    {
        string toPlay;
        var success = Playlist.TryDequeue(out toPlay);
        
        // Success is false if the queue is empty 
        if (!success)
        {
            MessageBox.Show("Playlist has finished");
            return;
        }
        
        // Check that the file still exists before attempting to play it 
        if (!IsValidPath(toPlay))
        {
            MessageBox.Show($"{toPlay} no longer exists");
            ChangeSong();
            return; 
        }
        
        // Play the song
        Player = new SoundPlayer(toPlay);
        Player.Play();
    }
}