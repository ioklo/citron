using Citron.Collections;
using Citron.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
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

    // MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
    // declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
    // public record TypeVarSymbolId(int Index) : SymbolId;    
    // => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
    // => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
    // => TypeVarSymbolId(5)로 참조하게 한다
    public record TypeVarSymbolId(int Index) : SymbolId;

    public record NullableSymbolId(SymbolId InnerTypeId) : SymbolId;

    public record VoidSymbolId : SymbolId;

    public record TupleSymbolId(ImmutableArray<(SymbolId TypeId, Name Name)> MemberVarIds) : SymbolId;

    public record VarSymbolId : SymbolId;

    public static class SymbolIdExtensions
    {
        public static ModuleSymbolId GetOuter(this ModuleSymbolId id)
        {
            Debug.Assert(id.Path != null);

            return new ModuleSymbolId(id.ModuleName, id.Path.Outer);
        }

        public static ModuleSymbolId Child(this ModuleSymbolId id, Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new ModuleSymbolId(id.ModuleName, id.Path.Child(name, typeArgs, paramIds));
        }

        public static bool IsList(this SymbolId symbolId, [NotNullWhen(returnValue: true)]out SymbolId? itemId)
        {
            if (symbolId is ModuleSymbolId moduleSymbolId)
            {
                var declSymbolId = moduleSymbolId.GetDeclSymbolId();
                if (declSymbolId.Equals(DeclSymbolId.List))
                {
                    Debug.Assert(moduleSymbolId.Path != null);

                    itemId = moduleSymbolId.Path.TypeArgs[0];
                    return true;
                }
            }

            itemId = null;
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
