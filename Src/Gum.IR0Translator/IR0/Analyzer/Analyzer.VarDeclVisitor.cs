using Gum.CompileTime;
using System.Collections.Generic;

namespace Gum.IR0
{
    partial class Analyzer
    {
        public abstract class VarDeclVisitor<TVarDecl>
        {
            internal VarDeclVisitor() { }
            public abstract void VisitElement(string name, TypeValue typeValue, Exp? initExp, Context context);
            public abstract TVarDecl Build();
        }

        public class PrivateGlobalVarDeclStmtBuilder : VarDeclVisitor<PrivateGlobalVarDeclStmt>
        {
            List<PrivateGlobalVarDeclStmt.Element> elems;
            Context context;

            public PrivateGlobalVarDeclStmtBuilder(Context context)
            {
                elems = new List<PrivateGlobalVarDeclStmt.Element>();
                this.context = context;
            }

            public override void VisitElement(string name, TypeValue typeValue, Exp? initExp, Context context)
            {
                context.AddPrivateGlobalVarInfo(name, typeValue);
                var typeId = context.GetTypeId(typeValue);
                elems.Add(new PrivateGlobalVarDeclStmt.Element(name, typeId, initExp));
            }

            public override PrivateGlobalVarDeclStmt Build()
            {
                return new PrivateGlobalVarDeclStmt(elems);
            }
        }

        public class LocalVarDeclBuilder : VarDeclVisitor<LocalVarDecl>
        {
            List<LocalVarDecl.Element> elems;
            Context context;

            public LocalVarDeclBuilder(Context context)
            {
                elems = new List<LocalVarDecl.Element>();
                this.context = context;
            }

            public override void VisitElement(string name, TypeValue typeValue, Exp? initExp, Context context)
            {
                context.AddLocalVarInfo(name, typeValue);
                var typeId = context.GetTypeId(typeValue);
                elems.Add(new LocalVarDecl.Element(name, typeId, initExp));
            }

            public override LocalVarDecl Build()
            {
                return new LocalVarDecl(elems);
            }
        }
    }
}
