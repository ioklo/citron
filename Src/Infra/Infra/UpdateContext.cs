using System.Collections.Generic;

namespace Citron.Infra
{
    public struct UpdateContext
    {
        HashSet<object> updated;

        public UpdateContext()
        {
            this.updated = new HashSet<object>();
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