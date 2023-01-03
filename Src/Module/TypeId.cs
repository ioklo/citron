using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Module
{
    public abstract record class TypeDeclId;

    public record class RootTypeDeclId(Name Module, NamespacePath? Namespace, TypeName Name) : TypeDeclId;
    public record class MemberTypeDeclId(TypeDeclId Outer, TypeName Name) : TypeDeclId;
}
