using Citron.Collections;
using Citron.Symbol;


using S = Citron.Syntax;
using R = Citron.IR0;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;
using System;
using System.Diagnostics;

namespace Citron.Analysis
{
    // MemberParent And Id Binder
    // (IntermediateExp, name, typeArgs) -> IntermediateExp
    struct MemberParentAndIdBinder : IIntermediateExpVisitor<IntermediateExp>
    {
        Name name;
        ImmutableArray<IType> typeArgs;
        ScopeContext context;
        S.ISyntaxNode nodeForErrorReport;

        public static IntermediateExp Bind(IntermediateExp parentExp, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
        {   
            var binder = new MemberParentAndIdBinder { name = name, typeArgs = typeArgs, context = context, nodeForErrorReport = nodeForErrorReport };
            return parentExp.Accept<MemberParentAndIdBinder, IntermediateExp>(ref binder);
        }

        record struct StaticParentVisitor(ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport) : ISymbolQueryResultVisitor<IntermediateExp>
        {
            IntermediateExp Fatal(SyntaxAnalysisErrorCode code)
            {
                context.AddFatalError(code, nodeForErrorReport);
                throw new UnreachableException();
            }
            
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
            {
                return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
            }

            // T.S
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStruct(SymbolQueryResult.Struct result)
            {
                var structSymbol = result.StructConstructor.Invoke(typeArgs);

                // check access
                if (!context.CanAccess(structSymbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.Struct(structSymbol);
            }

            // S.F
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
            {
                return new IntermediateExp.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, ExplicitInstance: null);
            }

            // S.x
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
            {
                var symbol = result.Symbol;

                if (!symbol.IsStatic())
                    return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

                if (!context.CanAccess(symbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.StructMemberVar(symbol, HasExplicitInstance: true, ExplicitInstance: null);
            }

            // T.C
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClass(SymbolQueryResult.Class result)
            {
                var classSymbol = result.ClassConstructor.Invoke(typeArgs);

                // check access
                if (!context.CanAccess(classSymbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.Class(classSymbol);
            }

            // C.F
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
            {
                return new IntermediateExp.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, null);
            }

            // C.x
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
            {
                var symbol = result.Symbol;

                if (!symbol.IsStatic())
                    return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

                if (!context.CanAccess(symbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.ClassMemberVar(symbol, HasExplicitInstance: true, ExplicitInstance: null);
            }

            // T.E
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnum(SymbolQueryResult.Enum result)
            {
                var enumSymbol = result.EnumConstructor.Invoke(typeArgs);

                // check access
                if (!context.CanAccess(enumSymbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.Enum(enumSymbol);
            }

            // E.First
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElem(SymbolQueryResult.EnumElem result)
            {
                return new IntermediateExp.EnumElem(result.Symbol);
            }            
            
            // 표현 불가능
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
            {
                throw new RuntimeFatalException();
            }

            // NS.F
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
            {
                return new IntermediateExp.GlobalFuncs(result.Infos, typeArgs);
            }

            // 표현 불가능
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
            {
                throw new RuntimeFatalException();
            }

            // NS.'NS'
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitNamespace(SymbolQueryResult.Namespace result)
            {
                return new IntermediateExp.Namespace(result.Symbol);
            }

            // 표현 불가능
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
            {
                throw new RuntimeFatalException();
            }
        }

        record struct InstanceParentVisitor(ResolvedExp parentResult, ImmutableArray<IType> typeArgs, ScopeContext context, S.ISyntaxNode nodeForErrorReport) : ISymbolQueryResultVisitor<IntermediateExp>
        {
            IntermediateExp Fatal(SyntaxAnalysisErrorCode code)
            {
                context.AddFatalError(code, nodeForErrorReport);
                throw new UnreachableException();
            }
            
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
            {
                return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
            }

            // exp.C
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClass(SymbolQueryResult.Class result)
            {   
                return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
            }

            // exp.F
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
            {
                return new IntermediateExp.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, parentResult);
            }

            // exp.x
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
            {
                var symbol = result.Symbol;

                // static인지 검사
                if (symbol.IsStatic())
                    return Fatal(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

                // access modifier 검사                            
                if (!context.CanAccess(symbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.ClassMemberVar(symbol, true, parentResult);
            }

            // exp.E
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnum(SymbolQueryResult.Enum result)
            {
                return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
            }

            // exp.First
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElem(SymbolQueryResult.EnumElem result)
            {
                return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
            }
            
            // exp.firstX
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
            {
                return new IntermediateExp.EnumElemMemberVar(result.Symbol, parentResult);
            }

            // 표현 불가
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
            {
                throw new RuntimeFatalException();
            }

            // 표현 불가
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
            {
                throw new RuntimeFatalException(); 
            }

            // 표현 불가
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitNamespace(SymbolQueryResult.Namespace result)
            {
                throw new RuntimeFatalException(); 
            }

            // exp.S
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStruct(SymbolQueryResult.Struct result)
            {
                return Fatal(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance);
            }

            // exp.F
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
            {
                return new IntermediateExp.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: true, parentResult);
            }

            // exp.x
            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
            {
                var symbol = result.Symbol;

                // static인지 검사
                if (symbol.IsStatic())
                    return Fatal(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

                // access modifier 검사                            
                if (!context.CanAccess(symbol))
                    return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new IntermediateExp.StructMemberVar(symbol, HasExplicitInstance: true, parentResult);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
            {
                throw new NotImplementedException();
            }
        }

        // T.x
        IntermediateExp VisitStaticParent(ISymbolNode parentSymbol)
        {
            var memberResult = parentSymbol.QueryMember(name, typeArgs.Length);
            if (memberResult == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            var visitor = new StaticParentVisitor(typeArgs, context, nodeForErrorReport);
            return memberResult.Accept<StaticParentVisitor, IntermediateExp>(ref visitor);
        }
        
        // exp.x
        IntermediateExp VisitInstanceParent(IntermediateExp parentImExp, IType instanceType)
        {
            var parentReExp = IntermediateExpResolvedExpTranslator.Translate(parentImExp, context, nodeForErrorReport);

            var memberResult = instanceType.QueryMember(name, typeArgs.Length);
            if (memberResult == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            var visitor = new InstanceParentVisitor(parentReExp, typeArgs, context, nodeForErrorReport);

            return memberResult.Accept<InstanceParentVisitor, IntermediateExp>(ref visitor);
        }

        IntermediateExp Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            throw new UnreachableException();
        }
        
        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitNamespace(IntermediateExp.Namespace result)
        {
            return VisitStaticParent(result.Symbol);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs result)
        {
            return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitClass(IntermediateExp.Class result)
        {
            return VisitStaticParent(result.Symbol);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs result)
        {
            return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitClassMemberVar(IntermediateExp.ClassMemberVar result)
        {
            return VisitInstanceParent(result, result.Symbol.GetDeclType());
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitEnum(IntermediateExp.Enum result)
        {
            return VisitStaticParent(result.Symbol);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitEnumElem(IntermediateExp.EnumElem result)
        {
            return Fatal(A2009_ResolveIdentifier_EnumElemCantHaveMember);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar result)
        {
            return VisitInstanceParent(result, result.Symbol.GetDeclType());
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitIR0Exp(IntermediateExp.IR0Exp result)
        {
            return VisitInstanceParent(result, result.Exp.GetExpType());
        }
        
        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar result)
        {
            return VisitInstanceParent(result, result.Symbol.GetDeclType());
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitLocalVar(IntermediateExp.LocalVar result)
        {
            return VisitInstanceParent(result, result.Type);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitStruct(IntermediateExp.Struct result)
        {
            return VisitStaticParent(result.Symbol);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs result)
        {
            return Fatal(A2006_ResolveIdentifier_FuncCantHaveMember);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitStructMemberVar(IntermediateExp.StructMemberVar result)
        {
            return VisitInstanceParent(result, result.Symbol.GetDeclType());
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitThis(IntermediateExp.ThisVar result)
        {
            return VisitInstanceParent(result, result.Type);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitTypeVar(IntermediateExp.TypeVar result)
        {
            return Fatal(A2012_ResolveIdentifier_TypeVarCantHaveMember);
        }

        IntermediateExp IIntermediateExpVisitor<IntermediateExp>.VisitListIndexer(IntermediateExp.ListIndexer exp)
        {
            throw new NotImplementedException();
        }
    }
}