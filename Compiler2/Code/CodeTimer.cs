using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    class CodeTimer : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeTimer> m_Entries = new List<CodeTimer>();

        public static int NoTimerEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeTimer GetEntry(int index)
        {
            return m_Entries[index];
        }

        private readonly List<CodeSequence> m_SequenceList = new List<CodeSequence>();



        public CodeTimer(int declarationLineNumber, int pass, string identifier, List<CodeSequence> sequenceList)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdAction)
        {
            m_SequenceList = sequenceList;

            if (pass == 2)
            {
                m_Entries.Add(this);
                m_NoEntries++;
            }
        }

        internal List<CodeSequence> SequenceList
        {
            get { return m_SequenceList; }
        }
    }
}
