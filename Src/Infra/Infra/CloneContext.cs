using Citron.Infra;
using System;
using System.Collections.Generic;

namespace Citron.Infra
{
    public struct CloneContext
    {
        Dictionary<object, object> clonedInstances;

        public static CloneContext Make()
        {
            var result = new CloneContext();
            result.clonedInstances = new Dictionary<object, object>();
            return result;
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

        public static object Make(Dictionary<object, object> dictionaries)
        {
            throw new NotImplementedException();
        }
    }
}