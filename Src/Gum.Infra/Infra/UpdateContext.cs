﻿using System.Collections.Generic;

namespace Gum.Infra
{
    public struct UpdateContext
    {
        HashSet<object> updated;

        public static UpdateContext Make()
        {
            var result = new UpdateContext();
            result.updated = new HashSet<object>();
            return result;
        }

        public void Update<T>(T dest, T src)
            where T : class, IMutable<T>
        {
            if (!updated.Contains(dest))
            {
                dest.Update(src, this);
                updated.Add(dest);
            }
        }
    }
}