using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compiler2.Code.Statement;

namespace compiler2.Code
{
    public class CodeAction : CodeBase
    {
        static private int m_NoEntries = 0;
        static private readonly List<CodeAction> m_Entries = new List<CodeAction>();

        public static int NoActionEntries
        {
            get { return m_NoEntries; }
        }

        public static CodeAction GetEntry(int index)
        {
            return m_Entries[index];
        }


        private List<StatementBase> m_StatementList;

        internal List<StatementBase> StatementList
        {
            get { return m_StatementList; }
        }

        public CodeAction(int declarationLineNumber, int pass, string identifier)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdAction)
        {
            m_Entries.Add(this);
            m_NoEntries++;
        }

        internal void SetStatementList(List<StatementBase> statementList)
        {
            m_StatementList = statementList;
        }
    }
}
