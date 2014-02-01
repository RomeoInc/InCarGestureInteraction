using System;

public class Logger
{
    private static readonly object syncLock = new object();

    private String filename;

    public Logger(String filename)
    {
        this.filename = filename;
    }

    public void Write(String message, bool timestamp)
    {
        if (timestamp)
        {
            DateTime now = DateTime.Now;

            message = String.Format("{0} {1}.{2}#{3}", now.ToShortDateString(), now.ToLongTimeString(), now.Millisecond, message);
        }

        Write(this.filename, message);
    }

    public static void Write(String filename, String message)
    {
        lock (syncLock)
        {
            FileManager.Append(filename, message);
        }
    }
}