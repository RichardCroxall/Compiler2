/*
    \file CodeTimer.cs
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
using compiler2.Compile;

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
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdProcedure, TypeEnum.TimerType)
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
