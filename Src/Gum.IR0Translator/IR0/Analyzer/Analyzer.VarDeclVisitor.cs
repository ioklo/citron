using Gum.CompileTime;
using System.Collections.Generic;
using static Gum.IR0.AnalyzeErrorCode;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {
        public abstract class VarDeclVisitor<TVarDecl>
        {
            internal VarDeclVisitor() { }
            public abstract void VisitElement(S.ISyntaxNode nodeForErrorReport, string name, TypeValue typeValue, Exp? initExp);
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

            public override void VisitElement(S.ISyntaxNode nodeForErrorReport, string name, TypeValue typeValue, Exp? initExp)
            {
                if (context.DoesPrivateGlobalVarNameExist(name))
                {
                    context.AddError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, nodeForErrorReport, $"전역 변수 {name}가 이미 선언되었습니다");
                }
                else
                {
                    context.AddPrivateGlobalVarInfo(name, typeValue);
                    var typeId = context.GetType(typeValue);
                    elems.Add(new PrivateGlobalVarDeclStmt.Element(name, typeId, initExp));
                }
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

            public override void VisitElement(S.ISyntaxNode nodeForErrorReport, string name, TypeValue typeValue, Exp? initExp)
            {
                if (context.DoesLocalVarNameExistInScope(name))
                {
                    context.AddError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, nodeForErrorReport, $"지역 변수 {name}이 같은 범위에 이미 선언되었습니다");
                }
                else
                {
                    context.AddLocalVarInfo(name, typeValue);
                    var typeId = context.GetType(typeValue);
                    elems.Add(new LocalVarDecl.Element(name, typeId, initExp));
                }
            }

            public override LocalVarDecl Build()
            {
                return new LocalVarDecl(elems);
            }
        }
    }
}
