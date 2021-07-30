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
            R.Path.Normal GetPath();
            NormalTypeValue? GetThisTypeValue();

            LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            TypeValue? GetRetTypeValue();
            void SetRetTypeValue(TypeValue retTypeValue);
            void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType);
            bool IsSeqFunc();

            void AddDecl(R.Decl decl);
            ImmutableArray<R.Decl> GetDecls();

            R.Name.Anonymous NewAnonymousName();
        }

        struct AnonymousIdComponent
        {
            int anonymousCount;

            public R.Name.Anonymous NewAnonymousName()
            {
                var anonymousId = new R.AnonymousId(anonymousCount);
                anonymousCount++;

                return new R.Name.Anonymous(anonymousId);
            }
        }
    }
}