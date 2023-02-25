using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Apollo.IO;
using Apollo.NeuralNet;
using Microsoft.Win32;
using Matrix = Apollo.MatrixMaths.Matrix;

namespace Apollo;

public partial class MainWindow : Window
{
    // Model for storing the settings in the network
    private StoredSettings Settings { get; set; }

    public ProfileManager ProfileManagement { get; set; }

    // Random instance used by the majority of the application 
    private Random R { get; }

    public MainWindow(string startingPage)
    {
        InitializeComponent();

        R = new Random();
        
        // Load settings
        if (File.Exists(StoredSettings.SettingsPath))
            LoadSettings();
        else
        {
            Settings = StoredSettings.Default();
            SaveSettings();
        }

        ProfileManagement = (Application.Current as App).ProfileManagement;
        
        // Initialise Neural Network
        InitialiseNetwork();

        // Open on the correct page
        CurrentlySelected = TrainButton;
        
        var buttonToHighlight = TrainButton; 
        switch (startingPage.ToLower())
        {
            case "train":
                buttonToHighlight = TrainButton;
                break;
            case "create":
                buttonToHighlight = CreateButton;
                break;
            case "listen":
                buttonToHighlight = ListenButton;
                break;
            case "settings":
                buttonToHighlight = SettingsButton;
                break;
        }
        ChangePage(buttonToHighlight);

        Mouse.OverrideCursor = null; // Change the mouse back to default
    }
    
    #region Initialising Neural Network

    // Names of the files to look for 
    private const int HIDDEN_SIZE = 32;
    private const int BATCH_SIZE = 4;
    
    private Rnn Network { get; set; }
    private Vocab VocabList { get; set; }
    private Matrix[][] TrainingData { get; set; }

    /// <summary>
    /// Initialise the neural network (and vocab list)
    /// </summary>
    private void InitialiseNetwork()
    {
        var profile = ProfileManagement.GetProfile(Settings.SelectedProfilePath);

        if (profile == null)
            throw new Exception("Selected profile is not found");

        // If one exists load it (check after state path first)
        if (File.Exists(profile.AfterStateFile))
        {
            Network = new Rnn(profile);
            Network.LoadState(profile.AfterStateFile);
            InitialiseVocab(profile);

        }
        else if (File.Exists(profile.BeforeStateFile))
        {
            Network = new Rnn(profile);
            Network.LoadState(profile.BeforeStateFile);
            InitialiseVocab(profile);
        }
        else
        {
            // If there isn't one then just start from scratch + display necessary message
            // But to start from scratch we must select training data for vocab 
            InitialiseVocab(profile);
            Network = new Rnn(profile, VocabList.Size, HIDDEN_SIZE, BATCH_SIZE, R);
        }
    }
    
    /// <summary>
    /// Initialise the vocab list 
    /// </summary>
    /// <param name="profile">The network profile constructed from schema.json</param>
    private void InitialiseVocab(Profile profile)
    {
        // If vocab was stored in schema.json, then use that to save time (since the vocab list should be the same across
        // sessions for a given profile) 
        if (profile.Vocab.Length > 0)
        {
            VocabList = new Vocab(profile.Vocab);
            return;
        }
        
        // If no vocab was stored, then build up the list from scratch
        VocabList = new Vocab();
        var stringReps = MidiManager.ReadDir(profile.TrainingDataDirectory);
        
        Parallel.ForEach(stringReps, stringRep =>
        {
            VocabList.AddCharacters(stringRep.ToCharArray());
        });
        
        // Save the vocab list so this does not have to be done again
        profile.Vocab = VocabList.AsString();
        SaveProfile(profile);
    }

    /// <summary>
    /// Prepare the training data for the network
    /// </summary>
    private void PrepareTrainingData()
    {
        // Get the string representation of the training data
        var profile = ProfileManagement.GetProfile(Settings.SelectedProfilePath);

        if (profile == null)
            throw new Exception("Selected profile does not exist");
        
        var midiStrings = MidiManager.ReadDir(profile.TrainingDataDirectory);

        TrainingData = new Matrix[midiStrings.Count][];

        // Convert the training data into one hot vectors
        // Parallel ok for this, since everything goes into a specific place
        Parallel.For(0, midiStrings.Count, i =>
        {
            var str = midiStrings[i];
            var vectorRep = VocabList.PrepareTrainingData(str);
            TrainingData[i] = vectorRep;
        });
    }
    
    #endregion

    #region Page Selection 
    
    private Button CurrentlySelected { get; set; }
    private readonly SolidColorBrush _selectedColour =
        new ((Color)ColorConverter.ConvertFromString("#f25c05"));
    private readonly SolidColorBrush _unselectedColour = 
        new ((Color)ColorConverter.ConvertFromString("#eaf205"));
    
    private void ChangePage(Button newSelected)
    {
        // Change window title
        Title = $"Apollo - {newSelected.Content}";
        
        // Highlight text
        CurrentlySelected.Foreground = _unselectedColour;
        newSelected.Foreground = _selectedColour;
        CurrentlySelected = newSelected;

        // Change page
        switch (newSelected.Name)
        {
            case "TrainButton":
                PageFrame.Source = new Uri("TrainingPage.xaml", UriKind.Relative);
                break;
            case "CreateButton":
                PageFrame.Source = new Uri("CreationPage.xaml", UriKind.Relative);
                break;
            case "ListenButton":
                PageFrame.Source = new Uri("ListenPage.xaml", UriKind.Relative);
                break;
            case "SettingsButton":
                PageFrame.Source = new Uri("SettingsPage.xaml", UriKind.Relative);
                break;
        }
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        var clickedButton = (Button)sender;
        ChangePage(clickedButton);
    }
    
