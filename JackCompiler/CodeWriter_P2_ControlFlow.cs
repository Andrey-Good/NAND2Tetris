using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>Statement; Statement; ...</summary>
        public void WriteStatements(StatementsSyntax statements)
        {
            throw new NotImplementedException();
        }

        private void WriteStatement(StatementSyntax statement)
        {
            var ok = TryWriteVarAssignmentStatement(statement)
                     || TryWriteProgramFlowStatement(statement)
                     || TryWriteDoStatement(statement) // будет реализована в следующий задачах
                     || TryWriteArrayAssignmentStatement(statement)  // будет реализована в следующий задачах
                     || TryWriteReturnStatement(statement);  // будет реализована в следующий задачах
            if (!ok)
                throw new FormatException($"Unknown statement [{statement}]");
        }

        /// <summary>let VarName = Expression;</summary>
        private bool TryWriteVarAssignmentStatement(StatementSyntax statement)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// if ( Expression ) { Statements } [else { Statements }
        /// while ( Expression ) { Statements }
        /// </summary>
        private bool TryWriteProgramFlowStatement(StatementSyntax statement)
        {
            throw new NotImplementedException();
        }
    }
}
