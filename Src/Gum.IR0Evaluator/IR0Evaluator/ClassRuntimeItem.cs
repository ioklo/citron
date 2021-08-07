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
    abstract class ClassRuntimeItem : AllocatableRuntimeItem
    {
        public abstract ClassInstance AllocInstance(TypeContext typeContext);
        public abstract R.Path.Nested GetActualType(TypeContext typeContext);
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0ClassRuntimeItem : ClassRuntimeItem
        {
            GlobalContext globalContext;
            // R.Path.Nested classPath;
            R.ClassDecl decl;

            public override R.Name Name => decl.Name;

            public override R.ParamHash ParamHash => new R.ParamHash(decl.TypeParams.Length, default);

            public override ClassInstance AllocInstance(TypeContext typeContext)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                foreach (var decl in decl.MemberDecls)
                {
                    if (decl is R.ClassMemberVarDecl varDecl)
                    {
                        foreach (var name in varDecl.Names)
                        {
                            var memberValue = globalContext.AllocValue(varDecl.Type);
                            builder.Add(name, memberValue);
                        }
                    }
                }

                return new ClassInstance(this, typeContext, builder.ToImmutable());
            }

            public override Value Alloc(TypeContext typeContext)
            {   
                return new ClassValue();
            }

            public override R.Path.Nested GetActualType(TypeContext typeContext)
            {
                throw new NotImplementedException();
                // return typeContext.Apply_Nested(classPath);
            }
        }
    }
}