using System;
using Gum.Collections;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class TupleTypeValue : TypeSymbol
    {
        RItemFactory ritemFactory;
        public ImmutableArray<(TypeSymbol Type, string? Name)> Elems { get; }

        public override TypeSymbol Apply(TypeEnv typeEnv)
        {   
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            var builder = ImmutableArray.CreateBuilder<R.TupleTypeElem>(Elems.Length);
            foreach(var elem in Elems)
            {
                var rpath = elem.Type.GetRPath();
                if (elem.Name == null)
                    throw new NotImplementedException(); // unnamed tuple
                var name = elem.Name;

                builder.Add(new R.TupleTypeElem(rpath, new R.Name.Normal(name)));
            }

            return ritemFactory.MakeTupleType(builder.MoveToImmutable());
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();
    }
}
