﻿/*
    \file TokenSet.cs
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

namespace compiler2.Compile
{
    class TokenSet
    {
        public static HashSet<TokenEnum> Set(TokenEnum tokenEnum)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.Add(tokenEnum);
            return result;
        }

        public static HashSet<TokenEnum> Set(TokenEnum tokenEnum1, TokenEnum tokenEnum2)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.Add(tokenEnum1);
            result.Add(tokenEnum2);
            return result;
        }

        public static HashSet<TokenEnum> Set(TokenEnum tokenEnum1, TokenEnum tokenEnum2, TokenEnum tokenEnum3)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.Add(tokenEnum1);
            result.Add(tokenEnum2);
            result.Add(tokenEnum3);
            return result;
        }

        public static HashSet<TokenEnum> Set(params TokenEnum[] tokenEnums)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            foreach (TokenEnum tokenEnum in tokenEnums)
            {
                result.Add(tokenEnum);
            }
            return result;
        }

        public static HashSet<TokenEnum> Set(HashSet<TokenEnum> set1, HashSet<TokenEnum> set2)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.UnionWith(set1);
            result.UnionWith(set2);
            return result;
        }

        public static HashSet<TokenEnum> Set(HashSet<TokenEnum> set1, TokenEnum tokenEnum1)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.UnionWith(set1);
            result.Add(tokenEnum1);
            return result;
        }
        public static HashSet<TokenEnum> Set(HashSet<TokenEnum> set1, TokenEnum tokenEnum1, TokenEnum tokenEnum2)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.UnionWith(set1);
            result.Add(tokenEnum1);
            result.Add(tokenEnum2);
            return result;
        }

        public static HashSet<TokenEnum> Set(HashSet<TokenEnum> set1, params TokenEnum[] tokenEnums)
        {
            HashSet<TokenEnum> result = new HashSet<TokenEnum>();
            result.UnionWith(set1);

            foreach (TokenEnum tokenEnum in tokenEnums)
            {
                result.Add(tokenEnum);
            }
            return result;
        }

    }
}
