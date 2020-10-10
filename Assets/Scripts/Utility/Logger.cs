﻿using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;


/// <summary>
/// Class used to log messages to the debug log and to a text file.
/// </summary>
public static class Logger
{
    /// <summary>
    /// The directory to store log files.
    /// </summary>
    private const string logDir = @"Logs";
    /// <summary>
    /// The path to write log files to.
    /// </summary>
    private const string logPath = logDir + @"\Log-";
    /// <summary>
    /// The path to write the current log file to.
    /// </summary>
    private static string currentLogPath = "";
    private const string dateTimeString = "yyyy-MM-dd HH-mm-ss";

    /// <summary>
    /// The maximum number of old logs to store.
    /// </summary>
    private const int maxLogs = 10;

    /// <summary>
    /// List of text in the log.
    /// </summary>
    private readonly static List<string> mainLogText = new List<string>();


    /// <summary>
    /// Starts the logger. Creates log directory if one doesn't exist. Deletes old logs if number of stored logs is greater than max allowed.
    /// Creates a new text file to contain log.
    /// </summary>
    public static void Start()
    {
        // If log directory doesn't exist, create it
        if(Directory.Exists(logDir) == false)
        {
            Directory.CreateDirectory(logDir);
        }
        // Set current log path to include datetime
        currentLogPath = logPath + DateTime.Now.ToString(dateTimeString) + ".txt";
        // Get a list of all old logs
        List<string> oldLogs = new List<string>(Directory.GetFiles(logDir, "Log-*.txt"));
        oldLogs.Sort();
        // If number of logs is greater than max allowed
        while(oldLogs.Count >= maxLogs)
        {
            File.Delete(oldLogs[0]);
            oldLogs.RemoveAt(0);
        }
        WriteLogToFile();
    }

    /// <summary>
    /// Appends all strings in main log to text file.
    /// </summary>
    public static void WriteLogToFile()
    {
        File.AppendAllLines(currentLogPath, mainLogText.ToArray());
        mainLogText.Clear();
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
        string time = $@"{DateTime.Now.ToString("[" + dateTimeString + "]")}";
        Debug.Log($@"{text}");
        mainLogText.Add($@"{time} - {text}");
    }

    /// <summary>
    /// Adds a string of error text to the log.
    /// </summary>
    /// <param name="error">The error to add to the log.</param>
    public static void Log(System.Exception error)
    {
        string time = $@"{DateTime.Now.ToString("[" + dateTimeString + "]")}";
        Debug.Log($@"{error}");
        mainLogText.Add($@"{time} - {error}");
    }
}