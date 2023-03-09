using System;

namespace Citron.IR0;

public interface IIR0LocVisitor
{
    void VisitTemp(TempLoc loc);
    void VisitLocalVar(LocalVarLoc loc);
    void VisitLambdaMemberVar(LambdaMemberVarLoc loc);
    void VisitListIndexer(ListIndexerLoc loc);        
    void VisitStructMember(StructMemberLoc loc);
    void VisitClassMember(ClassMemberLoc loc);
    void VisitEnumElemMember(EnumElemMemberLoc loc);
    void VisitThis(ThisLoc loc);
    void VisitDerefLoc(DerefLocLoc loc);
    void VisitDerefExp(DerefExpLoc loc);
    void VisitNullableValue(NullableValueLoc loc);
}