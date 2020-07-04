using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Compile;

namespace compiler2.Code.Statement
{
    abstract class StatementBase
    {
        private readonly TokenEnum m_TokenEnum;


        protected StatementBase(TokenEnum tokenEnum)
        {
            m_TokenEnum = tokenEnum;
        }

        public TokenEnum TokenEnumValue
        {
            get { return m_TokenEnum; }
        }

    }
}
