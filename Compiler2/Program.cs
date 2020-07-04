using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            Dictionary<string, CodeBase> idDictionary = new Dictionary<string, CodeBase>();

            SyntaxAnalyser syntaxAnalyser = null;
            for (int pass = 1; pass <= 2; pass++)
            {
                LexicalAnalyser lexicalAnalyser = new LexicalAnalyser(pass);
                syntaxAnalyser = new SyntaxAnalyser(lexicalAnalyser, idDictionary);
                syntaxAnalyser.Parse();
            }
            if (syntaxAnalyser != null && 
                syntaxAnalyser.ErrorCount == 0)
            {
                //GenerateHeader generateHeader = new GenerateHeader(syntaxAnalyser.CodeCalendarValue);
                //generateHeader.WriteRuntimeFile();

                GeneratePortableFile generatePortableFile = new GeneratePortableFile(syntaxAnalyser.CodeCalendarValue);
                generatePortableFile.WriteRuntimeFile();

                if (syntaxAnalyser.WarningCount != 0)
                {
                    Debug.Assert(false);
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }
    }
}
