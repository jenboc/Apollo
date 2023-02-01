using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Apollo.IO;
using Apollo.NeuralNet;
using Matrix = Apollo.MatrixMaths.Matrix;

namespace Apollo;

public partial class MainWindow : Window
{
    private StoredSettings Settings { get; set; }
    private Dictionary<string, Profile> Profiles { get; set; }
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

        // Init Profiles dictionary
        LoadProfiles(); 
        
        // Initialise Neural Network
        InitialiseNetwork();
        
        // Prepare Training Data 
        PrepareTrainingData();

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
        var profile = Profiles[Settings.SelectedProfilePath];
        
        // If one exists load it (check after state path first)
        if (File.Exists(profile.AfterStateFile))
        {
            Network = new Rnn();
            Network.LoadState(profile.AfterStateFile);
            InitialiseVocab(profile);
            return;
        }
        if (File.Exists(profile.BeforeStateFile))
        {
            Network = new Rnn();
            Network.LoadState(profile.BeforeStateFile);
            InitialiseVocab(profile);
            return; 
        }

        // If there isn't one then just start from scratch + display necessary message
        // But to start from scratch we must select training data for vocab 
        InitialiseVocab(profile);
        Network = new Rnn(profile, VocabList.Size, HIDDEN_SIZE, BATCH_SIZE, R);
    }
    
    /// <summary>
    /// Initialise the vocab list 
    /// </summary>
    /// <param name="profile">The network profile constructed from schema.json</param>
    private void InitialiseVocab(Profile profile)
    {
        VocabList = new Vocab();
        if (profile.Vocab.Length > 0)
        {
            VocabList.AddCharacters(profile.Vocab.ToCharArray());
            return;
        }

        var stringReps = MidiManager.ReadDir(profile.TrainingDataDirectory);
        
        Parallel.ForEach(stringReps, stringRep =>
        {
            VocabList.AddCharacters(stringRep.ToCharArray());
        });

        profile.Vocab = VocabList.AsString();
        SaveProfile(profile);
    }

    private void PrepareTrainingData()
    {
        var profile = Profiles[Settings.SelectedProfilePath];
        var midiStrings = MidiManager.ReadDir(profile.TrainingDataDirectory);

        TrainingData = new Matrix[midiStrings.Count][];

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

    private void LoadProfiles()
    {
        Profiles = new Dictionary<string, Profile>();
        var profileDirectories = Directory.GetDirectories(StoredSettings.ProfilesPath);

        if (profileDirectories.Length == 0)
            CreateDefaultProfile(); 
        
        // Check for valid profiles (profiles with a schema.json file) 
        foreach (var dir in profileDirectories)
        {
            var schemaPath = Path.Join(dir, "schema.json");

            if (!File.Exists(schemaPath)) // Continue if invalid 
                continue;
            
            // Read the schema + add to dictionary
            var profile = ReadJson<Profile>(schemaPath);
            LogManager.WriteLine(profile.TrainingDataDirectory);
            Profiles.Add(dir, profile);
        }
    }

    private void SaveProfile(Profile profile)
    {
        var rootDir = Path.GetDirectoryName(profile.AfterStateFile);
        var schema = Path.Join(rootDir, "schema.json");
        WriteJson(profile, schema);
    }

    private void CreateDefaultProfile()
    {
        Directory.CreateDirectory(Path.Join(StoredSettings.ProfilesPath, "default"));
        var schemaPath = Path.Join(StoredSettings.ProfilesPath, "default", "schema.json");
        var defaultProfile = Profile.Default(Settings.SelectedProfilePath); 
        
        WriteJson(defaultProfile, schemaPath);
    }
    
    #endregion

    #region Page Accessable Methods

    public void StartTraining(int minEpochs, int maxEpochs, float maxError, int batchesPerEpoch)
    {
        // Rnn is trained on each individual file separately 
        foreach (var file in TrainingData)
        {
            Network.Train(file, minEpochs, maxEpochs, maxError, batchesPerEpoch, R);
        }
    }

    #endregion
}