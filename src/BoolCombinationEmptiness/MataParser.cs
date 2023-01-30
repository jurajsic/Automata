using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Automata;

namespace Experimentation.NFA
{
    class MataBitAlphabetParser
    {
        private Dictionary<string, int> namesToStates = new Dictionary<string, int>();
        private Dictionary<string, BDD> namesToBDDs = new Dictionary<string, BDD>();
        private int maxState = 0;
        private int maxBDDVar = 0;

        private BDDAlgebra algebra;

        public MataBitAlphabetParser(BDDAlgebra algebra)
        {
            this.algebra = algebra;
        }

        private void reset()
        {
            namesToStates.Clear();
            //namesToBDDs.Clear();
            maxState = 0;
            //maxBDDVar = 0;
        }

        private int getStateNum(string name)
        {
            if (!namesToStates.TryGetValue(name, out int stateNum))
            {
                stateNum = maxState;
                namesToStates[name] = stateNum;
                ++maxState;
            }
            return stateNum;
        }

        private BDD getBDDFromName(string name)
        {
            if (name == "")
            {
                throw new Exception("Some formula in some transition has something wrong");
            }

            if (name == "false")
            {
                return algebra.False;
            }
            else if (name == "true")
            {
                return algebra.True;
            }

            if (!namesToBDDs.TryGetValue(name, out BDD returnBDD))
            {
                returnBDD = algebra.MkBvSet(maxBDDVar, algebra.True, algebra.False);
                namesToBDDs[name] = returnBDD;
                ++maxBDDVar;
            }
            return returnBDD;
        }

        private int indexOfNextNonMatchingEndingBracket(string input, int startIndex)
        {
            int numOfStartingBrackets = 0;
            for (int i = startIndex; i < input.Length; ++i)
            {
                switch (input[i])
                {
                    case '(':
                        numOfStartingBrackets++;
                        break;
                    case ')':
                        if (numOfStartingBrackets == 0)
                        {
                            return i;
                        }
                        else
                        {
                            numOfStartingBrackets--;
                        }
                        break;
                    default:
                        break;
                }
            }
            return -1;
        }

        private BDD getNextBDD(ref string restOfFormula)
        {
            if (restOfFormula[0] == '(')
            {
                //if (stringFormula.Last() != ')')
                //{
                //    throw new Exception(String.Format("Formula '{}' does not have a closing bracket", stringFormula));
                //}
                //else
                //{
                //    return getBDDFromStringFormula(stringFormula.Substring(1, stringFormula.Length - 1));
                //}
                int indexOfClosingBracket = indexOfNextNonMatchingEndingBracket(restOfFormula, 1);
                if (indexOfClosingBracket == -1)
                {
                    throw new Exception("Some formula does not have a closing bracket");
                }
                BDD insideFormula = getBDDFromStringFormula(restOfFormula.Substring(1, indexOfClosingBracket - 1));
                restOfFormula = restOfFormula.Substring(indexOfClosingBracket + 1);
                return insideFormula;
            }
            else if (restOfFormula[0] == '!')
            {
                restOfFormula = restOfFormula.Substring(1);
                return algebra.MkNot(getNextBDD(ref restOfFormula));
            }
            else
            {
                int indexOfNextOperator = restOfFormula.IndexOfAny(new char[] { '|', '&' });
                string token = (indexOfNextOperator == -1) ? restOfFormula : restOfFormula.Substring(0, indexOfNextOperator);
                restOfFormula = (indexOfNextOperator == -1) ? "" : restOfFormula.Substring(indexOfNextOperator);

                if (token.IndexOfAny(new char[] { '!', '(', ')' }) != -1)
                {
                    throw new Exception($"Unexpected token in subformula {restOfFormula}");
                }

                return getBDDFromName(token);
            }
        }

        private BDD getBDDFromStringFormula(String stringFormula)
        {
            BDD result = getNextBDD(ref stringFormula);
            while (stringFormula.Length != 0)
            {
                char operation = stringFormula[0];
                stringFormula = stringFormula.Substring(1);
                if (operation == '&')
                {
                    result = algebra.MkAnd(result, getNextBDD(ref stringFormula));
                }
                else if (operation == '|')
                {
                    result = algebra.MkOr(result, getNextBDD(ref stringFormula));
                }
                else
                {
                    throw new Exception($"This should not happen: {stringFormula}");
                }
            }
            return result;
        }

        public Automaton<BDD> parse(String inputFile)
        {
            int? initialState = null;
            List<int> nonfinalStates = new List<int>();
            var Transitions = new List<Move<BDD>>();

            foreach (string line in System.IO.File.ReadLines(inputFile))
            {
                var tokens = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (tokens[0] == "@NFA-bits") { continue; }
                else if (tokens[0] == "%Initial")
                { //initial state
                    if (tokens.Length > 2)
                    {
                        throw new Exception("More than one initial state in .mata file");
                    }
                    else
                    {
                        if (initialState == null)
                        {
                            initialState = getStateNum(tokens[1]);
                        }
                        else
                        {
                            throw new Exception("More than one initial formula in .mata file");
                        }
                    }
                }
                else if (tokens[0] == "%Final")
                { // (non)final states
                    if (tokens.Length == 2 && tokens[1] == "true") {
                        continue;
                    }
                    bool expectsOperator = false;
                    for (int i = 1; i < tokens.Length; ++i)
                    {
                        if (expectsOperator)
                        {
                            if (tokens[i] != "&")
                            {
                                throw new Exception("Final condition is expected to be in the form of conjunction of negated nonfinal states, there is missing conjunction");
                            }
                        }
                        else
                        {
                            if (tokens[i][0] != '!')
                            {
                                throw new Exception("Final condition is expected to be in the form of conjunction of negated nonfinal states, some state is not negated");
                            }
                            else
                            {
                                nonfinalStates.Add(getStateNum(tokens[i].Substring(1)));
                            }
                        }
                        expectsOperator = !expectsOperator;
                    }
                }
                else
                { // transition
                    string formula = String.Join("", tokens.Where((item, index) => ((index != 0) && (index != tokens.Length - 1))));
                    BDD predicate = getBDDFromStringFormula(formula);
                    if (predicate.IsEmpty)
                    {
                        continue;
                    }

                    int stateFrom = getStateNum(tokens[0]);
                    int stateTo = getStateNum(tokens.Last());

                    Transitions.Add(new Move<BDD>(stateFrom, stateTo, predicate));
                }
            }

            var finalStates = new List<int>();
            for (int i = 0; i < maxState; ++i)
            {
                if (!nonfinalStates.Contains(i))
                {
                    finalStates.Add(i);
                }
            }

            reset();
            return Automaton<BDD>.Create(algebra, initialState.Value, finalStates, Transitions, true, true);
        }
    }
}
