using Citron.Analysis;
using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Citron.IR0;

namespace Citron
{
    partial struct IR0Evaluator
    {
        async ValueTask<Value> EvalTempLocAsync(R.TempLoc loc)
        {
            var type = loc.Exp.GetTypeSymbol();

            var result = evalContext.AllocValue(type);
            await EvalExpAsync(loc.Exp, result);
            return result;
        }

        async ValueTask<Value> EvalListIndexerLocAsync(R.ListIndexerLoc loc)
        {
            var listValue = (ListValue)await EvalLocAsync(loc.List);

            var indexValue = evalContext.AllocValue<IntValue>(SymbolId.Int);
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

                case R.LambdaMemberVarLoc lambdaMemberVarLoc:
                    {
                        var lambdaThis = (LambdaValue)evalContext.GetThisValue();
                        return lambdaThis.GetMemberValue(lambdaMemberVarLoc.MemberVar.GetName());
                    }

                case R.ListIndexerLoc listIndexerLoc:
                    return await EvalListIndexerLocAsync(listIndexerLoc);

                case R.StructMemberLoc structMemberLoc:
                    {
                        if (structMemberLoc.Instance == null)
                            return globalContext.GetStructStaticMemberValue(structMemberLoc.MemberVar.GetSymbolId());

                        var structValue = (StructValue)await EvalLocAsync(structMemberLoc.Instance);
                        return globalContext.GetStructMemberValue(structValue, structMemberLoc.MemberVar.GetSymbolId());
                    }

                case R.ClassMemberLoc classMemberLoc:
                    {
                        if (classMemberLoc.Instance == null)
                            return globalContext.GetClassStaticMemberValue(classMemberLoc.MemberVar.GetSymbolId());

                        var classValue = (ClassValue)await EvalLocAsync(classMemberLoc.Instance);
                        return globalContext.GetClassMemberValue(classValue, classMemberLoc.MemberVar.GetSymbolId());
                    }

                case R.EnumElemMemberLoc enumMemberLoc:
                    var enumElemValue = (EnumElemValue)await EvalLocAsync(enumMemberLoc.Instance);
                    return globalContext.GetEnumElemMemberValue(enumElemValue, enumMemberLoc.MemberVar.GetSymbolId());

                case R.ThisLoc:
                    return evalContext.GetThisValue();

                case R.DerefLocLoc derefLoc:
                    {
                        var refValue = (RefValue)await EvalLocAsync(derefLoc.Loc);
                        return refValue.GetTarget();
                    }

                case R.DerefExpLoc derefExpLoc:
                    {
                        var refValue = evalContext.AllocRefValue();
                        await EvalExpAsync(derefExpLoc.Exp, refValue);
                        return refValue.GetTarget();
                    }

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
