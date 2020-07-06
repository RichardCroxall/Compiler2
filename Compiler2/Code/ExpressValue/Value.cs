/*
    \file Value.cs
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
