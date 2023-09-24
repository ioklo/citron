using Citron.Syntax;
using Citron.Symbol;

using R = Citron.IR0;
using Citron.Collections;
using System;

namespace Citron.Analysis;

partial class ScopeContext
{
    // 용어 정리
    // Func => (모듈, 클래스멤버함수, 구조체멤버함수, 글로벌함수, 람다)
    // OutermostFunc => (모듈, 클래스멤버함수, 구조체멤버함수, 글로벌 함수)
    // This는 OutermostFunc가 선언된 타입의 인스턴스 (클래스, 구조체)
    public IFuncDeclSymbol GetFuncDeclSymbol() => bodyContext.GetFuncDeclSymbol();
    internal IFuncDeclSymbol GetOutermostFuncDeclSymbol() => bodyContext.GetOutermostFuncDeclSymbol();

    public bool CanAccess(ISymbolNode node) => bodyContext.CanAccess(node);    
    public bool IsSeqFunc() => bodyContext.IsSeqFunc();

    public bool IsSetReturn() => bodyContext.IsSetReturn();
    public FuncReturn? GetReturn() => bodyContext.GetReturn();
    public void SetReturn(IType retType) => bodyContext.SetReturn(retType);
    public ImmutableArray<R.Argument> MakeLambdaArgs() => bodyContext.MakeLambdaArgs();

    public IType MakeType(TypeExp typeExp) => bodyContext.MakeType(typeExp);

    public Name NewAnonymousName()
    {
        throw new NotImplementedException();
    }
} 