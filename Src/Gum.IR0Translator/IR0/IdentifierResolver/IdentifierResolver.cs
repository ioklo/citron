using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    class IdentifierResolver
    {
        TypeInfoRepository itemInfoRepo;

        public IdentifierResolver(TypeInfoRepository itemInfoRepo)
        {
            this.itemInfoRepo = itemInfoRepo;
        }
        
        public void ResolveIdExp(S.IdentifierExp exp)
        {

        }

        public void ResolveMemberExp(S.MemberExp exp)
        {

        }
    }
}
