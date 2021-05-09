using System.Collections.Generic;

namespace Gum.Infra
{
    public struct UpdateContext
    {
        HashSet<object> updated;

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