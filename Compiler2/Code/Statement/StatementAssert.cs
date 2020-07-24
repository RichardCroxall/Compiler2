using System;
using System.Collections.Generic;
using System.Text;
using compiler2.Code;
using compiler2.Code.ExpressValue;
using compiler2.Code.Statement;
using compiler2.Compile;

namespace Compiler2.Code.Statement
{
    class StatementAssert : StatementBase
    {
        private readonly Expression m_Expression;


        public StatementAssert(Expression expression)
            : base(TokenEnum.token_error)
        {
            m_Expression = expression;
        }

        public Expression Expression
        {
            get { return m_Expression; }
        }
    }
}
