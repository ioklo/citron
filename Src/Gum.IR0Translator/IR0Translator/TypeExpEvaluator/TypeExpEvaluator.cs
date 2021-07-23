using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class TypeExpEvaluator
    {
        class TypeExpEvaluatorFatalException : Exception
        {
        }

        M.ModuleName internalModuleName;
        ModuleInfoRepository externalModuleInfoRepo;
        TypeSkeletonRepository skelRepo;
        IErrorCollector errorCollector;
        
        Dictionary<S.TypeExp, TypeExpInfo> infosByTypeExp;
        ImmutableDictionary<string, M.TypeVarType> typeEnv;

        public static TypeExpInfoService Evaluate(
            M.ModuleName internalModuleName,
            S.Script script,
            ModuleInfoRepository externalModuleInfoRepo,
            TypeSkeletonRepository skelRepo,
            IErrorCollector errorCollector)
        {
            var evaluator = new TypeExpEvaluator(internalModuleName, externalModuleInfoRepo, skelRepo, errorCollector);

            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        evaluator.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement funcDeclElem:
                        evaluator.VisitFuncDecl(funcDeclElem.FuncDecl);
                        break;

                    case S.StmtScriptElement stmtDeclElem:
                        evaluator.VisitStmt(stmtDeclElem.Stmt);
                        break;
                }
            }            

            if (errorCollector.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }
            
            return new TypeExpInfoService(evaluator.infosByTypeExp.ToImmutableDictionary());
        }

        TypeExpEvaluator(M.ModuleName internalModuleName, ModuleInfoRepository externalModuleInfoRepo, TypeSkeletonRepository skelRepo, IErrorCollector errorCollector)
        {
            this.internalModuleName = internalModuleName;
            this.externalModuleInfoRepo = externalModuleInfoRepo;
            this.skelRepo = skelRepo;
            this.errorCollector = errorCollector;
            
            infosByTypeExp = new Dictionary<S.TypeExp, TypeExpInfo>(ReferenceEqualityComparer.Instance);
            typeEnv = ImmutableDictionary<string, M.TypeVarType>.Empty;
        }        

        [DoesNotReturn]
        void Throw(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
        {
            errorCollector.Add(new AnalyzeError(code, node, msg));
            throw new TypeExpEvaluatorFatalException();
        }

        void AddInfo(S.TypeExp exp, TypeExpInfo info)
        {
            infosByTypeExp.Add(exp, info);
        }

        M.TypeVarType? GetTypeVar(string name)
        {
            return typeEnv.GetValueOrDefault(name);
        }

        void ExecInScope(ImmutableArray<string> typeParams, Action action)
        {
            var prevTypeEnv = typeEnv;

            int i = 0;
            foreach (var typeParam in typeParams)
            {
                typeEnv = typeEnv.SetItem(typeParam, new M.TypeVarType(i, typeParam));
                i++;
            }            

            try
            {
                action();
            }
            finally
            {
                typeEnv = prevTypeEnv;
            }
        }

        IEnumerable<TypeExpResult> GetTypeExpInfos(M.NamespacePath namespacePath, M.Name name, ImmutableArray<M.Type> typeArgs)
        {
            var itemPathEntry = new ItemPathEntry(name, typeArgs.Length);

            var typeSkel = skelRepo.GetRootTypeSkeleton(namespacePath, itemPathEntry);
            if (typeSkel != null)
            {
                var mtype = new M.GlobalType(internalModuleName, namespacePath, name, typeArgs);
                var typeExpInfo = new MTypeTypeExpInfo(mtype);
                yield return new InternalTypeExpResult(typeSkel, typeExpInfo);
            }

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var moduleInfo in externalModuleInfoRepo.GetAllModules())
            {
                var typeInfo = GlobalItemQueryService.GetGlobalItem(moduleInfo, namespacePath, itemPathEntry) as M.TypeInfo;
                if (typeInfo != null)
                {
                    var mtype = new M.GlobalType(moduleInfo.Name, namespacePath, name, typeArgs);
                    var typeExpInfo = new MTypeTypeExpInfo(mtype);
                    yield return new ExternalTypeExpResult(typeExpInfo, typeInfo);
                }
            }
        }        
       
        void VisitEnumDeclElement(S.EnumDeclElement enumDeclElem)
        {
            foreach (var param in enumDeclElem.Fields)
                VisitTypeExpOuterMost(param.Type);
        }

        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            ExecInScope(enumDecl.TypeParams, () =>
            {
                foreach (var elem in enumDecl.Elems)
                {
                    VisitEnumDeclElement(elem);
                }
            });
        }

        void VisitTypeDecl(S.TypeDecl typeDecl)
        {
            switch (typeDecl)
            {
                case S.StructDecl structDecl:
                    VisitStructDecl(structDecl);
                    break;

                case S.EnumDecl enumDecl:
                    VisitEnumDecl(enumDecl);
                    break;
            }
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            ExecInScope(structDecl.TypeParams, () =>
            {
                foreach (var baseType in structDecl.BaseTypes)
                    VisitTypeExpOuterMost(baseType);

                foreach(var elem in structDecl.Elems)
                {
                    switch(elem)
                    {
                        case S.TypeStructDeclElement typeDeclElem:
                            VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.FuncStructDeclElement funcDeclElem:
                            VisitFuncDecl(funcDeclElem.FuncDecl);
                            break;

                        case S.VarStructDeclElement varDeclElem:
                            VisitTypeExpOuterMost(varDeclElem.VarType);
                            break;

                        case S.ConstructorStructDeclElement constructorDeclElem:
                            VisitStructConstructorDecl(constructorDeclElem);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            });
        }

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {   
            ExecInScope(funcDecl.TypeParams, () =>
            {
                VisitTypeExpOuterMost(funcDecl.RetType);

                foreach (var param in funcDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }        

        void VisitStructConstructorDecl(S.ConstructorStructDeclElement constructorDeclElem)
        {
            ExecInScope(default, () =>
            {
                foreach (var param in constructorDeclElem.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(constructorDeclElem.Body);
            });
        }
        
        void VisitVarDecl(S.VarDecl varDecl)
        {
            VisitTypeExpOuterMost(varDecl.Type);

            foreach (var varDeclElem in varDecl.Elems)
            {
                if (varDeclElem.Initializer != null)                
                    VisitExp(varDeclElem.Initializer.Value.Exp);
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
                    default: throw new UnreachableCodeException();
                }
            }
        }

        ImmutableArray<M.Type> VisitTypeArgExps(ImmutableArray<S.TypeExp> typeArgExps)
        {
            var builder = ImmutableArray.CreateBuilder<M.Type>(typeArgExps.Length);
            foreach (var typeArgExp in typeArgExps)
            {
                var typeArgResult = VisitTypeExp(typeArgExp);

                if (typeArgResult.TypeExpInfo is MTypeTypeExpInfo mtypeArg)
                    builder.Add(mtypeArg.Type);
                else
                    throw new TypeExpEvaluatorFatalException();
            }

            return builder.MoveToImmutable();
        }

        void VisitTypeArgExpsOuterMost(ImmutableArray<S.TypeExp> typeArgExps)
        {
            foreach (var typeArgExp in typeArgExps)
                VisitTypeExpOuterMost(typeArgExp);
        }
    }
}
