/*
    \file Sequence.cs
    Copyright Notice\n
    Copyright (C) 1995-2020 Richard Croxall - developer and architect\n
    Copyright (C) 1995-2020 Dr Sylvia Croxall - code reviewer and tester\n

    This file is part of Compiler2.

    Compiler2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Compiler2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Compiler2.  If not, see <https://www.gnu.org/licenses/>.
*/
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
