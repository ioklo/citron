namespace Gum.Analysis
{
    public interface IQueryModuleTypeInfo
    {
        IModuleClassInfo GetClass(ItemPath classPath);
        IModuleStructInfo GetStruct(ItemPath structPath);
    }
}