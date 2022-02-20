using System;
using Citron.Collections;
using Citron.Infra;
using Pretune;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // ITypeSymbol과 IFuncSymbol의 성격을 동시에 가지는데, 그렇다면 ITypeSymbol이 더 일반적이다(함수적 성격은 멤버함수라고 생각하면 된다)
    [AutoConstructor]
    public partial class LambdaDeclSymbol : ITypeDeclSymbol, IFuncDeclSymbol
    {
        IHolder<IDeclSymbolNode> outer; // 대체로 FuncDeclSymbol이지만, TopLevel의 경우 module이나 namespace
        M.Name name;

        // Invoke 함수 시그니처
        FuncReturn @return;
        ImmutableArray<FuncParameter> parameters;

        // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
        ImmutableArray<LambdaMemberVarDeclSymbol> memberVars;

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            if (typeParamCount != 0 || !paramIds.IsEmpty)
                return null;

            foreach (var memberVar in memberVars)
                if (name.Equals(memberVar.GetName()))
                    return memberVar;

            return null;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
        }

        public LambdaMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public;
        }

        public FuncReturn GetReturn()
        {
            return @return;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }
    }
}
