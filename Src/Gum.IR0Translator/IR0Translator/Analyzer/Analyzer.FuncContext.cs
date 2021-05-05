using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using R = Gum.IR0;
using Pretune;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        struct LocalVarInfo
        {   
            public string Name { get; }
            public TypeValue TypeValue { get; }
            public LocalVarInfo(string name, TypeValue typeValue)
            {
                Name = name;
                TypeValue = typeValue;
            }
        }

        // 현재 분석중인 스코프 정보

        abstract class CallableContext
        {
            // TODO: 이름 수정, Lambda가 아니라 Callable
            public abstract LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            public abstract TypeValue? GetRetTypeValue();
            public abstract void SetRetTypeValue(TypeValue retTypeValue);
            public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            public abstract bool IsSeqFunc();            
        }

        class FuncContext : CallableContext
        {
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            public FuncContext(TypeValue? retTypeValue, bool bSequence)
            {
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;
            }

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                // TODO: 지금은 InnerFunc를 구현하지 않으므로, Outside가 없다. 나중에 지원
                return null;
            }

            public override TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public override void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                throw new UnreachableCodeException();
            }

            public override bool IsSeqFunc() 
            { 
                return bSequence; 
            }
        }

        class LambdaContext : CallableContext
        {
            LocalContext parentContext;
            TypeValue? retTypeValue;
            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;

            public LambdaContext(LocalContext parentContext, TypeValue? retTypeValue)
            {
                this.parentContext = parentContext;
                this.retTypeValue = retTypeValue;
                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, TypeValue>();
            }

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return parentContext.GetLocalVarInfo(varName);
            }

            public override TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public override void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                switch (lambdaCapture)
                {
                    case NoneLambdaCapture: break;
                    case ThisLambdaCapture: bCaptureThis = true; break;
                    case LocalLambdaCapture localCapture:
                        if (localCaptures.TryGetValue(localCapture.Name, out var prevType))
                            Debug.Assert(prevType.Equals(localCapture.Type));
                        else
                            localCaptures.Add(localCapture.Name, localCapture.Type);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }

            public override bool IsSeqFunc()
            {
                return false; // 아직 sequence lambda 기능이 없으므로 
            }

            public ImmutableArray<R.TypeAndName> GetCapturedLocalVars()
            {
                return localCaptures.Select(localCapture =>
                {
                    var name = localCapture.Key;
                    var type = localCapture.Value.GetRType();
                    return new R.TypeAndName(type, name);
                }).ToImmutableArray();
            }

            public bool NeedCaptureThis()
            {
                return bCaptureThis;
            }
        }

        // facade, StmtAndExpContext
        class StmtAndExpContext
        {
            CallableContext callableContext; // 이 로컬에서 가장 가까운 함수/람다 컨텍스트
            LocalContext localContext;

            StmtAndExpContext(CallableContext callableContext, LocalContext localContext)
            {
                this.callableContext = callableContext;
                this.localContext = localContext;
            }

            public StmtAndExpContext NewContext(bool bLoop)
            {
                var newLocalContext = localContext.NewLocalContext(bLoop);
                return new StmtAndExpContext(callableContext, newLocalContext);
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localContext.AddLocalVarInfo(name, typeValue);
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                return localContext.GetLocalVarInfo(varName);
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return callableContext.GetLocalVarOutsideLambda(varName);
            }

            public TypeValue? GetRetTypeValue()
            {
                return callableContext.GetRetTypeValue();
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                callableContext.SetRetTypeValue(retTypeValue);
            }

            public void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                callableContext.AddLambdaCapture(lambdaCapture);
            }

            public bool IsLocalVarOutsideLambda(string name)
            {
                return localContext.IsLocalVarOutsideLambda(name);
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return localContext.DoesLocalVarNameExistInScope(name);
            }

            public bool IsInLoop()
            {
                return localContext.IsInLoop();
            }
        }

        // 현재 분석이 진행되는 곳의 컨텍스트
        class LocalContext
        {
            CallableContext callableContext;
            LocalContext? parentLocalContext;
            ImmutableDictionary<string, LocalVarInfo> localVarInfos;
            bool bLoop;

            LocalContext(CallableContext callableContext, LocalContext? parentLocalContext, bool bLoop)
            {
                this.callableContext = callableContext;
                this.parentLocalContext = parentLocalContext;
                this.localVarInfos = ImmutableDictionary<string, LocalVarInfo>.Empty;
                this.bLoop = bLoop;
            }

            public LocalContext NewLocalContext(bool bLoop)
            {
                return new LocalContext(callableContext, this, bLoop);
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarInfos.SetItem(name, new LocalVarInfo(name, typeValue));
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                if (localVarInfos.TryGetValue(varName, out var info))
                    return info;

                if (parentLocalContext != null)
                    return parentLocalContext.GetLocalVarInfo(varName);

                return null;
            }                        
            
            public bool IsLocalVarOutsideLambda(string name)
            {
                // 현재 로컬에 존재하면 false
                if (localVarInfos.ContainsKey(name))
                    return false;

                var localVarInfo = callableContext.GetLocalVarOutsideLambda(name);
                return localVarInfo != null;
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return localVarInfos.ContainsKey(name);
            }

            public bool IsInLoop()
            {
                if (bLoop) return true;
                if (parentLocalContext != null) return parentLocalContext.IsInLoop();
                return false;
            }
        }

        // 람다 안쪽        
        class FuncContext2
        {
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            ScopedDictionary<string, LocalVarInfo> localVarsOutsideLambda; // 람다 구문 바깥에 있는 로컬 변수, 캡쳐대상이다
            ScopedDictionary<string, LocalVarInfo> localVarsByName;

            public FuncContext2(TypeValue? retTypeValue, bool bSequence)
            {
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;

                this.localVarsOutsideLambda = new ScopedDictionary<string, LocalVarInfo>();
                this.localVarsByName = new ScopedDictionary<string, LocalVarInfo>();

                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, TypeValue>();
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarsByName.Add(name, new LocalVarInfo(name, typeValue));
            }            

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return localVarsOutsideLambda.GetValueOrDefault(varName);
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                return localVarsByName.GetValueOrDefault(varName);
            }            

            public TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                
            }

            public bool IsSeqFunc()
            {
                return bSequence;
            }

            public TResult ExecInLambdaScope<TResult>(TypeValue? lambdaRetTypeValue, Func<TResult> action)
            {
                localVarsOutsideLambda.Push();
                var prevLocalVarsByName = localVarsByName;
                var prevRetTypeValue = retTypeValue;
                retTypeValue = lambdaRetTypeValue;

                // Merge
                foreach (var (name, info) in localVarsByName)
                    localVarsOutsideLambda.Add(name, info);

                localVarsByName = new ScopedDictionary<string, LocalVarInfo>();

                try
                {
                    return action.Invoke();
                }
                finally
                {
                    localVarsOutsideLambda.Pop();
                    localVarsByName = prevLocalVarsByName;
                    retTypeValue = prevRetTypeValue;
                }
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return localVarsByName.ContainsKeyOutmostScope(name);
            }

            public void ExecInLocalScope(Action action)
            {
                localVarsByName.Push();

                try
                {
                    action.Invoke();
                }
                finally
                {
                    localVarsByName.Pop();
                }
            }

            public TResult ExecInLocalScope<TResult>(Func<TResult> func)
            {
                localVarsByName.Push();

                try
                {
                    return func.Invoke();
                }
                finally
                {
                    localVarsByName.Pop();
                }
            }
            
        }
    }
}