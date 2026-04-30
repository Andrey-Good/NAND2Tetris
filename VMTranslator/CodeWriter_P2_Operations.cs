namespace VMTranslator;

public partial class CodeWriter
{
    private int comparisonLabelIndex;

    /// <summary>
    /// Транслирует инструкции:
    /// * арифметических операция: add sub, neg, inc, dec
    /// * логических операций: eq, gt, lt, and, or, not
    /// </summary>
    /// <returns>true − если это логическая или арифметическая инструкция, иначе — false.</returns>
    private bool TryWriteLogicAndArithmeticCode(VmInstruction instruction)
    {
        if (instruction.Args.Length != 0)
            return false;

        switch (instruction.Name)
        {
            case "add":
                WriteBinaryOperation("M=M+D");
                return true;
            case "sub":
                WriteBinaryOperation("M=M-D");
                return true;
            case "and":
                WriteBinaryOperation("M=M&D");
                return true;
            case "or":
                WriteBinaryOperation("M=M|D");
                return true;
            case "neg":
                WriteUnaryOperation("M=-M");
                return true;
            case "inc":
                WriteUnaryOperation("M=M+1");
                return true;
            case "dec":
                WriteUnaryOperation("M=M-1");
                return true;
            case "not":
                WriteUnaryOperation("M=!M");
                return true;
            case "eq":
                WriteComparisonOperation("JEQ");
                return true;
            case "gt":
                WriteComparisonOperation("JGT");
                return true;
            case "lt":
                WriteComparisonOperation("JLT");
                return true;
            default:
                return false;
        }
    }

    private void WriteUnaryOperation(string operation)
    {
        WriteAsm("@SP", "A=M-1", operation);
    }

    private void WriteBinaryOperation(string operation)
    {
        WriteAsm("@SP", "AM=M-1", "D=M", "A=A-1", operation);
    }

    private void WriteComparisonOperation(string jump)
    {
        var trueLabel = $"COMP_TRUE_{comparisonLabelIndex}";
        var endLabel = $"COMP_END_{comparisonLabelIndex}";
        comparisonLabelIndex++;

        WriteAsm(
            "@SP",
            "AM=M-1",
            "D=M",
            "A=A-1",
            "D=M-D",
            $"@{trueLabel}",
            $"D;{jump}",
            "@SP",
            "A=M-1",
            "M=0",
            $"@{endLabel}",
            "0;JMP",
            $"({trueLabel})",
            "@SP",
            "A=M-1",
            "M=-1",
            $"({endLabel})");
    }
}
