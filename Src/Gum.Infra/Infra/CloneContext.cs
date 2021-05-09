using Gum.Infra;
using System;
using System.Collections.Generic;

namespace Gum.Infra
{
    public struct CloneContext
    {
        Dictionary<object, object> clonedInstances;

        public CloneContext(Dictionary<object, object> clonedObjects)
        {
            this.clonedInstances = clonedObjects;
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