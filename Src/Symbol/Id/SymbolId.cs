using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Collections;
using Citron.Infra;

namespace Citron.Symbol
{
    // 'ModuleSymbol' Id
    public record class SymbolId(Name ModuleName, SymbolPath? Path) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(ModuleName), ModuleName);
            context.SerializeRef(nameof(Path), Path);
        }
    }

    public static class SymbolIdExtensions
    {
        public static SymbolId GetOuter(this SymbolId id)
        {
            Debug.Assert(id.Path != null);

            return new SymbolId(id.ModuleName, id.Path.Outer);
        }

        public static SymbolId Child(this SymbolId id, Name name, ImmutableArray<TypeId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new SymbolId(id.ModuleName, id.Path.Child(name, typeArgs, paramIds));
        }

        public static bool IsList(this SymbolId symbolId, [NotNullWhen(returnValue: true)]out TypeId? itemId)
        {
            var declSymbolId = symbolId.GetDeclSymbolId();
            if (declSymbolId.Equals(DeclSymbolId.List))
            {
                Debug.Assert(symbolId.Path != null);

                itemId = symbolId.Path.TypeArgs[0];
                return true;
            }            

            itemId = null;
            return false;
        }

        // ISeq 타입인지
        public static bool IsSeq(this SymbolId symbolId)
        {
            var declSymbolId = symbolId.GetDeclSymbolId();
            return declSymbolId.Equals(DeclSymbolId.Seq);
        }

        // list의 anonymous 타입은 기각, seq<T>인지 확인해야 한다
        //public static bool IsListIter(this SymbolId symbolId)
        //{
        //    if (symbolId is ModuleSymbolId moduleSymbolId)
        //    {
        //        var declSymbolId = moduleSymbolId.GetDeclSymbolId();
        //        return declSymbolId.Equals(DeclSymbol);
        //    }

        //    return false;
        //}
    }
}
