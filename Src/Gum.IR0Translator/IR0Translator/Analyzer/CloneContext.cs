using System;
using System.Collections.Generic;

namespace Gum.IR0Translator
{
    interface ICloneable<T> where T : class
    {
        T Clone(CloneContext context);
    }

    struct CloneContext
    {
        Dictionary<object, object> clonedInstances;
        public CloneContext(Dictionary<object, object> clonedObjects)
        {
            this.clonedInstances = clonedObjects;
        }

        public T GetClone<T>(T instance)
            where T : class, ICloneable<T>
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