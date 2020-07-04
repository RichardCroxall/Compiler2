using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    class StatementCall : StatementBase
    {
        private readonly CodeAction m_CodeAction;

        public StatementCall(CodeAction codeAction)
            : base(TokenEnum.token_call_action)
        {
            m_CodeAction = codeAction;
        }

        public CodeAction CodeActionValue
        {
            get { return m_CodeAction; }
        } 
    }
}
