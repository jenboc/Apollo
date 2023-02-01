namespace Apollo.IO;

/// <summary>
/// Class for managing and writing to a log text file 
/// </summary>
public static class LogManager
{
    private static string LogPath { get; set; }
    
    // Run on first use of log manager
    static LogManager()
    {
        LogPath = "logs";

        if (!Directory.Exists(LogPath))
            Directory.CreateDirectory(LogPath);
        
        DetermineFileName(); 
    }
    
    /// <summary>
    /// Creates a filename which does not exist in the current log path 
    /// </summary>
    /// <returns></returns>
    private static void DetermineFileName()
    {
        // Do not continue if the LogPath is already to a file. 
        if (LogPath.EndsWith(".log")) return;

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var fileName = $"apollo_logs_{date}_";

        var currentId = -1;
        var exists = true;
        while (exists)
        {
            currentId++;
            var path = Path.Join(LogPath, $"{fileName}{currentId}.log");
            exists = File.Exists(path);
        }

        LogPath = Path.Join(LogPath, $"{fileName}{currentId}.log");
    }
    
    /// <summary>
    /// Change the path of the log files
    /// </summary>
    /// <param name="newPath">New path to directory where files will be located</param>
    /// <param name="transferCurrent">Whether to transfer the content of the current log</param>
    public static void SetPath(string newPath, bool transferCurrent)
    {
        // Change root directory
        var oldPath = LogPath;
        LogPath = newPath;
        
        // Determine the file name 
        DetermineFileName();
        
        // Transfer the current log if required
        if (transferCurrent)
        {
            var currentLogContents = File.ReadAllText(oldPath);
            File.WriteAllText(LogPath, currentLogContents);
        }
    }

    /// <summary>
    /// Log some data
    /// </summary>
    /// <param name="data">The data to log</param>
    public static void WriteLine(object data)
    {
        // Write the data to the console (if there is one) 
        Console.WriteLine(data);

        // Create writer that does not overwrite the file
        var writer = new StreamWriter(LogPath, true);
        
        // Append the time (hours:minutes:seconds:miliseconds) to the data
        var time = DateTime.Now.ToString("hh:mm:ss:fff");
        data = $"[{time}]\n{data}";
        
        writer.WriteLine(data);
    }
}