using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class FileManager
{
    public static void Append(String path, String s)
    {
        using (StreamWriter file = new StreamWriter(path, true, Encoding.UTF8))
        {
            file.WriteLine(s, Encoding.UTF8);
        }
    }

    public static void Append(String path, List<String> lines)
    {
        using (StreamWriter file = new StreamWriter(path, true, Encoding.UTF8))
        {
            foreach (String line in lines)
            {
                file.WriteLine(line, Encoding.UTF8);
            }
        }
    }

    public static void Write(String path, String s)
    {
        File.WriteAllText(path, s, Encoding.UTF8);
    }

    public static void Write(String path, List<String> lines)
    {
        File.WriteAllLines(path, lines, Encoding.UTF8);
    }

    public static String Read(String path)
    {
        return File.ReadAllText(path, Encoding.UTF8);
    }

    public static List<String> ReadLines(String path)
    {
        return new List<String>(File.ReadAllLines(path, Encoding.UTF8));
    }

    public static Boolean Exists(String path)
    {
        return File.Exists(path);
    }
}