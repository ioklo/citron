using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;
using static Gum.IR0.IR0Factory;
using Gum.Infra;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        struct StructDeclEvaluator
        {
            GlobalContext globalContext;
            ItemContainer structContainer;

            R.Path.Nested structPath;
            int totalTypeParamCount;            

            public static void Eval(GlobalContext globalContext, ItemContainer curContainer, R.Path.Normal curPath, int totalTypeParamCount, R.StructDecl structDecl)
            {   
                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, structDecl.TypeParams.Length);
                var structPath = curPath.Child(structDecl.Name, new R.ParamHash(structDecl.TypeParams.Length, default), typeArgs);
                var structContainer = new ItemContainer();

                var evaluator = new StructDeclEvaluator(globalContext, structContainer, structPath, totalTypeParamCount + structDecl.TypeParams.Length);
                
                foreach (var memberDecl in structDecl.MemberDecls)
                {
                    evaluator.EvalMemberDecl(memberDecl);
                }

                curContainer.AddItemContainer(structDecl.Name, new R.ParamHash(structDecl.TypeParams.Length, default), structContainer);
                curContainer.AddRuntimeItem(new IR0StructRuntimeItem(globalContext, structDecl));
            }

            StructDeclEvaluator(GlobalContext globalContext, ItemContainer structContainer, R.Path.Nested structPath, int totalTypeParamCount)
            {
                this.globalContext = globalContext;
                this.structContainer = structContainer;
                this.structPath = structPath;
                this.totalTypeParamCount = totalTypeParamCount;
            }


            void EvalMemberVarDecl(R.StructMemberVarDecl varDecl)
            {
                foreach (var name in varDecl.Names)
                {
                    var memberVarItem = new IR0StructMemberVarRuntimeItem(name);
                    structContainer.AddRuntimeItem(memberVarItem);
                }
            }

            void EvalConstructorDecl(R.StructConstructorDecl constructorDecl)
            {
                // NOTICE: Constructor는 typeParamCount가 0 이다
                var paramHash = Misc.MakeParamHash(typeParamCount: 0, constructorDecl.Parameters);
                var runtimeItem = new IR0ConstructorRuntimeItem(globalContext, R.Name.Constructor.Instance, paramHash, constructorDecl.Parameters, constructorDecl.Body);

                var itemContainer = new ItemContainer();
                structContainer.AddItemContainer(R.Name.Constructor.Instance, paramHash, itemContainer);
                structContainer.AddRuntimeItem(runtimeItem);

                var constructorPath = structPath.Child(R.Name.Constructor.Instance, paramHash, default);
                DeclEvaluator.EvalDecls(globalContext, itemContainer, constructorPath, totalTypeParamCount, constructorDecl.Decls);
            }

            void EvalMemberFuncDecl(R.StructMemberFuncDecl funcDecl)
            {
                var paramHash = Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
                var runtimeItem = new IR0StructFuncRuntimeItem(globalContext, funcDecl);

                var itemContainer = new ItemContainer();
                structContainer.AddItemContainer(funcDecl.Name, paramHash, itemContainer);

                structContainer.AddRuntimeItem(runtimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, funcDecl.TypeParams.Length);
                var funcPath = structPath.Child(funcDecl.Name, paramHash, typeArgs);
                DeclEvaluator.EvalDecls(globalContext, itemContainer, funcPath, totalTypeParamCount + funcDecl.TypeParams.Length, funcDecl.Decls);
            }

            void EvalMemberSeqFuncDecl(R.StructMemberSeqFuncDecl seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, seqFuncDecl.Parameters);

                structContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var runtimeItem = new IR0StructSeqFuncRuntimeItem(globalContext, seqFuncDecl);
                structContainer.AddRuntimeItem(runtimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, typeParamCount);
                var seqFuncPath = structPath.Child(seqFuncDecl.Name, paramHash, typeArgs);

                DeclEvaluator.EvalDecls(globalContext, itemContainer, seqFuncPath, totalTypeParamCount + typeParamCount, seqFuncDecl.Decls);
            }

            void EvalMemberDecl(R.StructMemberDecl memberDecl)
            {
                switch (memberDecl)
                {
                    case R.StructMemberVarDecl varDecl:
                        EvalMemberVarDecl(varDecl);
                        break;

                    case R.StructConstructorDecl constructorDecl:
                        EvalConstructorDecl(constructorDecl);
                        break;

                    case R.StructMemberFuncDecl funcDecl:
                        EvalMemberFuncDecl(funcDecl);
                        break;

                    case R.StructMemberSeqFuncDecl seqFuncDecl:
                        EvalMemberSeqFuncDecl(seqFuncDecl);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }

        }
    }
}
