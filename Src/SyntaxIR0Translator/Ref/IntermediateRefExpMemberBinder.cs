using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;

namespace Citron.Analysis;

struct IntermediateRefExpMemberBinder : IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>
{
    Name name;
    ImmutableArray<IType> typeArgs;
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static TranslationResult<IntermediateRefExp> Bind(IntermediateRefExp parentImRefExp, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var binder = new IntermediateRefExpMemberBinder { name = name, typeArgs = typeArgs, context = context, nodeForErrorReport = nodeForErrorReport };
        return parentImRefExp.Accept<IntermediateRefExpMemberBinder, TranslationResult<IntermediateRefExp>>(ref binder);
    }

    TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return TranslationResult.Error<IntermediateRefExp>();
    }

    static TranslationResult<IntermediateRefExp> Valid(IntermediateRefExp imRefExp)
    {
        return TranslationResult.Valid(imRefExp);
    }

    static TranslationResult<IntermediateRefExp> Error()
    {
        return TranslationResult.Error<IntermediateRefExp>();
    }

    record struct StaticParentBinder(ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>
    {   
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(SymbolQueryResult.Class result)
        {
            return Valid(new IntermediateRefExp.Class(result.ClassConstructor.Invoke(typeArgs)));
        }

        // 에러,
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
        {
            return Fatal(A3001_Reference_CantMakeReference);
        }

        // C.x
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
        {
            var symbol = result.Symbol;

            if (!symbol.IsStatic())
                return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateRefExp.StaticRef(new ClassMemberLoc(Instance: null, symbol), symbol.GetDeclType()));
        }

        // E
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(SymbolQueryResult.Enum result)
        {
            return Valid(new IntermediateRefExp.Enum(result.EnumConstructor.Invoke(typeArgs)));
        }

        // &E.First.x
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(SymbolQueryResult.EnumElem result)
        {
            return Fatal(A3001_Reference_CantMakeReference);
        }

        // &E.x
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
        {
            // 표현 불가능
            throw new RuntimeFatalException();
        }

        // S.F
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
        {
            return Fatal(A3001_Reference_CantMakeReference);
        }

        // 
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
        {
            throw new RuntimeFatalException();
        }
        
        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
        {
            return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitNamespace(SymbolQueryResult.Namespace result)
        {
            return Valid(new IntermediateRefExp.Namespace(result.Symbol));
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(SymbolQueryResult.Struct result)
        {
            return Valid(new IntermediateRefExp.Struct(result.StructConstructor.Invoke(typeArgs)));
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
        {
            return Fatal(A3001_Reference_CantMakeReference);
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
        {
            var symbol = result.Symbol;

            if (!symbol.IsStatic())
                return Fatal(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            if (!context.CanAccess(symbol))
                return Fatal(A2011_ResolveIdentifier_TryAccessingPrivateMember);

            return Valid(new IntermediateRefExp.StaticRef(new StructMemberLoc(Instance: null, symbol), symbol.GetDeclType()));
        }

        TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
        {
            throw new RuntimeFatalException();
        }
    }

    TranslationResult<IntermediateRefExp> HandleStaticParent(ISymbolNode symbol)
    {
        var result = symbol.QueryMember(name, typeArgs.Length);
        if (result == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        var binder = new StaticParentBinder();
        return result.Accept<StaticParentBinder, TranslationResult<IntermediateRefExp>>(ref binder);
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(IntermediateRefExp.Class imRefExp)
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(IntermediateRefExp.Enum imRefExp)
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitNamespace(IntermediateRefExp.Namespace imRefExp)
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(IntermediateRefExp.Struct imRefExp)
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(IntermediateRefExp.TypeVar imRefExp)
    {
        // 이건 진짜
        throw new System.NotImplementedException();
    }

    record struct StaticRefTypeVisitor(IntermediateRefExp.StaticRef parent, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ITypeVisitor<TranslationResult<IntermediateRefExp>>
    {
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        // &(C.x).a
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxPtr(BoxPtrType type)
        {
            return Fatal(A3001_Reference_CantMakeReference);
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(ClassType type)
        {
            var memberVar = type.GetMemberVar(name);

            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

            // 이제 BoxRef로 변경
            return Valid(new IntermediateRefExp.BoxRef.ClassMember(parent.Loc, memberVar));
        }

        // Enum자체는 member를 가져올 수 없다
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(EnumType type)
        {
            return Fatal(A2007_ResolveIdentifier_NotFound);
        }

        // e.x (E.Second.x)
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(EnumElemType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

            return Valid(new IntermediateRefExp.StaticRef(new EnumElemMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
        }

        // &C.f.id
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitFunc(FuncType type)
        {
            return Fatal(A2019_ResolveIdentifier_FuncInstanceCantHaveMember);
        }

        // &C.i.id
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitInterface(InterfaceType type)
        {
            throw new NotImplementedException();
        }

        // &C.l.id
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(LambdaType type)
        {
            return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
        }

        // &C.pS.id;
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalPtr(LocalPtrType type)
        {
            return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
        }

        // &C.optS.id
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitNullable(NullableType type)
        {
            throw new NotImplementedException();
        }

        // &C.s.id
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(StructType type)
        {
            var memberVar = type.GetMemberVar(name);

            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);


            return Valid(new IntermediateRefExp.StaticRef(new StructMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTuple(TupleType type)
        {
            // TupleMemberLoc이 없으므로 일단 보류
            throw new NotImplementedException();
            //int count = type.GetMemberVarCount();
            //for (int i = 0; i < count; i++)
            //{
            //    var memberVar = type.GetMemberVar(i);
            //    if (memberVar.GetName().Equals(name))
            //    {
            //        return Valid(new IntermediateRefExp.StaticRef(new TupleMemberLoc parent.Loc)
            //    }
            //}

            //return Fatal();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(TypeVarType type)
        {
            throw new NotImplementedException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitVoid(VoidType type)
        {
            // void인 멤버가 나올 수 없으므로
            throw new RuntimeFatalException();
        }
    }

    // static ref가 부모이면
    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitStaticRef(IntermediateRefExp.StaticRef imRefExp)
    {
        var visitor = new StaticRefTypeVisitor(imRefExp, name, typeArgs, context, nodeForErrorReport);
        return imRefExp.LocType.Accept<StaticRefTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    record struct BoxRefTypeVisitor(IntermediateRefExp.BoxRef parent, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ITypeVisitor<TranslationResult<IntermediateRefExp>>
    {
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        // &c.p.x, 문법에러
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxPtr(BoxPtrType type)
        {
            return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
        }

        // &c.c.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(ClassType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);
            
            return Valid(new IntermediateRefExp.BoxRef.ClassMember(parent.MakeLoc(), memberVar));
        }

        // &c.e.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(EnumType type)
        {
            return Fatal(A2013_ResolveIdentifier_EnumInstanceCantHaveMember);
        }

        // &c.e.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(EnumElemType type)
        {
            //
            throw new NotImplementedException();
        }

        // &c.f.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitFunc(FuncType type)
        {
            return Fatal(A2019_ResolveIdentifier_FuncInstanceCantHaveMember);
        }

        // &c.i.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitInterface(InterfaceType type)
        {
            throw new NotImplementedException();
        }

        // &c.l.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(LambdaType type)
        {
            return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
        }

        // &c.p.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalPtr(LocalPtrType type)
        {
            return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
        }

        // &c.optS.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitNullable(NullableType type)
        {
            throw new NotImplementedException();
        }

        // &c.s.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(StructType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

            return Valid(new IntermediateRefExp.BoxRef.StructMember(parent, memberVar));
        }

        // &c.t.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTuple(TupleType type)
        {
            throw new NotImplementedException();
        }

        // &c.t.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(TypeVarType type)
        {
            throw new NotImplementedException();
        }

        // &c.v
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitVoid(VoidType type)
        {
            // void인 멤버가 나올 수 없으므로
            throw new RuntimeFatalException();
        }
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxRef(IntermediateRefExp.BoxRef imRefExp)
    {
        var visitor = new BoxRefTypeVisitor(imRefExp, name, typeArgs, context, nodeForErrorReport);
        return imRefExp.GetTargetType().Accept<BoxRefTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    record struct LocalRefTypeVisitor(IntermediateRefExp.LocalRef parent, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ITypeVisitor<TranslationResult<IntermediateRefExp>>
    {
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        // &s.p.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxPtr(BoxPtrType type)
        {
            return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
        }

        // &s.c.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(ClassType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);
            
            return Valid(new IntermediateRefExp.BoxRef.ClassMember(parent.Loc, memberVar));
        }

        // &s.e.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(EnumType type)
        {
            return Fatal(A2013_ResolveIdentifier_EnumInstanceCantHaveMember);
        }

        // &s.e.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(EnumElemType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);
            
            return Valid(new IntermediateRefExp.LocalRef(new EnumElemMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
        }

        // &s.f.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitFunc(FuncType type)
        {
            return Fatal(A2019_ResolveIdentifier_FuncInstanceCantHaveMember);
        }

        // &s.i.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitInterface(InterfaceType type)
        {
            throw new NotImplementedException();
        }

        // &s.l.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(LambdaType type)
        {
            return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
        }

        // &s.p.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalPtr(LocalPtrType type)
        {
            return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
        }

        // &s.optS.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitNullable(NullableType type)
        {
            throw new NotImplementedException();
        }

        // &s.s.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(StructType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);
            
            return Valid(new IntermediateRefExp.LocalRef(new StructMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
        }

        // &s.t.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTuple(TupleType type)
        {
            throw new NotImplementedException();
        }

        // &s.t.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(TypeVarType type)
        {
            throw new NotImplementedException();
        }
        
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitVoid(VoidType type)
        {
            // void인 멤버가 나올 수 없으므로
            throw new RuntimeFatalException();
        }
    }

    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalRef(IntermediateRefExp.LocalRef imRefExp)
    {
        var visitor = new LocalRefTypeVisitor();
        return imRefExp.LocType.Accept<LocalRefTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    // *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
    record struct BoxValueTypeVisitor(IntermediateRefExp.DerefedBoxValue parent, Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ITypeVisitor<TranslationResult<IntermediateRefExp>>
    {
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxPtr(BoxPtrType type)
        {
            return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
        }

        // &(*pC).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(ClassType type)
        {
            throw new RuntimeFatalException();
        }

        // (*pE).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(EnumType type)
        {
            throw new RuntimeFatalException();
        }

        // box E.Second* pE = ...
        // &(*pE).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(EnumElemType type)
        {
            throw new NotImplementedException();

            //var memberVar = type.Symbol.GetMemberVar(name);
            //if (memberVar == null)
            //    return Fatal();

            //return Valid(new IntermediateRefExp.BoxRef.EnumMember(parent, memberVar));
        }

        // box ref contained
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitFunc(FuncType type)
        {
            throw new RuntimeFatalException();
        }

        // box ref contained
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitInterface(InterfaceType type)
        {
            throw new RuntimeFatalException();
        }

        // 
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(LambdaType type)
        {
            // doesn't have member variable
            return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalPtr(LocalPtrType type)
        {
            throw new RuntimeFatalException();
        }

        // &(*pOptS).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitNullable(NullableType type)
        {
            throw new NotImplementedException();
        }

        // &(*pS).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(StructType type)
        {
            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);

            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);
            
            return Valid(new IntermediateRefExp.BoxRef.StructIndirectMember(parent.InnerExp, memberVar));
        }

        // &(*pT).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTuple(TupleType type)
        {
            throw new NotImplementedException();
        }

        // &(*pT).x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(TypeVarType type)
        {
            throw new NotImplementedException();
        }
        
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitVoid(VoidType type)
        {
            throw new RuntimeFatalException();
        }
    }

    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitDerefedBoxValue(IntermediateRefExp.DerefedBoxValue imRefExp)
    {
        var visitor = new BoxValueTypeVisitor(imRefExp, name, typeArgs, context, nodeForErrorReport);
        var innerExpType = context.GetExpType(imRefExp.InnerExp);

        return innerExpType.Accept<BoxValueTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    // exp.id
    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalValue(IntermediateRefExp.LocalValue imRefExp)
    {
        // 함수 호출 인자 제외 temp 참조 불가
        return Fatal(A3002_Reference_CantReferenceTempValue);
    }

    record struct ThisTypeVisitor(Name name, ImmutableArray<IType> typeArgs, ScopeContext context, ISyntaxNode nodeForErrorReport) : ITypeVisitor<TranslationResult<IntermediateRefExp>>
    {
        TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code)
        {
            context.AddFatalError(code, nodeForErrorReport);
            return TranslationResult.Error<IntermediateRefExp>();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitBoxPtr(BoxPtrType type)
        {
            return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
        }

        // &this.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitClass(ClassType type)
        {
            if (typeArgs.Length != 0)
                return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

            var memberVar = type.GetMemberVar(name);
            if (memberVar == null)
                return Fatal(A2007_ResolveIdentifier_NotFound);
            
            return Valid(new IntermediateRefExp.BoxRef.ClassMember(new ThisLoc(), memberVar));
        }

        // &this.x
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitStruct(StructType type)
        {
            // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
            return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnum(EnumType type)
        {
            // Enum이 멤버 함수를 갖기 전까진 여기 들어오지 않는다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitEnumElem(EnumElemType type)
        {
            // EnumElem이 멤버함수를 갖기 전까진 여기 들어오지 않는다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitFunc(FuncType type)
        {
            // Func가 멤버함수를 갖기 전까진 여기 들어오지 않는다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitInterface(InterfaceType type)
        {
            // Interface가 멤버함수를 갖기 전까진 여기 들어오지 않는다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(LambdaType type)
        {
            // Lambda는 멤버함수를 가질 수 없다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitLocalPtr(LocalPtrType type)
        {
            return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitNullable(NullableType type)
        {
            // Nullable은 멤버함수를 가질 수 없다
            throw new RuntimeFatalException();
        }
        
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTuple(TupleType type)
        {
            // Tuple은 멤버함수를 가질 수 없다
            throw new RuntimeFatalException();
        }

        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitTypeVar(TypeVarType type)
        {
            // TypeVar는 멤버함수를 가질 수 없다
            throw new RuntimeFatalException();
        }
        
        TranslationResult<IntermediateRefExp> ITypeVisitor<TranslationResult<IntermediateRefExp>>.VisitVoid(VoidType type)
        {
            // void는 멤버함수를 가질 수 없다
            throw new RuntimeFatalException();
        }
    }

    // this.id
    TranslationResult<IntermediateRefExp> IIntermediateRefExpVisitor<TranslationResult<IntermediateRefExp>>.VisitThis(IntermediateRefExp.ThisVar imRefExp)
    {
        var visitor = new ThisTypeVisitor(name, typeArgs, context, nodeForErrorReport);
        return imRefExp.Type.Accept<ThisTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }
}

