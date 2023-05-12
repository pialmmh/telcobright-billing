using System;
using System.Threading;

public static class Program
{
    private static Timer _timer = null;
    
    public static void Main(string[] args)
    {
        string appName = args[0];
        // Create a Timer object that knows to call our TimerCallback
        // method once every 2000 milliseconds.
        _timer = new Timer(TimerCallback, appName, 0, 2000);
        // Wait for the user to hit <Enter>
        Console.ReadLine();
    }

    private static void TimerCallback(Object o)
    {
        // Display the date/time when this method got called.
        string appName = (string)o;
        Console.Title = appName;
        Console.WriteLine($"In app={appName}: " + DateTime.Now);
    }
}