/*
    \file CodeHouseCode.cs
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
    public class CodeHouseCode : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeHouseCode> m_Entries = new List<CodeHouseCode>();

        public static int NoHouseCodeEntries
        {
            get { return CodeHouseCode.m_NoEntries; }
        }

        public static CodeHouseCode GetEntry(int index)
        {
            return m_Entries[index];
        }

        public static CodeHouseCode GetEntry(char houseCode)
        {
            CodeHouseCode resultCodeHouseCode = null;

            for (int i = 0; i < m_NoEntries; i++)
            {
                if (m_Entries[i].HouseCode == houseCode)
                {
                    resultCodeHouseCode = m_Entries[i];
                }
            }
            return resultCodeHouseCode;
        }

        private readonly char m_HouseCode;
        private readonly string m_Name;
        private readonly CodeProcedure m_OffProcedure;
        private readonly CodeProcedure m_OnProcedure;


        public CodeHouseCode(int declarationLineNumber, int pass, char houseCode, string identifier, CodeProcedure offProcedure, CodeProcedure onProcedure)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdHouseCode)
        {

            Debug.Assert(houseCode >= 'A' && houseCode <= 'P');
            m_HouseCode = houseCode;
            m_Name = identifier;
            m_OffProcedure = offProcedure;
            m_OnProcedure = onProcedure;

            m_Entries.Add(this);
            m_NoEntries++;
        }

        public char HouseCode
        {
            get { return m_HouseCode; }
        }


        public string Name
        {
            get { return m_Name; }
        }


        public CodeProcedure OffProcedure
        {
            get { return m_OffProcedure; }
        }


        public CodeProcedure OnProcedure
        {
            get { return m_OnProcedure; }
        }

    }
}
