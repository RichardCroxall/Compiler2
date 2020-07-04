using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    class CodeUndefined : CodeBase
    {
        public CodeUndefined(int declarationLineNumber, int pass, string id) : base(declarationLineNumber, pass, id, 0, IdentifierTypeEnum.IdUndefined)
        {
        }
    }
}
