/*
    \file CodeBase.cs
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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    public enum IdentifierTypeEnum
    {
        IdRoom,
        IdProcedure,
        IdHouseCode,
        IdDevice,
        IdTimeout,
        IdFlag,
        IdConst,
        IdUndefined,
    }

    public abstract class CodeBase
    {
        private readonly string m_Identifier;
        private readonly int _pass;
        private readonly int m_EntryNo;
        private readonly IdentifierTypeEnum m_IdentifierTypeEnum;
        private int m_UseCount = 0;
        private readonly int _declarationLineNumber;

        protected CodeBase(int declarationLineNumber, int pass, string identifier, int entryNo, IdentifierTypeEnum identifierTypeEnum)
        {
            _declarationLineNumber = declarationLineNumber;
            _pass = pass;
            m_Identifier = identifier;
            m_EntryNo = entryNo;
            m_IdentifierTypeEnum = identifierTypeEnum;
        }

        public int Pass { get { return _pass; } }

        public void NoteUsage()
        {
            m_UseCount++;
        }

        public string Identifier
        {
            get { return m_Identifier; }
        }

        public short EntryNo
        {
            get { return (short) m_EntryNo; }
        }

        public IdentifierTypeEnum IdentifierType
        {
            get { return m_IdentifierTypeEnum; }
        }

        public int UseCount
        {
            get { return m_UseCount; }
        }

        public int DeclarationLineNumber { get { return _declarationLineNumber; } }
    }
}
