namespace Apollo;

public class Profile
{
    public string BeforeStateFile { get; set; }
    public string AfterStateFile { get; set; }
    public string TrainingDataDirectory { get; set; }
    public string Vocab { get; set; }

    public static Profile Default(string pathToFiles)
    {
        var profile = new Profile();
        profile.BeforeStateFile = Path.Join(pathToFiles, "apollo_before.state");
        profile.AfterStateFile = Path.Join(pathToFiles, "apollo_after.state");
        profile.TrainingDataDirectory = Path.Join(pathToFiles, "training");
        profile.Vocab = "";

        return profile;
    }
}