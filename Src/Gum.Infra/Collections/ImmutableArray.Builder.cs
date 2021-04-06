﻿using System;
using System.Collections.Generic;

namespace Gum.Collections
{
    public partial struct ImmutableArray<T>
    {
        public struct Builder
        {
            System.Collections.Immutable.ImmutableArray<T>.Builder builder;

            public Builder(System.Collections.Immutable.ImmutableArray<T>.Builder builder)
            {
                this.builder = builder;
            }

            public void Add(T item)
            {
                builder.Add(item);
            }

            public int Count { get => builder.Count; }

            public ImmutableArray<T> ToImmutable()
            {
                return new ImmutableArray<T>(builder.ToImmutable());
            }

            public ImmutableArray<T> MoveToImmutable()
            {
                return new ImmutableArray<T>(builder.MoveToImmutable());
            }

            public void AddRange(IEnumerable<T> items)
            {
                builder.AddRange(items);
            }
        }
    }
}
