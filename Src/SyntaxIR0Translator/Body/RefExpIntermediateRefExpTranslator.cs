using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.ComponentModel.DataAnnotations;

namespace Citron.Analysis;

struct IntermediateExpIntermediateRefExpTranslator : IIntermediateExpVisitor<IntermediateRefExp?>
{   
    ScopeContext context;
    S.ISyntaxNode nodeForErrorReport;

    public static IntermediateRefExp? Translate(IntermediateExp exp, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
    {
        var translator = new IntermediateExpIntermediateRefExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return exp.Accept<IntermediateExpIntermediateRefExpTranslator, IntermediateRefExp?>(ref translator);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitNamespace(IntermediateExp.Namespace exp)
    {
        return new IntermediateRefExp.Namespace(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitTypeVar(IntermediateExp.TypeVar exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClass(IntermediateExp.Class exp)
    {
        return new IntermediateRefExp.Class(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStruct(IntermediateExp.Struct exp)
    {
        return new IntermediateRefExp.Struct(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnum(IntermediateExp.Enum exp)
    {
        return new IntermediateRefExp.Enum(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnumElem(IntermediateExp.EnumElem exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitThis(IntermediateExp.ThisVar exp)
    {
        return new IntermediateRefExp.ThisVar(exp.Type);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLocalVar(IntermediateExp.LocalVar exp)
    {
        return new IntermediateRefExp.LocalVar(exp.Type, exp.Name);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar exp)
    {
        return new IntermediateRefExp.LambdaMemberVar(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClassMemberVar(IntermediateExp.ClassMemberVar exp)
    {
        return new IntermediateRefExp.ClassMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStructMemberVar(IntermediateExp.StructMemberVar exp)
    {
        return new IntermediateRefExp.StructMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar exp)
    {
        return new IntermediateRefExp.EnumElemMemberVar(exp.Symbol, exp.Instance);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitListIndexer(IntermediateExp.ListIndexer exp)
    {
        return new IntermediateRefExp.ListIndexer(exp.Instance, exp.Index, exp.ItemType);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLocalDeref(IntermediateExp.LocalDeref exp)
    {
        return new IntermediateRefExp.LocalDeref(exp.Target);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitBoxDeref(IntermediateExp.BoxDeref exp)
    {
        return new IntermediateRefExp.BoxDeref(exp.Target);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitIR0Exp(IntermediateExp.IR0Exp exp)
    {
        return new IntermediateRefExp.IR0Exp(exp.Exp);
    }
}

// & exp syntax를 중간과정으로 번역해주는 역할
struct RefExpIntermediateRefExpTranslator : S.IExpVisitor<TranslationResult<IntermediateRefExp>>
{
    ScopeContext context;
    S.ISyntaxNode nodeForErrorReport;

    static TranslationResult<IntermediateRefExp> Valid(IntermediateRefExp imRefExp)
    {
        return TranslationResult.Valid(imRefExp);
    }

    static TranslationResult<IntermediateRefExp> Error()
    {
        return TranslationResult.Error<IntermediateRefExp>();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIdentifier(S.IdentifierExp exp)
    {
        try
        {
            var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
            var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (imExp == null)
            {
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
                return Error();
            }

            var imRefExp = IntermediateExpIntermediateRefExpTranslator.Translate(imExp, context, nodeForErrorReport);
            if (imRefExp == null)
            {
                context.AddFatalError(A3001_Reference_CantMakeReference, exp);
                return Error();
            }

            return Valid(imRefExp);
        }
        catch (IdentifierResolverMultipleCandidatesException)
        {
            context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
            return Error();
        }
    }
    
    // string은 중간과정에서는 value로 평가하면 될 것 같다
    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitString(S.StringExp exp)
    {
        
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitCall(S.CallExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(S.LambdaExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIndexer(S.IndexerExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitMember(S.MemberExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitList(S.ListExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitNew(S.NewExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBox(S.BoxExp exp)
    {
        throw new System.NotImplementedException();
    }
}

