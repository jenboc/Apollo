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

internal class WordNotFoundException : Exception
{
    public WordNotFoundException()
    {
    }

    public WordNotFoundException(string message) : base(message)
    {
    }

    public WordNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}

internal class InvalidOneHotException : Exception
{
    public InvalidOneHotException()
    {
    }

    public InvalidOneHotException(string message) : base(message)
    {
    }

    public InvalidOneHotException(string message, Exception inner) : base(message, inner)
    {
    }
}