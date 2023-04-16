using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Infra
{
    public struct UniqueQueryResult<T>
    {
        enum State
        {
            NotFound,
            MultipleError,
            Found // value 활성
        }

        State state;
        T? value;
        ImmutableArray<T> multipleCandidatesErrorValue;

        UniqueQueryResult(State state, T? value, ImmutableArray<T> multipleCandidatesErrorValue)
        {
            this.state = state;
            this.value = value;
            this.multipleCandidatesErrorValue = multipleCandidatesErrorValue;
        }

        public bool IsNotFound() { return state == State.NotFound; }
        public bool IsMultipleError(out ImmutableArray<T> candidates) 
        {
            if (state == State.MultipleError)
            {
                candidates = this.multipleCandidatesErrorValue;
                return true;
            }
            else
            {
                candidates = default;
                return false;
            }
        }
        
        public bool IsFound([NotNullWhen(returnValue: true)] out T? value)
        {
            if (state != State.Found)
            {
                value = default;
                return false;
            }
            else
            {
                Debug.Assert(this.value != null);
                value = this.value;
                return true;
            }
        }

        // constructor
        public static UniqueQueryResult<T> NotFound() { return new UniqueQueryResult<T>(State.NotFound, default, default); }
        public static UniqueQueryResult<T> MultipleError(ImmutableArray<T> candidates) { return new UniqueQueryResult<T>(State.MultipleError, default, candidates); }
        public static UniqueQueryResult<T> Found(T t) { return new UniqueQueryResult<T>(UniqueQueryResult<T>.State.Found, t, default); }
    }
}
