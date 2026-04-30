namespace Assembler
{
    public class Parser
    {
        /// <summary>
        /// Удаляет все комментарии и пустые строки из программы. Удаляет все пробелы из команд.
        /// </summary>
        /// <param name="asmLines">Строки ассемблерного кода</param>
        /// <returns>Только значащие строки строки ассемблерного кода без комментариев и лишних пробелов</returns>
        public string[] RemoveWhitespacesAndComments(string[] asmLines)
        {
            List<string> result = new List<string>();
            foreach (var line in asmLines){
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                {
                    continue; // Пропускаем пустые строки и комментарии
                }
                // Удаляем все пробелы из строки
                string cleanedLine = trimmedLine.Replace(" ", "").Replace("\t", "");
                var commentIndex = cleanedLine.IndexOf("//");
                if (commentIndex != -1)
                {
                    result.Add(cleanedLine.Substring(0, commentIndex));
                }
                else
                {
                    result.Add(cleanedLine);
                }
            }
            return result.ToArray();
        }
    }
}
