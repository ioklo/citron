using Citron.Collections;
using Citron.Infra;
using Citron.IR0;
using Citron.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    struct IR0LocEvaluator : IIR0LocVisitor<ValueTask<Value>>
    {
        IR0EvalContext context;

        public IR0LocEvaluator(IR0EvalContext context)
        {
            this.context = context;
        }

        public static ValueTask<Value> EvalAsync(Loc loc, IR0EvalContext context)
        {
            var evaluator = new IR0LocEvaluator(context);
            return loc.Accept<IR0LocEvaluator, ValueTask<Value>>(ref evaluator);
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitTemp(TempLoc loc)
        {
            var type = loc.Exp.GetExpType();

            var result = context.AllocValue(type);
            await IR0ExpEvaluator.EvalAsync(loc.Exp, context, result);
            return result;
        }

        ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitLocalVar(LocalVarLoc loc)
        {
            return new ValueTask<Value>(context.GetLocalValue(loc.Name));
        }

        ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitLambdaMemberVar(LambdaMemberVarLoc loc)
        {
            var lambdaThis = (LambdaValue)context.GetThisValue();
            return new ValueTask<Value>(lambdaThis.GetMemberValue(loc.MemberVar.GetName()));
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitListIndexer(ListIndexerLoc loc)
        {
            var listValue = (ListValue)await EvalAsync(loc.List, context);

            var indexValue = context.AllocValue<IntValue>(TypeIds.Int);
            await IR0ExpEvaluator.EvalAsync(loc.Index, context, indexValue);

            var list = listValue.GetList();
            return list[indexValue.GetInt()];
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitStructMember(StructMemberLoc loc)
        {
            if (loc.Instance == null)
                return context.GetStructStaticMemberValue(loc.MemberVar.GetSymbolId());

            var structValue = (StructValue)await EvalAsync(loc.Instance, context);
            return context.GetStructMemberValue(structValue, loc.MemberVar.GetSymbolId());
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitClassMember(ClassMemberLoc loc)
        {
            if (loc.Instance == null)
                return context.GetClassStaticMemberValue(loc.MemberVar.GetSymbolId());

            var classValue = (ClassValue)await EvalAsync(loc.Instance, context);
            return context.GetClassMemberValue(classValue, loc.MemberVar.GetSymbolId());
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitEnumElemMember(EnumElemMemberLoc loc)
        {
            var enumElemValue = (EnumElemValue)await EvalAsync(loc.Instance, context);
            return context.GetEnumElemMemberValue(enumElemValue, loc.MemberVar.GetSymbolId());
        }

        ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitThis(ThisLoc loc)
        {
            return new ValueTask<Value>(context.GetThisValue());
        }

        async ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitLocalDeref(LocalDerefLoc loc)
        {
            var refValue = (LocalRefValue)await EvalAsync(loc.InnerLoc, context);
            return refValue.GetTarget();
        }

        ValueTask<Value> IIR0LocVisitor<ValueTask<Value>>.VisitNullableValue(NullableValueLoc loc)
        {
            throw new NotImplementedException();
        }
    }
}
