using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class StructRuntimeItem : AllocatableRuntimeItem
    {
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0StructRuntimeItem : StructRuntimeItem
        {
            GlobalContext globalContext;
            R.StructDecl decl;

            public override R.Name Name => decl.Name;

            public override R.ParamHash ParamHash => new R.ParamHash(decl.TypeParams.Length, default);

            public override Value Alloc(TypeContext typeContext)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                foreach (var decl in decl.MemberDecls)
                {
                    if (decl is R.StructMemberVarDecl varDecl)
                    {
                        foreach (var name in varDecl.Names)
                        {
                            var memberValue = globalContext.AllocValue(varDecl.Type);
                            builder.Add(name, memberValue);                            
                        }
                    }
                }

                return new StructValue(builder.ToImmutable());
            }
        }
    }
}
