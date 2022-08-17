using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Citron.Log;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Symbol.DeclSymbolIdExtensions;
using Citron.Symbol;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class FatalException : Exception
        {
        }
        
        // TypeExpInfo를 Syntax 트리에 추가한다
        public static void Evaluate(
            M.Name internalModuleName,
            S.Script script,
            ImmutableArray<ModuleDeclSymbol> referenceModules,
            ILogger logger)
        {
            var skelRepo = TypeSkeletonCollector.Collect(script);
            var typeEnv = TypeEnv.Empty;            
            var context = new Context(internalModuleName, referenceModules, skelRepo, logger);
            var declVisitor = new DeclVisitor(new DeclSymbolId(internalModuleName, null), typeEnv, 0, context);
            var stmtVisitor = new StmtVisitor(typeEnv, context);

            try
            {

                foreach (var elem in script.Elements)
                {
                    switch (elem)
                    {
                        case S.TypeDeclScriptElement typeDeclElem:
                            declVisitor.VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.GlobalFuncDeclScriptElement funcDeclElem:
                            declVisitor.VisitGlobalFuncDecl(funcDeclElem.FuncDecl);
                            break;

                        case S.StmtScriptElement stmtDeclElem:
                            stmtVisitor.VisitStmt(stmtDeclElem.Stmt);
                            break;
                    }
                }
            }
            catch(FatalException)
            {

            }

            if (logger.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }            
        }

        struct DeclVisitor
        {
            DeclSymbolId declId;
            TypeEnv typeEnv;
            int totalTypeParamCount;
            Context context;

            public DeclVisitor(DeclSymbolId declId, TypeEnv typeEnv, int totalTypeParamCount, Context context)
            {
                this.declId = declId;
                this.typeEnv = typeEnv;
                this.totalTypeParamCount = totalTypeParamCount;
                this.context = context;
            }

            void VisitEnumElemDecl(S.EnumElemDecl enumDeclElem)
            {
                foreach (var param in enumDeclElem.MemberVars)
                    TypeExpVisitor.Visit(param.Type, typeEnv, context);
            }            

            void VisitEnumDecl(S.EnumDecl enumDecl)
            {
                var newDeclId = declId.Child(new M.Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length, default);
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in enumDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }

                var newVisitor = new DeclVisitor(newDeclId, newTypeEnv, newTotalTypeParamCount, context);                
                foreach (var elem in enumDecl.Elems)
                    newVisitor.VisitEnumElemDecl(elem);
                
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

            void VisitStructDecl(S.StructDecl structDecl)
            {
                var newDeclId = declId.Child(new M.Name.Normal(structDecl.Name), structDecl.TypeParams.Length, default);
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in structDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }

                var newVisitor = new DeclVisitor(newDeclId, newTypeEnv, newTotalTypeParamCount, context);

                foreach (var baseType in structDecl.BaseTypes)
                    TypeExpVisitor.Visit(baseType, newTypeEnv, context);

                foreach (var elem in structDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.StructMemberTypeDecl typeDecl:
                            newVisitor.VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.StructMemberFuncDecl funcDecl:
                            newVisitor.VisitStructMemberFuncDecl(funcDecl);
                            break;

                        case S.StructMemberVarDecl varDecl:
                            TypeExpVisitor.Visit(varDecl.VarType, newTypeEnv, context);
                            break;

                        case S.StructConstructorDecl constructorDecl:
                            newVisitor.VisitStructConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            void VisitClassDecl(S.ClassDecl classDecl)
            {
                var newDeclId = declId.Child(new M.Name.Normal(classDecl.Name), classDecl.TypeParams.Length, default);
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in classDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }

                var newVisitor = new DeclVisitor(newDeclId, newTypeEnv, newTotalTypeParamCount, context);

                foreach (var baseType in classDecl.BaseTypes)
                    TypeExpVisitor.Visit(baseType, newTypeEnv, context);

                foreach (var elem in classDecl.MemberDecls)
                {
                    switch (elem)
                    {
                        case S.ClassMemberTypeDecl typeDecl:
                            newVisitor.VisitTypeDecl(typeDecl.TypeDecl);
                            break;

                        case S.ClassMemberFuncDecl funcDecl:
                            newVisitor.VisitClassMemberFuncDecl(funcDecl);
                            break;

                        case S.ClassMemberVarDecl varDecl:
                            TypeExpVisitor.Visit(varDecl.VarType, newTypeEnv, context);
                            break;

                        case S.ClassConstructorDecl constructorDecl:
                            newVisitor.VisitClassConstructorDecl(constructorDecl);
                            break;

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            public void VisitGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in funcDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }

                
                TypeExpVisitor.Visit(funcDecl.RetType, newTypeEnv, context);

                foreach (var param in funcDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, newTypeEnv, context);

                StmtVisitor.Visit(funcDecl.Body, newTypeEnv, context);                
            }

            void VisitStructMemberFuncDecl(S.StructMemberFuncDecl funcDecl)
            {
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in funcDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }
                
                TypeExpVisitor.Visit(funcDecl.RetType, newTypeEnv, context);

                foreach (var param in funcDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, newTypeEnv, context);

                StmtVisitor.Visit(funcDecl.Body, newTypeEnv, context);
            }

            void VisitStructConstructorDecl(S.StructConstructorDecl constructorDecl)
            {   
                foreach (var param in constructorDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, typeEnv, context);

                StmtVisitor.Visit(constructorDecl.Body, typeEnv, context);
            }

            void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl funcDecl)
            {
                var newTypeEnv = typeEnv;
                var newTotalTypeParamCount = totalTypeParamCount;

                foreach (var typeParam in funcDecl.TypeParams)
                {
                    newTypeEnv.Add(declId, typeParam, newTotalTypeParamCount);
                    newTotalTypeParamCount++;
                }
                
                TypeExpVisitor.Visit(funcDecl.RetType, newTypeEnv, context);

                foreach (var param in funcDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, newTypeEnv, context);

                StmtVisitor.Visit(funcDecl.Body, newTypeEnv, context);
            }

            void VisitClassConstructorDecl(S.ClassConstructorDecl constructorDecl)
            {
                foreach (var param in constructorDecl.Parameters)
                    TypeExpVisitor.Visit(param.Type, typeEnv, context);

                StmtVisitor.Visit(constructorDecl.Body, typeEnv, context);
            }
        }
    }
}
