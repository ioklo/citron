using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Citron.IR0;

namespace Citron.IR0Evaluator
{
    abstract class StructRuntimeItem : AllocatableRuntimeItem
    {
    }

        [AutoConstructor]
        partial class IR0StructRuntimeItem : StructRuntimeItem
        {
            IR0GlobalContext globalContext;
            R.StructDecl decl;

            public override R.Name Name => new R.Name.Normal(decl.Name);

            public override R.ParamHash ParamHash => new R.ParamHash(decl.TypeParams.Length, default);

            //public override Value Alloc(TypeContext typeContext)
            //{
            //    var builder = ImmutableDictionary.CreateBuilder<string, Value>();

            //    foreach (var varDecl in decl.MemberVarDecls)
            //    {
            //        foreach (var name in varDecl.Names)
            //        {
            //            var memberValue = context.AllocValue(varDecl.Type);
            //            builder.Add(name, memberValue);
            //        }                    
            //    }

            //    return new StructValue(builder.ToImmutable());
            //}
        }
}
