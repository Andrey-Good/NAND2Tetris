using System;
using System.Collections.Generic;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * push [segment] [index] — записывает на стек значение взятое из ячейки [index] сегмента [segment].
    /// * pop [segment] [index] — снимает со стека значение и записывает его в ячейку [index] сегмента [segment].
    ///
    /// Сегменты:
    /// * constant — виртуальный сегмент, по индексу [index] содержит значение [index]
    /// * local — начинается в памяти по адресу Ram[LCL]
    /// * argument — начинается в памяти по адресу Ram[ARG]
    /// * this — начинается в памяти по адресу Ram[THIS]
    /// * that — начинается в памяти по адресу Ram[THAT]
    /// * pointer - по индексу 0, содержит значение Ram[THIS], а по индексу 1 — значение Ram[THAT] 
    /// * temp - начинается в памяти по адресу 5
    /// * static — хранит значения по адресу, который ассемблер выделит переменной @{moduleName}.{index}
    /// </summary>
    /// <returns>
    /// true − если это инструкция работы со стеком, иначе — false.
    /// Если метод возвращает false, он не должен менять ResultAsmCode
    /// </returns>
    private bool TryWriteStackCode(VmInstruction instruction, string moduleName)
    {
        if (instruction.Name != "push" && instruction.Name != "pop")
            return false;
        if (instruction.Args.Length != 2)
            throw new FormatException($"Invalid stack instruction [{instruction}]");

        var segment = instruction.Args[0];
        var index = instruction.Args[1];
        var isPush = instruction.Name == "push";

        if (isPush)
        {
            switch (segment)
            {
                case "constant":
                    WritePushConstant(index);
                    return true;
                case "local":
                    WritePushFromBaseAddress("LCL", index);
                    return true;
                case "argument":
                    WritePushFromBaseAddress("ARG", index);
                    return true;
                case "this":
                    WritePushFromBaseAddress("THIS", index);
                    return true;
                case "that":
                    WritePushFromBaseAddress("THAT", index);
                    return true;
                case "pointer":
                    WritePushPointer(index, instruction);
                    return true;
                case "temp":
                    WritePushFromFixedBaseAddress("5", index);
                    return true;
                case "static":
                    WritePushFromDirectAddress($"{moduleName}.{index}");
                    return true;
                default:
                    throw new FormatException($"Unknown segment [{instruction}]");
            }
        }

        switch (segment)
        {
            case "local":
                WritePopToBaseAddress("LCL", index);
                return true;
            case "argument":
                WritePopToBaseAddress("ARG", index);
                return true;
            case "this":
                WritePopToBaseAddress("THIS", index);
                return true;
            case "that":
                WritePopToBaseAddress("THAT", index);
                return true;
            case "pointer":
                WritePopToPointer(index, instruction);
                return true;
            case "temp":
                WritePopToFixedBaseAddress("5", index);
                return true;
            case "static":
                WritePopToDirectAddress($"{moduleName}.{index}");
                return true;
            case "constant":
                throw new FormatException($"Cannot pop to constant segment [{instruction}]");
            default:
                throw new FormatException($"Unknown segment [{instruction}]");
        }
    }

    private void WritePushConstant(string constant)
    {
        if (short.TryParse(constant, out var value) && value < 0)
        {
            WriteNegativeConstantToD(value);
            WritePushD();
            return;
        }

        WriteAsm($"@{constant}", "D=A");
        WritePushD();
    }

    private void WriteNegativeConstantToD(short value)
    {
        if (value == short.MinValue)
        {
            WriteAsm("@32767", "D=A", "D=D+1");
            return;
        }

        WriteAsm($"@{-value}", "D=A", "D=-D");
    }

    private void WritePushFromBaseAddress(string baseAddress, string index)
    {
        WriteAsm($"@{baseAddress}", "D=M", $"@{index}", "A=D+A", "D=M");
        WritePushD();
    }

    private void WritePushFromFixedBaseAddress(string baseAddress, string index)
    {
        WriteAsm($"@{baseAddress}", "D=A", $"@{index}", "A=D+A", "D=M");
        WritePushD();
    }

    private void WritePushFromDirectAddress(string address)
    {
        WriteAsm($"@{address}", "D=M");
        WritePushD();
    }

    private void WritePushPointer(string index, VmInstruction instruction)
    {
        WriteAsm($"@{GetPointerRegister(index, instruction)}", "D=M");
        WritePushD();
    }

    private void WritePopToBaseAddress(string baseAddress, string index)
    {
        WriteAsm($"@{baseAddress}", "D=M", $"@{index}", "D=D+A", "@R13", "M=D");
        WritePopToD();
        WriteAsm("@R13", "A=M", "M=D");
    }

    private void WritePopToFixedBaseAddress(string baseAddress, string index)
    {
        WriteAsm($"@{baseAddress}", "D=A", $"@{index}", "D=D+A", "@R13", "M=D");
        WritePopToD();
        WriteAsm("@R13", "A=M", "M=D");
    }

    private void WritePopToDirectAddress(string address)
    {
        WritePopToD();
        WriteAsm($"@{address}", "M=D");
    }

    private void WritePopToPointer(string index, VmInstruction instruction)
    {
        WritePopToD();
        WriteAsm($"@{GetPointerRegister(index, instruction)}", "M=D");
    }

    private string GetPointerRegister(string index, VmInstruction instruction)
    {
        return index switch
        {
            "0" => "THIS",
            "1" => "THAT",
            _ => throw new FormatException($"Unknown pointer index [{instruction}]")
        };
    }

    private void WritePushD()
    {
        WriteAsm("@SP", "A=M", "M=D", "@SP", "M=M+1");
    }

    private void WritePopToD()
    {
        WriteAsm("@SP", "AM=M-1", "D=M");
    }
}
