using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    public class CodeConst : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeConst> m_Entries = new List<CodeConst>();

        public static int NoFlagEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeConst GetEntry(int index)
        {
            return m_Entries[index];
        }


        private readonly int m_Value;

        public CodeConst(int declarationLineNumber, int pass, string identifier, int value)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdConst)
        {
            m_Value = value;
            m_NoEntries++;
            m_Entries.Add(this);
        }

        public int Value
        {
            get { return m_Value; }
        }

    }
}

