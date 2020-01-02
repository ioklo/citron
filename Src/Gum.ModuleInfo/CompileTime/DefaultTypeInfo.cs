using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    // Skeleton, StaticVariable은 QsTypeInst에서 얻을 수 있게 된다
    public abstract class DefaultTypeInfo : ITypeInfo
    {
        public ModuleItemId? OuterTypeId { get; }
        public ModuleItemId TypeId { get; }

        ImmutableArray<string> typeParams;
        TypeValue? baseTypeValue;
        ImmutableDictionary<Name, ModuleItemId> memberTypeIds;
        ImmutableDictionary<Name, ModuleItemId> memberFuncIds;
        ImmutableDictionary<Name, ModuleItemId> memberVarIds;        

        // 거의 모든 TypeValue에서 thisTypeValue를 쓰기 때문에 lazy하게 선언해야 한다
        public DefaultTypeInfo(
            ModuleItemId? outerTypeId,
            ModuleItemId typeId, 
            IEnumerable<string> typeParams,
            TypeValue? baseTypeValue,
            IEnumerable<ModuleItemId> memberTypeIds,
            IEnumerable<ModuleItemId> memberFuncIds,
            IEnumerable<ModuleItemId> memberVarIds)
        {
            OuterTypeId = outerTypeId;
            TypeId = typeId;

            this.typeParams = typeParams.ToImmutableArray();
            this.baseTypeValue = baseTypeValue;
            this.memberTypeIds = memberTypeIds.ToImmutableDictionary(memberTypeId => memberTypeId.Name);
            this.memberFuncIds = memberFuncIds.ToImmutableDictionary(memberFuncId => memberFuncId.Name);
            this.memberVarIds = memberVarIds.ToImmutableDictionary(memberVarId => memberVarId.Name);
        }
        
        public IReadOnlyList<string> GetTypeParams()
        {
            return typeParams;
        }

        public TypeValue? GetBaseTypeValue()
        {
            return baseTypeValue;
        }

        public bool GetMemberTypeId(string name, [NotNullWhen(returnValue: true)] out ModuleItemId? outTypeId)
        {
            if (memberTypeIds.TryGetValue(Name.MakeText(name), out var typeId))
            {
                outTypeId = typeId;
                return true;
            }
            else
            {
                outTypeId = null;
                return false;
            }
        }

        public bool GetMemberFuncId(Name memberFuncName, [NotNullWhen(returnValue: true)] out ModuleItemId? outFuncId)
        {
            // TODO: 같은 이름 체크?
            if (memberFuncIds.TryGetValue(memberFuncName, out var funcId))
            {
                outFuncId = funcId;
                return true;
            }

            outFuncId = null;
            return false;
        }

        public bool GetMemberVarId(Name varName, [NotNullWhen(returnValue: true)] out ModuleItemId? outVarId)
        {
            // TODO: 같은 이름 체크
            if (memberVarIds.TryGetValue(varName, out var varId))
            {
                outVarId = varId;
                return true;
            }

            outVarId = null;
            return false;
        }
    }    

    // 'Func' 객체에 대한 TypeValue가 아니라 호출가능한 값의 타입이다
    // void Func(int x) : int => void
    // 
    // class X<T>
    //     List<T> Func(); -> (void => (null, List<>, [T]))
    //     List<T> Func<U>(U u);     (U => (null, List<>, [T]))
    // Runtime 'Func'에 대한 내용이 아니라, 호출이 가능한 함수에 대한 내용이다 (lambda일수도 있고)
    //public class QsFuncType : QsType
    //{
    //    public bool bThisCall { get; } // thiscall이라면 첫번째 ArgType은 this type이다
    //    public ImmutableArray<string> TypeParams { get; }
    //    public QsTypeValue RetType { get; }
    //    public ImmutableArray<QsTypeValue> ArgTypes { get; }

    //    public QsFuncType(QsMetaItemId typeId, bool bThisCall, ImmutableArray<string> typeParams, QsTypeValue retType, ImmutableArray<QsTypeValue> argTypes)
    //        : base(typeId)
    //    {
    //        this.bThisCall = bThisCall;
    //        TypeParams = typeParams;
    //        RetType = retType;
    //        ArgTypes = argTypes;
    //    }

    //    public QsFuncType(QsMetaItemId typeId, bool bThisCall, ImmutableArray<string> typeParams, QsTypeValue retType, params QsTypeValue[] argTypes)
    //        : base(typeId)
    //    {
    //        this.bThisCall = bThisCall;
    //        TypeParams = typeParams;
    //        RetType = retType;
    //        ArgTypes = ImmutableArray.Create(argTypes);
    //    }

    //    public override ImmutableArray<string> GetTypeParams() => ImmutableArray<string>.Empty;
    //    public override QsTypeValue? GetBaseTypeValue()
    //    {
    //        // TODO: Runtime 'Func<>' 이어야 한다
    //        throw new NotImplementedException();
    //    }

    //    public override QsType? GetMemberType(string name) => null;
    //    public override QsFuncType? GetMemberFuncType(QsMemberFuncId memberFuncId) => null;
    //    public override QsTypeValue? GetMemberVarTypeValue(string name) => null;

    //}

}

