using System.Collections.Generic;
using System.Linq;

namespace Gum.Misc
{
    public class MultiDict<TKey, TValue>
    {
        Dictionary<TKey, List<TValue>> data;

        public MultiDict()
        {
            data = new Dictionary<TKey, List<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            if (!data.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                data.Add(key, list);
            }

            list.Add(value);
        }

        public IEnumerable<TValue> GetValues(TKey key)
        {
            if (data.TryGetValue(key, out var list))
                return list;

            return Enumerable.Empty<TValue>();
        }
    }
}