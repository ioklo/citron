using Citron.Collections;
using Citron.Symbol;
using Pretune;
using R = Citron.IR0;

namespace Citron.Analysis;

// IntermediateExp
// Resolve하는데 필요한 중간과정
// Namespace는 MemberExp의 Parent의 결과로 만들어질 수 있으니 존재한다
abstract partial class IntermediateExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IIntermediateExpVisitor<TResult>;

    public interface IFuncs<TFuncDeclSymbol, TFuncSymbol>
        where TFuncDeclSymbol : IFuncDeclSymbol
        where TFuncSymbol : IFuncSymbol
    {
        int GetCount();
        TFuncDeclSymbol GetDecl(int i);

        ImmutableArray<IType> GetPartialTypeArgs();
        TFuncSymbol MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context);
    }

    [AutoConstructor]
    public partial class Namespace : IntermediateExp
    {
        public NamespaceSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
    }

    public class GlobalFuncs : IntermediateExp, IFuncs<GlobalFuncDeclSymbol, GlobalFuncSymbol>
    {
        FuncsWithPartialTypeArgsComponent<GlobalFuncDeclSymbol, GlobalFuncSymbol> component;

        public GlobalFuncs(
            ImmutableArray<(ISymbolNode Outer, GlobalFuncDeclSymbol DeclSymbol)> outerAndDeclSymbols,
            ImmutableArray<IType> partialTypeArgs)
        {
            component = new FuncsWithPartialTypeArgsComponent<GlobalFuncDeclSymbol, GlobalFuncSymbol>(outerAndDeclSymbols, partialTypeArgs);
        }

        int IFuncs<GlobalFuncDeclSymbol, GlobalFuncSymbol>.GetCount() => component.GetCount();
        GlobalFuncDeclSymbol IFuncs<GlobalFuncDeclSymbol, GlobalFuncSymbol>.GetDecl(int i) => component.GetDecl(i);
        GlobalFuncSymbol IFuncs<GlobalFuncDeclSymbol, GlobalFuncSymbol>.MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context) => component.MakeSymbol(i, typeArgs, context);
        ImmutableArray<IType> IFuncs<GlobalFuncDeclSymbol, GlobalFuncSymbol>.GetPartialTypeArgs() => component.GetPartialTypeArgs();
        
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitGlobalFuncs(this);
    }

    [AutoConstructor]
    public partial class TypeVar : IntermediateExp
    {
        public TypeVarType Type { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
    }

    [AutoConstructor]
    public partial class Class : IntermediateExp
    {
        public ClassSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
    }

    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null    
    public class ClassMemberFuncs : IntermediateExp, IFuncs<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>
    {
        FuncsWithPartialTypeArgsComponent<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol> component;

        public ClassMemberFuncs(
            ImmutableArray<(ISymbolNode Outer, ClassMemberFuncDeclSymbol DeclSymbol)> outerAndDeclSymbols,
            ImmutableArray<IType> partialTypeArgs, 
            bool hasExplicitInstance,
            ResolvedExp? explicitInstance)
        {
            this.component = new FuncsWithPartialTypeArgsComponent<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>(outerAndDeclSymbols, partialTypeArgs);
            this.HasExplicitInstance = hasExplicitInstance;
            this.ExplicitInstance = explicitInstance;
        }

        public bool HasExplicitInstance { get; }
        public ResolvedExp? ExplicitInstance { get; }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberFuncs(this);

        int IFuncs<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>.GetCount() => component.GetCount();
        ClassMemberFuncDeclSymbol IFuncs<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>.GetDecl(int i) => component.GetDecl(i);
        ImmutableArray<IType> IFuncs<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>.GetPartialTypeArgs() => component.GetPartialTypeArgs();
        ClassMemberFuncSymbol IFuncs<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>.MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context)
            => component.MakeSymbol(i, typeArgs, context);
    }

    [AutoConstructor]
    public partial class Struct: IntermediateExp
    {
        public StructSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
    }

    public class StructMemberFuncs : IntermediateExp, IFuncs<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>
    {
        FuncsWithPartialTypeArgsComponent<StructMemberFuncDeclSymbol, StructMemberFuncSymbol> component;


        public StructMemberFuncs(
            ImmutableArray<(ISymbolNode Outer, StructMemberFuncDeclSymbol DeclSymbol)> outerAndDeclSymbols,
            ImmutableArray<IType> partialTypeArgs,
            bool hasExplicitInstance,
            ResolvedExp? explicitInstance)
        {
            this.component = new FuncsWithPartialTypeArgsComponent<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>(outerAndDeclSymbols, partialTypeArgs);
            this.HasExplicitInstance = hasExplicitInstance;
            this.ExplicitInstance = explicitInstance;
        }

        public bool HasExplicitInstance { get; }
        public ResolvedExp? ExplicitInstance { get; }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberFuncs(this);

        int IFuncs<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>.GetCount() => component.GetCount();
        StructMemberFuncDeclSymbol IFuncs<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>.GetDecl(int i) => component.GetDecl(i);
        ImmutableArray<IType> IFuncs<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>.GetPartialTypeArgs() => component.GetPartialTypeArgs();
        StructMemberFuncSymbol IFuncs<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>.MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context)
            => component.MakeSymbol(i, typeArgs, context);
    }

    [AutoConstructor]
    public partial class Enum : IntermediateExp
    {
        public EnumSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }

    [AutoConstructor]
    public partial class EnumElem : IntermediateExp
    {
        public EnumElemSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    }

    // exp로 사용할 수 있는
    [AutoConstructor]
    public partial class ThisVar : IntermediateExp
    {
        public IType Type { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }

    [AutoConstructor]
    public partial class LocalVar : IntermediateExp
    {
        public IType Type { get; }
        public Name Name { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    }

    [AutoConstructor]
    public partial class LambdaMemberVar : IntermediateExp
    {
        public LambdaMemberVarSymbol Symbol { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    }

    [AutoConstructor]
    public partial class ClassMemberVar : IntermediateExp
    {
        public ClassMemberVarSymbol Symbol { get; }
        public bool HasExplicitInstance { get; }
        public ResolvedExp? ExplicitInstance { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
    }

    [AutoConstructor]
    public partial class StructMemberVar : IntermediateExp
    {
        public StructMemberVarSymbol Symbol { get; }
        public bool HasExplicitInstance { get; }
        public ResolvedExp? ExplicitInstance { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
    }

    [AutoConstructor]
    public partial class EnumElemMemberVar : IntermediateExp
    {
        public EnumElemMemberVarSymbol Symbol { get; }
        public ResolvedExp Instance { get; }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
    }

    [AutoConstructor]
    public partial class ListIndexer : IntermediateExp
    {
        public ResolvedExp Instance { get; }
        public R.Loc Index { get; }
        public IType ItemType { get; }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
    }

    [AutoConstructor]
    public partial class LocalDeref : IntermediateExp
    {
        public ResolvedExp Target { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
    }

    [AutoConstructor]
    public partial class BoxDeref : IntermediateExp
    {
        public ResolvedExp Target { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
    }

    // 기타의 경우
    [AutoConstructor]
    public partial class IR0Exp : IntermediateExp
    {
        public IR0ExpResult ExpResult { get; }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIR0Exp(this);
    }
}
