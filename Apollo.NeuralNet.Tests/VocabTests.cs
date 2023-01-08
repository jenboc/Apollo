namespace Apollo.NeuralNet.Tests;

public class VocabTests
{
    private readonly List<char> _testingVocabList = new() { 'a', 'b', 'c', 'd', 'e' };

    [Fact]
    // Testing that vocab gives the correct character given an ID
    public void CharFromId()
    {
        var vocab = new Vocab(_testingVocabList);

        Assert.Equal('a', vocab[0]);
        Assert.Equal('b', vocab[1]);
        Assert.Equal('c', vocab[2]);
        Assert.Equal('d', vocab[3]);
        Assert.Equal('e', vocab[4]);
    }

    [Fact]
    // Testing that vocab gives the correct ID given a character
    public void IdFromChar()
    {
        var vocab = new Vocab(_testingVocabList);

        Assert.Equal(0, vocab['a']);
        Assert.Equal(1, vocab['b']);
        Assert.Equal(2, vocab['c']);
        Assert.Equal(3, vocab['d']);
        Assert.Equal(4, vocab['e']);
    }

    [Fact]
    // Testing that AddCharacter() does not add duplicate characters.
    public void NoDuplicateCharacters()
    {
        var vocab = new Vocab(_testingVocabList);

        vocab.AddCharacter('e');
        vocab.AddCharacter('f');

        // If the duplicate e was added, vocab[5] would be e
        Assert.Equal('f', vocab[5]);
    }
}