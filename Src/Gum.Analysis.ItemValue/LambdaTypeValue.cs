using System;
using Gum.Collections;
using R = Gum.IR0;

namespace Gum.Analysis
{
    // ArgTypeValues => RetValueTypes
    public class LambdaTypeValue : TypeSymbol, IEquatable<LambdaTypeValue>
    {
        RItemFactory ritemFactory;
        public R.Path.Nested Lambda { get; } // Type의 path가 아니라 Lambda의 path
        public TypeSymbol Return { get; }
        public ImmutableArray<ParamInfo> Params { get; }

        public LambdaTypeValue(RItemFactory ritemFactory, R.Path.Nested lambda, TypeSymbol ret, ImmutableArray<ParamInfo> parameters)
        {
            this.ritemFactory = ritemFactory;
            this.Lambda = lambda;
            this.Return = ret;
            this.Params = parameters;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LambdaTypeValue);
        }

        public bool Equals(LambdaTypeValue? other)
        {
            if (other == null) return false;

            // 아이디만 비교한다. 같은 위치에서 다른 TypeContext에서 생성되는 람다는 id도 바뀌어야 한다
            return Lambda.Equals(other.Lambda);
        }

        // lambdatypevalue를 replace할 일이 있는가
        // void Func<T>()
        // {
        //     var l = (T t) => t; // { l => LambdaTypeValue({id}, T, [T]) }
        // }
        // 분석중에 Apply할 일은 없고, 실행중에 할 일은 있을 것 같다
        public override TypeSymbol Apply(TypeEnv typeEnv)
        {
            throw new InvalidOperationException();
        }

        public override R.Path GetRPath()
        {
            return Lambda;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lambda);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();

        public override int GetTotalTypeParamCount()
        {
            throw new NotImplementedException();
        }
    }
}
