using System.Text;
using Apollo.IO;
using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
/// Wrapper class containing all necessary objects and methods for high level control of RNN 
/// </summary>
public class NeuralNetwork
{
    private Rnn Network { get; set; }
    public Vocab VocabList { get; set; }
    private Matrix[][]? TrainingData { get; set; }
    private int HiddenSize { get; }
    private int BatchSize { get; }
    private Random R { get; }
    public Profile CurrentProfile { get; private set; }
    

    public NeuralNetwork(Profile initialProfile, int hiddenSize, int batchSize, Random r)
    {
        HiddenSize = hiddenSize;
        BatchSize = batchSize;
        R = r;
        TrainingData = null;
        ChangeProfile(initialProfile);
        PopulateVocabList();
    }

    private void PopulateTrainingArray()
    {
        var midiStrings = MidiManager.ReadDir(CurrentProfile.TrainingDataDirectory);
        TrainingData = new Matrix[midiStrings.Count][];

        Parallel.For(0, midiStrings.Count, i =>
        {
            TrainingData[i] = VocabList.PrepareTrainingData(midiStrings[i]);
        });
    }

    private void PopulateVocabList()
    {
        if (CurrentProfile.Vocab.Length > 0)
        {
            VocabList = new Vocab(CurrentProfile.Vocab);
            return;
        }

        VocabList = new Vocab();
        var stringReps = MidiManager.ReadDir(CurrentProfile.TrainingDataDirectory);

        Parallel.ForEach(stringReps, stringRep =>
        {
            VocabList.AddCharacters(stringRep.ToCharArray());
        });

        CurrentProfile.Vocab = VocabList.AsString(); 
    }
    
    /// <summary>
    /// Train the network
    /// </summary>
    public void Train(int minEpochs, int maxEpochs, float maxError, int batchesPerEpoch)
    {
        if (TrainingData == null) 
            PopulateTrainingArray();

        foreach (var vectorisedFile in TrainingData)
        {
            Network.Train(vectorisedFile, minEpochs, maxEpochs, maxError, batchesPerEpoch,
                CurrentProfile.BeforeStateFile, CurrentProfile.AfterStateFile, R);
        }
    }

    private Matrix CreateGenerationSeed()
    {
        var rows = new Matrix[Network.BatchSize];

        for (var i = 0; i < rows.Length; i++)
        {
            var charId = R.Next(VocabList.Size);
            rows[i] = VocabList.CreateOneHot(VocabList[charId]);
        }

        return Matrix.StackArray(rows);
    }

    private string InterpretVectorOutput(Matrix[] outputs)
    {
        var stringOutput = new StringBuilder();

        foreach (var output in outputs)
        {
            var rowContents = new float[1, output.Columns];
            for (var j = 0; j < output.Columns; j++) rowContents[0, j] = output[output.Rows - 1, j];
            var mat = new Matrix(rowContents);
            stringOutput.Append(VocabList.InterpretOneHot(mat)); 
        }

        return stringOutput.ToString();
    }
    
    /// <summary>
    /// Use the network to generate, outputting the created data to a MIDI file  
    /// </summary>
    public void Generate(int genLength, int bpm, string savePath)
    {
        var logBuffer = $"Generating with:\nGeneration Length: {genLength}\nBPM: {bpm}\nSave Path: {savePath}";
        var seed = CreateGenerationSeed();
        var networkOutputs = Network.Forward(seed, genLength);
        var stringOutput = InterpretVectorOutput(networkOutputs);
        logBuffer += $"\nGenerated Text:\n{stringOutput}";
        LogManager.WriteLine(logBuffer);
        MidiManager.WriteFile(stringOutput, savePath, bpm);
    }
    
    /// <summary>
    /// Change the state profile that the network is using
    /// </summary>
    /// <param name="profile">The profile to change to</param>
    public void ChangeProfile(Profile profile)
    {
        CurrentProfile = profile;
        PopulateVocabList();
        TrainingData = null;

        Network = File.Exists(CurrentProfile.AfterStateFile)
            ? new Rnn(CurrentProfile.AfterStateFile)
            : new Rnn(VocabList.Size, HiddenSize, BatchSize, R);
    }
}