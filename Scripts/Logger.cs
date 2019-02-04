using System.Collections.Generic;

// Class for Logging-to-file functions
public class Logger : ILoopable
{
    // Logger objects
    public static Logger MainLog = new Logger();
    private readonly List<string> mainlogtxt = new List<string>();

    // Instantiate Logger
    public static void Instantiate()
    {
        MainLoopable.MLInstance.RegisterLoops(MainLog);
    }

    // Logger Start
    public void Start()
    {

    }

    // Logger Update: Write mainlogtxt to Lob.txt FILE
    public void Update()
    {
        System.IO.File.WriteAllLines("Log.txt", new List<string>(this.mainlogtxt).ToArray());
    }

    // Logger On Application Quit
    public void OnApplicationQuit()
    {
        
    }

    // Logging methods
    public static void Log(string ll)
    {
        MainLog.log(ll);
    }
    public static void Log(System.Exception e)
    {
        MainLog.log(e);
    }
    public void log(string ll)
    {
        this.mainlogtxt.Add(ll);
    }
    public void log(System.Exception e)
    {
        this.mainlogtxt.Add(e.ToString());
    }
}
