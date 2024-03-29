﻿using System.Text;
using Apollo.IO;
using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

/// <summary>
///     Wrapper class containing all necessary objects and methods for high level control of RNN
/// </summary>
public class NeuralNetwork
{
    public NeuralNetwork(Profile initialProfile, int hiddenSize, int batchSize, Random r)
    {
        HiddenSize = hiddenSize;
        BatchSize = batchSize;
        R = r;
        TrainingData = null;
        ChangeProfile(initialProfile);
        PopulateVocabList();
    }

    private Rnn Network { get; set; } // RNN itself
    public Vocab VocabList { get; set; } // The vocab used by the RNN 
    private Matrix[][]? TrainingData { get; set; } // TrainingData used during training 
    private int HiddenSize { get; } // The hidden size of the network
    private int BatchSize { get; } // The batch size of the network
    private Random R { get; } // Random instance used for ALL RNG in the network
    public Profile CurrentProfile { get; private set; } // The profile currently being used by the network

    /// <summary>
    ///     Populate the training data jagged array with the profile's training data
    /// </summary>
    private void PopulateTrainingArray()
    {
        var trainingDataFiles = Directory.GetFiles(CurrentProfile.TrainingDataDirectory)
            .Where(filePath => filePath.EndsWith(".td")).ToArray();

        if (trainingDataFiles.Length == 0) // If the folder doesn't have any training data files  
        {
            // Convert the midi files into training data
            var midiStrings = MidiManager.ReadDir(CurrentProfile.TrainingDataDirectory);
            TrainingData = new Matrix[midiStrings.Count][];

            Parallel.For(0, midiStrings.Count,
                i => { TrainingData[i] = VocabList.PrepareTrainingData(midiStrings[i]); });

            // Save the just-converted training data into training data files
            for (var i = 0; i < midiStrings.Count; i++)
            {
                var fileName = Path.Join(CurrentProfile.TrainingDataDirectory, $"file{i}.td");
                TrainingFileManager.Write(TrainingData[i], fileName);
            }

            // Delete the MIDI files
            MidiManager.PurgeDir(CurrentProfile.TrainingDataDirectory);
        }
        else // If training data files exist, use them instead 
        {
            TrainingData = TrainingFileManager.ReadDir(CurrentProfile.TrainingDataDirectory);
        }
    }

    /// <summary>
    ///     Populate the Vocab object with the different characters used in the training data
    /// </summary>
    private void PopulateVocabList()
    {
        // If the profile does not already have a vocab list saved, then it cannot be used
        // This is because the original MIDI files will have been replaced with the training data files before this point
        if (CurrentProfile.Vocab.Length == 0) 
        {
            var directory = Path.GetDirectoryName(CurrentProfile.AfterStateFile);
            Directory.Delete(directory, true);
            throw new Exception("The profile you are using is corrupted, please reopen the program and select a" +
                                "different profile");
        }

        VocabList = new Vocab(CurrentProfile.Vocab); // Create a vocab object from the stored list
    }

    /// <summary>
    ///     Train the network
    /// </summary>
    public void Train(int minEpochs, int maxEpochs, float maxError, int batchesPerEpoch)
    {
        // Populate the array if it is empty
        if (TrainingData == null)
            PopulateTrainingArray();

        // Save before state
        Network.SaveState(CurrentProfile.BeforeStateFile);

        foreach (var vectorisedFile in TrainingData) // Perform a training loop for each file in the training data
            Network.Train(vectorisedFile, minEpochs, maxEpochs, maxError, batchesPerEpoch, R);

        // Save after state
        Network.SaveState(CurrentProfile.AfterStateFile);
    }

    /// <summary>
    ///     Create a generation seed
    /// </summary>
    /// <returns>A matrix representing the initial input into the RNN when generating</returns>
    private Matrix CreateGenerationSeed()
    {
        var rows = new Matrix[Network.BatchSize]; // Seed must be of size VocabSize x BatchSize

        for (var i = 0; i < rows.Length; i++) // For each row, select a character at random to be the seed
        {
            var charId = R.Next(VocabList.Size);
            rows[i] = VocabList.CreateOneHot(VocabList[charId]);
        }

        return Matrix.StackArray(rows); // Stack the array into one matrix so it can be inputted
    }

    /// <summary>
    ///     Interpret a matrix which was outputted from the RNN during generation
    /// </summary>
    /// <param name="outputs">What the RNN outputted during generation</param>
    /// <returns>A string representation of the RNN output which can be passed to MidiManager</returns>
    private string InterpretVectorOutput(Matrix[] outputs)
    {
        var stringOutput = new StringBuilder();

        foreach (var output in outputs)
        {
            // The next character to be written is always the last row. 
            var rowContents = new float[1, output.Columns];
            for (var j = 0; j < output.Columns; j++) rowContents[0, j] = output[output.Rows - 1, j];
            var mat = new Matrix(rowContents);

            // Interpret the last row of the output matrix, and add it to the string builder
            stringOutput.Append(VocabList.InterpretOneHot(mat));
        }

        return stringOutput.ToString();
    }

    /// <summary>
    ///     Use the network to generate, outputting the created data to a MIDI file
    /// </summary>
    /// <param name="genLength">The amount of iterations of the RNN to do</param>
    /// <param name="bpm">The beats per minute of the MIDI file</param>
    /// <param name="savePath">The path to save the MIDI file to</param>
    public void Generate(int genLength, int bpm, string savePath)
    {
        // Create generation seed 
        var seed = CreateGenerationSeed();

        // Pass seed to RNN and interpret the output as a string 
        var networkOutputs = Network.Forward(seed, genLength);
        var stringOutput = InterpretVectorOutput(networkOutputs);

        // Log generation details and string representation 
        var logBuffer = $"Generating with:\nGeneration Length: {genLength}\nBPM: {bpm}\nSave Path: {savePath}" +
                        $"\nGenerated Text:\n{stringOutput}";
        LogManager.WriteLine(logBuffer);

        // Write and save the MIDI file using string representation
        MidiManager.WriteFile(stringOutput, savePath, bpm);
    }

    /// <summary>
    ///     Change the state profile that the network is using
    /// </summary>
    /// <param name="profile">The profile to change to</param>
    public void ChangeProfile(Profile profile)
    {
        // Change profile object here
        CurrentProfile = profile;
        // Wipe vocab list and populate it with relevant characters
        VocabList = new Vocab();
        PopulateVocabList();
        // Wipe TrainingData since new profile may use different data
        TrainingData = null;

        Console.WriteLine(VocabList.Size);

        // Create RNN with the data in the after state file (if it exists) 
        // Otherwise, create it from scratch
        Network = File.Exists(CurrentProfile.AfterStateFile)
            ? new Rnn(CurrentProfile.AfterStateFile)
            : new Rnn(VocabList.Size, HiddenSize, BatchSize, R);
    }

    /// <summary>
    ///     Try to revert the training done to the network
    /// </summary>
    public bool TryRevertTraining()
    {
        if (!File.Exists(CurrentProfile.BeforeStateFile)) // If there is no before state, training cannot be reverted
            return false;

        // Load the before state file and overwrite the after state file
        Network.LoadState(CurrentProfile.BeforeStateFile);
        Network.SaveState(CurrentProfile.AfterStateFile);

        return true;
    }
}