using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.Misc
{
    public class ScopedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        List<Dictionary<TKey, TValue>> stack;

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public ScopedDictionary()
        {
            stack = new List<Dictionary<TKey, TValue>>() { new Dictionary<TKey, TValue>() };
        }

        public void Push()
        {
            stack.Add(new Dictionary<TKey, TValue>());
        }

        public void Pop()
        {
            stack.RemoveAt(stack.Count - 1);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue outValue)
        {
            for (int i = stack.Count - 1; 0 <= i; i--)
                if (stack[i].TryGetValue(key, out var value))
                {
                    outValue = value;
                    return true;
                }

            outValue = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            stack[stack.Count - 1].Add(key, value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var names = new HashSet<TKey>();

            for(int i = stack.Count - 1; 0 <= i; i--)
            {
                foreach(var keyValuePair in stack[i])
                {
                    if(!names.Contains(keyValuePair.Key))
                    {
                        names.Add(keyValuePair.Key);
                        yield return keyValuePair;
                    }
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            for (int i = stack.Count - 1; 0 <= i; i--)
                if (stack[i].ContainsKey(key))
                    return true;

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKeyOutmostScope(TKey name)
        {
            return stack[stack.Count - 1].ContainsKey(name);
        }
    }
}
