using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using static Gum.IR0.AnalyzeErrorCode;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class TypeSkeletonCollector
    {   
        bool CollectEnumDecl(S.EnumDecl enumDecl, Context context)
        {
            var enumElemNames = enumDecl.Elems.Select(elem => elem.Name);

            context.AddTypeSkeleton(enumDecl, enumDecl.Name, enumDecl.TypeParams.Length, enumElemNames);
            return true;
        }

        bool CollectStructDecl(S.StructDecl structDecl, Context context)
        {
            var typeSkeleton = context.AddTypeSkeleton(structDecl, structDecl.Name, structDecl.TypeParams.Length, Array.Empty<string>());

            context.ExecInNewTypeScope(typeSkeleton, () => {

                foreach(var elem in structDecl.Elems)
                {
                    switch( elem)
                    {
                        case S.StructDecl.TypeDeclElement typeElem:
                            CollectTypeDecl(typeElem.TypeDecl, context);
                            break;

                        case S.StructDecl.FuncDeclElement funcElem:
                            context.AddFunc(funcElem, funcElem.Name, funcElem.TypeParams.Length);
                            break;
                    }
                }

            });

            return true;
        }

        bool CollectFuncDecl(S.FuncDecl funcDecl, Context context)
        {
            context.AddFunc(funcDecl, funcDecl.Name, funcDecl.TypeParams.Length);
            return true;
        }

        bool CollectTypeDecl(S.TypeDecl typeDecl, Context context)
        {
            switch(typeDecl)
            {
                case S.EnumDecl enumDecl:
                    return CollectEnumDecl(enumDecl, context);

                case S.StructDecl structDecl:
                    return CollectStructDecl(structDecl, context);

                default:
                    throw new InvalidOperationException();
            }
        }

        bool CollectScript(S.Script script, Context context)
        {
            foreach (var scriptElem in script.Elements)
            {
                switch(scriptElem)
                {
                    case S.Script.TypeDeclElement typeDeclElem:                        
                        if (!CollectTypeDecl(typeDeclElem.TypeDecl, context))
                            return false;
                        break;

                    case S.Script.FuncDeclElement funcElem:
                        if (!CollectFuncDecl(funcElem.FuncDecl, context))
                            return false;
                        break;
                }
            }

            return true;
        }

        public (SyntaxNodeModuleItemService SyntaxNodeModuleItemService, ImmutableArray<TypeSkeleton> TypeSkeletons)? 
            CollectScript(S.Script script, IErrorCollector errorCollector)
        {
            var context = new Context();

            if (!CollectScript(script, context))
            {
                errorCollector.Add(new AnalyzeError(S0101_Failed, script, $"타입 정보 모으기에 실패했습니다"));
                return null;
            }

            var syntaxNodeModuleItemService = new SyntaxNodeModuleItemService(
                context.GetTypePathsByNode(), 
                context.GetFuncPathsByNode());

            return (syntaxNodeModuleItemService, context.GetTypeSkeletons());
        }
    }
}
