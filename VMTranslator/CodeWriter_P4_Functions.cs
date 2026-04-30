using System;

namespace VMTranslator;

public partial class CodeWriter
{
    private int functionCallLabelIndex;
    private string currentFunctionName;

    /// <summary>
    /// Добавляет вызов функции Sys.init без аргументов
    /// </summary>
    public void WriteSysInitCall()
    {
        TryWriteFunctionCallCode(new VmInstruction(0, "call", "Sys.init", "0"));
    }

    /// <summary>
    /// Транслирует инструкции call, function, return, tailrec
    /// </summary>
    private bool TryWriteFunctionCallCode(VmInstruction instruction)
    {
        switch (instruction.Name)
        {
            case "call":
                if (instruction.Args.Length != 2)
                    throw new FormatException($"Invalid call instruction [{instruction}]");

                WriteCall(instruction.Args[0], instruction.Args[1]);
                return true;

            case "function":
                if (instruction.Args.Length != 2)
                    throw new FormatException($"Invalid function instruction [{instruction}]");

                WriteFunction(instruction.Args[0], instruction.Args[1]);
                return true;

            case "return":
                if (instruction.Args.Length != 0)
                    throw new FormatException($"Invalid return instruction [{instruction}]");

                WriteReturn();
                return true;

            case "tailrec":
                if (instruction.Args.Length != 2)
                    throw new FormatException($"Invalid tailrec instruction [{instruction}]");

                WriteTailRec(instruction.Args[0], instruction.Args[1], instruction);
                return true;

            default:
                return false;
        }
    }

    private void WriteCall(string functionName, string argsCount)
    {
        var returnLabel = $"RET_{functionCallLabelIndex}";
        functionCallLabelIndex++;

        // Сохраняем адрес возврата
        WriteAsm(
            $"@{returnLabel}",
            "D=A",
            "@SP",
            "A=M",
            "M=D",
            "@SP",
            "M=M+1");

        // Сохраняем кадр вызывающей функции
        PushRegisterValue("LCL");
        PushRegisterValue("ARG");
        PushRegisterValue("THIS");
        PushRegisterValue("THAT");

        // Переставляем ARG и LCL
        WriteAsm(
            "@SP",
            "D=M",
            "@5",
            "D=D-A",
            $"@{argsCount}",
            "D=D-A",
            "@ARG",
            "M=D",
            "@SP",
            "D=M",
            "@LCL",
            "M=D",
            $"@{functionName}",
            "0;JMP",
            $"({returnLabel})");
    }

    private void WriteFunction(string functionName, string localCount)
    {
        currentFunctionName = functionName;
        WriteAsm($"({functionName})");

        var locals = int.Parse(localCount);
        for (var i = 0; i < locals; i++)
        {
            // Инициализируем локальные нулями
            WriteAsm(
                "@0",
                "D=A",
                "@SP",
                "A=M",
                "M=D",
                "@SP",
                "M=M+1");
        }
    }

    private void WriteTailRec(string functionName, string argsCount, VmInstruction instruction)
    {
        if (currentFunctionName != functionName)
            throw new FormatException($"Tail recursion must call current function [{instruction}]");

        var arguments = int.Parse(argsCount);
        if (arguments < 0)
            throw new FormatException($"Invalid tailrec instruction [{instruction}]");

        if (arguments > 0)
        {
            // R13 = ARG + argumentsCount - 1
            WriteAsm(
                $"@{arguments}",
                "D=A",
                "@1",
                "D=D-A",
                "@ARG",
                "D=M+D",
                "@R13",
                "M=D");

            // Переносим аргументы со стека в сегмент ARG
            for (var i = 0; i < arguments; i++)
            {
                WriteAsm(
                    "@SP",
                    "AM=M-1",
                    "D=M",
                    "@R13",
                    "A=M",
                    "M=D",
                    "@R13",
                    "M=M-1");
            }
        }

        // Сбрасываем стек текущего кадра
        WriteAsm(
            "@LCL",
            "D=M",
            "@SP",
            "M=D",
            $"@{functionName}",
            "0;JMP");
    }

    private void WriteReturn()
    {
        // FRAME = LCL
        WriteAsm(
            "@LCL",
            "D=M",
            "@R13",
            "M=D");

        // RET = *(FRAME - 5)
        WriteAsm(
            "@5",
            "A=D-A",
            "D=M",
            "@R14",
            "M=D");

        // *ARG = pop()
        WriteAsm(
            "@SP",
            "AM=M-1",
            "D=M",
            "@ARG",
            "A=M",
            "M=D");

        // SP = ARG + 1
        WriteAsm(
            "@ARG",
            "D=M+1",
            "@SP",
            "M=D");

        // Восстанавливаем сегменты
        RestoreSegment("THAT");
        RestoreSegment("THIS");
        RestoreSegment("ARG");
        RestoreSegment("LCL");

        // Переходим по адресу возврата
        WriteAsm(
            "@R14",
            "A=M",
            "0;JMP");
    }

    private void PushRegisterValue(string registerName)
    {
        WriteAsm(
            $"@{registerName}",
            "D=M",
            "@SP",
            "A=M",
            "M=D",
            "@SP",
            "M=M+1");
    }

    private void RestoreSegment(string segmentName)
    {
        WriteAsm(
            "@R13",
            "AM=M-1",
            "D=M",
            $"@{segmentName}",
            "M=D");
    }
}
