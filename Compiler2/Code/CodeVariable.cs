/*
    \file CodeFlag.cs
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
    public class CodeVariable : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeVariable> m_Entries = new List<CodeVariable>();

        public static int NoFlagEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeVariable GetEntry(int index)
        {
            return m_Entries[index];
        }


        private readonly int m_InitialValue;
        private readonly TypeEnum m_TypeEnum;

        public CodeVariable(int declarationLineNumber, int pass, string identifier, bool initialValue, TypeEnum typeEnum = TypeEnum.BoolType)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdBool, typeEnum)
        {
            m_InitialValue = initialValue ? 1 : 0;
            m_NoEntries++;
            m_TypeEnum = typeEnum;
            m_Entries.Add(this);
        }

        public CodeVariable(int declarationLineNumber, int pass, string identifier, int initialValue, TypeEnum typeEnum = TypeEnum.IntType)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdBool, typeEnum)
        {
            m_InitialValue = initialValue;
            m_NoEntries++;
            m_TypeEnum = typeEnum;
            m_Entries.Add(this);
        }

        public int InitialValue
        {
            get { return m_InitialValue; }
        }

        public TypeEnum GetTypeEnum
        {
            get { return m_TypeEnum; }
        }
    }
}
