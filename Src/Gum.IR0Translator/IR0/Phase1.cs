using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using S = Gum.Syntax;
using static Gum.IR0.AnalyzeErrorCode;
using Gum.CompileTime;
using System.Collections.Immutable;
using Gum.Infra;

namespace Gum.IR0
{
    // 더 이상 진행하기 힘들면, Phase1Exception 예외를 날린다
    // 리턴값은 진행할 수 있는 상태이다 (바로 리턴하는 경우를 없앤다.. 그건 예외처리)
    partial class Phase1
    {
        S.Script script;
        TypeExpEvaluator typeExpEvaluator;
        SkeletonRepository skelRepo;
        IErrorCollector errorCollector;
        Phase2Factory phase2Factory;

        public Phase1(S.Script script, TypeExpEvaluator typeExpEvaluator, SkeletonRepository skelRepo, IErrorCollector errorCollector, Phase2Factory phase2Factory)
        {
            this.script = script;
            this.typeExpEvaluator = typeExpEvaluator;
            this.skelRepo = skelRepo;
            this.errorCollector = errorCollector;
            this.phase2Factory = phase2Factory;
        }

        void VisitEnumDeclElement(S.EnumDeclElement enumDeclElem)
        {
            foreach (var param in enumDeclElem.Params)
                VisitTypeExp(param.Type);
        }

        void VisitEnumSkel(Skeleton.Enum enumSkel)
        {
            typeExpEvaluator.ExecInScope(enumSkel.Path, enumSkel.EnumDecl.TypeParams, () =>
            {
                foreach (var elem in enumSkel.EnumDecl.Elems)
                {
                    VisitEnumDeclElement(elem);                    
                }
            });
        }

        void VisitStructSkel(Skeleton.Struct structSkel)
        {
            typeExpEvaluator.ExecInScope(structSkel.Path, structSkel.StructDecl.TypeParams, () =>
            {
                foreach (var member in structSkel.GetMembers())
                {
                    VisitSkel(member);
                }
            });
        }

        void VisitFuncSkel(Skeleton.Func funcSkel)
        {
            var funcDecl = funcSkel.FuncDecl;

            typeExpEvaluator.ExecInScope(funcSkel.Path, funcDecl.TypeParams, () =>
            {
                VisitTypeExpNoResult(funcDecl.RetType);

                foreach (var param in funcDecl.ParamInfo.Parameters)
                    VisitTypeExpNoResult(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }

        void VisitVarSkel(Skeleton.Var varSkel)
        {
            var varDecl = varSkel.VarDecl;
            var varDeclElem = varDecl.Elems[varSkel.ElemIndex];

            // NOTICE: 여러번 하게 되기 때문에, 한번만 하도록 0일때만 하게 했다
            if (varSkel.ElemIndex == 0)
                VisitTypeExpNoResult(varDecl.Type);

            if (varDeclElem.InitExp != null)
                VisitExp(varDeclElem.InitExp);
        }

        void VisitVarDecl(S.VarDecl varDecl)
        {
            VisitTypeExpNoResult(varDecl.Type);

            foreach (var varDeclElem in varDecl.Elems)
            {
                if (varDeclElem.InitExp != null)
                    VisitExp(varDeclElem.InitExp);
            }
        }

        void VisitStringExpElements(ImmutableArray<S.StringExpElement> elems)
        {
            foreach (var elem in elems)
            {
                switch (elem)
                {
                    case S.TextStringExpElement _: break;
                    case S.ExpStringExpElement expElem: VisitExp(expElem.Exp); break;
                    default: throw new InvalidOperationException();
                }
            }
        }

        List<TypeValue> VisitTypeArgExps(ImmutableArray<S.TypeExp> typeArgExps)
        {
            var typeArgs = new List<TypeValue>(typeArgExps.Length);
            foreach (var typeArgExp in typeArgExps)
            {
                var typeArgResult = VisitTypeExp(typeArgExp);
                typeArgs.Add(typeArgResult.Info.GetTypeValue());
            }

            return typeArgs;
        }

        void VisitTypeArgExpsNoResult(ImmutableArray<S.TypeExp> typeArgExps)
        {
            try
            {
                VisitTypeArgExps(typeArgExps);
            }
            catch(TypeExpEvaluatorException ex)
            {
                errorCollector.Add(ex.Error);
            }
        }
        
        void VisitSkel(Skeleton skel)
        {
            // type skel matching directly, not through Skeleton.Type
            switch (skel)
            {
                case Skeleton.Enum enumSkel:
                    VisitEnumSkel(enumSkel);
                    break;

                case Skeleton.Struct structSkel:
                    VisitStructSkel(structSkel);
                    break;

                case Skeleton.Func funcSkel:
                    VisitFuncSkel(funcSkel);
                    break;

                case Skeleton.Var varSkel:
                    VisitVarSkel(varSkel);
                    break;
            }
        }

        public Phase2 Run()
        {   
            foreach (var globalSkel in skelRepo.GetGlobalSkeletons())
                VisitSkel(globalSkel);

            // Toplevel statements
            foreach (var elem in script.Elements)
            {
                if (elem is S.Script.StmtElement stmtElem)
                {
                    if (stmtElem.Stmt is S.VarDeclStmt) continue; // 최상위 varDecl은 넘어간다
                    VisitStmt(stmtElem.Stmt);
                }
            }
            
            if (errorCollector.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }

            var typeExpTypeValueService = typeExpEvaluator.MakeTypeExpTypeValueService();
            return phase2Factory.Make(skelRepo, typeExpTypeValueService);
        }
    }
}
