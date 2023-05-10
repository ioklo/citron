using Citron.IR0;

namespace Citron.Analysis;

struct ResolvedExpIR0BoxRefTranslator : IResolvedExpVisitor<Exp>
{
    Exp IResolvedExpVisitor<Exp>.VisitClassMemberVar(ResolvedExp.ClassMemberVar exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitStructMemberVar(ResolvedExp.StructMemberVar exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitIR0Exp(ResolvedExp.IR0Exp exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitListIndexer(ResolvedExp.ListIndexer exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitLocalDeref(ResolvedExp.LocalDeref exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitBoxDeref(ResolvedExp.BoxDeref exp)
    {
        throw new System.NotImplementedException();
    }

    Exp IResolvedExpVisitor<Exp>.VisitLocalVar(ResolvedExp.LocalVar exp)
    {
        throw new System.NotImplementedException();
    }

    

    Exp IResolvedExpVisitor<Exp>.VisitThis(ResolvedExp.ThisVar exp)
    {
        throw new System.NotImplementedException();
    }
}
