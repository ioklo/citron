using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record DeclSymbolId(M.Name ModuleName, DeclSymbolPath? Path);

    public static class DeclSymbolIdExtensions
    {
        public static DeclSymbolId GetDeclSymbolId(this ModuleSymbolId moduleSymbolId)
        {
            var declPath = moduleSymbolId.Path.GetDeclSymbolPath();
            return new DeclSymbolId(moduleSymbolId.ModuleName, declPath);            
        }

        public static DeclSymbolId Child(this DeclSymbolId declId, M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return new DeclSymbolId(declId.ModuleName, declId.Path.Child(name, typeParamCount, paramIds));
        }

        public static DeclSymbolId GetDeclSymbolId(this IDeclSymbolNode decl)
        {
            var outerDecl = decl.GetOuterDeclNode();
            if (outerDecl != null)
            {
                var outerDeclId = outerDecl.GetDeclSymbolId();
                var nodeName = decl.GetNodeName();
                return outerDeclId.Child(nodeName.Name, nodeName.TypeParamCount, nodeName.ParamIds);
            }
            else if (decl is ModuleDeclSymbol moduleDecl)
            {
                return new DeclSymbolId(moduleDecl.GetName(), null);
            }
            else
            {
                throw new UnreachableCodeException();
            }
        }
    }
}
