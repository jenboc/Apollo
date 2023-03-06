using Apollo.MatrixMaths;

namespace Apollo.IO;

/// <summary>
///     Static class responsible for reading and writing Training Data files
/// </summary>
public static class TrainingFileManager
{
    /// <summary>
    ///     Write training data to a file
    /// </summary>
    /// <param name="trainingData">The training data to write to the file</param>
    /// <param name="filePath">The path of the file to write it to</param>
    public static void Write(Matrix[] trainingData, string filePath)
    {
        var vocabLength = trainingData[0].Columns;
        using (var stream = File.Open(filePath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(vocabLength);
                writer.Write(trainingData.Length);
                foreach (var vector in trainingData) vector.WriteToFile(writer);
            }
        }
    }

    /// <summary>
    ///     Read training data from a file
    /// </summary>
    /// <param name="filePath">The path of the file to read from</param>
    /// <returns>The training data contained in the file</returns>
    public static Matrix[] Read(string filePath)
    {
        if (!File.Exists(filePath) || !filePath.EndsWith(".td"))
            throw new FileNotFoundException($"{filePath} is not a valid file");

        Matrix[] trainingData;

        using (var stream = File.Open(filePath, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream))
            {
                var vocabSize = reader.ReadInt32();
                var arrayLength = reader.ReadInt32();

                trainingData = new Matrix[arrayLength];

                for (var i = 0; i < arrayLength; i++) trainingData[i] = Matrix.ReadFromFile(reader, 1, vocabSize);
            }
        }

        return trainingData;
    }

    public static Matrix[][] ReadDir(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            throw new DirectoryNotFoundException($"{dirPath} is not a valid directory");

        var files = Directory.GetFiles(dirPath).Where(fileName => fileName.EndsWith(".td")).ToArray();
        var trainingData = new Matrix[files.Length][];

        for (var i = 0; i < trainingData.Length; i++)
        {
            var filePath = files[i];
            var fileData = Read(filePath);
            trainingData[i] = fileData;
        }

        return trainingData;
    }
}