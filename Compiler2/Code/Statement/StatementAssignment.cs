using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Code.ExpressValue;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    class StatementAssignment : StatementBase
    {
        private readonly CodeFlag m_CodeFlag;
        private readonly Expression m_Expression;


        public StatementAssignment(CodeFlag codeFlag, Expression expression)
            : base(TokenEnum.token_equals)
        {
            m_CodeFlag = codeFlag;
            m_Expression = expression;
        }

        public CodeFlag CodeFlag
        {
            get { return m_CodeFlag; }
        }


        public Expression Expression
        {
            get { return m_Expression; }
        }
    }
}
