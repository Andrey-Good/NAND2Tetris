using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assembler
{
    public class Preprocessor
    {
        private static readonly Regex MemoryAccessRegex = new(@"\[([^\[\]]+)\]");
        private static readonly HashSet<string> JumpCommands = new()
        {
            "JGT", "JEQ", "JGE", "JLT", "JNE", "JLE", "JMP"
        };

        /// <summary>
        /// Преобразует нестандартные макро-инструкции в инструкции обычного языка ассемблера.
        /// </summary>
        public string[] PreprocessAsm(string[] instructions)
        {
            var asmCode = new List<string>();
            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                try
                {
                    TranslateInstruction(instr, asmCode);
                }
                catch (Exception e)
                {
                    throw new FormatException($"Can't parse at line {i + 1}: {instr}", e);
                }
            }

            return asmCode.ToArray();
        }

        public void TranslateInstruction(string instruction, List<string> asmCode)
        {
            instruction = ExpandMemoryAccess(instruction, asmCode);
            instruction = ExpandShortJump(instruction);
            asmCode.Add(instruction);
        }

        private static string ExpandMemoryAccess(string instruction, List<string> asmCode)
        {
            var matches = MemoryAccessRegex.Matches(instruction);
            if (matches.Count == 0)
            {
                return instruction;
            }

            var addresses = matches
                .Select(match => match.Groups[1].Value)
                .Distinct()
                .ToArray();

            if (addresses.Length != 1)
            {
                throw new FormatException("Only one address can be used in square brackets per instruction");
            }

            asmCode.Add("@" + addresses[0]);
            return MemoryAccessRegex.Replace(instruction, "");
        }

        private static string ExpandShortJump(string instruction)
        {
            if (instruction.Contains("=") || instruction.Contains(";") || !JumpCommands.Contains(instruction))
            {
                return instruction;
            }

            return instruction == "JMP"
                ? "0;JMP"
                : $"D;{instruction}";
        }
    }
}
