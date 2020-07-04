using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code
{
    public class CodeRoom : CodeBase
    {
        private static int m_NoEntries = 0;
        private static List<CodeRoom> m_Entries = new List<CodeRoom>();

        public static int NoFlagEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeRoom GetEntry(int index)
        {
            return m_Entries[index];
        }


        private readonly int m_Value;
        private readonly Dictionary<string, CodeDevice> m_CodeDeviceDictionary = new Dictionary<string, CodeDevice>(); 

        public CodeRoom(int declarationLineNumber, int pass, string identifier, int value)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdRoom)
        {
            m_Value = value;
            m_NoEntries++;
            m_Entries.Add(this);
        }

        public int Value
        {
            get { return m_Value; }
        }

        public Dictionary<string, CodeDevice> CodeDeviceDictionary
        {
            get { return m_CodeDeviceDictionary; }
        }


    }
}
