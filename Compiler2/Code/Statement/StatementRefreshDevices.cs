using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using compiler2.Code.Statement;
using compiler2.Compile;

namespace Compiler.Code.Statement
{
    class StatementRefreshDevices : StatementBase
    {
        public StatementRefreshDevices() : base(TokenEnum.token_refreshDevices)
        {
        }
    }
}
