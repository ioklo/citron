﻿using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;
using static Gum.IR0.IR0Factory;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        static ImmutableArray<R.Path> MakeTypeArgsByTypeParams(int baseTotalParamCount, int typeParamCount)
        {
            var builder = ImmutableArray.CreateBuilder<R.Path>(typeParamCount);
            for (int i = 0; i < typeParamCount; i++)
                builder.Add(new R.Path.TypeVarType(baseTotalParamCount + i));

            return builder.MoveToImmutable();
        }


        // R.Decl을 읽어 들여서 객체로 만든다
        partial struct DeclEvaluator
        {
            GlobalContext globalContext;
            ItemContainer curContainer;
            R.Path.Normal curPath;
            int totalTypeParamCount;

            public static void EvalDecls(GlobalContext globalContext, ItemContainer container, R.Path.Normal curPath, int totalParamCount, ImmutableArray<R.Decl> decls)
            {
                var declEvaluator = new DeclEvaluator(globalContext, container, curPath, totalParamCount);

                foreach (var decl in decls)
                    declEvaluator.EvalDecl(decl);
            }

            DeclEvaluator(GlobalContext globalContext, ItemContainer itemContainer, R.Path.Normal curPath, int totalParamCount)
            {
                this.globalContext = globalContext;
                this.curContainer = itemContainer;
                this.curPath = curPath;
                this.totalTypeParamCount = totalParamCount;
            }

            void EvalLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                var item = new IR0LambdaRuntimeItem(globalContext, lambdaDecl);
                curContainer.AddRuntimeItem(item);
            }

            void EvalNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                var funcContainer = new ItemContainer();
                
                // 하위 아이템을 저장할 container와 invoker를 추가한다 (같은 키로)
                var typeParamCount = normalFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, normalFuncDecl.Parameters);
                curContainer.AddItemContainer(normalFuncDecl.Name, paramHash, funcContainer);

                var funcRuntimeItem = new IR0FuncRuntimeItem(globalContext, normalFuncDecl);
                curContainer.AddRuntimeItem(funcRuntimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, normalFuncDecl.TypeParams.Length);
                var funcPath = curPath.Child(normalFuncDecl.Name, paramHash, typeArgs);
                EvalDecls(globalContext, funcContainer, funcPath, totalTypeParamCount + normalFuncDecl.TypeParams.Length, normalFuncDecl.Decls);
            }

            void EvalSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, seqFuncDecl.Parameters);

                curContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var runtimeItem = new IR0SeqFuncRuntimeItem(globalContext, seqFuncDecl);
                curContainer.AddRuntimeItem(runtimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, seqFuncDecl.TypeParams.Length);
                var funcPath = curPath.Child(seqFuncDecl.Name, paramHash, typeArgs);
                EvalDecls(globalContext, itemContainer, funcPath, totalTypeParamCount + seqFuncDecl.TypeParams.Length, seqFuncDecl.Decls);
            }            

            void EvalDecl(R.Decl decl)
            {
                switch(decl)
                {
                    case R.StructDecl structDecl:
                        StructDeclEvaluator.Eval(globalContext, curContainer, curPath, totalTypeParamCount, structDecl);                        
                        break;

                    case R.ClassDecl classDecl:
                        ClassDeclEvaluator.Eval(globalContext, curContainer, curPath, totalTypeParamCount, classDecl);                        
                        break;

                    case R.LambdaDecl lambdaDecl:
                        EvalLambdaDecl(lambdaDecl);
                        break;

                    case R.NormalFuncDecl normalFuncDecl:
                        EvalNormalFuncDecl(normalFuncDecl);
                        break;

                    case R.SequenceFuncDecl seqFuncDecl:
                        EvalSequenceFuncDecl(seqFuncDecl);
                        break;

                    case R.EnumDecl enumDecl:
                        EvalEnumDecl(enumDecl);
                        break;

                    case R.CapturedStatementDecl capturedStmtDecl:
                        EvalCapturedStmtDecl(capturedStmtDecl);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }
            
            void EvalEnumDecl(R.EnumDecl enumDecl)
            {
                var enumContainer = new ItemContainer();

                foreach (var enumElem in enumDecl.Elems)
                {
                    var enumElemItem = new IR0EnumElemRuntimeItem(globalContext, enumElem);
                    var enumElemContainer = new ItemContainer();

                    int fieldIndex = 0;
                    foreach(var enumElemField in enumElem.Fields)
                    {
                        var enumElemFieldItem = new IR0EnumElemFieldRuntimeItem(enumElemField.Name, fieldIndex);
                        enumElemContainer.AddRuntimeItem(enumElemFieldItem);

                        fieldIndex++;
                    }

                    enumContainer.AddItemContainer(enumElem.Name, R.ParamHash.None, enumElemContainer);
                    enumContainer.AddRuntimeItem(enumElemItem);
                }

                curContainer.AddItemContainer(enumDecl.Name, new R.ParamHash(enumDecl.TypeParams.Length, default), enumContainer);
                curContainer.AddRuntimeItem(new IR0EnumRuntimeItem(enumDecl));
            }

            void EvalCapturedStmtDecl(R.CapturedStatementDecl capturedStmtDecl)
            {
                var runtimeItem = new IR0CapturedStmtRuntimeItem(capturedStmtDecl);
                curContainer.AddRuntimeItem(runtimeItem);
            }
        }
    }
}
