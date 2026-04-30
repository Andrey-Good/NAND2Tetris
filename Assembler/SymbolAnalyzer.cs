using System.Collections.Generic;

namespace Assembler
{
    public class SymbolAnalyzer
    {
        public Dictionary<string, int> CreateSymbolsTable(string[] instructionsWithLabels,
            out string[] instructionsWithoutLabels)
        {
            var labelValues = new Dictionary<string, int>();
            
            for (int i = 0; i <= 15; i++)
            {
                labelValues[$"R{i}"] = i;
            }
            labelValues["SCREEN"] = 16384;
            labelValues["KBD"] = 24576;
            labelValues["SP"] = 0;
            labelValues["LCL"] = 1;
            labelValues["ARG"] = 2;
            labelValues["THIS"] = 3;
            labelValues["THAT"] = 4;

            var cleanInstructions = new List<string>();
            
            int currentInstructionAddress = 0;

            foreach (var line in instructionsWithLabels)
            {
                if (line.StartsWith("(") && line.EndsWith(")"))
                {
                    var label = line.Substring(1, line.Length - 2);
                    
                    labelValues[label] = currentInstructionAddress;
                }
                else
                {
                    cleanInstructions.Add(line);
                    
                    currentInstructionAddress++;
                }
            }

            instructionsWithoutLabels = cleanInstructions.ToArray();

            return labelValues;
        }
    }
}