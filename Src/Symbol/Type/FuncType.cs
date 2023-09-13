using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

// c#의 delegate처럼 함수 매개변수의 프로퍼티를 다 보존한다
[AutoConstructor]
public partial class FuncType : IInterfaceType, ICyclicEqualityComparableClass<FuncType>
{
    FuncReturn funcRet;
    ImmutableArray<FuncParameter> parameters;

    FuncType Apply(TypeEnv typeEnv)
    {
        var appliedReturn = funcRet.Apply(typeEnv);
        var appliedParametersBuilder = ImmutableArray.CreateBuilder<FuncParameter>(parameters.Length);
        foreach (var parameter in parameters)
        {
            var appliedParameter = parameter.Apply(typeEnv);
            appliedParametersBuilder.Add(appliedParameter);
        }

        return new FuncType(appliedReturn, appliedParametersBuilder.MoveToImmutable());
    }

    FuncTypeId GetTypeId()
    {
        var builder = ImmutableArray.CreateBuilder<FuncParameterId>(parameters.Length);
        foreach (var parameter in parameters)
            builder.Add(parameter.GetId());

        return new FuncTypeId(funcRet.GetId(), builder.MoveToImmutable());
    }
    
    bool CyclicEquals(FuncType other, ref CyclicEqualityCompareContext context)
    {
        if (!funcRet.CyclicEquals(ref other.funcRet, ref context))
            return false;

        if (!parameters.CyclicEqualsStructItem(ref parameters, ref context))
            return false;

        return true;
    }
    
    public IType GetReturnType()
    {
        return funcRet.Type;
    }

    IType IType.GetTypeArg(int index) => throw new RuntimeFatalException();
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => GetTypeId();
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitFunc(this);

    bool ICyclicEqualityComparableClass<IInterfaceType>.CyclicEquals(IInterfaceType other, ref CyclicEqualityCompareContext context)
        => other is FuncType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is FuncType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeValue(nameof(funcRet), funcRet);
        context.SerializeValueArray(nameof(parameters), parameters);
    }

    bool ICyclicEqualityComparableClass<FuncType>.CyclicEquals(FuncType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);
}
