using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    class CodeEvent
    {
        private readonly TimeSpan m_TimeSpan;
        private readonly CodeAction m_CodeAction;

        public CodeEvent(TimeSpan timeSpan, CodeAction codeAction)
        {
            m_TimeSpan = timeSpan;
            m_CodeAction = codeAction;
        }

        public TimeSpan TimeSpanValue
        {
            get { return m_TimeSpan; }
        }


        public CodeAction CodeActionValue
        {
            get { return m_CodeAction; }
        }
    }
}
