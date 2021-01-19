using Gum.CompileTime;
using Gum.Infra;
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    // TypeExp를 TypeValue로 바꿔서 저장합니다.
    partial class TypeExpEvaluator : ISyntaxScriptVisitor
    {
        class TypeExpEvaluatorFatalException : Exception
        {
        }

        ModuleInfoRepository externalModuleInfoRepo;
        TypeSkeletonRepository skelRepo;
        IErrorCollector errorCollector;

        int nestedTypeDepth;
        Dictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;
        ImmutableDictionary<string, TypeValue.TypeVar> typeEnv;

        public static TypeExpTypeValueService Evaluate(
            S.Script script,
            ModuleInfoRepository externalModuleInfoRepo,
            TypeSkeletonRepository skelRepo,
            IErrorCollector errorCollector)
        {
            var evaluator = new TypeExpEvaluator(externalModuleInfoRepo, skelRepo, errorCollector);

            Misc.VisitScript(script, evaluator);

            if (errorCollector.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }
            
            return new TypeExpTypeValueService(evaluator.typeValuesByTypeExp.ToImmutableDictionary());
        }

        TypeExpEvaluator(ModuleInfoRepository externalModuleInfoRepo, TypeSkeletonRepository skelRepo, IErrorCollector errorCollector)
        {
            this.externalModuleInfoRepo = externalModuleInfoRepo;
            this.skelRepo = skelRepo;
            this.errorCollector = errorCollector;

            nestedTypeDepth = 0;
            typeValuesByTypeExp = new Dictionary<S.TypeExp, TypeValue>();            
            typeEnv = ImmutableDictionary<string, TypeValue.TypeVar>.Empty;
        }

        #region ISyntaxScriptVisitor implementation

        void ISyntaxScriptVisitor.VisitGlobalFuncDecl(S.FuncDecl funcDecl)
        {
            VisitFuncDecl(funcDecl);
        }
        
        void ISyntaxScriptVisitor.VisitTypeDecl(S.TypeDecl typeDecl)
        {
            VisitTypeDecl(typeDecl);
        }

        void ISyntaxScriptVisitor.VisitTopLevelStmt(S.Stmt stmt)
        {
            VisitStmt(stmt);
        }

        #endregion

        [DoesNotReturn]
        void Throw(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
        {
            errorCollector.Add(new AnalyzeError(code, node, msg));
            throw new TypeExpEvaluatorFatalException();
        }

        void AddTypeValue(S.TypeExp exp, TypeValue typeValue)
        {
            typeValuesByTypeExp.Add(exp, typeValue);
        }        

        TypeValue.TypeVar? GetTypeVar(string name)
        {
            return typeEnv.GetValueOrDefault(name);
        }

        void ExecInScope(IEnumerable<string> typeParams, Action action)
        {
            var prevTypeEnv = typeEnv;

            int i = 0;
            foreach (var typeParam in typeParams)
            {
                typeEnv = typeEnv.SetItem(typeParam, new TypeValue.TypeVar(nestedTypeDepth, i, typeParam));
                i++;
            }

            nestedTypeDepth++;

            try
            {
                action();
            }
            finally
            {
                nestedTypeDepth--;
                typeEnv = prevTypeEnv;
            }
        }

        TypeExpInfo HandleBuiltInType(S.IdTypeExp exp, string name, ItemId itemId)
        {
            if (exp.TypeArgs.Length != 0)
                Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, $"{name}은 타입 인자를 가질 수 없습니다");
            
            AddTypeValue(exp, new TypeValue.Normal(itemId)); // NOTICE: no type args
            return new TypeExpInfo.NoMember(new TypeValue.Normal(itemId));
        }

        TypeInfo? GetExternalType(IModuleInfo moduleInfo, ItemPath path)
        {
            if (path.OuterEntries.Length == 0)
                return moduleInfo.GetGlobalItem(path.NamespacePath, path.Entry) as TypeInfo;

            var curTypeInfo = moduleInfo.GetGlobalItem(path.NamespacePath, path.OuterEntries[0]) as TypeInfo;
            if (curTypeInfo == null) return null;

            for (int i = 1; i < path.OuterEntries.Length; i++)
            {
                curTypeInfo = curTypeInfo.GetItem(path.OuterEntries[i]) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo.GetItem(path.Entry) as TypeInfo;
        }

        IEnumerable<TypeInfo> GetExternalTypes(ItemPath itemPath)
        {
            foreach (var externalModuleInfo in externalModuleInfoRepo.GetAllModules())
            {
                var typeInfo = GetExternalType(externalModuleInfo, itemPath);
                if (typeInfo != null)
                    yield return typeInfo;
            }
        }

        IEnumerable<TypeExpInfo> GetTypeExpInfos(AppliedItemPath appliedItemPath)
        {
            var itemPath = appliedItemPath.GetItemPath();
            var typeSkel = skelRepo.GetTypeSkeleton(itemPath);

            if (typeSkel != null)
                yield return new TypeExpInfo.Internal(typeSkel, new TypeValue.Normal(ModuleName.Internal, appliedItemPath));

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var typeInfo in GetExternalTypes(itemPath))
                yield return new TypeExpInfo.External(typeInfo, new TypeValue.Normal(typeInfo.GetId().ModuleName, appliedItemPath));
        }        
       
        void VisitEnumDeclElement(S.EnumDeclElement enumDeclElem)
        {
            foreach (var param in enumDeclElem.Params)
                VisitTypeExpNoThrow(param.Type);
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
                foreach(var elem in structDecl.Elems)
                {
                    switch(elem)
                    {
                        case S.StructDecl.TypeDeclElement typeDeclElem:
                            VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.StructDecl.FuncDeclElement funcDeclElem:
                            VisitFuncDecl(funcDeclElem.FuncDecl);
                            break;

                        case S.StructDecl.VarDeclElement varDeclElem:
                            VisitTypeExpNoThrow(varDeclElem.VarType);
                            break;                        
                    }

                    throw new UnreachableCodeException();
                }
            });
        }

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {   
            ExecInScope(funcDecl.TypeParams, () =>
            {
                VisitTypeExpNoThrow(funcDecl.RetType);

                foreach (var param in funcDecl.ParamInfo.Parameters)
                    VisitTypeExpNoThrow(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }        
        
        void VisitVarDecl(S.VarDecl varDecl)
        {
            VisitTypeExpNoThrow(varDecl.Type);

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
                    default: throw new UnreachableCodeException();
                }
            }
        }

        List<TypeValue> VisitTypeArgExps(ImmutableArray<S.TypeExp> typeArgExps)
        {
            var typeArgs = new List<TypeValue>(typeArgExps.Length);
            foreach (var typeArgExp in typeArgExps)
            {
                var typeArgInfo = VisitTypeExp(typeArgExp);
                typeArgs.Add(typeArgInfo.GetTypeValue());
            }

            return typeArgs;
        }

        void VisitTypeArgExpsNoReturn(ImmutableArray<S.TypeExp> typeArgExps)
        {
            try
            {
                VisitTypeArgExps(typeArgExps);
            }
            catch (TypeExpEvaluatorFatalException)
            {
            }
        }
    }
}
