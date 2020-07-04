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
