using Gum.CompileTime;
using System.Collections.Generic;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
    {
        public abstract class VarDeclVisitor<TVarDecl>
        {
            internal VarDeclVisitor() { }
            public abstract void VisitElement(string name, TypeValue typeValue, IR0.Exp? initExp, Context context);
            public abstract TVarDecl Build();
        }

        public class PrivateGlobalVarDeclStmtBuilder : VarDeclVisitor<IR0.PrivateGlobalVarDeclStmt>
        {
            List<IR0.PrivateGlobalVarDeclStmt.Element> elems;
            Context context;

            public PrivateGlobalVarDeclStmtBuilder(Context context)
            {
                elems = new List<IR0.PrivateGlobalVarDeclStmt.Element>();
                this.context = context;
            }

            public override void VisitElement(string name, TypeValue typeValue, IR0.Exp? initExp, Context context)
            {
                context.AddPrivateGlobalVarInfo(name, typeValue);
                elems.Add(new IR0.PrivateGlobalVarDeclStmt.Element(name, typeValue, initExp));
            }

            public override IR0.PrivateGlobalVarDeclStmt Build()
            {
                return new IR0.PrivateGlobalVarDeclStmt(elems);
            }
        }

        public class LocalVarDeclBuilder : VarDeclVisitor<IR0.LocalVarDecl>
        {
            List<IR0.LocalVarDecl.Element> elems;
            Context context;

            public LocalVarDeclBuilder(Context context)
            {
                elems = new List<IR0.LocalVarDecl.Element>();
                this.context = context;
            }

            public override void VisitElement(string name, TypeValue typeValue, IR0.Exp? initExp, Context context)
            {
                context.AddLocalVarInfo(name, typeValue);
                elems.Add(new IR0.LocalVarDecl.Element(name, typeValue, initExp));
            }

            public override IR0.LocalVarDecl Build()
            {
                return new IR0.LocalVarDecl(elems);
            }
        }
    }
}
