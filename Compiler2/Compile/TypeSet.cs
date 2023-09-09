/*
    \file TypeSet.cs
    Copyright Notice\n
    Copyright (C) 1995-2023 Richard Croxall - developer and architect\n
    Copyright (C) 1995-2023 Dr Sylvia Croxall - code reviewer and tester\n

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
    class TypeSet
    {
        public static HashSet<TypeEnum> Set(TypeEnum typeEnum)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.Add(typeEnum);
            return result;
        }

        public static HashSet<TypeEnum> Set(TypeEnum typeEnum1, TypeEnum typeEnum2)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.Add(typeEnum1);
            result.Add(typeEnum2);
            return result;
        }

        public static HashSet<TypeEnum> Set(TypeEnum typeEnum1, TypeEnum typeEnum2, TypeEnum typeEnum3)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.Add(typeEnum1);
            result.Add(typeEnum2);
            result.Add(typeEnum3);
            return result;
        }

        public static HashSet<TypeEnum> Set(params TypeEnum[] typeEnums)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            foreach (TypeEnum typeEnum in typeEnums)
            {
                result.Add(typeEnum);
            }
            return result;
        }

        public static HashSet<TypeEnum> Set(HashSet<TypeEnum> set1, HashSet<TypeEnum> set2)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.UnionWith(set1);
            result.UnionWith(set2);
            return result;
        }

        public static HashSet<TypeEnum> Set(HashSet<TypeEnum> set1, TypeEnum typeEnum1)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.UnionWith(set1);
            result.Add(typeEnum1);
            return result;
        }
        public static HashSet<TypeEnum> Set(HashSet<TypeEnum> set1, TypeEnum typeEnum1, TypeEnum typeEnum2)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.UnionWith(set1);
            result.Add(typeEnum1);
            result.Add(typeEnum2);
            return result;
        }

        public static HashSet<TypeEnum> Set(HashSet<TypeEnum> set1, params TypeEnum[] typeEnums)
        {
            HashSet<TypeEnum> result = new HashSet<TypeEnum>();
            result.UnionWith(set1);

            foreach (TypeEnum typeEnum in typeEnums)
            {
                result.Add(typeEnum);
            }
            return result;
        }

    }
}
