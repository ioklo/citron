using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    class LocEvaluator
    {
        Evaluator evaluator;
        public LocEvaluator(Evaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        async ValueTask<Value> EvalTempLocAsync(R.TempLoc loc, EvalContext context)
        {
            var result = evaluator.AllocValue(loc.Type, context);
            await evaluator.EvalExpAsync(loc.Exp, result, context);
            return result;
        }

        async ValueTask<Value> EvalListIndexerLocAsync(R.ListIndexerLoc loc, EvalContext context)
        {
            var listValue = (ListValue)await EvalLocAsync(loc.List, context);
            var indexValue = (IntValue)await evaluator.EvalLocAsync(loc.Index, context);
            
            var list = listValue.GetList();
            return list[indexValue.GetInt()];
        }

        public async ValueTask<Value> EvalLocAsync(R.Loc loc, EvalContext context)
        {
            switch (loc)
            {
                case R.TempLoc tempLoc:
                    return await EvalTempLocAsync(tempLoc, context);

                case R.GlobalVarLoc globalVarLoc:
                    return context.GetGlobalValue(globalVarLoc.Name);

                case R.LocalVarLoc localVarLoc:
                    return context.GetLocalValue(localVarLoc.Name);

                case R.ListIndexerLoc listIndexerLoc:
                    return await EvalListIndexerLocAsync(listIndexerLoc, context);

                case R.StaticMemberLoc staticMemberLoc:
                    var staticValue = (StaticValue)context.GetStaticValue(staticMemberLoc.Type);
                    return staticValue.GetMemberValue(staticMemberLoc.MemberName);

                case R.StructMemberLoc structMemberLoc:
                    var structValue = (StructValue)await EvalLocAsync(structMemberLoc.Instance, context);
                    return structValue.GetMemberValue(structMemberLoc.MemberName);

                case R.ClassMemberLoc classMemberLoc:
                    var classValue = (ClassValue)await EvalLocAsync(classMemberLoc.Instance, context);
                    return classValue.GetMemberValue(classMemberLoc.MemberName);

                case R.EnumMemberLoc enumMemberLoc:
                    var enumValue = (EnumValue)await EvalLocAsync(enumMemberLoc.Instance, context);
                    return enumValue.GetMemberValue(enumMemberLoc.MemberName);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
