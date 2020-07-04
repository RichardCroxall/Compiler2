using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    public class CodeFlag : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeFlag> m_Entries = new List<CodeFlag>();

        public static int NoFlagEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeFlag GetEntry(int index)
        {
            return m_Entries[index];
        }


        private readonly int m_InitialValue;


        public CodeFlag(int declarationLineNumber, int pass, string identifier, bool initialValue)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdFlag)
        {
            m_InitialValue = initialValue ? 1 : 0;
            m_NoEntries++;
            m_Entries.Add(this);
        }

        public CodeFlag(int declarationLineNumber, int pass, string identifier, int initialValue)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdFlag)
        {
            m_InitialValue = initialValue;
            m_NoEntries++;
            m_Entries.Add(this);
        }

        public int InitialValue
        {
            get { return m_InitialValue; }
        }

    }
}
