using Citron.Symbol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    public interface IIdentifierVisitor
    {   
        void VisitGlobalVarLoc(GlobalVarLocIdentifier id); // "g_x"
        void VisitLocalVarLoc(LocalVarLocIdentifier id); // "x"
        void VisitThisLoc(ThisLocIdentifier id); // "this"
        void VisitLambdaMemberVarLoc(LambdaMemberVarLocIdentifier id); // "x" => "lambdaContext.x"
        void VisitClassMemberVarLoc(ClassMemberVarLocIdentifier id);   // c.x
        void VisitStructMemberVarLoc(StructMemberVarLocIdentifier id); // s.x
        void VisitEnumElemMemberVarLoc(EnumElemMemberVarLocIdentifier id); // e.x

        // Type Identifiers
        void VisitClass(ClassIdentifier id);    // NS.C
        void VisitStruct(StructIdentifier id);  // NS.S
        void VisitEnum(EnumIdentifier id);      // NS.E

        // Funcs Identifiers
        void VisitGlobalFuncs(GlobalFuncsIdentifier id);
        void VisitClassMemberFuncs(ClassMemberFuncsIdentifier id);
        void VisitStructMemberFuncs(StructMemberFuncsIdentifier id);

        // First => E.First, enum elem constructor
        void VisitEnumElem(EnumElemIdentifier id);
    }

    public static class IdentifierVisitorExtensions
    {
        public static void Accept<TIdentifierVisitor>(this Identifier id, ref TIdentifierVisitor visitor)
            where TIdentifierVisitor : struct, IIdentifierVisitor
        {
            var impl = (IdentifierImpl)id; // 강제 캐스팅
            impl.Visit(ref visitor);
        }
    }  

}
