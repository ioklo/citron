using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        class LocEvaluator
        {
            Evaluator evaluator;
            public LocEvaluator(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            async ValueTask<Value> EvalTempLocAsync(R.TempLoc loc)
            {
                var result = evaluator.AllocValue(loc.Type);
                await evaluator.EvalExpAsync(loc.Exp, result);
                return result;
            }

            async ValueTask<Value> EvalListIndexerLocAsync(R.ListIndexerLoc loc)
            {
                var listValue = (ListValue)await EvalLocAsync(loc.List);

                var indexValue = evaluator.AllocValue<IntValue>(R.Path.Int);
                await evaluator.EvalExpAsync(loc.Index, indexValue);

                var list = listValue.GetList();
                return list[indexValue.GetInt()];
            }

            public async ValueTask<Value> EvalLocAsync(R.Loc loc)
            {
                switch (loc)
                {
                    case R.TempLoc tempLoc:
                        return await EvalTempLocAsync(tempLoc);

                    case R.GlobalVarLoc globalVarLoc:
                        return evaluator.context.GetGlobalValue(globalVarLoc.Name);

                    case R.LocalVarLoc localVarLoc:
                        return evaluator.context.GetLocalValue(localVarLoc.Name);

                    case R.CapturedVarLoc capturedVarLoc:
                        return evaluator.context.GetCapturedValue(capturedVarLoc.Name);

                    case R.ListIndexerLoc listIndexerLoc:
                        return await EvalListIndexerLocAsync(listIndexerLoc);

                    case R.StaticMemberLoc staticMemberLoc:
                        var staticValue = (StaticValue)evaluator.context.GetStaticValue(staticMemberLoc.Type);
                        return staticValue.GetMemberValue(staticMemberLoc.MemberName);

                    case R.StructMemberLoc structMemberLoc:
                        var structValue = (StructValue)await EvalLocAsync(structMemberLoc.Instance);
                        throw new NotImplementedException();
                        // return structValue.GetMemberValue(structMemberLoc.MemberName);

                    case R.ClassMemberLoc classMemberLoc:
                        var classValue = (ClassValue)await EvalLocAsync(classMemberLoc.Instance);
                        return classValue.GetMemberValue(classMemberLoc.MemberName);

                    case R.EnumElemMemberLoc enumMemberLoc:
                        var enumElemValue = (EnumElemValue)await EvalLocAsync(enumMemberLoc.Instance);
                        var enumElemFieldRuntimeItem = evaluator.context.GetRuntimeItem<EnumElemFieldRuntimeItem>(enumMemberLoc.EnumElemField);

                        return enumElemFieldRuntimeItem.GetMemberValue(enumElemValue);

                    case R.ThisLoc thisLoc:
                        throw new NotImplementedException();

                    case R.DerefLocLoc derefLoc:
                        var refValue = (RefValue)await EvalLocAsync(derefLoc.Loc);
                        return refValue.GetTarget();

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }
    }
}
