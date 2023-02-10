﻿using System.Diagnostics;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Symbol.DeclSymbolIdExtensions;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        struct DeclVisitor
        {   
            LocalContext localContext;
            GlobalContext globalContext;

            public DeclVisitor(LocalContext localContext, GlobalContext globalContext)
            {   
                this.localContext = localContext;
                this.globalContext = globalContext;
            }

            public void VisitTypeDecl(S.TypeDecl typeDecl)
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

            void VisitFuncParams(LocalContext thisLocalContext, Holder<ImmutableArray<FuncParamId>> holder, ImmutableArray<S.FuncParam> funcParams)
            {
                var builder = ImmutableArray.CreateBuilder<FuncParamId>(funcParams.Length);
                foreach (var param in funcParams)
                {
                    var info = TypeExpVisitor.Visit(param.Type, thisLocalContext, globalContext);

                    // NOTICE: 여기 SymbolId는 This없이도 가능하다
                    var paramId = new FuncParamId(param.Kind.ToFuncParameterKind(), info.GetSymbolId());
                    builder.Add(paramId);
                }

                holder.SetValue(builder.MoveToImmutable());
            }
            
            #region Global
            public void VisitGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var holder = new Holder<ImmutableArray<FuncParamId>>();
                var newLocalContext = localContext.NewLocalContextWithFuncDecl(holder, funcDecl);

                TypeExpVisitor.Visit(funcDecl.RetType, newLocalContext, globalContext);

                VisitFuncParams(newLocalContext, holder, funcDecl.Parameters);

                StmtVisitor.Visit(funcDecl.Body, newLocalContext, globalContext);
            }
            #endregion

            #region Struct

            void VisitStructDecl(S.StructDecl structDecl)
            {
                var newLocalContext = localContext.NewLocalContext(new M.Name.Normal(structDecl.Name), structDecl.TypeParams.Length);

                // 베이스 타입
                foreach (var baseType in structDecl.BaseTypes)
                    TypeExpVisitor.Visit(baseType, newLocalContext, globalContext);

                // 멤버 타입들
                var newVisitor = new DeclVisitor(newLocalContext, globalContext);
                newVisitor.VisitStructMemberDecls(structDecl.MemberDecls);
            }

            void VisitStructMemberDecls(ImmutableArray<S.StructMemberDecl> memberDecls)
            {
                foreach (var memberDecl in memberDecls)
                {
                    switch (memberDecl)
                    {
                        case S.StructMemberTypeDecl typeDecl:
                            VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.StructMemberFuncDecl funcDecl:
                            VisitStructMemberFuncDecl(funcDecl);
                            break;

                        case S.StructMemberVarDecl varDecl:
                            TypeExpVisitor.Visit(varDecl.VarType, localContext, globalContext);
                            break;

                        case S.StructConstructorDecl constructorDecl:
                            VisitStructConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            void VisitStructMemberFuncDecl(S.StructMemberFuncDecl funcDecl)
            {
                // skeleton에는 타입에 관한 정보만 들어있었기 때문에, funcDecl의 typeVar는 손수 넣어줘야 한다                
                var holder = new Holder<ImmutableArray<FuncParamId>>();
                var newLocalContext = localContext.NewLocalContextWithFuncDecl(holder, funcDecl);
                
                TypeExpVisitor.Visit(funcDecl.RetType, newLocalContext, globalContext);

                VisitFuncParams(newLocalContext, holder, funcDecl.Parameters);

                StmtVisitor.Visit(funcDecl.Body, newLocalContext, globalContext);
            }

            void VisitStructConstructorDecl(S.StructConstructorDecl constructorDecl)
            {
                foreach (var param in constructorDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, localContext, globalContext);

                StmtVisitor.Visit(constructorDecl.Body, localContext, globalContext);
            }
            #endregion

            #region Class
            void VisitClassDecl(S.ClassDecl classDecl)
            {
                var newLocalContext = localContext.NewLocalContext(new M.Name.Normal(classDecl.Name), classDecl.TypeParams.Length);

                // 베이스 타입
                foreach (var baseType in classDecl.BaseTypes)
                    TypeExpVisitor.Visit(baseType, newLocalContext, globalContext);

                // 멤버 타입들
                var newVisitor = new DeclVisitor(newLocalContext, globalContext);
                newVisitor.VisitClassMemberDecls(classDecl.MemberDecls);
            }

            void VisitClassMemberDecls(ImmutableArray<S.ClassMemberDecl> memberDecls)
            {
                foreach (var memberDecl in memberDecls)
                {
                    switch (memberDecl)
                    {
                        case S.ClassMemberTypeDecl typeDecl:
                            VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.ClassMemberFuncDecl funcDecl:
                            VisitClassMemberFuncDecl(funcDecl);
                            break;

                        case S.ClassMemberVarDecl varDecl:
                            TypeExpVisitor.Visit(varDecl.VarType, localContext, globalContext);
                            break;

                        case S.ClassConstructorDecl constructorDecl:
                            VisitClassConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl funcDecl)
            {
                var holder = new Holder<ImmutableArray<FuncParamId>>();

                // skeleton에는 타입에 관한 정보만 들어있었기 때문에, funcDecl의 typeVar는 손수 넣어줘야 한다
                var newLocalContext = localContext.NewLocalContextWithFuncDecl(holder, funcDecl);

                TypeExpVisitor.Visit(funcDecl.RetType, newLocalContext, globalContext);

                VisitFuncParams(newLocalContext, holder, funcDecl.Parameters);

                StmtVisitor.Visit(funcDecl.Body, newLocalContext, globalContext);
            }

            void VisitClassConstructorDecl(S.ClassConstructorDecl constructorDecl)
            {
                foreach (var param in constructorDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, localContext, globalContext);

                StmtVisitor.Visit(constructorDecl.Body, localContext, globalContext);
            }
            #endregion

            #region Enum
            void VisitEnumDecl(S.EnumDecl enumDecl)
            {
                var newLocalContext = localContext.NewLocalContext(new M.Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length);

                // 멤버 타입들
                var newVisitor = new DeclVisitor(newLocalContext, globalContext);
                newVisitor.VisitEnumElemDecls(enumDecl.Elems);

            }

            void VisitEnumElemDecls(ImmutableArray<S.EnumElemDecl> enumElemDecls)
            {
                foreach (var enumElemDecl in enumElemDecls)
                {
                    foreach (var param in enumElemDecl.MemberVars)
                        TypeExpVisitor.Visit(param.Type, localContext, globalContext);
                }
            }
            #endregion
        }
    }
}