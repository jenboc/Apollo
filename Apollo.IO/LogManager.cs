﻿using System.Globalization;

namespace Apollo.IO;

/// <summary>
///     Class for managing and writing to a log text file
/// </summary>
public static class LogManager // Static class used so it is easily accessible across all libraries in the application
{
    private const int DAY_THRESHOLD = 2;

    private static string LogPath { get; set; }
    private static bool Initialised { get; set; }

    /// <summary>
    ///     Method ran once at the start of the program so that the LogManager can receive the path to the logs folder
    /// </summary>
    /// <param name="logPath">The path where the logs will be saved</param>
    public static void Init(string logPath)
    {
        Initialised = true;

        // Read from settings
        LogPath = logPath;

        // Create path for logs if it doesn't exist
        if (!Directory.Exists(LogPath))
            Directory.CreateDirectory(LogPath);

        // Delete any old logs first
        DeleteOldLogs();

        // Get name for log file of current session 
        DetermineFileName();
    }

    /// <summary>
    ///     Change log path
    /// </summary>
    public static void ChangeLogPath(string newPath)
    {
        if (!Initialised)
        {
            Init(newPath);
            return;
        }

        var logName = Path.GetFileName(LogPath);
        LogPath = Path.Join(newPath, logName);
    }

    /// <summary>
    ///     Creates a filename which does not exist in the current log path
    /// </summary>
    /// <returns></returns>
    private static void DetermineFileName()
    {
        if (!Initialised)
            return;

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
    ///     Log some data
    /// </summary>
    /// <param name="data">The data to log</param>
    public static void WriteLine(object data)
    {
        if (!Initialised)
            return;

        // Write the data to the console (if there is one) 
        Console.WriteLine(data);

        // Create writer that does not overwrite the file
        var writer = new StreamWriter(LogPath, true);

        // Append the time (hours:minutes:seconds:miliseconds) to the data
        var time = DateTime.Now.ToString("HH:mm:ss:fff");
        data = $"[{time}]\n{data}";

        writer.WriteLine(data);
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    ///     Get the date from the name of the log file
    /// </summary>
    /// <param name="logName">The name of the log file</param>
    /// <returns>A DateTime object of the date in the log file name</returns>
    private static DateTime DateFromLogName(string logName)
    {
        // Example name: apollo_logs_2023-02-28_8.log
        // Splitting at _'s will mean it is at index 2
        var splitName = logName.Split('_');
        var stringDate = splitName[2];

        return DateTime.ParseExact(stringDate, "yyyy-MM-dd", CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///     Delete all old logs in the directory
    /// </summary>
    private static void DeleteOldLogs()
    {
        if (!Initialised)
            return;

        // Get all logs in the directory
        var logFiles = Directory.GetFiles(LogPath).Where(fileName => fileName.EndsWith(".log"));

        // Iterate through logs
        foreach (var logPath in logFiles)
        {
            // Get date from filename 
            var logDate = DateFromLogName(Path.GetFileName(logPath));

            // Check if created before threshold 
            var difference = DateTime.Now.Subtract(logDate);

            if (difference.TotalDays >= DAY_THRESHOLD)
                // Delete if needed
                File.Delete(logPath);
        }
    }
}