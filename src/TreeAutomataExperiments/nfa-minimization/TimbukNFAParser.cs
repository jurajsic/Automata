﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using Microsoft.Automata.Z3;
using Microsoft.Automata.Z3.Internal;
using Microsoft.Automata;
using Microsoft.Z3;
using System.Diagnostics;


namespace RunExperiments
{
    class TimbukNFAParser
    {
        public static Automaton<BDD> ParseVataFile(string pathFN, CharSetSolver solver)
        {
            string text = System.IO.File.ReadAllText(pathFN);
            return ParseVataFormat(text, solver);
        }

        public static Automaton<BDD> ParseVataFormat(string vataString, CharSetSolver solver)
        {
            var lines = vataString.Split('\r', '\n');

            HashSet<int> finStates = new HashSet<int>();
            var rules = new List<Move<BDD>>();

            Dictionary<string, int> stateNames = new Dictionary<string, int>();
            Dictionary<string, char> constructorNames = new Dictionary<string, char>();            

            bool transitionsStarted = false;
            var initialStates = new HashSet<int>();

            foreach (var line in lines)
            {
                if (!transitionsStarted)
                {
                    if (line.StartsWith("Ops"))
                    {
                        var constructors = line.Split(' ');

                        foreach (var constructor in constructors)
                        {
                            var sp = constructor.Split(':');
                            if (sp.Length > 1)
                                if (!constructorNames.ContainsKey(sp[0]))
                                    constructorNames[sp[0]]= Convert.ToChar(constructorNames.Count);
                        }
                        if (constructorNames.Count == 0)
                            return null;
                    }

                    if (line.StartsWith("Final"))
                    {
                        var sp = line.Split(' ');
                        for (int i = 2; i < sp.Length; i++)
                        {
                            if (sp[i].Length > 0)
                                finStates.Add(GetState(sp[i], stateNames));
                        }
                    }
                    if (line.StartsWith("Transit"))
                    {
                        transitionsStarted = true;
                    }
                }
                else
                {
                    var sp = line.Split('-', '>');
                    if (sp.Length > 1)
                    {
                        var pieces = sp[0].Split('(', ',', ')', ' ');
                        var constructor = pieces[0];
                        List<int> from = new List<int>();
                        for (int i = 1; i < pieces.Length - 1; i++)
                            if (pieces[i].Length > 0)
                                from.Add(GetState(pieces[i], stateNames));

                        

                        var to = GetState(sp[sp.Length - 1], stateNames);

                        if(from.Count==0){
                            initialStates.Add(to);
                        }
                        else{
                            if (from.Count == 1)
                            {
                                var pred = solver.MkCharConstraint(constructorNames[constructor]);
                                var move = new Move<BDD>(from[0], to, pred);
                                rules.Add(move);
                            }
                            else
                            {
                                throw new Exception("tree automaton not NFA");
                            }
                        }
                    }

                }
            }
            if (initialStates.Count > 1)
                throw new Exception("More than one init state");

            return Automaton<BDD>.Create(solver, new List<int>(initialStates)[0], finStates, rules).RemoveEpsilonLoops();
        }

        public static int GetState(string st, Dictionary<string, int> names)
        {
            var n = st.Trim();
            if (names.ContainsKey(n))
                return names[n];

            names[n] = names.Count;
            return names[n];
        }

    }
}
