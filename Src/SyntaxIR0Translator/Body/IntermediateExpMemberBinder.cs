using System;
using System.Diagnostics;

using Citron.Collections;
using Citron.Symbol;
using Citron.Infra;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

// MemberParent And Id Binder
// (IntermediateExp, name, typeArgs) -> IntermediateExp
struct IntermediateExpMemberBinder : IIntermediateExpVisitor<TranslationResult<IntermediateExp>>
{
    Name name;
    ImmutableArray<IType> typeArgs;
    ScopeContext context;
    S.ISyntaxNode nodeForErrorReport;

    public static TranslationResult<IntermediateExp> Bind(IntermediateExp parentExp, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
    {   
        var binder = new IntermediateExpMemberBinder { name = name, typeArgs = typeArgs, context = context, nodeForErrorReport = nodeForErrorReport };
        return parentExp.Accept<IntermediateExpMemberBinder, TranslationResult<IntermediateExp>>(ref binder);
    }

    static TranslationResult<IntermediateExp> Valid(IntermediateExp imExp)
    {
        return TranslationResult.Valid<IntermediateExp>(imExp);
    }

    record struct StaticParentVisitor(ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport) 
        : ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>
    {
        TranslationResult<IntermediateExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return Error();
        }
        
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
        {
            return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
        }

        // T.S
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStruct(SymbolQueryResult.Struct result)
        {
            var structSymbol = result.StructConstructor.Invoke(typeArgs);

            // check access
            if (!context.CanAccess(structSymbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.Struct(structSymbol));
        }

        // S.F
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
        {
            return Valid(new IntermediateExp.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, ExplicitInstance: null));
        }

        // S.x
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
        {
            var symbol = result.Symbol;

            if (!symbol.IsStatic())
                return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.StructMemberVar(symbol, HasExplicitInstance: true, ExplicitInstance: null));
        }

        // T.C
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClass(SymbolQueryResult.Class result)
        {
            var classSymbol = result.ClassConstructor.Invoke(typeArgs);

            // check access
            if (!context.CanAccess(classSymbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.Class(classSymbol));
        }

        // C.F
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
        {
            return Valid(new IntermediateExp.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, null));
        }

