using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Generate;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    class StatementResetTimeout : StatementBase
    {
        private readonly CodeTimeout m_CodeTimeout;
        private readonly TimeSpan m_TimeSpan;

        public StatementResetTimeout(CodeTimeout codeTimeout, TimeSpan timeSpan)
            : base(TokenEnum.token_reset)
        {
            m_CodeTimeout = codeTimeout;
            m_TimeSpan = timeSpan;
        }


        public CodeTimeout CodeTimeoutValue
        {
            get { return m_CodeTimeout; }
        }

        public TimeSpan TimeSpanValue
        {
            get { return m_TimeSpan; }
        } 
    }
}
