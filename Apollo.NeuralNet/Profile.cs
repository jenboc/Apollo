namespace Apollo;

/// <summary>
///     Model for containing data about a network state profile (read from and written to a JSON file)
/// </summary>
public class Profile
{
    public string BeforeStateFile { get; set; }
    public string AfterStateFile { get; set; }
    public string TrainingDataDirectory { get; set; }
    public string Vocab { get; set; }
    
    /// <summary>
    ///     Returns the default profile 
    /// </summary>
    /// <param name="pathToFiles">Path to the files associated with the new profile</param>
    /// <returns>A profile containing default data</returns>
    public static Profile Default(string pathToFiles)
    {
        var profile = new Profile();
        profile.BeforeStateFile = Path.Join(pathToFiles, "apollo_before.state");
        profile.AfterStateFile = Path.Join(pathToFiles, "apollo_after.state");
        profile.TrainingDataDirectory = Path.Join(pathToFiles, "training");
        profile.Vocab = "";

        return profile;
    }
    
    /// <summary>
    ///     Evaluates whether two profiles are the same
    /// </summary>
    /// <param name="a">The first profile</param>
    /// <param name="b">The second profile</param>
    /// <returns>A boolean depicting whether the two contain the exact same data or not</returns>
    public static bool operator ==(Profile a, Profile b)
    {
        return a.BeforeStateFile == b.BeforeStateFile
               && a.AfterStateFile == b.AfterStateFile
               && a.TrainingDataDirectory == b.TrainingDataDirectory
               && a.Vocab == b.Vocab;
    }
    
    /// <summary>
    ///     Evaluates whether two profiles are different
    /// </summary>
    /// <param name="a">The first profile</param>
    /// <param name="b">The second profile</param>
    /// <returns>A boolean depicting whether the two contain different data or not</returns>
    public static bool operator !=(Profile a, Profile b)
    {
        return !(a == b);
    }
}