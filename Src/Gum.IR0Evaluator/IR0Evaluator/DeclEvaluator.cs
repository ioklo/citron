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
            ImmutableArray<R.Decl> decls;

            public SharedContext Eval()
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
                throw new NotImplementedException();
            }

            void EvalNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                throw new NotImplementedException();
            }

            void EvalSequenceFuncDecl(R.SequenceFuncDecl sequenceFuncDecl)
            {
                throw new NotImplementedException();
            }

            void EvalEnumDecl(R.EnumDecl enumDecl)
            {
                throw new NotImplementedException();
            }
        }
    }
}
