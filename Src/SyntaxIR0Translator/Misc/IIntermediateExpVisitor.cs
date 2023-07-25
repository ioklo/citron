namespace Citron.Analysis;

interface IIntermediateExpVisitor<TResult>
{
    TResult VisitNamespace(IntermediateExp.Namespace exp);
    TResult VisitGlobalFuncs(IntermediateExp.GlobalFuncs exp);
    TResult VisitTypeVar(IntermediateExp.TypeVar exp);
    TResult VisitClass(IntermediateExp.Class exp);
    TResult VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs exp);
    TResult VisitStruct(IntermediateExp.Struct exp);
    TResult VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs exp);
    TResult VisitEnum(IntermediateExp.Enum exp);
    TResult VisitEnumElem(IntermediateExp.EnumElem exp);
    TResult VisitThis(IntermediateExp.ThisVar exp);
    TResult VisitLocalVar(IntermediateExp.LocalVar exp);
    TResult VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar exp);
    TResult VisitClassMemberVar(IntermediateExp.ClassMemberVar exp);
    TResult VisitStructMemberVar(IntermediateExp.StructMemberVar exp);
    TResult VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar exp);
    TResult VisitListIndexer(IntermediateExp.ListIndexer exp);
    
    TResult VisitLocalDeref(IntermediateExp.LocalDeref exp);
    TResult VisitBoxDeref(IntermediateExp.BoxDeref exp);

    TResult VisitIR0Exp(IntermediateExp.IR0Exp exp);
}