using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// "string constant"
        /// </summary>
        private bool TryWriteStringValue(TermSyntax term)
        {
            return false;
        }

        /// <summary>
        /// arr[index]
        /// </summary>
        private bool TryWriteArrayAccess(TermSyntax term)
        {
            return false;
        }

        /// <summary>
        /// let arr[index] = expr;
        /// </summary>
        private bool TryWriteArrayAssignmentStatement(StatementSyntax statement)
        {
            return false;
        }
    }
}
