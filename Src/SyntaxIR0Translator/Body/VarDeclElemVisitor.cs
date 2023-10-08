using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;
using System.Diagnostics;
using Citron.Collections;
using Pretune;

namespace Citron.Analysis;

public enum DeclTypeInfoKind
{
    Normal,
    PlainVar,
    LocalInterfaceVar,
    BoxPtrVar,
    LocalPtrVar,
    NullableVar,
}

[AutoConstructor]
public partial struct DeclTypeInfo
{
    DeclTypeInfoKind kind;
    IType? type;

    public DeclTypeInfoKind GetKind() { return kind; }

    public IType GetDeclType()
    {
        Debug.Assert(kind == DeclTypeInfoKind.Normal);
        return type!;
    }
}

struct VarDeclVisitor
{   
    public static TranslationResult<ImmutableArray<R.Stmt>> Visit(S.VarDecl decl, ScopeContext context)
    {
        DeclTypeInfo info = context.GetDeclTypeInfo(decl.Type);
        return VarDeclElemVisitor.Visit(info, context, decl.Elems);
    }
}



struct VarDeclElemVisitor
{
    DeclTypeInfo declTypeInfo;
    ScopeContext context;
    ImmutableArray<R.Stmt>.Builder builder;

    bool Error(SyntaxAnalysisErrorCode errorCode, S.VarDeclElement syntax)
    {
        context.AddFatalError(errorCode, syntax);
        return false;
    }

    public static TranslationResult<ImmutableArray<R.Stmt>> Visit(DeclTypeInfo declTypeInfo, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        var visitor = new VarDeclElemVisitor { declTypeInfo = declTypeInfo, context = context, builder = builder };
        foreach (var elem in elems)
        {
            if (!visitor.VisitElem(elem))
                return TranslationResult.Error<ImmutableArray<R.Stmt>>();
        }

        return TranslationResult.Valid(builder.ToImmutable());
    }

    bool CheckVarConsistency(IType initExpType, S.VarDeclElement syntax)
    {
        // var 꼴별로 에러 체크
        switch (declTypeInfo.GetKind())
        {
            case DeclTypeInfoKind.Normal:
                Debug.Assert(false);
                return false;

            // local, boxptr, localptr, nullable 인지 체크한다 
            case DeclTypeInfoKind.PlainVar:

                switch (initExpType)
                {
                    case IInterfaceType initExpInterfaceType when initExpInterfaceType.IsLocal():
                        return Error(A0113_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface, syntax);

                    case BoxPtrType:
                        return Error(A0114_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr, syntax);

                    case LocalPtrType:
                        return Error(A0115_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr, syntax);

                    case NullableType:
                        return Error(A0116_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr, syntax);
                }
                break;

            case DeclTypeInfoKind.LocalInterfaceVar:
                if (initExpType is not IInterfaceType)
                    return Error(A0117_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface, syntax);
                break;

            case DeclTypeInfoKind.BoxPtrVar:
                if (initExpType is not BoxPtrType)
                    return Error(A0118_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr, syntax);
                break;

            case DeclTypeInfoKind.LocalPtrVar:
                if (initExpType is not LocalPtrType)
                    return Error(A0119_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr, syntax);
                break;

            case DeclTypeInfoKind.NullableVar:
                if (initExpType is not NullableType)
                    return Error(A0120_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable, syntax);
                break;
        }

        return true;
    }

    bool HandleVarDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            return Error(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
        }

        // var꼴로 나오는 경우 hintType은 없다
        var initResult = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, hintType: null);
        if (!initResult.IsValid(out var initExpResult))
            return false;

        var (initExp, initExpType) = initExpResult;
        if (!CheckVarConsistency(initExpType, syntax))
            return false;
        
        builder.Add(new R.LocalVarDeclStmt(initExpType, syntax.VarName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(syntax.VarName));

        return true;
    }

    bool HandleExplicitDeclType(S.VarDeclElement syntax)
    {
        Debug.Assert(declTypeInfo.GetKind() == DeclTypeInfoKind.Normal);
        var declType = declTypeInfo.GetDeclType();

        R.Exp? initExp;
        if (syntax.InitExp != null)
        {
            var initResult = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, declType);
            if (!initResult.IsValid(out var initExpResult))
                return false;

            initExp = BodyMisc.TryCastExp_Exp(initExpResult.Exp, initExpResult.ExpType, declType, context);

            if (initExp == null)
                return Error(A0121_VarDecl_InitExpTypeMismatch, syntax);
        }
        else
        {
            initExp = null;
        }

        builder.Add(new R.LocalVarDeclStmt(declType, syntax.VarName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(syntax.VarName));

        return true;
    }

    bool VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
        {
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);
            return true;
        }

        if (declTypeInfo.GetKind() != DeclTypeInfoKind.Normal)
        {
            return HandleVarDeclType(syntax);
        }
        else
        {
            return HandleExplicitDeclType(syntax);
        }
    }
}