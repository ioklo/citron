using Gum.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Analysis;

namespace Gum.Analysis
{
    // 모두 immutable
    public abstract partial class TypeValue : ItemValue
    {   
        public virtual ItemQueryResult GetMember(M.Name memberName, int typeParamCount) { return ItemQueryResult.NotFound.Instance; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }
        public abstract TypeValue Apply_TypeValue(TypeEnv typeEnv);        

        public sealed override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            return Apply_TypeValue(typeEnv);
        }

        public abstract R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member);
    }
}
