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
            Evaluator evaluator;
            ImmutableArray<R.Decl> decls;
            ItemContainer curContainer;

            public DeclEvaluator(Evaluator evaluator, ImmutableArray<R.Decl> decls)
            {
                this.evaluator = evaluator;
                this.decls = decls;
            }

            public void Eval()
            {
                foreach (var decl in decls)
                {
                    Eval(decl);
                }
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

                var typeParamCount = normalFuncDecl.TypeParams.Length;
                var paramTypes = ImmutableArray.CreateRange(normalFuncDecl.ParamInfos, paramInfo => paramInfo.Type);
                var paramHash = new R.ParamHash(typeParamCount, paramTypes);

                curContainer.AddItemContainer(normalFuncDecl.Name, paramHash, itemContainer);

                var savedContainer = curContainer;
                curContainer = itemContainer;

                foreach (var lambdaDecl in normalFuncDecl.LambdaDecls)
                    EvalLambdaDecl(lambdaDecl);

                curContainer = savedContainer;                
            }

            void EvalSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl)
            {
                var itemContainer = new ItemContainer();

                var typeParamCount = seqFuncDecl.TypeParams.Length;
                var paramTypes = ImmutableArray.CreateRange(seqFuncDecl.ParamInfos, paramInfo => paramInfo.Type);
                var paramHash = new R.ParamHash(typeParamCount, paramTypes);

                curContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var savedContainer = curContainer;
                curContainer = itemContainer;

                foreach (var lambdaDecl in seqFuncDecl.LambdaDecls)
                    EvalLambdaDecl(lambdaDecl);

                curContainer = savedContainer;
            }

            void EvalEnumDecl(R.EnumDecl enumDecl)
            {
                throw new NotImplementedException();
            }
        }
    }
}
