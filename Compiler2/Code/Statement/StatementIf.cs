using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Code.ExpressValue;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    class StatementIf : StatementBase
    {
        private readonly Expression m_Condition;
        private readonly List<StatementBase> m_ThenStatementList;
        private readonly List<StatementBase> m_ElseStatementList;


        public StatementIf(Expression condition, List<StatementBase> thenStatementList, List<StatementBase> elseStatementList)
            : base(TokenEnum.token_if)
        {
            m_Condition = condition;
            m_ThenStatementList = thenStatementList;
            m_ElseStatementList = elseStatementList;
        }

        public Expression Condition
        {
            get { return m_Condition; }
        }


        public List<StatementBase> ThenStatementList
        {
            get { return m_ThenStatementList; }
        }


        public List<StatementBase> ElseStatementList
        {
            get { return m_ElseStatementList; }
        }

    }
}
