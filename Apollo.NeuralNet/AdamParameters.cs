namespace Apollo.NeuralNet;

public struct AdamParameters
{
    public float Alpha;
    public float Beta1;
    public float Beta2;
    public float Epsilon;

    public AdamParameters(float alpha, float beta1, float beta2, float epsilon)
    {
        Alpha = alpha;
        Beta1 = beta1;
        Beta2 = beta2;
        Epsilon = epsilon;
    }
}