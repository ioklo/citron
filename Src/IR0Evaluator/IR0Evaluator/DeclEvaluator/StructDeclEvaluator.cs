using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Infra;

using R = Citron.IR0;
using static Citron.IR0.PathExtensions;

namespace Citron.IR0Evaluator
{
        struct StructDeclEvaluator
        {
            IR0GlobalContext globalContext;
            ItemContainer structContainer;

            R.Path.Nested structPath;
            int totalTypeParamCount;            

            public static void Eval(IR0GlobalContext globalContext, ItemContainer curContainer, R.Path.Normal curPath, int totalTypeParamCount, R.StructDecl structDecl)
            {   
                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, structDecl.TypeParams.Length);
                var structPath = curPath.Child(new R.Name.Normal(structDecl.Name), new R.ParamHash(structDecl.TypeParams.Length, default), typeArgs);
                var structContainer = new ItemContainer();

                var evaluator = new StructDeclEvaluator(globalContext, structContainer, structPath, totalTypeParamCount + structDecl.TypeParams.Length);

                foreach (var constructorDecl in structDecl.ConstructorDecls)
                    evaluator.EvalConstructorDecl(constructorDecl);

                foreach (var memberFuncDecl in structDecl.MemberFuncDecls)
                    evaluator.EvalMemberFuncDecl(memberFuncDecl);

                foreach (var memberVarDecl in structDecl.MemberVarDecls)
                    evaluator.EvalMemberVarDecl(memberVarDecl);

                curContainer.AddItemContainer(new R.Name.Normal(structDecl.Name), new R.ParamHash(structDecl.TypeParams.Length, default), structContainer);
                curContainer.AddRuntimeItem(new IR0StructRuntimeItem(globalContext, structDecl));
            }

            StructDeclEvaluator(IR0GlobalContext globalContext, ItemContainer structContainer, R.Path.Nested structPath, int totalTypeParamCount)
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
                DeclEvaluator.EvalCallableMemberDecls(globalContext, itemContainer, constructorPath, totalTypeParamCount, constructorDecl.CallableMemberDecls);
            }

            void EvalMemberFuncDecl(R.FuncDecl memberFuncDecl)
            {                
                DeclEvaluator.EvalFuncDecl(globalContext, structContainer, structPath, totalTypeParamCount, memberFuncDecl);
            }
        }

}
