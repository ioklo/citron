using System;
using System.Collections.Generic;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    // ArgTypeValues => RetValueTypes
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaSymbol : ITypeSymbol, IFuncSymbol, ICyclicEqualityComparableClass<LambdaSymbol>
    {
        SymbolFactory factory;
        IFuncSymbol outer;
        LambdaDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeDeclSymbol ITypeSymbol.GetDeclSymbolNode() => decl;
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;

        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IFuncDeclSymbol IFuncSymbol.GetDeclSymbolNode() => decl;

        public IDeclSymbolNode? GetDeclSymbolNode()
        {
            return decl;
        }

        public LambdaSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambda(appliedOuter, decl);
        }
        
        public ISymbolNode GetOuter()
        {
            return outer;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public LambdaMemberVarSymbol GetMemberVar(int index)
        {
            var memberVarDecl = decl.GetMemberVar(index);
            return factory.MakeLambdaMemberVar(this, memberVarDecl);
        }
        
        public FuncReturn GetReturn()
        {
            return decl.GetReturn();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(outer.GetTypeEnv());
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        SymbolQueryResult ISymbolNode.QueryMember(Name memberName, int typeParamCount)
        {
            int memberVarCount = decl.GetMemberVarCount();

            for(int i = 0; i < memberVarCount; i++)
            {
                var memberVar = decl.GetMemberVar(i);

                if (memberVar.GetName().Equals(memberName))
                {
                    if (typeParamCount != 0)
                        return SymbolQueryResults.Error.VarWithTypeArg;

                    var memberVarSymbol = factory.MakeLambdaMemberVar(this, memberVar);
                    return new SymbolQueryResult.LambdaMemberVar(memberVarSymbol);
                }
            }

            return SymbolQueryResults.NotFound;
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer as ITypeSymbol;
        }

        IType ITypeSymbol.MakeType()
        {
            return new LambdaType(this);
        }

        IType? ITypeSymbol.GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            return null;
        }

        IType ISymbolNode.GetTypeArg(int i)
        {
            throw new RuntimeFatalException();
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is LambdaSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is LambdaSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
        => other is LambdaSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<LambdaSymbol>.CyclicEquals(LambdaSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(LambdaSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(decl), decl);
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitLambda(this);
        }

        public LambdaDeclSymbol GetDeclSymbol()
        {
            return decl;
        }


        //public LambdaSymbol(RItemFactory ritemFactory, R.Path.Nested lambda, ITypeSymbol ret, ImmutableArray<ParamInfo> parameters)
        //{
        //    this.ritemFactory = ritemFactory;
        //    this.Lambda = lambda;
        //    this.Return = ret;
        //    this.Params = parameters;
        //}

        //public override bool Equals(object? obj)
        //{
        //    return Equals(obj as LambdaSymbol);
        //}

        //public bool Equals(LambdaSymbol? other)
        //{
        //    if (other == null) return false;

        //    // 아이디만 비교한다. 같은 위치에서 다른 TypeContext에서 생성되는 람다는 id도 바뀌어야 한다
        //    return Lambda.Equals(other.Lambda);
        //}

        //// lambdatypevalue를 replace할 일이 있는가
        //// void Func<T>()
        //// {
        ////     var l = (T t) => t; // { l => LambdaTypeValue({id}, T, [T]) }
        //// }
        //// 분석중에 Apply할 일은 없고, 실행중에 할 일은 있을 것 같다
        //public override ITypeSymbol Apply(TypeEnv typeEnv)
        //{
        //    throw new InvalidOperationException();
        //}

        //public override R.Path GetRPath()
        //{
        //    return Lambda;
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(Lambda);
        //}

        //public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        //    => throw new InvalidOperationException();

        //public override int GetTotalTypeParamCount()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
