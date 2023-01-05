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

        UniqueQueryResult(State state, T? value)
        {
            this.state = state;
            this.value = value;
        }

        public bool IsNotFound() { return state == State.NotFound; }
        public bool IsMultipleError() { return state == State.MultipleError; }
        
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
        public static UniqueQueryResult<T> NotFound() { return new UniqueQueryResult<T>(State.NotFound, default); }
        public static UniqueQueryResult<T> MultipleError() { return new UniqueQueryResult<T>(State.MultipleError, default); }
        public static UniqueQueryResult<T> Found(T t) { return new UniqueQueryResult<T>(UniqueQueryResult<T>.State.Found, t); }
    }
}
