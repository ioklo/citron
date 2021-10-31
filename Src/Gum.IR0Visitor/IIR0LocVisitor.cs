using System;
using R = Gum.IR0;

namespace Gum.IR0Visitor
{
    public interface IIR0LocVisitor
    {
        void VisitTempLoc(R.TempLoc loc);
        void VisitGlobalVarLoc(R.GlobalVarLoc loc);
        void VisitLocalVarLoc(R.LocalVarLoc loc);
        void VisitCapturedVarLoc(R.CapturedVarLoc loc);
        void VisitListIndexerLoc(R.ListIndexerLoc loc);
        void VisitStaticMemberLoc(R.StaticMemberLoc loc);
        void VisitStructMemberLoc(R.StructMemberLoc loc);
        void VisitClassMemberLoc(R.ClassMemberLoc loc);
        void VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc);
        void VisitThisLoc(R.ThisLoc loc);
        void VisitDerefLocLoc(R.DerefLocLoc loc);
        void VisitDerefExpLoc(R.DerefExpLoc loc);
    }

    public interface IIR0LocVisitorWithRet<TRet>
    {
        TRet VisitTempLoc(R.TempLoc loc);
        TRet VisitGlobalVarLoc(R.GlobalVarLoc loc);
        TRet VisitLocalVarLoc(R.LocalVarLoc loc);
        TRet VisitCapturedVarLoc(R.CapturedVarLoc loc);
        TRet VisitListIndexerLoc(R.ListIndexerLoc loc);
        TRet VisitStaticMemberLoc(R.StaticMemberLoc loc);
        TRet VisitStructMemberLoc(R.StructMemberLoc loc);
        TRet VisitClassMemberLoc(R.ClassMemberLoc loc);
        TRet VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc);
        TRet VisitThisLoc(R.ThisLoc loc);
        TRet VisitDerefLocLoc(R.DerefLocLoc loc);
        TRet VisitDerefExpLoc(R.DerefExpLoc loc);
    }
}
