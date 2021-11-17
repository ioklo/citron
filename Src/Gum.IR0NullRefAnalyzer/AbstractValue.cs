using Gum.IR0;
using System;
using System.Diagnostics;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    // (Id -> Loc) * (Loc -> Value)
    // Value = Location of Loc
    //       | Nullable
    // Loc -> Value를 instance로 구현해도 된다. AbstractValue가 Loc의 역할

    abstract class AbstractValue
    {
        public abstract void Set(AbstractValue other);
        public abstract void SetNotNull();
        public abstract void SetNull();
        public abstract void SetUnknown();

        public abstract bool IsNotNull();
        public abstract AbstractValue GetMemberValue(Path.Nested memberPath);
    }

    // ExpStmt에서 사용하는 Value, Assign등이 쓸일 없으므로 무효화된다
    class EmptyAbstractValue : AbstractValue
    {
        public readonly static EmptyAbstractValue Instance = new EmptyAbstractValue();
        EmptyAbstractValue() { }

        public override void Set(AbstractValue other)
        {
            // ignore, 여기 들어올 일이 있는가
            throw new InvalidOperationException();
        }

        public override void SetNotNull() 
        {
            throw new InvalidOperationException();
        }

        public override void SetNull() 
        {
            throw new InvalidOperationException();
        }

        public override void SetUnknown() 
        {
            throw new InvalidOperationException();
        }

        public override bool IsNotNull()
        {
            throw new InvalidOperationException();
        }

        public override AbstractValue GetMemberValue(Path.Nested memberPath)
        {
            throw new InvalidOperationException();
        }
    }

    // 일반 값    
    class NullableAbstractValue : AbstractValue
    {
        enum Kind { Unknown, Null, NotNull };

        bool bNullAllowed;
        Kind kind;

        public NullableAbstractValue(bool bNullAllowed)
        {
            this.bNullAllowed = bNullAllowed;
        }

        public override void Set(AbstractValue other)
        {
            // 타입체크를 통과했으므로 항상 참이어야 한다
            var nullableOther = other as NullableAbstractValue;
            Debug.Assert(nullableOther != null);
            Debug.Assert(bNullAllowed == nullableOther.bNullAllowed);

            this.kind = nullableOther.kind;
        }

        public override void SetUnknown()
        {
            if (!bNullAllowed)
                kind = Kind.NotNull;

            kind = Kind.Unknown;
        }

        public override void SetNull()
        {
            Debug.Assert(bNullAllowed);
            kind = Kind.Null;
        }

        public override void SetNotNull()
        {
            kind = Kind.NotNull;
        }


        public override bool IsNotNull()
        {
            throw new NotImplementedException();
        }

        public override AbstractValue GetMemberValue(Path.Nested memberPath)
        {
            throw new NotImplementedException();
        }
    }
}