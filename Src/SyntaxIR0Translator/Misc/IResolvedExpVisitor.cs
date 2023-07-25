namespace Citron.Analysis;

// ResolvedExp
// Syntax Exp의 Identifier들을 Resolve한 최종 결과
// Namespace 등은 Exp로 쓰일수 없으므로 존재하지 않는다
interface IResolvedExpVisitor<TResult>
{
    TResult VisitThis(ResolvedExp.ThisVar exp);
    TResult VisitLocalVar(ResolvedExp.LocalVar exp);
    TResult VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar exp);
    TResult VisitClassMemberVar(ResolvedExp.ClassMemberVar exp);
    TResult VisitStructMemberVar(ResolvedExp.StructMemberVar exp);
    TResult VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar exp);
    TResult VisitListIndexer(ResolvedExp.ListIndexer exp);

    TResult VisitLocalDeref(ResolvedExp.LocalDeref exp);
    TResult VisitBoxDeref(ResolvedExp.BoxDeref exp);

    TResult VisitIR0Exp(ResolvedExp.IR0Exp exp);
    
}

