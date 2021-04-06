namespace Gum.Collections
{
    public partial struct ImmutableArray<T>
    {
        public struct Enumerator
        {
            bool bValid; // false
            System.Collections.Immutable.ImmutableArray<T>.Enumerator enumerator;

            public Enumerator(System.Collections.Immutable.ImmutableArray<T>.Enumerator enumerator)
            {
                bValid = true;
                this.enumerator = enumerator;
            }

            public bool MoveNext()
            {
                if (!bValid) return false;
                return enumerator.MoveNext();
            }

            public T Current => enumerator.Current;
        }
    }
}

