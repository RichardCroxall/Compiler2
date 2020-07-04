using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Generate;

namespace compiler2.Code
{
    class CodeSequence
    {
        private readonly TimeSpan m_TimeSpan;
        private readonly dayEnum m_DaysToFire;
        private readonly string m_Description;
        private readonly List<CodeEvent> m_EventList = new List<CodeEvent>();

        public CodeSequence(string description, TimeSpan timeSpan, dayEnum daysToFire, List<CodeEvent> eventList)
        {
            m_Description = description;
            m_TimeSpan = timeSpan;
            m_DaysToFire = daysToFire;
            m_EventList = eventList;
        }

        public string Description
        {
            get { return m_Description; }
        }


        public TimeSpan TimeSpan
        {
            get { return m_TimeSpan; }
        }


        public dayEnum DaysToFire
        {
            get { return m_DaysToFire; }
        }


        internal List<CodeEvent> EventList
        {
            get { return m_EventList; }
        } 

    }
}
