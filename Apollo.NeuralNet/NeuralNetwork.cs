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
    private Matrix[][] TrainingData { get; set; }
    private int HiddenSize { get; }
    private int BatchSize { get; }

    public Profile CurrentProfile => Network.StateProfile;

    public NeuralNetwork(Profile initialProfile, int hiddenSize, int batchSize)
    {
        HiddenSize = hiddenSize;
        BatchSize = batchSize; 
        ChangeProfile(initialProfile);
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
    public void Train(int minEpochs, int maxEpochs, float maxError, int batchesPerEpoch, Random r)
    {
        if (TrainingData?.GetLength(0) == null) 
            PopulateTrainingArray();

        foreach (var file in TrainingData)
        {
            Network.Train(file, minEpochs, maxEpochs, maxError, batchesPerEpoch, r);
        }
    }

    private Matrix CreateGenerationSeed(Random r)
    {
        var rows = new Matrix[Network.BatchSize];

        for (var i = 0; i < rows.Length; i++)
        {
            var charId = r.Next(VocabList.Size);
            rows[i] = VocabList.CreateOneHot(VocabList[charId]);
        }

        return Matrix.StackArray(rows);
    }

    private string InterpretVectorOutput(Matrix[] outputs)
    {
        return string.Empty;
    }
    
    /// <summary>
    /// Use the network to generate, outputting the created data to a MIDI file  
    /// </summary>
    public void Generate(int genLength, int bpm, string savePath, Random r)
    {
        var seed = CreateGenerationSeed(r);
        var networkOutputs = Network.Forward(seed, genLength);
        var stringOutput = InterpretVectorOutput(networkOutputs); 
        
        MidiManager.WriteFile(stringOutput, savePath, bpm);
    }
    
    /// <summary>
    /// Change the state profile that the network is using
    /// </summary>
    /// <param name="profile">The profile to change to</param>
    public void ChangeProfile(Profile profile)
    {
        Network.StateProfile = profile;
    }
}