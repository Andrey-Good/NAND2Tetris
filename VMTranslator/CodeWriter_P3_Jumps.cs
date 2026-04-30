using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции: label, goto, if-goto
    /// </summary>
    private bool TryWriteProgramFlowCode(VmInstruction instruction, string moduleName)
    {
        //Через switch-case сохраняем нужные шаблоны для каждой из 3х инструкций
        switch (instruction.Name)
        {   case "label":
                WriteLabel(instruction.Args[0], moduleName);
                return true;
            case "goto":
                WriteGoto(instruction.Args[0], moduleName);
                return true;
            case "if-goto":
                WriteIfGoto(instruction.Args[0], moduleName);
                return true;
        }
        return false;

        void WriteLabel(string label, string moduleName)
        {
            WriteAsm($"({moduleName}${label})");
        }

        void WriteGoto(string label, string moduleName)
        {
            WriteAsm(
                $"@{moduleName}${label}",
                "0;JMP"
            );
        }

        void WriteIfGoto(string label, string moduleName)
        {
            WriteAsm(
                "@SP",
                "AM=M-1",
                "D=M",
                $"@{moduleName}${label}",
                "D;JNE"
            );
        }
    }
}
