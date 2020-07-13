using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compiler2
{
    public class Arguments
    {
        public string SourceFile { get; private set; }
        public string CodeFile { get; private set; }
        public int Errors { get; private set; }

        public Arguments(string[] args)
        {
            Errors = 0;
            SourceFile = "smart.txt";
            CodeFile = @"D:\usr\richard\projects\Smart8r\Smart8r\smart.smt";

            char flag = ' ';
            foreach (string arg in args)
            {
                if (arg.Length == 2 &&
                    arg[0] == '-' &&
                    char.IsLetter(arg[1]))
                {
                    flag = arg[1];
                }
                else
                {
                    switch (flag)
                    {
                        case ' ':
                            SourceFile = arg;
                            break;

                        case 'o':
                            CodeFile = arg;
                            break;

                        default:
                            Errors++;
                            Console.WriteLine(String.Format("Illegal flag '-{0}'", flag));
                            break;
                    }
                    flag = ' ';
                }
            }

            if (!File.Exists(SourceFile))
            {
                Errors++;
                Console.WriteLine(String.Format("Cannot find source file '{0}'", SourceFile));
            }

            String outputfolder = Path.GetDirectoryName(CodeFile);
            if (outputfolder.Length > 0 &&
                !Directory.Exists(outputfolder))
            {
                Errors++;
                Console.WriteLine(String.Format("Cannot find output folder '{0}'", outputfolder));
            }
        }
    }
}
