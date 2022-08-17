using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Module
{
    public abstract record TypeDeclId;

    public record RootTypeDeclId(Name Module, NamespacePath? Namespace, TypeName Name) : TypeDeclId;
    public record MemberTypeDeclId(TypeDeclId Outer, TypeName Name) : TypeDeclId;
}
