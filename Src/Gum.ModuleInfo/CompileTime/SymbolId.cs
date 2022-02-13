using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    // Module, SymbolPath
    public record SymbolId
    {
        //boolDeclId = new DeclSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Boolean"));
        //boolId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Boolean"));

        //intDeclId = new DeclSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Int32"));
        //intId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Int32"));

        //stringDeclId = new DeclSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("String"));
        //stringId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("String"));

        //listDeclId = new DeclSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("List"), 1);
        //nullableDeclId = new DeclSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Nullable"), 1);

        public readonly static SymbolId Void = new VoidSymbolId();
        public readonly static SymbolId Bool = new ModuleSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));
        public readonly static SymbolId Int = new ModuleSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));
        public readonly static SymbolId String = new ModuleSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));

    }

    // 'ModuleSymbol' Id
    public record ModuleSymbolId(Name ModuleName, SymbolPath? Path) : SymbolId;

    public record TypeVarSymbolId(DeclSymbolId DeclSymbolId, int Index) : SymbolId;
    
    public record NullableSymbolId(SymbolId InnerTypeId) : SymbolId;

    public record VoidSymbolId : SymbolId;

    public record TupleSymbolId(ImmutableArray<(SymbolId TypeId, Name Name)> MemberVarIds) : SymbolId;

    public record VarSymbolId : SymbolId;

    public static class SymbolIdExtensions
    {
        public static ModuleSymbolId Child(this ModuleSymbolId id, Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new ModuleSymbolId(id.ModuleName, id.Path.Child(name, typeArgs, paramIds));
        }

        public static bool IsList(this SymbolId symbolId)
        {
            if (symbolId is ModuleSymbolId moduleSymbolId)
            {
                var declSymbolId = moduleSymbolId.GetDeclSymbolId();
                return declSymbolId.Equals(DeclSymbolId.List);
            }

            return false;
        }

        // ISeq 타입인지
        public static bool IsSeq(this SymbolId symbolId)
        {
            if (symbolId is ModuleSymbolId moduleSymbolId)
            {
                var declSymbolId = moduleSymbolId.GetDeclSymbolId();
                return declSymbolId.Equals(DeclSymbolId.Seq);
            }

            return false;
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
