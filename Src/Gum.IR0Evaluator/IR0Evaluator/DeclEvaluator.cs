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
                    EvalDecl(decl);
                }

                // (R.ModuleName moduleName, ItemContainer container)
                evaluator.context.AddRootItemContainer(moduleName, curContainer);
            }

            void EvalLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                var item = new IR0LambdaRuntimeItem(lambdaDecl);
                curContainer.AddRuntimeItem(item);
            }

            void EvalNormalFuncDecl(R.NormalFuncDecl normalFuncDecl)
            {
                var itemContainer = new ItemContainer();
                
                // 하위 아이템을 저장할 container와 invoker를 추가한다 (같은 키로)
                var typeParamCount = normalFuncDecl.TypeParams.Length;
                var paramHash = Misc.MakeParamHash(typeParamCount, normalFuncDecl.ParamInfo);
                curContainer.AddItemContainer(normalFuncDecl.Name, paramHash, itemContainer);

                var funcRuntimeItem = new IR0FuncRuntimeItem(normalFuncDecl);
                curContainer.AddRuntimeItem(funcRuntimeItem);

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
                var paramHash = Misc.MakeParamHash(typeParamCount, seqFuncDecl.ParamInfo);

                curContainer.AddItemContainer(seqFuncDecl.Name, paramHash, itemContainer);

                var runtimeItem = new IR0SeqFuncRuntimeItem(seqFuncDecl);
                curContainer.AddRuntimeItem(runtimeItem);

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
                    var enumElemItem = new IR0EnumElemRuntimeItem(enumElem);
                    var enumElemContainer = new ItemContainer();

                    int fieldIndex = 0;
                    foreach(var enumElemField in enumElem.Params)
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
