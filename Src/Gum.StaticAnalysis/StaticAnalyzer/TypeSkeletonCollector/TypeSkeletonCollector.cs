using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Gum.StaticAnalysis
{
    public partial class TypeSkeletonCollector
    {   
        bool CollectEnumDecl(EnumDecl enumDecl, Context context)
        {
            var enumElemNames = enumDecl.Elems.Select(elem => elem.Name);

            context.AddTypeSkeleton(enumDecl, enumDecl.Name, enumDecl.TypeParams.Length, enumElemNames);
            return true;
        }

        bool CollectFuncDecl(FuncDecl funcDecl, Context context)
        {
            var funcId = ModuleItemId.Make(funcDecl.Name, funcDecl.TypeParams.Length);
            context.AddFuncId(funcDecl, funcId);
            return true;
        }

        bool CollectScript(Script script, Context context)
        {
            foreach (var scriptElem in script.Elements)
            {
                switch(scriptElem)
                {
                    case EnumDeclScriptElement enumElem:
                        if (!CollectEnumDecl(enumElem.EnumDecl, context))
                            return false;
                        break;

                    case FuncDeclScriptElement funcElem:
                        if (!CollectFuncDecl(funcElem.FuncDecl, context))
                            return false;
                        break;
                }
            }

            return true;
        }

        public (SyntaxNodeModuleItemService SyntaxNodeModuleItemService, ImmutableArray<TypeSkeleton> TypeSkeletons)? 
            CollectScript(Script script, IErrorCollector errorCollector)
        {
            var context = new Context();

            if (!CollectScript(script, context))
            {
                errorCollector.Add(script, $"타입 정보 모으기에 실패했습니다");
                return null;
            }

            var syntaxNodeModuleItemService = new SyntaxNodeModuleItemService(
                context.GetTypeIdsByNode(), 
                context.GetFuncIdsByNode());

            return (syntaxNodeModuleItemService, context.GetTypeSkeletons());
        }
    }
}
