namespace Citron.Analysis;

interface IIdentifierResultVisitor<TResult>
{
    TResult VisitNamespace(IdentifierResult.Namespace result);
    TResult VisitLocalVar(IdentifierResult.LocalVar result);
    TResult VisitTypeVar(IdentifierResult.TypeVar result);
    TResult VisitLambdaMemberVar(IdentifierResult.LambdaMemberVar result);
    TResult VisitThis(IdentifierResult.ThisVar result);
    TResult VisitGlobalFuncs(IdentifierResult.GlobalFuncs result);

    TResult VisitClass(IdentifierResult.Class result);
    TResult VisitThisClassMemberVar(IdentifierResult.ThisClassMemberVar result);
    TResult VisitThisClassMemberFuncs(IdentifierResult.ThisClassMemberFuncs result);

    TResult VisitStruct(IdentifierResult.Struct result);
    TResult VisitThisStructMemberFuncs(IdentifierResult.ThisStructMemberFuncs result);
    TResult VisitThisStructMemberVar(IdentifierResult.ThisStructMemberVar result);

    TResult VisitEnum(IdentifierResult.Enum result);
    TResult VisitEnumElem(IdentifierResult.EnumElem result);
}
