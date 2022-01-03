using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IQueryModuleTypeDecl
    {
        // 빌드가 완성된 class/struct가 나오도록 한다
        ClassSymbol GetClass(M.TypeId declId);
        StructSymbol GetStruct(M.TypeId declId);
    }
}