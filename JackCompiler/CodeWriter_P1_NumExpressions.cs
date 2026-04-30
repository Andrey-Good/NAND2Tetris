using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>2+x</summary>
        public void WriteExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        private void WriteTerm(TermSyntax term)
        {
            var ok = TryWriteStringValue(term) // будет реализована в следующих задачах
                     || TryWriteArrayAccess(term) // будет реализована в следующих задачах
                     || TryWriteObjectValue(term) // будет реализована в следующих задачах
                     || TryWriteSubroutineCall(term) // будет реализована в следующих задачах
                     || TryWriteNumericTerm(term);
            if (!ok)
                throw new FormatException($"Unknown term [{term}]");
        }

        /// <summary>42 | true | false | varName | -x | ( x )</summary>
        private bool TryWriteNumericTerm(TermSyntax term)
        {
            return false;
        }
    }
}
