using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    public class Misc
    {
        static void GetTypeValueNormalEntryString(AppliedItemPathEntry entry, StringBuilder sb)
        {
            sb.Append(entry.Name);

            if (entry.TypeArgs.Length != 0)
            {
                sb.Append('<');

                bool bFirst = true;
                foreach (var typeArg in entry.TypeArgs)
                {
                    if (bFirst) bFirst = false;
                    else sb.Append(',');

                    GetTypeValueString(typeArg, sb);
                }

                sb.Append('>');
            }
        }

        // [ModuleName].Namespace.Namespace.Type...


        static void GetTypeValueString(TypeValue typeValue, StringBuilder sb)
        {
            switch (typeValue)
            {
                case TypeValue.Var _:
                    throw new InvalidOperationException();

                case TypeValue.TypeVar typeVar:
                    sb.Append($"`{typeVar.Depth}{typeVar.Name}");
                    return;

                case TypeValue.Normal normal:
                    // [ModuleName]
                    sb.Append($"[{normal.ModuleName}]");

                    // Namespace
                    sb.AppendJoin('.', normal.NamespacePath.Entries.Select(entry => entry.Value));

                    if (normal.NamespacePath.Entries.Length != 0)
                        sb.Append('.');

                    // A<B>.C<D>
                    bool bFirst = true;
                    foreach (var outerEntry in normal.OuterEntries)
                    {
                        if (bFirst) bFirst = false;
                        else sb.Append('.');
                        GetTypeValueNormalEntryString(outerEntry, sb);
                    }

                    // E            
                    if (normal.OuterEntries.Length != 0)
                        sb.Append('.');

                    GetTypeValueNormalEntryString(normal.Entry, sb);
                    return;

                case TypeValue.Void _:
                    sb.Append("void");
                    return;

                case TypeValue.Func _:
                    throw new InvalidOperationException();

                case TypeValue.EnumElem _:
                    throw new InvalidOperationException();
            }
        }
        static string GetTypeValueString(TypeValue typeValue)
        {
            var sb = new StringBuilder();
            GetTypeValueString(typeValue, sb);
            return sb.ToString();
        }


        public static string MakeParamHash(IEnumerable<TypeValue> typeValues)
        {
            var sb = new StringBuilder();

            bool bFirst = true;
            foreach (var typeValue in typeValues)
            {
                if (bFirst) bFirst = false;
                else sb.Append(" * ");

                GetTypeValueString(typeValue, sb);
            }

            return sb.ToString();
        }
    }
}
