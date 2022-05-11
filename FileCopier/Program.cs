using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace FileCopier
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = "";
            string destinationFile = "";

            if (args.Length != 0)
            {
                if (args.Length == 2)
                {
                    sourceFile = args[0].ToString() ?? string.Empty;
                    destinationFile = args[1].ToString() ?? string.Empty;
                    try
                    {
                        FileCopier.FileCopy fc = new FileCopier.FileCopy(sourceFile, destinationFile);
                        fc.InteractiveMode = true;
                        fc.BufferSize = 2048;

                        if (fc.Copy())
                        {
                            Console.Write($"\nCopying time: {fc.GetCopyTime.Milliseconds} ms\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                    Console.WriteLine("Syntax [source file] [destination file]");
            }
            else
                Console.WriteLine("Syntax [source file] [destination file]");
        }
    }
}
