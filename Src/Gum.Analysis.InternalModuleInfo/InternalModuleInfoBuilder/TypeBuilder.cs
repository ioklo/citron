using System.Collections.Generic;

namespace Gum.Analysis
{
    // syntax를 다 돌고 난 뒤의 후처리
    class TypeBuilder : IQueryModuleTypeInfo
    {
        Dictionary<ItemPath, InternalModuleClassInfo> classes;
        Dictionary<ItemPath, InternalModuleStructInfo> structs;

        public TypeBuilder()
        {
            classes = new Dictionary<ItemPath, InternalModuleClassInfo>();
            structs = new Dictionary<ItemPath, InternalModuleStructInfo>();
        }

        public void AddClass(ItemPath path, InternalModuleClassInfo classInfo)
        {
            classes.Add(path, classInfo);
        }

        public void AddStruct(ItemPath path, InternalModuleStructInfo structInfo)
        {
            structs.Add(path, structInfo);
        }

        IModuleClassInfo IQueryModuleTypeInfo.GetClass(ItemPath path, IItemValueFactoryByMType factory)
        {
            var cls = classes[path];
            cls.SetBaseAndBuildTrivialConstructor(this, factory);

            // TODO: external 처리
            return cls;
        }

        IModuleStructInfo IQueryModuleTypeInfo.GetStruct(ItemPath path, IItemValueFactoryByMType factory)
        {
            var s = structs[path];
            s.SetBaseAndBuildTrivialConstructor(this, factory);

            return s;
        }
        
        public void SetBasesAndBuildTrivialConstructors(IItemValueFactoryByMType factory)
        {
            foreach(var c in classes.Values)
                c.SetBaseAndBuildTrivialConstructor(this, factory);

            foreach (var s in structs.Values)
                s.SetBaseAndBuildTrivialConstructor(this, factory);
        }
    }

    
}