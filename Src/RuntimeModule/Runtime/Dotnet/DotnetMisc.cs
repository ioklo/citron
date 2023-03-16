using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Citron.Runtime.Dotnet
{
    static class DotnetMisc
    {
        public static ItemId? MakeTypeId(TypeId type)
        {
            // ([^\s]+(`(\d+))?)([\.+][^\s]+(`(\d+))?)+
            // C20200622_Reflection.Type1`1+Type2`1<T, U>
            // System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
            // System.Collections.Generic.List`1

            var elems = type.FullName.Split('.', '+');
            ItemId? curId = null;

            foreach (var elem in elems)
            {
                var match = Regex.Match(elem, @"(?<Name>[^`]+)(`(?<TypeParamCount>\d+))?");

                if (!match.Success) continue;

                var name = match.Groups["Name"].Value;
                var typeParamCountText = match.Groups["TypeParamCount"].Value;

                int typeParamCount = 0;
                if (typeParamCountText.Length != 0)
                    typeParamCount = int.Parse(typeParamCountText);

                if (curId != null)
                    curId = curId.Append(name, typeParamCount);
                else
                    curId = new ItemId(name, typeParamCount);
            }

            return curId;
        }

    }
}
