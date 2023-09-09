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
using compiler2.Compile;

namespace compiler2.Code.ExpressValue
{
    public enum SimpleValueType
    {
        SimpleConstant,
        Variable,
        Device,
        Timeout,
    }

    public class Value
    {
        private readonly TypeEnum m_TypeEnum;
        private readonly SimpleValueType m_ValueType;
        private readonly int m_IntegerValue;
        private readonly CodeDevice m_CodeDevice;
        private readonly CodeTimeout m_CodeTimeout;

        public SimpleValueType ValueType
        {
            get { return m_ValueType; }
        }


        public int IntegerValue
        {
            get { return m_IntegerValue; }
        }

        public TypeEnum GetTypeEnum
        {
            get { return m_TypeEnum; }
        }

        private readonly CodeVariable m_CodeVariable;

        public CodeVariable CodeVariable
        {
            get { return m_CodeVariable; }
        } 

        public CodeDevice CodeDevice
        { 
            get {return m_CodeDevice;}
        }

        public CodeTimeout CodeTimeout
        {
            get { return m_CodeTimeout; }
        }

        public Value(TypeEnum typeEnum, int integerValue)
        {
            m_ValueType = SimpleValueType.SimpleConstant;
            m_IntegerValue = integerValue;
            m_TypeEnum = typeEnum;
        }

        public Value(int integerValue)
        {
            m_ValueType = SimpleValueType.SimpleConstant;
            m_IntegerValue = integerValue;
            m_TypeEnum = TypeEnum.IntType;
        }

        public Value(bool boolValue)
        {
            m_ValueType = SimpleValueType.SimpleConstant;
            m_IntegerValue = boolValue ? 1 : 0;
            m_TypeEnum = TypeEnum.BoolType;
        }

        
        public Value(CodeConst codeConst)
        {
            m_ValueType = SimpleValueType.SimpleConstant;
            m_IntegerValue = codeConst.Value;
            m_TypeEnum = codeConst.GetTypeEnum;
        }
        public Value(CodeVariable codeVariable)
        {
            m_ValueType = SimpleValueType.Variable;
            m_CodeVariable = codeVariable;
            m_TypeEnum = codeVariable.GetTypeEnum;
        }

        public Value(CodeDevice codeDevice)
        {
            m_ValueType = SimpleValueType.Device;
            m_CodeDevice = codeDevice;
            m_TypeEnum = TypeEnum.DeviceType;
        }

        public Value(CodeTimeout codeTimeout)
        {
            m_ValueType = SimpleValueType.Timeout;
            m_CodeTimeout = codeTimeout;
            m_TypeEnum = TypeEnum.TimeoutType;
        }

    }
}
