using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    class IdentifierResolver
    {
        ItemInfoRepository itemInfoRepo;

        public IdentifierResolver(ItemInfoRepository itemInfoRepo)
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
