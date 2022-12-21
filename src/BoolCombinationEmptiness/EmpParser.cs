using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Automata;

namespace Experimentation.NFA
{
    class EmpParser
    {
        Dictionary<int, Automaton<BDD>> numToAutomaton = new Dictionary<int, Automaton<BDD>>();
        string filePath;


        private int getAutNumFromName(string name)
        {
            if (!name.StartsWith("aut"))
            {
                throw new Exception("Automata names should be in form autN for some number N");
            }

            return Int32.Parse(name.Substring(3));
        }

        public EmpParser(String filePath)
        {
            this.filePath = filePath;
        }

        public bool parseAndCheckEmptiness()
        {
            BDDAlgebra algebra = new BDDAlgebra();
            var mataParser = new MataBitAlphabetParser(algebra);
            int? autNumToCheck = null;

            var pathToAutDir = System.IO.Path.Combine(System.IO.Directory.GetParent(filePath).FullName, "gen_aut");

            foreach (string line in System.IO.File.ReadLines(filePath))
            {
                var tokens = line.Replace("=", "").Replace("(", "").Replace(")", "").Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (tokens[0] == "load_automaton")
                {
                    numToAutomaton[getAutNumFromName(tokens[1])] = mataParser.parse(System.IO.Path.Combine(pathToAutDir, tokens[1] + ".mata")).Minimize();
                }
                else if (tokens[0] == "is_empty")
                {
                    autNumToCheck = getAutNumFromName(tokens[1]);
                }
                else
                {
                    if (!numToAutomaton.TryGetValue(getAutNumFromName(tokens[2]), out Automaton<BDD> result))
                    {
                        throw new Exception("Trying to apply operation on not already parsed/processed automaton");
                    }

                    if (tokens[1] == "compl")
                    {
                        result = result.Determinize().Minimize().MkComplement(algebra);
                        // MkComplement of minimal deteministic automaton is also minimal deterministic automaton, so we do not minimize result
                        numToAutomaton[getAutNumFromName(tokens[0])] = result;
                    }
                    else
                    {
                        for (int i = 3; i < tokens.Length; i++)
                        {
                            if (!numToAutomaton.TryGetValue(getAutNumFromName(tokens[i]), out Automaton<BDD> operand))
                            {
                                throw new Exception("Trying to apply operation on not already parsed/processed automaton");
                            }

                            if (tokens[1] == "union")
                            {
                                result = result.Union(operand);
                            }
                            else if (tokens[1] == "inter")
                            {
                                result = result.Intersect(operand);
                            }
                            else
                            {
                                throw new Exception("Unknown operation");
                            }

                            // we also minimize result so that next operations are faster
                            numToAutomaton[getAutNumFromName(tokens[0])] = result.Minimize();
                        }
                    }
                    int resAutNum = getAutNumFromName(tokens[0]);
                    numToAutomaton[resAutNum] = result.Minimize();
                }
            }
            if (!numToAutomaton.TryGetValue(autNumToCheck.Value, out Automaton<BDD> autToCheck))
            {
                throw new Exception("Checking emptiness of non-existing automaton");
            }
            return autToCheck.IsEmpty;
        }
    }
}
