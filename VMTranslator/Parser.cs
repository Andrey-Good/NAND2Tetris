using System;
using System.Collections.Generic;

namespace VMTranslator;

public class Parser
{
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmLines)
    {
        //Создаем списо для добавления инструкций, после цикл по которому проходимся по всем строкам, если строка пустая или начинается с комментария, то пропускаем ее, иначе парсим строку в инструкцию и добавляем ее в список
        var instructions = new List<VmInstruction>();
        
        for (int i = 0; i < vmLines.Length; i++)
        {
            var line = vmLines[i];
            var trimmedLine = line.Trim();
            
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))            
            {
                continue;
            }
            
            //Убираем комментарии в конце строки, если они есть
            var commentIndex = trimmedLine.IndexOf("//");
            if (commentIndex != -1)            
            {
                trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
            }
            
            //Суем в список VmInstruction, который принимает номер строки, имя инструкции и аргументы.
            var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0];
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
            
            instructions.Add(new VmInstruction(i + 1, name, args));
        }
        
        return instructions.ToArray();
    }
}