using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // 리턴 가능한 블럭의 정보
        interface ICallableContext : IMutable<ICallableContext>
        {
            ITypeSymbol? GetThisType();

            LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            FuncReturn? GetReturn(); // no return일 경우 null
            void SetRetType(ITypeSymbol retTypeValue);
            LambdaMemberVarSymbol AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType);
            bool IsSeqFunc();

            void AddCallableMemberDecl(R.CallableMemberDecl lambdaDecl);
            ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls();

            M.Name.Anonymous NewAnonymousName();
            IFuncSymbol GetThisNode();
            void AddLambdaDecl(LambdaDeclSymbol lambdaDecl);
        }

        struct AnonymousIdComponent
        {
            int anonymousCount;

            public R.Name.Anonymous NewAnonymousName()
            {
                anonymousCount++;

                return new R.Name.Anonymous(anonymousCount);
            }
        }
    }
}