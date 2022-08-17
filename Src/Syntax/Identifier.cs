using Citron.Collections;
using Citron.Symbol;

namespace Citron.Syntax
{
    // x.F<int, short>(); 
    // ClassMemberFuncsIdentifier(FuncsSymbol: { C.F }, instance: GlobalVarIdentifier("x")); // <int, short>는 어쩔거냐

    // Identifier
    public abstract record Identifier;

    // Location Identifiers
    public abstract record LocIdentifier : Identifier;
    public record GlobalVarLocIdentifier : LocIdentifier; // "g_x"
    public record LocalVarLocIdentifier : LocIdentifier; // "x"
    public record ThisLocIdentifier : LocIdentifier; // "this"
    public record LambdaMemberVarLocIdentifier : LocIdentifier; // "x" => "lambdaContext.x"
    public record ClassMemberVarLocIdentifier(ClassMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier;   // c.x
    public record StructMemberVarLocIdentifier(StructMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier; // s.x
    public record EnumElemMemberVarLocIdentifier(EnumElemMemberVarSymbol Symbol, LocIdentifier Instance) : LocIdentifier; // e.x

    // Type Identifiers
    public record ClassIdentifier : Identifier { } // NS.C
    public record StructIdentifier(StructSymbol Symbol) : Identifier;  // NS.S
    public record EnumIdentifier(EnumSymbol Symbol) : Identifier; // NS.E

    // Funcs Identifiers
    public record GlobalFuncsIdentifier(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : Identifier;
    public record ClassMemberFuncsIdentifier : Identifier;
    public record StructMemberFuncsIdentifier(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : Identifier;

    // First => E.First, enum elem constructor
    public record EnumElemIdentifier(EnumElemSymbol EnumElemSymbol) : Identifier;
}