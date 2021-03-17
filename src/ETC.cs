using System;
using System.IO;

class ETC
{
    internal static Debugger debugger = new Debugger();
    private static VM vm = new VM();

    static void Main(string[] args)
    {
        //args = new string[] { "C:\\Users\\nimis\\source\\repos\\ETC\\ETC\\source.etc" };

        Console.Write("\n---------------------------------------------------------------------------\n\n");

        if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            Console.WriteLine("*** Invalid number of arguments to compiler. ***");
        }

        Console.Write("\n\n---------------------------------------------------------------------------\n");
        //Console.ReadKey();
    }

    private static void RunFile(in string path)
    {
        string source = ReadFile(path);
        VM.InterpretResult result = vm.Interpret(source);

        if(result == VM.InterpretResult.COMPILE_ERROR)
        {
            Console.ReadKey();
            System.Environment.Exit(65);
        }
        else if(result == VM.InterpretResult.RUNTIME_ERROR)
        {
            Console.ReadKey();
            System.Environment.Exit(70);
        }
    }

    private static string ReadFile(in string path)
    {
        FileInfo file = new FileInfo(path);
        if(!file.Exists)
        {
            Console.WriteLine("*** File does not exist. ***");
            Console.ReadKey();
            System.Environment.Exit(74);
        }
        string source;
        using (StreamReader reader = file.OpenText())
        {
            source = reader.ReadToEnd();
        }
        return source + "\0";
    }
}