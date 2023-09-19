using Citron.Infra;
using System;
using System.Collections.Generic;

namespace Citron.Infra
{
    public struct CloneContext
    {
        Dictionary<object, object> clonedInstances;

        // default constructor
        public CloneContext()
        {
            this.clonedInstances = new Dictionary<object, object>();
        }

        public T GetClone<T>(T instance)
            where T : class, IMutable<T>
        {
            if (!clonedInstances.TryGetValue(instance, out var clonedInstance))
            {
                clonedInstance = instance.Clone(this);
                clonedInstances.Add(instance, clonedInstance);
            }

            return (T)clonedInstance;
        }
    }
}