using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using compiler2.Generate;

namespace compiler2.Code
{
    class CodeTimeout : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeTimeout> m_Entries = new List<CodeTimeout>();

        public static int NoTimeoutEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeTimeout GetEntry(int index)
        {
            return m_Entries[index];
        }

        private readonly TimeSpan m_DefaultDurationTimeSpan;
        private readonly CodeAction m_OffAction;

        public CodeTimeout(int declarationLineNumber, int pass, string identifier, TimeSpan defaultDurationDefaultDurationTimeSpan, CodeAction offAction)
            :base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdTimeout)
        {
            m_DefaultDurationTimeSpan = defaultDurationDefaultDurationTimeSpan;
            m_OffAction = offAction;
            m_NoEntries++;
            m_Entries.Add(this);
        }

        public TimeSpan DefaultDurationTimeSpan
        {
            get { return m_DefaultDurationTimeSpan; }
        }

        public CodeAction /*ActionCode*/ OffAction
        {
            get { return m_OffAction; }
        }
    }
}

