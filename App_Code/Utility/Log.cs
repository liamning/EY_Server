using System;
using System.Collections.Generic;
using System.Web;

using System.IO;

/// <summary>
/// Summary description for Log
/// </summary>
public class Log
{
    public static readonly string LogFolder = HttpContext.Current.Server.MapPath("~/Log");

    private const string info = "Info";
    private const string error = "Error";
    private static FileStream fileStream;
    private static StreamWriter writer;

    public static void Info(string message)
    {
        fileStream = File.Open(LogFolder + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt", FileMode.Append);
        writer = new StreamWriter(fileStream);

        writer.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}]: {2}", DateTime.Now, info, message));

        writer.Close();
        fileStream.Close();
    }

    public static void Error(string message)
    {
        fileStream = File.Open(LogFolder + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt", FileMode.Append);
        writer = new StreamWriter(fileStream);

        writer.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}]: {2}", DateTime.Now, error, message));

        writer.Close();
        fileStream.Close();
    }

}