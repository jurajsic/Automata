﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using Microsoft.Automata;
using System.Diagnostics;

namespace RunExperiments
{
    class Program
    {
        static internal string path = @"..\..\benchmark\";
        static internal int timeOut = 299999; // 5 minutes
        static internal int numTests = 2;

        static internal string timbukFile = "timbukResults.txt";
        static internal string timbukFileDM = "dtaminrestimbuk.txt";
        static internal string largeAlphabetFile = "largeAlphabetResults.txt";
        static internal string fastFile = "fastResults.txt";

        static internal string regexOutputFile = "nfaRegexMinResults.txt";
        static internal string verifNFAOutputFile = "nfaVerificationMinResults.txt";

        static internal string regexInputFile = "regexlib-clean.txt";

        static internal string timbukPrefix = "timbuk";
        static internal string largeAlphabetPrefix = "large";
        static internal string fastPrefix = "fast";

        static int Main(string[] args)
        {
            //Test generation for Fast
            // This takes hours
            //RecognizerGenerator.GenerateTAUsingFast(10, 12, 50);
            // These two takes minutes
            //RecognizerGenerator.GenerateTAUsingFast(6, 15, 1000);
            //RecognizerGenerator.GenerateTAUsingFast(5, 16, 1000);

            //DTAMINParsing.RunLargeGeneration();
            //DTAMINParsing.RunGeneration();

            // Run experiments                    
            //LargeAlphabetExperiment.RunTest();
            //FastExperiment.RunTest();
            //TimbukExperiment.RunTest();

            ////Gather results in text files            
            //Util.GatherResults(largeAlphabetFile, largeAlphabetPrefix);
            //Util.GatherResults(fastFile, fastPrefix);
            //Util.GatherResultsTimbuk(timbukFile, timbukFileDM, timbukPrefix);


            //RegexExperiment.RunTest();

            //VerificationNFAExperiment.RunTest();

            /*
            Console.WriteLine("regex");
            RegexExperiment.RunTest();
            Console.WriteLine("nfa-verification");
            VerificationNFAExperiment.RunFinAlphTest();
             */

            //var tests = new RegexExtensionMethodTests();
            //tests.TestRegex_CompileToSymbolicRegex_IsMatch_IgnoreCaseTrue();

            if (args.Length != 1)
            {
                Console.Error.WriteLine("Program expects one argument - the path to .emp file");
                return -1;
            }

            if ((new Experimentation.NFA.EmpParser(args[0])).parseAndCheckEmptiness())
            {
                Console.WriteLine("EMPTY");
            }
            else
            {
                Console.WriteLine("NOT EMPTY");
            }
            return 0;
        }

    }
}
