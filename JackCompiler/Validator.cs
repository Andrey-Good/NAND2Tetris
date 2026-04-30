using System.Collections.Generic;

namespace JackCompiling;

public record CompilationError(string Message, int LineNumber);

public class Validator
{
    private readonly List<CompilationError> errors = new();
    
    public IReadOnlyList<CompilationError> Errors => errors;

    public void ValidateClass(ClassSyntax classSyntax)
    {
        // Пример добавления ошибки валидации:
        // var lineNumber = subroutine.SubroutineBody.Open.LineNumber;
        // errors.Add(new CompilationError("Not all execution paths return a value", lineNumber));
    }
}
