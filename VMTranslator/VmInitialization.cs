using System;

namespace VMTranslator;

public static class VmInitialization
{
    public const int Sp = 256;
    public const int Local = 300;
    public const int Argument = 400;
    public const int This = 3000;
    public const int That = 3010;


    /// <summary>
    /// Генерирует код инициализации значения регистров SP, LCL, ARG, THIS, THAT в их начальные значения (константы выше)
    /// </summary>
    public static void WriteMemoryInitialization(this CodeWriter translator)
    {
        translator.ResultAsmCode.AddRange(new[]
        {
            $"@{Sp}", "D=A", "@SP", "M=D",
            $"@{Local}", "D=A", "@LCL", "M=D",
            $"@{Argument}", "D=A", "@ARG", "M=D",
            $"@{This}", "D=A", "@THIS", "M=D",
            $"@{That}", "D=A", "@THAT", "M=D",
        });
    }
}
