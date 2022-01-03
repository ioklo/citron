using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    // Module을 뺀 
    public abstract record TypeDeclPath(TypeName Name);

    public record RootTypeDeclPath(NamespacePath? Namespace, TypeName Name) : TypeDeclPath(Name);
    public record MemberTypeDeclPath(TypeDeclPath Outer, TypeName Name) : TypeDeclPath(Name);
}
