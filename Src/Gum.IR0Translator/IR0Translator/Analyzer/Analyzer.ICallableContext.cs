using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // 리턴 가능한 블럭의 정보
        interface ICallableContext : IMutable<ICallableContext>
        {
            R.Path.Normal MakeRPath();
            ITypeSymbol? GetThisType();

            LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            FuncReturn? GetReturn(); // no return일 경우 null
            void SetRetType(ITypeSymbol retTypeValue);
            LambdaMemberVarSymbol AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType);
            bool IsSeqFunc();

            void AddCallableMemberDecl(R.CallableMemberDecl lambdaDecl);
            ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls();

            R.Name.Anonymous NewAnonymousName();
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