using Citron.Collections;
using Citron.Symbol;

namespace Citron
{
    public abstract record class Identifier;

    public abstract record class IdentifierImpl : Identifier
    {
        public abstract void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor)
            where TIdentifierVisitor : struct, IIdentifierVisitor;
    }

    // x.F<int, short>(); 
    // ClassMemberFuncsIdentifier(FuncsSymbol: { C.F }, instance: GlobalVarIdentifier("x")); // <int, short>는 어쩔거냐

    // Location Identifiers
    public abstract record class LocIdentifier : IdentifierImpl;

    // "g_x"
    public record class GlobalVarLocIdentifier : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitGlobalVarLoc(this); }
    }

    // "x"
    public record class LocalVarLocIdentifier : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitLocalVarLoc(this); }
    }

    // "this"
    public record class ThisLocIdentifier : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitThisLoc(this); }
    }

    // "x" => "lambdaContext.x"
    public record class LambdaMemberVarLocIdentifier : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitLambdaMemberVarLoc(this); }
    }

    // c.x
    public record class ClassMemberVarLocIdentifier(ClassMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitClassMemberVarLoc(this); }
    }

    // s.x
    public record class StructMemberVarLocIdentifier(StructMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitStructMemberVarLoc(this); }
    }

    // e.x
    public record class EnumElemMemberVarLocIdentifier(EnumElemMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitEnumElemMemberVarLoc(this); }
    }

    // Type Identifiers

    // NS.C
    public record class ClassIdentifier : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitClass(this); }
    }

    // NS.S
    public record class StructIdentifier(StructSymbol Symbol) : IdentifierImpl  
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitStruct(this); }
    }

    // NS.E
    public record class EnumIdentifier(EnumSymbol Symbol) : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitEnum(this); }
    }

    // Funcs Identifiers
    public record class GlobalFuncsIdentifier(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitGlobalFuncs(this); }
    }

    public record class ClassMemberFuncsIdentifier : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitClassMemberFuncs(this); }
    }

    public record class StructMemberFuncsIdentifier(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitStructMemberFuncs(this); }
    }

    // First => E.First, enum elem constructor
    public record class EnumElemIdentifier(EnumElemSymbol EnumElemSymbol) : IdentifierImpl
    {
        public override void Visit<TIdentifierVisitor>(ref TIdentifierVisitor visitor) { visitor.VisitEnumElem(this); }
    }
}