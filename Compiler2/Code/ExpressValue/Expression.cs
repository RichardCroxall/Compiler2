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
