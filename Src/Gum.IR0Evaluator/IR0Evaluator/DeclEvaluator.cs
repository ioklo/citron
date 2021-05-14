using Gum.Collections;
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
        [AutoConstructor]
        partial class DeclEvaluator
        {
            R.ModuleName moduleName;

            Evaluator evaluator;
            ImmutableArray<R.Decl> decls;
            ItemContainer curContainer;

            public DeclEvaluator(R.ModuleName moduleName, Evaluator evaluator, ImmutableArray<R.Decl> decls)
            {
                this.moduleName = moduleName;

                this.evaluator = evaluator;
                this.decls = decls;

                this.curContainer = new ItemContainer();
            }

            public void Eval()
            {
                // 이 모듈의 이름을 알아야 한다

                foreach (var decl in decls)
                {
                    Eval(decl);
                }

                // (R.ModuleName moduleName, ItemContainer container)
                evaluator.context.AddRootItemContainer(moduleName, curContainer);
            }

            void Eval(R.Decl decl)
            {
                switch (decl)
                {
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

                }
            }

            void EvalLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                curContainer.AddLambdaDecl(lambdaDecl);
            }

            void EvalNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                var itemContainer = new ItemContainer();
                
                // 하위 아이템을 저장할 container와 invoker를 추가한다 (같은 키로)
                var typeParamCount = normalFuncDecl.TypeParams.Length;
                var paramTypes = ImmutableArray.CreateRange(normalFuncDecl.ParamInfo.Parameters, paramInfo => paramInfo.Type);
                var paramHash = new R.ParamHash(typeParamCount, paramTypes);
                curContainer.AddItemContainer(normalFuncDecl.Name, paramHash, itemContainer);

                var funcInvoker = new IR0FuncInvoker(evaluator, normalFuncDecl.Body, normalFuncDecl.ParamInfo);
                curContainer.AddFuncInvoker(normalFuncDecl.Name, paramHash, funcInvoker);

                var savedContainer = curContainer;
                curContainer = itemContainer;

                foreach (var decl in normalFuncDecl.Decls)
                    EvalDecl(decl);

                curContainer = savedContainer;                
            }

            void EvalSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramTypes = ImmutableArray.CreateRange(seqFuncDecl.ParamInfo.Parameters, paramInfo => paramInfo.Type);
                var paramHash = new R.ParamHash(typeParamCount, paramTypes);

                curContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                curContainer.AddSequenceFuncDecl(seqFuncDecl);

                var savedContainer = curContainer;
                curContainer = itemContainer;

                foreach (var decl in seqFuncDecl.Decls)
                {
                    EvalDecl(decl);
                }

                curContainer = savedContainer;
            }

            void EvalDecl(R.Decl decl)
            {
                switch(decl)
                {
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
                }
            }

            void EvalEnumDecl(R.EnumDecl enumDecl)
            {
                throw new NotImplementedException();
            }
        }
    }
}
