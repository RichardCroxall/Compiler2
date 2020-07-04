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
        IdAction,
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
