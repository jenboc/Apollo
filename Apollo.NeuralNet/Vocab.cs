using Apollo.MatrixMaths;

namespace Apollo.NeuralNet;

public class Vocab
{
    private List<char> VocabList { get; }

    public Vocab()
    {
        VocabList = new List<char>(); 
    }

    public Vocab(List<char> vocabList)
    {
        VocabList = new List<char>();
        
        // Add each character one by one to avoid duplicates 
        foreach (var c in vocabList)
        {
            AddCharacter(c); // So that duplicates are not added 
        }
    }

    public void AddCharacters(char[] characters)
    {
        foreach (var c in characters) 
            AddCharacter(c);
    }

    /// <summary>
    /// Retrieve a word from the vocab list using its ID 
    /// </summary>
    /// <param name="id">The ID of the character to retrieve</param>
    public char this[int id] => VocabList[id];
    
    /// <summary>
    /// Retrieve the ID of a character
    /// </summary>
    /// <param name="c">The character to retrieve the ID of</param>
    public int this[char c] => VocabList.IndexOf(c);

    public int Size => VocabList.Count;

    /// <summary>
    /// Add a character to the vocabulary list
    /// </summary>
    /// <param name="c">The character to add to the vocabulary list</param>
    public void AddCharacter(char c)
    {
        // Only add if it is not already in the vocabulary list
        if (this[c] == -1)
            VocabList.Add(c);
    }
    
    /// <summary>
    /// Create a one-hot vector for a character in the vocab list
    /// </summary>
    /// <param name="c">The character to represent</param>
    /// <param name="column">Flag variable to decide if the vector is a column vector, true by default</param>
    /// <returns>A column or row one-hot vector</returns>
    public Matrix CreateOneHot(char c, bool column=true)
    {
        var vector = new Matrix(Size, 1);
        var charIndex = this[c];

        if (charIndex == -1)
            throw new WordNotFoundException("Word does not exist in the vocab list");

        vector[charIndex, 0] = 1;
        
        if (!column)
            vector.Transpose();

        return vector;
    }

    /// <summary>
    /// Interpret a one-hot vector
    /// </summary>
    /// <param name="vector">The one-hot vector</param>
    /// <returns>The character represented by the one-hot vector</returns>
    public char InterpretOneHot(Matrix vector)
    {
        // A row or column vector should be passed, meaning the vector should have:
        // 1 row/column
        // vocabSize columns/rows
        if ((vector.Rows > 1 && vector.Columns > 1) || 
            (vector.Rows == 1 && vector.Columns != Size) || 
            (vector.Columns == 1 && vector.Rows != Size)) 
            throw new InvalidOneHotException("A one-hot vector should have shape 1 x vocabSize or vocabSize x 1");
        
        if (vector.Rows == 1 && vector.Columns > 1) // Ensure it is a column vector 
            vector.Transpose();
        
        // Find the index of the 1
        var index = -1;
        var numOnes = 0;
        for (var i = 0; i < vector.Rows; i++)
        {
            if (vector[i, 0] == 1f)
            {
                index = i;
                numOnes++;
            }
        }

        if (numOnes != 1)
            throw new InvalidOneHotException("A one-hot vector should only contain one 1");

        return this[index];
    }

    public Matrix[] PrepareTrainingData(string midiString)
    {
        var trainingData = new Matrix[midiString.Length];
        for (var i = 0; i < midiString.Length; i++)
        {
            trainingData[i] = CreateOneHot(midiString[i]);
        }

        return trainingData;
    }
}