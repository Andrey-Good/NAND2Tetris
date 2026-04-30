using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// class Name { ... }
        /// </summary>
        public void WriteClass(ClassSyntax classSyntax)
        {
        }

        /// <summary>
        /// method Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteMethod(SubroutineDecSyntax subroutine)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// function Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteFunction(SubroutineDecSyntax subroutine)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// constructor Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteConstructor(SubroutineDecSyntax subroutine)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ObjOrClassName . SubroutineName ( ExpressionList ) 
        /// </summary>
        private bool TryWriteSubroutineCall(TermSyntax term)
        {
            return false;
        }

        /// <summary>
        /// do SubroutineCall ; 
        /// </summary>
        private bool TryWriteDoStatement(StatementSyntax statement)
        {
            return false;
        }

        /// <summary>
        /// return ;
        /// return Expression ;
        /// </summary>
        private bool TryWriteReturnStatement(StatementSyntax statement)
        {
            return false;
        }

        /// <summary>
        /// this | null
        /// </summary>
        private bool TryWriteObjectValue(TermSyntax term)
        {
            return false;
        }
    }
}
