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
        private readonly CodeAction m_OffAction;
        private readonly CodeAction m_OnAction;


        public CodeHouseCode(int declarationLineNumber, int pass, char houseCode, string identifier, CodeAction offAction, CodeAction onAction)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdHouseCode)
        {

            Debug.Assert(houseCode >= 'A' && houseCode <= 'P');
            m_HouseCode = houseCode;
            m_Name = identifier;
            m_OffAction = offAction;
            m_OnAction = onAction;

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


        public CodeAction OffAction
        {
            get { return m_OffAction; }
        }


        public CodeAction OnAction
        {
            get { return m_OnAction; }
        }

    }
}
