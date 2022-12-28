namespace Apollo.NeuralNet;

internal class LstmInputException : Exception
{
    public LstmInputException()
    {
    }

    public LstmInputException(string message) : base(message)
    {
    }

    public LstmInputException(string message, Exception inner) : base(message, inner)
    {
    }
} 