using Gum.Infra;
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
}