        // C.x
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
        {
            var symbol = result.Symbol;

            if (!symbol.IsStatic())
                return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.ClassMemberVar(symbol, HasExplicitInstance: true, ExplicitInstance: null));
        }

        // T.E
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnum(SymbolQueryResult.Enum result)
        {
            var enumSymbol = result.EnumConstructor.Invoke(typeArgs);

            // check access
            if (!context.CanAccess(enumSymbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.Enum(enumSymbol));
        }

        // E.First
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnumElem(SymbolQueryResult.EnumElem result)
        {
            return Valid(new IntermediateExp.EnumElem(result.Symbol));
        }            
        
        // 표현 불가능
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
        {
            throw new RuntimeFatalException();
        }

        // NS.F
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
        {
            return Valid(new IntermediateExp.GlobalFuncs(result.Infos, typeArgs));
        }

        // 표현 불가능
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
        {
            throw new RuntimeFatalException();
        }

        // NS.'NS'
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitNamespace(SymbolQueryResult.Namespace result)
        {
            return Valid(new IntermediateExp.Namespace(result.Symbol));
        }

        // 표현 불가능
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
        {
            throw new RuntimeFatalException();
        }
    }

    record struct InstanceParentVisitor(ResolvedExp instanceReExp, ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport) : ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>
    {
        TranslationResult<IntermediateExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return Error();
        }
        
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
        {
            return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
        }

        // exp.C
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClass(SymbolQueryResult.Class result)
        {   
            return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        }

        // exp.F
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
        {
            return Valid(new IntermediateExp.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, instanceReExp));
        }

        // exp.x
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
        {
            var symbol = result.Symbol;

            // static인지 검사
            if (symbol.IsStatic())
                return Fatal(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

            // access modifier 검사                            
            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.ClassMemberVar(symbol, true, instanceReExp));
        }

        // exp.E
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnum(SymbolQueryResult.Enum result)
        {
            return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        }

        // exp.First
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnumElem(SymbolQueryResult.EnumElem result)
        {
            return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        }
        
        // exp.firstX
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
        {
            return Valid(new IntermediateExp.EnumElemMemberVar(result.Symbol, instanceReExp));
        }

        // 표현 불가
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
        {
            throw new RuntimeFatalException();
        }

        // 표현 불가
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
        {
            throw new RuntimeFatalException(); 
        }

        // 표현 불가
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitNamespace(SymbolQueryResult.Namespace result)
        {
            throw new RuntimeFatalException(); 
        }

        // exp.S
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStruct(SymbolQueryResult.Struct result)
        {
            return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        }

        // exp.F
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
        {
            return Valid(new IntermediateExp.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, instanceReExp));
        }

        // exp.x
        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
        {
            var symbol = result.Symbol;

            // static인지 검사
            if (symbol.IsStatic())
                return Fatal(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

            // access modifier 검사                            
            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateExp.StructMemberVar(symbol, HasExplicitInstance: true, instanceReExp));
        }

        TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
        {
            throw new NotImplementedException();
        }
    }

    // T.x
    TranslationResult<IntermediateExp> VisitStaticParent(ISymbolNode parentSymbol)
    {
        var memberResult = parentSymbol.QueryMember(name, typeArgs.Length);
        if (memberResult == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        var visitor = new StaticParentVisitor(typeArgs, context, nodeForErrorReport);
        return memberResult.Accept<StaticParentVisitor, TranslationResult<IntermediateExp>>(ref visitor);
    }
    
    // exp.x
    TranslationResult<IntermediateExp> VisitInstanceParent(IntermediateExp instImExp)
    {
        var instReExpResult = IntermediateExpResolvedExpTranslator.Translate(instImExp, context, nodeForErrorReport);
        if (!instReExpResult.IsValid(out var instReExp))
            return Error();
        
        var instReExpType = instReExp.GetExpType();

        var memberResult = instReExpType.QueryMember(name, typeArgs.Length);
        if (memberResult == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        var visitor = new InstanceParentVisitor(instReExp, typeArgs, context, nodeForErrorReport);
        return memberResult.Accept<InstanceParentVisitor, TranslationResult<IntermediateExp>>(ref visitor);
    }

    static TranslationResult<IntermediateExp> Error()
    {
        return TranslationResult.Error<IntermediateExp>();
    }

    TranslationResult<IntermediateExp> Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return Error();
    }
    
    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitNamespace(IntermediateExp.Namespace result)
    {
        return VisitStaticParent(result.Symbol);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs result)
    {
        return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitClass(IntermediateExp.Class result)
    {
        return VisitStaticParent(result.Symbol);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs result)
    {
        return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitClassMemberVar(IntermediateExp.ClassMemberVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitEnum(IntermediateExp.Enum result)
    {
        return VisitStaticParent(result.Symbol);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitEnumElem(IntermediateExp.EnumElem result)
    {
        return Fatal(A2009_ResolveIdentifier_EnumElemCantHaveMember); // ?
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitIR0Exp(IntermediateExp.IR0Exp result)
    {
        return VisitInstanceParent(result);
    }
    
    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitLocalVar(IntermediateExp.LocalVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitStruct(IntermediateExp.Struct result)
    {
        return VisitStaticParent(result.Symbol);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs result)
    {
        return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitStructMemberVar(IntermediateExp.StructMemberVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitThis(IntermediateExp.ThisVar result)
    {
        return VisitInstanceParent(result);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitTypeVar(IntermediateExp.TypeVar result)
    {
        throw new NotImplementedException();
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitListIndexer(IntermediateExp.ListIndexer exp)
    {
        throw new NotImplementedException();
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitLocalDeref(IntermediateExp.LocalDeref exp)
    {
        return VisitInstanceParent(exp);
    }

    TranslationResult<IntermediateExp> IIntermediateExpVisitor<TranslationResult<IntermediateExp>>.VisitBoxDeref(IntermediateExp.BoxDeref exp)
    {
        return VisitInstanceParent(exp);
    }
}