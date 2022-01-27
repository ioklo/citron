using System;
using System.Collections.Generic;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // ArgTypeValues => RetValueTypes
    [AutoConstructor]
    public partial class LambdaSymbol : ITypeSymbol, IFuncSymbol
    {
        SymbolFactory factory;
        IFuncSymbol outer; // IFuncSymbol | ITypeSymbol
        LambdaDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);        
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public LambdaSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeLambda(appliedOuter, decl);
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }        

        ITypeDeclSymbol ITypeSymbol.GetDeclSymbolNode()
        {
            return decl;
        }

        IFuncDeclSymbol IFuncSymbol.GetDeclSymbolNode()
        {
            return decl;
        }

        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode()
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

        public ITypeSymbol? GetOuterType()
        {
            return outer as ITypeSymbol;
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