    #endregion
    
    #region Settings and Profile Management

    private T ReadJson<T>(string path)
    {
        var str = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(str);
    }

    private void WriteJson<T>(T obj, string path)
    {
        var jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(path, jsonString);
    }
    
    private void LoadSettings()
    {
        Settings = ReadJson<StoredSettings>(StoredSettings.SettingsPath);
    }

    private void SaveSettings()
    {
        WriteJson(Settings, StoredSettings.SettingsPath);
    }

    private void SaveProfile(Profile profile)
    {
        var rootDir = Path.GetDirectoryName(profile.AfterStateFile);
        var schema = Path.Join(rootDir, "schema.json");
        WriteJson(profile, schema);
    }

    private void CreateDefaultProfile()
    {
        // Get the training files first in case none are provided, in which case the program can quit without having 
        // created or moved any files
        MessageBox.Show("No network profiles were found, creating a default network");
        MessageBox.Show("In order to create a network file, you must select the midi files to use as" +
                        " training data");
        var trainingFiles = GetTrainingFiles(); 
        
        // Create profile directory + schema.json 
        Directory.CreateDirectory(Path.Join(StoredSettings.ProfilesPath, "default"));
        var schemaPath = Path.Join(StoredSettings.ProfilesPath, "default", "schema.json");
        var defaultProfile = Profile.Default(Settings.SelectedProfilePath);

        // Create default training data directory + copy the provided training files
        Directory.CreateDirectory(defaultProfile.TrainingDataDirectory);

        foreach (var file in trainingFiles)
        {
            var currentPath = Path.GetFullPath(file);
            var name = Path.GetFileName(file);
            var newPath = Path.Join(defaultProfile.TrainingDataDirectory, name);
            
            File.Copy(currentPath, newPath);
        }

        WriteJson(defaultProfile, schemaPath);
    }

    /// <summary>
    /// Create an OpenFileDialog so the user can find the training files they want to use 
    /// </summary>
    private string[] GetTrainingFiles()
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Multiselect = true;
        fileDialog.Title = "Select multiple training files";
        fileDialog.Filter = "MIDI File|*.mid";

        if (fileDialog.ShowDialog() == true)
        {
            return fileDialog.FileNames;
        }
        
        throw new Exception("You cannot use the program without selecting the training files");
    }
    
    #endregion

    #region Page Accessable Methods

    public void StartTraining(int minEpochs, int maxEpochs, float maxError, int batchesPerEpoch)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        
        // Check that there is training data to use 
        // If there isn't any loaded training data then prepare some 
        if (TrainingData?.GetLength(0) == null) 
            PrepareTrainingData();
        
        // Rnn is trained on each individual file separately 
        foreach (var file in TrainingData)
        {
            Network.Train(file, minEpochs, maxEpochs, maxError, batchesPerEpoch, R);
        }

        Mouse.OverrideCursor = null;
        MessageBox.Show($"Training Complete.", "Apollo", MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    /// <summary>
    /// Create new seed for the first input into the network
    /// </summary>
    /// <returns>Returns a seed for music generation</returns>
    private Matrix CreateGenerationSeed()
    {
        var rows = new Matrix[Network.BatchSize];

        for (var i = 0; i < rows.Length; i++)
        {
            // Random => these characters will not actually be used in the final product
            // It is just to allow the network to start generating
            var charId = R.Next(VocabList.Size);
            rows[i] = VocabList.CreateOneHot(VocabList[charId]);
        }

        // Stack the matrices vertically into one matrix
        return Matrix.StackArray(rows);
    }

    /// <summary>
    /// Turns the network's outputs into a string
    /// </summary>
    /// <param name="outputs">List of outputs generated by the network</param>
    /// <returns>The string generated by the network</returns>
    private string InterpretNetworkOutput(Matrix[] outputs)
    {
        var stringOutput = "";

        foreach (var output in outputs)
        {
            // New character is the final row only. 
            var rowContents = new float [1, output.Columns]; // 2D used to turn into matrix later
            for (var j = 0; j < output.Columns; j++) rowContents[0, j] = output[output.Rows - 1, j];
            var mat = new Matrix(rowContents);
            stringOutput += VocabList.InterpretOneHot(mat);
        }

        return stringOutput;
    }
    
    public void StartCreating(int generationLength, int bpm, string savePath)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        
        // Create new seed, generate and interpret.
        var generationSeed = CreateGenerationSeed();
        var outputs = Network.Forward(generationSeed, generationLength);
        var stringOutput = InterpretNetworkOutput(outputs); 
        
        LogManager.WriteLine(stringOutput);
        
        // Save what was generated as a midi file 
        MidiManager.WriteFile(stringOutput, savePath, bpm);
    
        // Notify the user
        var fileName = Path.GetFileName(savePath);
        Mouse.OverrideCursor = null;
        MessageBox.Show($"Finished creating {fileName}", "Creation Complete", MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    #endregion
}