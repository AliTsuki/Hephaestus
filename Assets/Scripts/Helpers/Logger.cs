using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using UnityEngine;


/// <summary>
/// Class used to log messages to the debug log and to a text file.
/// </summary>
public static class Logger
{
    /// <summary>
    /// The directory to store log files.
    /// </summary>
    private const string LogDir = @"Logs";
    /// <summary>
    /// The path to write log files to.
    /// </summary>
    private const string LogPath = LogDir + @"\Log-";
    /// <summary>
    /// The path to write the current log file to.
    /// </summary>
    private static string CurrentLogPath = "";

    /// <summary>
    /// The maximum number of old logs to store.
    /// </summary>
    private const int MaxLogs = 10;

    /// <summary>
    /// List of text in the log.
    /// </summary>
    private readonly static List<string> MainLogText = new List<string>();


    /// <summary>
    /// Starts the logger. Creates log directory if one doesn't exist. Deletes old logs if number of stored logs is greater than max allowed.
    /// Creates a new text file to contain log.
    /// </summary>
    public static void Start()
    {
        // If log directory doesn't exist, create it
        if(Directory.Exists(LogDir) == false)
        {
            Directory.CreateDirectory(LogDir);
        }
        // Set current log path to include datetime
        CurrentLogPath = LogPath + DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss") + ".txt";
        // Get a list of all logs
        string[] oldLogs = Directory.GetFiles(LogDir, "Log-*.txt");
        // Get number of logs
        int numOldLogs = oldLogs.Length;
        // If number of logs is greater than max allowed
        if(numOldLogs > MaxLogs)
        {
            // Create an array of datetimes length of number of logs
            DateTime[] logTimes = new DateTime[numOldLogs];
            // Loop through logs
            for(int i = 0; i < numOldLogs; i++)
            {
                // Set logTimes to parsed datetime of log name
                logTimes[i] = DateTime.ParseExact(oldLogs[i].Substring(9, 19), "MM-dd-yyyy-HH-mm-ss", CultureInfo.InvariantCulture);
            }
            // Get the earliest date in logTimes
            DateTime earliest = logTimes.Min(date => date);
            // Loop through logTimes
            foreach(DateTime time in logTimes)
            {
                // If current time is the earliest time
                if(time == earliest)
                {
                    // Convert time back to name of oldest log
                    string logName = LogDir + $@"\Log-{time:MM-dd-yyyy-HH-mm-ss}.txt";
                    // Check that that log exists in logs
                    if(oldLogs.Contains(logName) == true)
                    {
                        // If there is a file for that log
                        if(File.Exists(LogDir + @"\" + logName))
                        {
                            // Delete oldest log and break out of loop
                            File.Delete(LogDir + @"\" + logName);
                            break;
                        }
                    }
                }
            }
        }
        WriteLogToFile();
    }

    /// <summary>
    /// Appends all strings in main log to text file.
    /// </summary>
    public static void WriteLogToFile()
    {
        File.AppendAllLines(CurrentLogPath, MainLogText.ToArray());
        MainLogText.Clear();
    }

    /// <summary>
    /// Called upon application quit, writes log to file one final time.
    /// </summary>
    public static void Quit()
    {
        WriteLogToFile();
    }

    /// <summary>
    /// Adds a string of text to the log.
    /// </summary>
    /// <param name="text">The string of text to add to the log.</param>
    public static void Log(string text)
    {
        string time = $@"{DateTime.Now:[MM/dd/yyyy HH:mm:ss]}";
        Debug.Log($@"{text}");
        MainLogText.Add($@"{time} - {text}");
    }

    /// <summary>
    /// Adds a string of error text to the log.
    /// </summary>
    /// <param name="error">The error to add to the log.</param>
    public static void Log(System.Exception error)
    {
        string time = $@"{DateTime.Now:[MM/dd/yyyy HH:mm:ss]}";
        Debug.Log($@"{error}");
        MainLogText.Add($@"{time} - {error}");
    }
}
