using Gum.CompileTime;
using Gum.Syntax;
using System.Collections.Immutable;

namespace Gum.StaticAnalysis
{
    public partial class ModuleInfoBuilder
    {
        public class Result
        {
            public ScriptModuleInfo ModuleInfo { get; }
            public TypeExpTypeValueService TypeExpTypeValueService { get; }
            public ImmutableDictionary<FuncDecl, FuncInfo> FuncInfosByDecl { get; }
            public ImmutableDictionary<EnumDecl, EnumInfo> EnumInfosByDecl{ get; }

            public Result(
                ScriptModuleInfo moduleInfo,
                TypeExpTypeValueService typeExpTypeValueService,
                ImmutableDictionary<FuncDecl, FuncInfo> funcInfosbyDecl,
                ImmutableDictionary<EnumDecl, EnumInfo> enumInfosByDecl)
            {
                ModuleInfo = moduleInfo;
                TypeExpTypeValueService = typeExpTypeValueService;
                FuncInfosByDecl = funcInfosbyDecl;
                EnumInfosByDecl = enumInfosByDecl;
            }
        }
    }
}