using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Infra;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;
using Gum.Collections;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public partial class Evaluator
    {
        EvalContext context;
        ImmutableArray<R.Stmt> topLevelStmts;

        ExpEvaluator expEvaluator;
        StmtEvaluator stmtEvaluator;
        LocEvaluator locEvaluator;

        public Evaluator(ICommandProvider commandProvider, R.Script script)
        {
            var topLevelRetValue = AllocValue(R.Path.Int);

            this.context = new EvalContext(script.Decls, topLevelRetValue);
            this.topLevelStmts = script.TopLevelStmts;

            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
            this.locEvaluator = new LocEvaluator(this);
        }

        Evaluator(EvalContext context, StmtEvaluator stmtEvaluator, Value? thisValue, ImmutableDictionary<string, Value> localVars)
        {
            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = stmtEvaluator.Clone(this);
            this.locEvaluator = new LocEvaluator(this);

            this.context = new EvalContext(
                context,
                localVars,
                EvalFlowControl.None,
                ImmutableArray<Task>.Empty,
                thisValue,
                VoidValue.Instance);
        }

        Evaluator CloneWithNewContext(Value? thisValue, ImmutableDictionary<string, Value> localVars)
        {
            return new Evaluator(context, stmtEvaluator, thisValue, localVars);
        }

        ValueTask EvalStringExpAsync(R.StringExp command, Value result)
        {
            return expEvaluator.EvalStringExpAsync(command, result);
        }

        bool GetBaseType(R.Path type, [NotNullWhen(true)] out R.Path? outBaseType)
        {
            throw new NotImplementedException();
            //var typeInst = context.GetTypeInst(type);

            //if (typeInst is ClassInst classInst)
            //{
            //    var baseType = classInst.GetBaseType();
            //    if (baseType != null)
            //    {
            //        outBaseType = baseType;
            //        return true;
            //    }
            //}

            //outBaseType = null;
            //return false;
        }
        
        // xType이 y타입인가 묻는 것
        bool IsType(R.Path xType, R.Path yType)
        {
            R.Path? curType = xType;

            while (curType != null)
            {
                if (EqualityComparer<R.Path?>.Default.Equals(curType, yType))
                    return true;

                if (!GetBaseType(curType, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curType = baseTypeValue;
            }

            return false;
        }

        TValue AllocValue<TValue>(R.Path type)
            where TValue : Value
        {
            return (TValue)AllocValue(type);
        }

        // type은 ir0 syntax의 일부분이다
        Value AllocValue(R.Path typePath)
        {
            if (typePath.Equals(R.Path.Bool))
            {
                return new BoolValue();
            }
            else if (typePath.Equals(R.Path.Int))
            {
                return new IntValue();
            }
            else if (typePath.Equals(R.Path.String))
            {
                return new StringValue();
            }
            else if (typePath.Equals(R.Path.Void.Instance))
            {
                return VoidValue.Instance;
            }
            else if (R.PathExtensions.IsList(typePath))
            {
                return new ListValue();
            }

            else if (R.PathExtensions.IsLambda(typePath))
            {
                // outer를 가져와서 
                var lambdaAllocator = context.GetLambdaAllocator(typePath);
                return lambdaAllocator.Alloc();

                // 여기서 부터 다시.
                throw new NotImplementedException();

                // NOTICE: 아래를 주석처리 했다 LambdaAllocator.Alloc에 넣는다
                //var lambdaDecl = context.GetDecl<R.LambdaDecl>(lambdaType.DeclId);

                //Value? capturedThis = null;
                //if (lambdaDecl.CapturedStatement.ThisType != null)
                //    capturedThis = AllocValue(lambdaDecl.CapturedStatement.ThisType);

                //var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                //foreach (var (elemType, elemName) in lambdaDecl.CapturedStatement.OuterLocalVars)
                //{
                //    var elemValue = AllocValue(elemType);
                //    capturesBuilder.Add(elemName, elemValue);
                //}

                //return new LambdaValue(lambdaType.DeclId, capturedThis, capturesBuilder.ToImmutable());
            }

            throw new NotImplementedException();

        }

                 

        // 캡쳐는 람다 Value안에 값을 세팅한다        
        void CaptureLocals(Value? capturedThis, ImmutableDictionary<string, Value> localVars, R.CapturedStatement capturedStatement)
        {
            if (capturedStatement.ThisType != null)
            {
                Debug.Assert(capturedThis != null);
                capturedThis.SetValue(context.GetThisValue()!);
            }

            foreach (var typeAndName in capturedStatement.OuterLocalVars)
            {
                var origValue = context.GetLocalValue(typeAndName.Name);
                localVars[typeAndName.Name].SetValue(origValue);
            }
        }

        async ValueTask EvalLocalVarDeclAsync(R.LocalVarDecl localVarDecl)
        {
            foreach (var elem in localVarDecl.Elems)
            {
                var value = AllocValue(elemType);
                context.AddLocalVar(elem.Name, value);

                // InitExp가 있으면 
                if (elem.InitExp != null)
                    await expEvaluator.EvalAsync(elem.InitExp, value);
            }
        }

        ValueTask EvalExpAsync(R.Exp exp, Value result)
        {
            return expEvaluator.EvalAsync(exp, result);
        }
        
        ValueTask<Value> EvalLocAsync(R.Loc loc)
        {
            return locEvaluator.EvalLocAsync(loc);
        }

        // internal for IR0FuncInvoker.Invoke
        internal IAsyncEnumerable<Gum.Infra.Void> EvalStmtAsync(R.Stmt stmt)
        {
            return stmtEvaluator.EvalStmtAsync(stmt);
        }
        
        public async ValueTask<int> EvalAsync()
        {
            foreach (var topLevelStmt in topLevelStmts)
            {
                await foreach (var _ in stmtEvaluator.EvalStmtAsync(topLevelStmt))
                {
                }

                if (context.GetFlowControl() == EvalFlowControl.Return)
                    break;
            }

            return ((IntValue)context.GetRetValue()).GetInt();
        }

        internal void SetYieldValue(Value yieldValue)
        {
            context.SetYieldValue(yieldValue);
        }
    }
}