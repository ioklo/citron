using System;

namespace Citron.IR0;

public interface IIR0LocVisitor<TResult>
{
    TResult VisitTemp(TempLoc loc);
    TResult VisitLocalVar(LocalVarLoc loc);
    TResult VisitLambdaMemberVar(LambdaMemberVarLoc loc);
    TResult VisitListIndexer(ListIndexerLoc loc);        
    TResult VisitStructMember(StructMemberLoc loc);
    TResult VisitClassMember(ClassMemberLoc loc);
    TResult VisitEnumElemMember(EnumElemMemberLoc loc);
    TResult VisitThis(ThisLoc loc);
    TResult VisitLocalDeref(LocalDerefLoc loc);
    TResult VisitBoxDeref(BoxDerefLoc loc);
    TResult VisitNullableValue(NullableValueLoc loc);
}