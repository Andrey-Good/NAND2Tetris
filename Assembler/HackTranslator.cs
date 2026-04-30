using System.Collections.Generic;
using System;

namespace Assembler
{
    //Пишем класс со словарями для перевода инструкций в бинарный код
    public class HackTranslator
    {
        private int _nextVariableAddress = 16;

        private static readonly Dictionary<string, string> destTable = new Dictionary<string, string>
        {
            {"null", "000"}, {"M", "001"}, {"D", "010"}, {"MD", "011"},
            {"A", "100"}, {"AM", "101"}, {"AD", "110"}, {"AMD", "111"}
        };

        private static readonly Dictionary<string, string> compTable = new Dictionary<string, string>
        {
            {"0", "0101010"}, {"1", "0111111"}, {"-1", "0111010"},
            {"D", "0001100"}, {"A", "0110000"}, {"M", "1110000"},
            {"!D", "0001101"}, {"!A", "0110001"}, {"!M", "1110001"},
            {"-D", "0001111"}, {"-A", "0110011"}, {"-M", "1110011"},
            {"D+1", "0011111"}, {"A+1", "0110111"}, {"M+1", "1110111"},
            {"D-1", "0001110"}, {"A-1", "0110010"}, {"M-1", "1110010"},
            {"D+A", "0000010"}, {"D+M", "1000010"},
            {"D-A", "0010011"}, {"D-M", "1010011"},
            {"A-D", "0000111"}, {"M-D", "1000111"},
            {"D&A", "0000000"}, {"D&M", "1000000"},
            {"D|A", "0010101"}, {"D|M", "1010101"}
        };

        private static readonly Dictionary<string, string> jumpTable = new Dictionary<string, string>
        {
            {"null", "000"}, {"JGT", "001"}, {"JEQ", "010"}, {"JGE", "011"},
            {"JLT", "100"}, {"JNE", "101"}, {"JLE", "110"}, {"JMP", "111"}
        };

        public string[] TranslateAsmToHack(string[] instructions, Dictionary<string, int> symbolTable)
        {
            // Создаем массив для хранения переведенных инструкций и пишем цикл для перевода каждой строки
            var hackCode = new string[instructions.Length];
            
            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                if (instr.StartsWith("@"))
                {
                    hackCode[i] = AInstructionToCode(instr, symbolTable);
                }
                else
                {
                    hackCode[i] = CInstructionToCode(instr);
                }
            }
            return hackCode;
        }

        public string AInstructionToCode(string aInstruction, Dictionary<string, int> symbolTable)
        {
            // Удаляем @ и пытаемся преобразовать оставшуюся часть в число. Иначе, значит это символ, который нужно заменить на адрес, если он в табилце, берем его значение, иначе добавляем его в таблицу с новым адресом и возвращаем этот адрес
            var symbol = aInstruction.Substring(1);
            int address;
            
            if (!int.TryParse(symbol, out address))
            {                
                if (symbolTable.ContainsKey(symbol))
                {
                    address = symbolTable[symbol];
                }
                else
                {
                    address = _nextVariableAddress;
                    symbolTable[symbol] = address;
                    _nextVariableAddress++;
                }
            }
            
            return "0" + Convert.ToString(address, 2).PadLeft(15, '0');
        }

        public string CInstructionToCode(string cInstruction)
        {
            //Делим инструкцию на части исходя из = и ; и далее по таблицам переводим их и соединяем
            string dest = "null", comp, jump = "null";
            
            if (cInstruction.Contains("="))
            {
                var parts = cInstruction.Split('=');
                dest = parts[0].Trim();
                cInstruction = parts[1].Trim();
            }
            if (cInstruction.Contains(';'))
            {
                var parts = cInstruction.Split(';');
                comp = parts[0].Trim();
                jump = parts[1].Trim();
            }
            else
            {
                comp = cInstruction;
            }
            
            return "111" + compTable[comp] + destTable[dest] + jumpTable[jump];
        }
    }
}