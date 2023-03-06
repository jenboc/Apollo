namespace Apollo.NeuralNet;

/// <summary>
///     Static class for storing the recommended ADAM optimiser hyperparameters
/// </summary>
internal static class AdamParameters
{
    // Recommended parameters.
    public const float ALPHA = 0.001f;
    public const float BETA1 = 0.9f;
    public const float BETA2 = 0.999f;
    public const float EPSILON = 1e-8f;
}