using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Infra
{
    public abstract partial record UniqueQueryResult<T>
    {
        // switch에서는 이렇게 쓰면 되고
        public record NotFound : UniqueQueryResult<T>;
        public record MultipleError : UniqueQueryResult<T>;
        public record Found(T Value) : UniqueQueryResult<T>;
    }
    
    // values, constructors
    public static class UniqueQueryResults<T>
    {
        public static UniqueQueryResult<T>.NotFound NotFound = new UniqueQueryResult<T>.NotFound();
        public static UniqueQueryResult<T>.MultipleError MultipleError = new UniqueQueryResult<T>.MultipleError();
        public static UniqueQueryResult<T>.Found Found(T t) => new UniqueQueryResult<T>.Found(t);
    }

}
