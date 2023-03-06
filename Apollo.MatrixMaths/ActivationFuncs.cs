using System;

namespace Apollo.MatrixMaths;

public class ActivationFuncs
{
    // Values taken from the graphs 
    private static readonly int TanhClip = 4; // tanh(x)

    private static readonly int SigmoidClip = 6; // 1 / (1 + e^-x)

    /// <summary>
    ///     Clipped Tanh (in order to avoid NaN)
    /// </summary>
    public static float Tanh(float x)
    {
        // Values taken from the tanh graph
        if (x > TanhClip)
            return 1;
        if (x < -TanhClip)
            return -1;

        return (float)Math.Tanh(x);
    }

    /// <summary>
    ///     Derivative of hyperbolic tangent
    /// </summary>
    public static float DTanh(float x)
    {
        // tanh'(x) = 0 when tanh(x) = 1 or tanh(x) = -1  (since the function no longer increases or decreases)
        if (x > TanhClip || x < -TanhClip)
            return 0;

        return 1 / (float)Math.Pow(Math.Cosh(x), 2);
    }

    /// <summary>
    ///     Clipped Sigmoid function (in order to avoid NaN)
    /// </summary>
    public static float Sigmoid(float x)
    {
        // Values taken from the graph of the function 
        if (x > SigmoidClip)
            return 1;
        if (x < -SigmoidClip)
            return 0;

        return 1 / (1 + (float)Math.Exp(-x));
    }

    /// <summary>
    ///     Derivative of the sigmoid function
    /// </summary>
    public static float DSigmoid(float x)
    {
        // sigmoid'(x) = 0 when sigmoid(x) = 1  or sigmoid(x) = -1 (since the function no longer increases or decreases)
        if (x > SigmoidClip || x < -SigmoidClip)
            return 0;

        return (float)Math.Exp(-x) / (float)Math.Pow(1 + Math.Exp(-x), 2);
    }
}