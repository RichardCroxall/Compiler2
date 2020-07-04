using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    class StatementIncrementDecrement : StatementBase
    {
        private readonly CodeFlag m_CodeFlag;
        private int m_Value;


        public StatementIncrementDecrement(CodeFlag codeFlag, int value)
            : base(TokenEnum.token_plusPlus)
        {
            m_CodeFlag = codeFlag;
            Debug.Assert(value == -1 || value == +1);
            m_Value = value;
        }

        public CodeFlag CodeFlag
        {
            get { return m_CodeFlag; }
        }


        public int Value
        {
            get { return m_Value; }
        }
    }
}
