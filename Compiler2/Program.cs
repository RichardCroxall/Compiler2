/*
    \file Program.cs
    Copyright Notice\n
    Copyright (C) 1995-2020 Richard Croxall - developer and architect\n
    Copyright (C) 1995-2020 Dr Sylvia Croxall - code reviewer and tester\n

    This file is part of Compiler2.

    Compiler2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Compiler2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Compiler2.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Compiler2;
using compiler2.Code;
using compiler2.Compile;
using compiler2.Generate;


/* TODO
 * 1) Check warnings out in compiler. It looks like they shouldn't be coming out.
 * 2) Make semantic checks on expressions.
 * 3) Rename flags as variables
 * 4) On toprint of an expression, sort out variables.
 * 
 */
namespace compiler2
{
    class Program
    {
        static void Main(string[] args)
        {
            Arguments arguments = new Arguments(args);
            if (arguments.Errors == 0)
            {
                Dictionary<string, CodeBase> idDictionary = new Dictionary<string, CodeBase>();

                SyntaxAnalyser syntaxAnalyser = null;
                for (int pass = 1; pass <= 2; pass++)
                {
                    LexicalAnalyser lexicalAnalyser = new LexicalAnalyser(arguments.SourceFile, pass);
                    syntaxAnalyser = new SyntaxAnalyser(lexicalAnalyser, idDictionary);
                    syntaxAnalyser.Parse();
                }

                if (syntaxAnalyser != null)
                {
                    if (syntaxAnalyser.ErrorCount == 0)
                    {
                        GeneratePortableFile generatePortableFile =
                            new GeneratePortableFile(syntaxAnalyser.CodeCalendarValue);
                        generatePortableFile.WriteRuntimeFile(arguments.CodeFile);

                        if (syntaxAnalyser.WarningCount != 0)
                        {
                            Console.WriteLine(String.Format("Listed {0} warnings. Code generated.", syntaxAnalyser.WarningCount));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("No errors or warnings. Code generated."));
                        }
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Listed {0} errors and {1} warnings. No code generated.",
                            syntaxAnalyser.ErrorCount, syntaxAnalyser.WarningCount));
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }
    }
}
