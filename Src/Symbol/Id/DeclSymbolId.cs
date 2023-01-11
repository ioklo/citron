using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    public record class DeclSymbolId(Name ModuleName, DeclSymbolPath? Path)
    {
        public readonly static DeclSymbolId List = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), 1);
        public readonly static DeclSymbolId Seq = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("ISeq"), 1);

        // NullableSymbol과 헷갈릴수 있어서 다른 이름이 필요할 것 같다
        // public readonly static DeclSymbolId Nullable = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Nullable"), 1);
    }

    public static class DeclSymbolIdExtensions
    {
        public static DeclSymbolId GetDeclSymbolId(this SymbolId symbolId)
        {
            var declPath = symbolId.Path.GetDeclSymbolPath();
            return new DeclSymbolId(symbolId.ModuleName, declPath);            
        }

        public static DeclSymbolId Child(this DeclSymbolId declId, Name name, int typeParamCount = 0, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new DeclSymbolId(declId.ModuleName, declId.Path.Child(name, typeParamCount, paramIds));
        }
    }
}
