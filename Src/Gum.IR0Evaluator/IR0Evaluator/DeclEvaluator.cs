using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        // R.Decl을 읽어 들여서 객체로 만든다
        partial struct DeclEvaluator
        {
            GlobalContext globalContext;
            ItemContainer curContainer;

            public static ItemContainer EvalRoot(GlobalContext globalContext, ImmutableArray<R.Decl> decls)
            {
                ItemContainer itemContainer = new ItemContainer();

                var declEvaluator = new DeclEvaluator(globalContext, itemContainer);

                foreach (var decl in decls)
                    declEvaluator.EvalDecl(decl);

                return itemContainer;
            }

            DeclEvaluator(GlobalContext globalContext, ItemContainer itemContainer)
            {
                this.globalContext = globalContext;
                this.curContainer = itemContainer;
            }

            void EvalLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                var item = new IR0LambdaRuntimeItem(globalContext, lambdaDecl);
                curContainer.AddRuntimeItem(item);
            }

            void EvalNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                var itemContainer = new ItemContainer();
                
                // 하위 아이템을 저장할 container와 invoker를 추가한다 (같은 키로)
                var typeParamCount = normalFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, normalFuncDecl.Parameters);
                curContainer.AddItemContainer(normalFuncDecl.Name, paramHash, itemContainer);

                var funcRuntimeItem = new IR0FuncRuntimeItem(globalContext, normalFuncDecl);
                curContainer.AddRuntimeItem(funcRuntimeItem);

                var newDeclEvaluator = new DeclEvaluator(globalContext, itemContainer);
                foreach (var decl in normalFuncDecl.Decls)
                    newDeclEvaluator.EvalDecl(decl);
            }

            void EvalSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, seqFuncDecl.Parameters);

                curContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var runtimeItem = new IR0SeqFuncRuntimeItem(globalContext, seqFuncDecl);
                curContainer.AddRuntimeItem(runtimeItem);

                var newDeclEvaluator = new DeclEvaluator(globalContext, itemContainer);
                foreach (var decl in seqFuncDecl.Decls)
                    newDeclEvaluator.EvalDecl(decl);
            }

            void EvalDecl(R.Decl decl)
            {
                switch(decl)
                {
                    case R.StructDecl structDecl:
                        EvalStructDecl(structDecl);
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
            
            void EvalStructDeclMemberVarDecl(ItemContainer structContainer, R.StructDecl.MemberDecl.Var varDecl)
            {
                foreach (var name in varDecl.Names)
                {
                    var memberVarItem = new IR0StructMemberVarRuntimeItem(name);
                    structContainer.AddRuntimeItem(memberVarItem);
                }
            }

            void EvalStructDeclConstructorDecl(ItemContainer structContainer, R.StructDecl.MemberDecl.Constructor constructorDecl)
            {
                // NOTICE: Constructor는 typeParamCount가 0 이다
                var paramHash = Misc.MakeParamHash(typeParamCount: 0, constructorDecl.Parameters);
                var runtimeItem = new IR0ConstructorRuntimeItem(globalContext, constructorDecl, paramHash, constructorDecl.Parameters);

                var itemContainer = new ItemContainer();
                curContainer.AddItemContainer(R.Name.Constructor.Instance, paramHash, itemContainer);

                structContainer.AddRuntimeItem(runtimeItem);

                var newDeclEvaluator = new DeclEvaluator(globalContext, itemContainer);
                foreach (var decl in constructorDecl.Decls)
                    newDeclEvaluator.EvalDecl(decl);
            }

            void EvalStructDeclMemberFuncDecl(ItemContainer structContainer, R.StructDecl.MemberDecl.Func funcDecl)
            {
                var paramHash = Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
                var runtimeItem = new IR0StructFuncRuntimeItem(globalContext, funcDecl);

                var itemContainer = new ItemContainer();
                structContainer.AddItemContainer(funcDecl.Name, paramHash, itemContainer);

                structContainer.AddRuntimeItem(runtimeItem);

                var newDeclEvaluator = new DeclEvaluator(globalContext, itemContainer);
                foreach (var decl in funcDecl.Decls)
                    newDeclEvaluator.EvalDecl(decl);
            }

            void EvalStructDeclMemberSeqFuncDecl(ItemContainer structContainer, R.StructDecl.MemberDecl.SeqFunc seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, seqFuncDecl.Parameters);

                structContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var runtimeItem = new IR0StructSeqFuncRuntimeItem(globalContext, seqFuncDecl);
                structContainer.AddRuntimeItem(runtimeItem);

                var newDeclEvaluator = new DeclEvaluator(globalContext, itemContainer);
                foreach (var decl in seqFuncDecl.Decls)
                    newDeclEvaluator.EvalDecl(decl);
            }

            void EvalStructDeclMemberDecl(ItemContainer structContainer, R.StructDecl.MemberDecl memberDecl)
            {
                switch(memberDecl)
                {
                    case R.StructDecl.MemberDecl.Var varDecl:
                        EvalStructDeclMemberVarDecl(structContainer, varDecl);
                        break;

                    case R.StructDecl.MemberDecl.Constructor constructorDecl:
                        EvalStructDeclConstructorDecl(structContainer, constructorDecl);
                        break;

                    case R.StructDecl.MemberDecl.Func funcDecl:
                        EvalStructDeclMemberFuncDecl(structContainer, funcDecl);
                        break;

                    case R.StructDecl.MemberDecl.SeqFunc seqFuncDecl:
                        EvalStructDeclMemberSeqFuncDecl(structContainer, seqFuncDecl);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }

            void EvalStructDecl(R.StructDecl structDecl)
            {
                var structContainer = new ItemContainer();
                foreach (var memberDecl in structDecl.MemberDecls)
                {
                    EvalStructDeclMemberDecl(structContainer, memberDecl);
                }

                curContainer.AddItemContainer(structDecl.Name, new R.ParamHash(structDecl.TypeParams.Length, default), structContainer);
                curContainer.AddRuntimeItem(new IR0StructRuntimeItem(globalContext, structDecl));
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
