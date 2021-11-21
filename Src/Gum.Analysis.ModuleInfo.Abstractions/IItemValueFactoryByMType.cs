using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IItemValueFactoryByMType
    {
        IClassTypeValue? MakeClassTypeValue(M.Type mtype);
        IStructTypeValue? MakeStructTypeValue(M.Type mtype);
    }
}