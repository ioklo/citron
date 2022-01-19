using System;
using System.Collections.Generic;
using Gum.Collections;
using Gum.Infra;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class LambdaMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<LambdaDeclSymbol> outerHolder;
        ITypeSymbol type;
        M.Name name;

        public M.Name GetName()
        {
            return name;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambdaMemberVar(this);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }
    }

    public class LambdaMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        LambdaSymbol outer;
        LambdaMemberVarDeclSymbol decl;

        public ISymbolNode Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambdaMemberVar(appliedOuter, decl);
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }
    }

    // ITypeSymbol과 IFuncSymbol의 성격을 동시에 가지는데, 그렇다면 ITypeSymbol이 더 일반적이다(함수적 성격은 멤버함수라고 생각하면 된다)
    public class LambdaDeclSymbol : ITypeDeclSymbol
    {
        IFuncDeclSymbol outer;

        int anonymousId;

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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
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
            return new DeclSymbolNodeName(new M.Name.Anonymous(anonymousId), 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public ImmutableArray<LambdaMemberVarDeclSymbol> GetMemberVars()
        {
            return memberVars;
        }
    }

    // ArgTypeValues => RetValueTypes
    public class LambdaSymbol : ITypeSymbol // , IEquatable<LambdaSymbol>
    {
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        SymbolFactory factory;
        IFuncSymbol outer;
        LambdaDeclSymbol decl;

        public LambdaSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambda(appliedOuter, decl);
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
        {
            foreach (var memberVar in decl.GetMemberVars())
            {
                if (memberVar.GetName().Equals(memberName))
                {
                    if (typeParamCount != 0)
                        return SymbolQueryResult.Error.VarWithTypeArg.Instance;

                    var memberVarSymbol = factory.MakeLambdaMemberVar(this, memberVar);
                    return new SymbolQueryResult.LambdaMemberVar(memberVarSymbol);
                }
            }

            return SymbolQueryResult.Error.NotFound.Instance;
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
