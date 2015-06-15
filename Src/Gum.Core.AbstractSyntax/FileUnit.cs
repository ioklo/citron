using System.Collections.Generic;

namespace Gum.Core.AbstractSyntax
{
    public class FileUnit
    {
        public IReadOnlyList<IFileUnitDecl> Decls { get; private set; }

        public FileUnit(IEnumerable<IFileUnitDecl> decls)
        {
            Decls = new List<IFileUnitDecl>(decls);
        }
    }    
}