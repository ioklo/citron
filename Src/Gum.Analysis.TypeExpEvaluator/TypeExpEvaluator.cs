using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using Gum.Log;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    public partial class TypeExpEvaluator
    {
        class TypeExpEvaluatorFatalException : Exception
        {
        }

        M.Name internalModuleName;
        ImmutableArray<ModuleDeclSymbol> referenceModules;
        TypeSkeletonRepository skelRepo;
        ILogger logger;
        
        Dictionary<S.TypeExp, TypeExpInfo> infosByTypeExp;
        ImmutableDictionary<string, M.TypeVarTypeId> typeEnv;
        int totalTypeParamCount;

        public static TypeExpInfoService Evaluate(
            M.Name internalModuleName,
            S.Script script,
            ImmutableArray<ModuleDeclSymbol> externalInfos,
            ILogger logger)
        {
            var skelRepo = TypeSkeletonCollector.Collect(script);
            var evaluator = new TypeExpEvaluator(internalModuleName, externalInfos, skelRepo, logger);

            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        evaluator.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement funcDeclElem:
                        evaluator.VisitGlobalFuncDecl(funcDeclElem.FuncDecl);
                        break;

                    case S.StmtScriptElement stmtDeclElem:
                        evaluator.VisitStmt(stmtDeclElem.Stmt);
                        break;
                }
            }            

            if (logger.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }
            
            return new TypeExpInfoService(evaluator.infosByTypeExp.ToImmutableDictionary());
        }

        TypeExpEvaluator(M.Name internalModuleName, ImmutableArray<IModuleDecl> externalInfos, TypeSkeletonRepository skelRepo, ILogger logger)
        {
            this.internalModuleName = internalModuleName;
            this.referenceModules = externalInfos;
            this.skelRepo = skelRepo;
            this.logger = logger;
            
            infosByTypeExp = new Dictionary<S.TypeExp, TypeExpInfo>(ReferenceEqualityComparer.Instance);
            typeEnv = ImmutableDictionary<string, M.TypeVarTypeId>.Empty;
            totalTypeParamCount = 0;
        }        

        [DoesNotReturn]
        void Throw(TypeExpErrorCode code, S.ISyntaxNode node, string msg)
        {
            logger.Add(new TypeExpErrorLog(code, node, msg));
            throw new TypeExpEvaluatorFatalException();
        }

        void AddInfo(S.TypeExp exp, TypeExpInfo info)
        {
            infosByTypeExp.Add(exp, info);
        }

        M.TypeVarTypeId? GetTypeVar(string name)
        {
            return typeEnv.GetValueOrDefault(name);
        }

        void ExecInScope(ImmutableArray<string> typeParams, Action action)
        {
            var prevTypeEnv = typeEnv;
            var prevTotalTypeParamCount = totalTypeParamCount;            
            
            foreach (var typeParam in typeParams)
            {
                typeEnv = typeEnv.SetItem(typeParam, new M.TypeVarTypeId(totalTypeParamCount, typeParam));
                totalTypeParamCount++;
            }            

            try
            {
                action();
            }
            finally
            {
                typeEnv = prevTypeEnv;
                totalTypeParamCount = prevTotalTypeParamCount;
            }
        }

        static TypeExpInfoKind GetTypeExpInfoKind(TypeSkeletonKind kind)
        {
            switch(kind)
            {
                case TypeSkeletonKind.Class: return TypeExpInfoKind.Class;
                case TypeSkeletonKind.Struct: return TypeExpInfoKind.Struct;
                case TypeSkeletonKind.Interface: return TypeExpInfoKind.Interface;
                case TypeSkeletonKind.Enum: return TypeExpInfoKind.Enum;
            }

            throw new UnreachableCodeException();
        }

        static TypeExpInfoKind GetTypeExpInfoKind(ITypeDeclSymbolNode node)
        {
            return node switch
            {
                ClassDeclSymbol => TypeExpInfoKind.Class,
                StructDeclSymbol => TypeExpInfoKind.Struct,
                EnumDeclSymbol => TypeExpInfoKind.Enum,
                EnumElemDeclSymbol => TypeExpInfoKind.EnumElem,
                _ => throw new UnreachableCodeException()
            };
        }        
        
        IEnumerable<TypeExpResult> GetTypeExpInfos(M.NamespacePath? ns, M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            var declPath = new M.RootTypeDeclPath(ns, new M.TypeName(name, typeArgs.Length));

            var typeSkel = skelRepo.GetRootTypeSkeleton(declPath);
            if (typeSkel != null)
            {
                var mtype = new M.RootTypeId(internalModuleName, ns, name, typeArgs);
                var kind = GetTypeExpInfoKind(typeSkel.Kind);

                var typeExpInfo = new MTypeTypeExpInfo(mtype, kind, true);
                yield return new InternalTypeExpResult(typeSkel, typeExpInfo);
            }

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var referenceModule in referenceModules)
            {
                var typeInfo = referenceModule.GetType(declPath);
                
                if (typeInfo != null)
                {
                    var mtype = new M.RootTypeId(referenceModule.GetName(), ns, name, typeArgs);
                    var kind = GetTypeExpInfoKind(typeInfo);
                    var typeExpInfo = new MTypeTypeExpInfo(mtype, kind, false);
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

                case S.ClassDecl classDecl:
                    VisitClassDecl(classDecl);
                    break;

                case S.EnumDecl enumDecl:
                    VisitEnumDecl(enumDecl);
                    break;

                

                default:
                    throw new UnreachableCodeException();
            }
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            ExecInScope(structDecl.TypeParams, () =>
            {
                foreach (var baseType in structDecl.BaseTypes)
                    VisitTypeExpOuterMost(baseType);

                foreach(var elem in structDecl.MemberDecls)
                {
                    switch(elem)
                    {
                        case S.StructMemberTypeDecl typeDecl:
                            VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.StructMemberFuncDecl funcDecl:
                            VisitStructMemberFuncDecl(funcDecl);
                            break;

                        case S.StructMemberVarDecl varDecl:
                            VisitTypeExpOuterMost(varDecl.VarType);
                            break;

                        case S.StructConstructorDecl constructorDecl:
                            VisitStructConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            });
        }

        void VisitClassDecl(S.ClassDecl classDecl)
        {
            ExecInScope(classDecl.TypeParams, () =>
            {
                foreach (var baseType in classDecl.BaseTypes)
                    VisitTypeExpOuterMost(baseType);

                foreach (var elem in classDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.ClassMemberTypeDecl typeDecl:
                            VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.ClassMemberFuncDecl funcDecl:
                            VisitClassMemberFuncDecl(funcDecl);
                            break;

                        case S.ClassMemberVarDecl varDecl:
                            VisitTypeExpOuterMost(varDecl.VarType);
                            break;

                        case S.ClassConstructorDecl constructorDecl:
                            VisitClassConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            });
        }


        void VisitGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
        {   
            ExecInScope(funcDecl.TypeParams, () =>
            {
                VisitTypeExpOuterMost(funcDecl.RetType);

                foreach (var param in funcDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }
        
        void VisitStructMemberFuncDecl(S.StructMemberFuncDecl funcDecl)
        {
            ExecInScope(funcDecl.TypeParams, () =>
            {
                VisitTypeExpOuterMost(funcDecl.RetType);

                foreach (var param in funcDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }

        void VisitStructConstructorDecl(S.StructConstructorDecl constructorDecl)
        {
            ExecInScope(default, () =>
            {
                foreach (var param in constructorDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(constructorDecl.Body);
            });
        }

        void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl funcDecl)
        {
            ExecInScope(funcDecl.TypeParams, () =>
            {
                VisitTypeExpOuterMost(funcDecl.RetType);

                foreach (var param in funcDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(funcDecl.Body);
            });
        }

        void VisitClassConstructorDecl(S.ClassConstructorDecl constructorDecl)
        {
            ExecInScope(default, () =>
            {
                foreach (var param in constructorDecl.Parameters)
                    VisitTypeExpOuterMost(param.Type);

                VisitStmt(constructorDecl.Body);
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

        ImmutableArray<M.TypeId> VisitTypeArgExps(ImmutableArray<S.TypeExp> typeArgExps)
        {
            var builder = ImmutableArray.CreateBuilder<M.TypeId>(typeArgExps.Length);
            foreach (var typeArgExp in typeArgExps)
            {
                var typeArgResult = VisitTypeExp(typeArgExp);

                var mtypeArg = typeArgResult.GetMType();

                if (mtypeArg == null)
                    throw new TypeExpEvaluatorFatalException();

                builder.Add(mtypeArg);
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
