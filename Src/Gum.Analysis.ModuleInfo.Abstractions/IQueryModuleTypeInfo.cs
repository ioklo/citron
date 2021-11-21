namespace Gum.Analysis
{
    public interface IQueryModuleTypeInfo
    {
        // 빌드가 완성된 class/struct가 나오도록 한다
        IModuleClassInfo GetClass(ItemPath classPath, IItemValueFactoryByMType factory);
        IModuleStructInfo GetStruct(ItemPath structPath, IItemValueFactoryByMType factory);
    }
}