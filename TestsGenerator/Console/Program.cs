using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using lab4TestsGenerator.Core;

namespace lab4TestsGenerator.ConsoleApp;

public class Program
{

    static void ParseDirArgs(string[] args, out string srcDir, out string resDir)
    {
        resDir = args[1];
        srcDir = args[0];
    }
    static bool TryParseInt(string arg, out int val)
    {
        if (!int.TryParse(arg, out val))
        {
            Console.WriteLine($"arg \"{arg}\" is supposed to be int");
            return false;
        }
        return true;
    }

    public static PipeLine? GetPipeLine(string[] args, out string srcDir, out string resDir, bool test = false)
    {
        // Display the number of command line arguments.
        if (args.Length == 5)
        {
            ParseDirArgs(args, out srcDir, out resDir);
            bool isOk = true;
            isOk &= TryParseInt(args[2], out var maxLoadTasks);
            isOk &= TryParseInt(args[3], out var maxSaveTasks);
            isOk &= TryParseInt(args[4], out var maxProcessTasks);
            if (isOk)
            {
                return new PipeLine(maxLoadTasks, maxProcessTasks, maxSaveTasks);
            }
        }
        else if (args.Length == 3)
        {
            ParseDirArgs(args, out srcDir, out resDir);
            if (TryParseInt(args[2], out var maxTasks))
            {
                return new PipeLine(maxTasks);
            }
        }
        else if (test || (args.Length == 1 && args[0].Equals("default")))
        {
            srcDir = "@tests/src/";
            resDir = "@tests/res/";
            return new PipeLine();
        }
        Console.WriteLine("Wrong arguments!");
        srcDir = "";
        resDir = "";
        return null;
    }

    static async Task Main(string[] args)
    {
        PipeLine? p = GetPipeLine(args, out var srcDir, out var resDir, true);
        if (p == null)
        {
            Console.WriteLine("Wrong arguments!");
            return;
        }
        Console.WriteLine("Pipeline created!");
        // work with pipeline
        try
        {
            if (Directory.Exists(resDir))
            {
                Directory.Delete(resDir, recursive: true);
            }
            Directory.CreateDirectory(resDir);

            await p.Process(srcDir, resDir);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
