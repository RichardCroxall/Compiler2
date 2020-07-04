using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using compiler2.Code.Statement;
using compiler2.Compile;

namespace Compiler.Code.Statement
{
    class StatementResynchClock : StatementBase
    {
        public StatementResynchClock() : base(TokenEnum.token_resynchClock)
        {
        }
    }
}
