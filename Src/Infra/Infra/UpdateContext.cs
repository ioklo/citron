using System.Collections.Generic;

namespace Citron.Infra
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
                updated.Add(dest);
                dest.Update(src, this);
            }
        }
    }
}