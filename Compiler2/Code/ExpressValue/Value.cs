using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compiler2.Code.ExpressValue
{
    public enum SimpleValueType
    {
        SimpleConstant,
        Variable,
        Device,
    }

    public class Value
    {
        private readonly SimpleValueType m_ValueType;
        private readonly int m_IntegerValue;
        private readonly CodeDevice m_CodeDevice;

        public SimpleValueType ValueType
        {
            get { return m_ValueType; }
        }


        public int IntegerValue
        {
            get { return m_IntegerValue; }
        }

        private readonly CodeFlag m_CodeFlag;

        public CodeFlag CodeFlag
        {
            get { return m_CodeFlag; }
        } 

        public CodeDevice CodeDevice
        { 
            get {return m_CodeDevice;}
        }


        public Value(int integerValue)
        {
            m_ValueType = SimpleValueType.SimpleConstant;
            m_IntegerValue = integerValue;
        }

        public Value(CodeFlag codeFlag)
        {
            m_ValueType = SimpleValueType.Variable;
            m_CodeFlag = codeFlag;
        }

        public Value(CodeDevice codeDevice)
        {
            m_ValueType = SimpleValueType.Device;
            m_CodeDevice = codeDevice;
        }
    }
}
