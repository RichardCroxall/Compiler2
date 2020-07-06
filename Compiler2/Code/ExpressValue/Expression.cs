/*
    \file Expression.cs
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

namespace compiler2.Code.ExpressValue
{
    public enum ExpressionType
    {
        SimpleValue,
        UnaryExpression,
        BinaryExpression,
    }

    public enum UnaryOperator
    {
        Not,
        Negate,
    }

    public enum BinaryOperator
    {
        NotEqual, // != - default for backwards compatibility with original which is effectively if flag != 0 then
        Equal, // ==
        LessThan, //<
        LessThanEqual, // <=
        GreaterThan, //>
        GreaterThanEquals, // >=
        Plus, //+
        Minus, //-
        Times, //*
        Div, // /
        Mod, // %
        LogicalAnd, // &&
        LogicalOr, // ||
        BitwiseAnd, // &
        BitwiseOr, // |
        LeftShift, // <<
        RightShift, // >>
    }

    public class Expression
    {
        private readonly ExpressionType m_ExpressionType;
        private readonly Value m_Value;
        private readonly Expression m_Expression1;
        private readonly Expression m_Expression2;
        private readonly UnaryOperator m_UnaryOperator;
        private readonly BinaryOperator m_BinaryOperator;

        public Expression(Value value)
        {
            m_ExpressionType = ExpressionType.SimpleValue;
            m_Value = value;
        }

        public Expression(UnaryOperator unaryOperator, Expression expression1)
        {
            m_ExpressionType = ExpressionType.UnaryExpression;
            m_UnaryOperator = unaryOperator;
            m_Expression1 = expression1;
        }

        public Expression(Expression expression1, BinaryOperator binaryOperator, Expression expression2)
        {
            m_ExpressionType = ExpressionType.BinaryExpression;
            m_Expression1 = expression1;
            m_BinaryOperator = binaryOperator;
            m_Expression2 = expression2;
        }


        public ExpressionType ExpressionType
        {
            get { return m_ExpressionType; }
        }

        public Value Value
        {
            get
            {
                Debug.Assert(m_ExpressionType == ExpressionType.SimpleValue);
                return m_Value;
            }
        }


        public Expression Expression1
        {
            get
            {
                //Debug.Assert(m_ExpressionType != ExpressionType.SimpleValue);
                return m_Expression1; 
            }
        }


        public Expression Expression2
        {
            get
            {
                Debug.Assert(m_ExpressionType == ExpressionType.BinaryExpression); 
                return m_Expression2;
            }
        }

        public UnaryOperator UnaryOperator
        {
            get 
            {
                Debug.Assert(m_ExpressionType == ExpressionType.UnaryExpression);
                return m_UnaryOperator;
            }
        }

        public BinaryOperator BinaryOperator
        {
            get
            {
                Debug.Assert(m_ExpressionType == ExpressionType.BinaryExpression);
                return m_BinaryOperator;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            switch (m_ExpressionType)
            {
                case ExpressionType.SimpleValue:
                    stringBuilder.AppendFormat("(simple value:{0})", m_Value.IntegerValue);
                    break;

                case ExpressionType.UnaryExpression:
                    stringBuilder.AppendFormat("(unary-op:{0} {1})",m_UnaryOperator, m_Expression2.ToString());
                    break;

                case ExpressionType.BinaryExpression:
                    stringBuilder.AppendFormat("({0} binary-op:{1} {2})", m_Expression1.ToString(), m_BinaryOperator, m_Expression2.ToString());
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            return stringBuilder.ToString();
        }
    }
}
