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

    public void WriteToFile(BinaryWriter writer)
    {
        writer.Write(Alpha);
        writer.Write(Beta1);
        writer.Write(Beta2);
        writer.Write(Epsilon);
    }

    public static AdamParameters ReadFromFile(BinaryReader reader)
    {
        var adamParams = new AdamParameters((float)reader.ReadDecimal(), (float)reader.ReadDecimal(),
            (float)reader.ReadDecimal(), (float)reader.ReadDecimal());

        return adamParams;
    }
}