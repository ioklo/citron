using Gum.Collections;
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
            GlobalContext globalContext;
            EvalContext context;
            LocalContext localContext;

            public static ValueTask<Value> EvalAsync(GlobalContext globalContext, EvalContext context, LocalContext localContext, R.Loc loc)
            {
                var evaluator = new LocEvaluator(globalContext, context, localContext);
                return evaluator.EvalLocAsync(loc);
            }

            LocEvaluator(GlobalContext globalContext, EvalContext context, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.context = context;
                this.localContext = localContext;
            }

            ValueTask EvalExpAsync(R.Exp exp, Value result) => ExpEvaluator.EvalAsync(globalContext, context, localContext, exp, result);

            async ValueTask<Value> EvalTempLocAsync(R.TempLoc loc)
            {
                var result = globalContext.AllocValue(loc.Type);
                await EvalExpAsync(loc.Exp, result);
                return result;
            }

            async ValueTask<Value> EvalListIndexerLocAsync(R.ListIndexerLoc loc)
            {
                var listValue = (ListValue)await EvalLocAsync(loc.List);

                var indexValue = globalContext.AllocValue<IntValue>(R.Path.Int);
                await EvalExpAsync(loc.Index, indexValue);

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
                        return globalContext.GetGlobalValue(globalVarLoc.Name);

                    case R.LocalVarLoc localVarLoc:
                        return localContext.GetLocalValue(localVarLoc.Name);

                    case R.CapturedVarLoc capturedVarLoc:
                        return context.GetCapturedValue(capturedVarLoc.Name);

                    case R.ListIndexerLoc listIndexerLoc:
                        return await EvalListIndexerLocAsync(listIndexerLoc);

                    case R.StaticMemberLoc staticMemberLoc:                        

                        var staticValue = (StaticValue)context.GetStaticValue(staticMemberLoc.MemberPath.Outer);
                        var name = ((R.Name.Normal)staticMemberLoc.MemberPath.Name).Value;

                        return staticValue.GetMemberValue(name);

                    case R.StructMemberLoc structMemberLoc:
                        {
                            var structValue = (StructValue)await EvalLocAsync(structMemberLoc.Instance);
                            var memberVarItem = globalContext.GetRuntimeItem<StructMemberVarRuntimeItem>(structMemberLoc.structMember);

                            return memberVarItem.GetMemberValue(structValue);
                        }

                    case R.ClassMemberLoc classMemberLoc:
                        var classValue = (ClassValue)await EvalLocAsync(classMemberLoc.Instance);
                        return classValue.GetMemberValue(classMemberLoc.MemberName);

                    case R.EnumElemMemberLoc enumMemberLoc:
                        var enumElemValue = (EnumElemValue)await EvalLocAsync(enumMemberLoc.Instance);
                        var enumElemFieldRuntimeItem = globalContext.GetRuntimeItem<EnumElemFieldRuntimeItem>(enumMemberLoc.EnumElemField);

                        return enumElemFieldRuntimeItem.GetMemberValue(enumElemValue);

                    case R.ThisLoc thisLoc:
                        return context.GetThisValue();

                    case R.DerefLocLoc derefLoc:
                        {
                            var refValue = (RefValue)await EvalLocAsync(derefLoc.Loc);
                            return refValue.GetTarget();
                        }

                    case R.DerefExpLoc derefExpLoc:
                        {
                            var refValue = globalContext.AllocRefValue();
                            await EvalExpAsync(derefExpLoc.Exp, refValue);
                            return refValue.GetTarget();
                        }

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }
    }
}
