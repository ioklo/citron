using Citron.Infra;
using Pretune;
using System.Diagnostics;

namespace Citron.Symbol;

partial struct CommonFuncDeclSymbolComponent : ICyclicEqualityComparableStruct<CommonFuncDeclSymbolComponent>, ISerializable
{
    enum InitState
    {
        BeforeInitLastParameterVariadic,
        AfterInitLastParameterVariadic
    }

    bool bLastParameterVariadic;
    InitState initState;

    public CommonFuncDeclSymbolComponent()
    {
        this.bLastParameterVariadic = false;
        this.initState = InitState.BeforeInitLastParameterVariadic;
    }

    public CommonFuncDeclSymbolComponent(bool bLastParameterVariadic)
    {
        this.bLastParameterVariadic = bLastParameterVariadic;
        this.initState = InitState.AfterInitLastParameterVariadic;
    }

    public void InitLastParameterVariadic(bool bVariadic)
    {
        Debug.Assert(InitState.BeforeInitLastParameterVariadic == initState);
        this.bLastParameterVariadic = bVariadic;
        this.initState = InitState.AfterInitLastParameterVariadic;
    }

    public bool IsLastParameterVariadic()
    {
        Debug.Assert(InitState.BeforeInitLastParameterVariadic < initState);
        return bLastParameterVariadic;
    }

    bool ICyclicEqualityComparableStruct<CommonFuncDeclSymbolComponent>.CyclicEquals(ref CommonFuncDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
    {
        if (bLastParameterVariadic != other.bLastParameterVariadic)
            return false;

        if (initState != other.initState)
            return false;

        return true;
    }

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeBool(nameof(bLastParameterVariadic), bLastParameterVariadic);
        context.SerializeString(nameof(initState), initState.ToString());
    }
}