﻿using Citron.Collections;
using Citron.Infra;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Formats;

namespace Citron.Symbol
{
    public interface ITypeIdVisitor<TResult>
    {
        TResult VisitTypeVar(TypeVarTypeId typeId);
        TResult VisitNullable(NullableTypeId typeId);
        TResult VisitVoid(VoidTypeId typeId);
        TResult VisitTuple(TupleTypeId typeId);
        TResult VisitFunc(FuncTypeId typeId);
        TResult VisitLambda(LambdaTypeId typeId);

        TResult VisitLocalPtr(LocalPtrTypeId typeId);
        TResult VisitBoxPtr(BoxPtrTypeId typeId);

        TResult VisitSymbol(SymbolTypeId typeId);
    }

    public abstract record class TypeId : ISerializable
    {
        public abstract void DoSerialize(ref SerializeContext context);
        public abstract TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor)
            where TTypeIdVisitor : ITypeIdVisitor<TResult>;
    }


    // MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
    // declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
    // public record class TypeVarSymbolId(int Index) : SymbolId;    
    // => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
    // => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
    // => TypeVarSymbolId(5)로 참조하게 한다
    public record class TypeVarTypeId(int Index, Name Name) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeInt(nameof(Index), Index);
            context.SerializeRef(nameof(Name), Name);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitTypeVar(this);
    }

    public record class NullableTypeId(TypeId InnerTypeId) : TypeId
    {   
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerTypeId), InnerTypeId);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitNullable(this);
    }

    public record class VoidTypeId : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitVoid(this);
    }

    public record struct TupleMemberVarId(TypeId TypeId, Name Name) : ISerializable
    {
        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(TypeId), TypeId);
            context.SerializeRef(nameof(Name), Name);
        }
    }

    public record class TupleTypeId(ImmutableArray<TupleMemberVarId> MemberVarIds) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeValueArray(nameof(MemberVarIds), MemberVarIds);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitTuple(this);
    }
    
    public record class FuncTypeId(FuncReturnId RetId, ImmutableArray<FuncParameterId> ParamIds) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeValue(nameof(RetId), RetId);
            context.SerializeValueArray(nameof(ParamIds), ParamIds);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitFunc(this);
    }

    public record class SymbolTypeId(SymbolId SymbolId) : TypeId
    {
        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitSymbol(this);

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(SymbolId), SymbolId);
        }
    }

    public record class LambdaTypeId : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitLambda(this);
    }

    public record class LocalPtrTypeId(TypeId InnerTypeId) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerTypeId), InnerTypeId);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitLocalPtr(this);
    }

    public record class BoxPtrTypeId(TypeId InnerTypeId) : TypeId
    {
        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerTypeId), InnerTypeId);
        }

        public override TResult Accept<TTypeIdVisitor, TResult>(ref TTypeIdVisitor visitor) => visitor.VisitBoxPtr(this);
    }

    public static class TypeIds
    {
        public readonly static VoidTypeId Void = new VoidTypeId();
        public readonly static TypeId Bool = new SymbolTypeId(SymbolIds.Bool);
        public readonly static TypeId Int = new SymbolTypeId(SymbolIds.Int);
        public readonly static TypeId String = new SymbolTypeId(SymbolIds.String);
    }
    
    public static class TypeIdExtensions
    {
        public static bool IsList(this TypeId typeId, [NotNullWhen(returnValue: true)] out TypeId? itemId)
        {
            var symbolId = (typeId as SymbolTypeId)?.SymbolId;
            if (symbolId == null)
            {
                itemId = null;
                return false;
            }

            var declSymbolId = symbolId.GetDeclSymbolId();
            if (declSymbolId.Equals(DeclSymbolIds.List))
            {
                Debug.Assert(symbolId.Path != null);

                itemId = symbolId.Path.TypeArgs[0];
                return true;
            }

            itemId = null;
            return false;
        }

        // ISeq 타입인지
        public static bool IsSeq(this TypeId typeId)
        {
            var symbolId = (typeId as SymbolTypeId)?.SymbolId;
            if (symbolId == null) return false;

            var declSymbolId = symbolId.GetDeclSymbolId();
            return declSymbolId.Equals(DeclSymbolIds.Seq);
        }
    }
}
