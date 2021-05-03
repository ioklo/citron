using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using R = Gum.IR0;
using Pretune;

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
        abstract class ScopeContext
        {
            public abstract void AddLocalVarInfo(string name, TypeValue typeValue);
            public abstract LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            public abstract LocalVarInfo? GetLocalVarInfo(string varName);
            public abstract TypeValue? GetRetTypeValue();
            public abstract void SetRetTypeValue(TypeValue retTypeValue);
            public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            public abstract bool IsSeqFunc();
            public abstract TResult ExecInLambdaScope<TResult>(TypeValue? lambdaRetTypeValue, Func<TResult> action);
            public abstract bool DoesLocalVarNameExistInScope(string name);
            public abstract void ExecInLocalScope(Action action);
            public abstract TResult ExecInLocalScope<TResult>(Func<TResult> func);
            public abstract bool IsLocalVarOutsideLambda(string name);
            public abstract ImmutableArray<R.TypeAndName> GetCapturedLocalVars();
            public abstract bool NeedCaptureThis();
        }

        // 람다 안쪽
        [AutoConstructor]
        class LambdaContext
        {
            ScopeContext parentContext;
            ImmutableDictionary<string, LocalVarInfo> localVarInfos;

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarInfos.SetItem(name, new LocalVarInfo(name, typeValue));
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return parentContext.GetLocalVarInfo(varName);
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                return localVarInfos.GetValueOrDefault(varName);
            }

            public TypeValue? GetRetTypeValue();
            public void SetRetTypeValue(TypeValue retTypeValue);
            public void AddLambdaCapture(LambdaCapture lambdaCapture);
            public bool IsSeqFunc();
            public TResult ExecInLambdaScope<TResult>(TypeValue? lambdaRetTypeValue, Func<TResult> action);
            public bool DoesLocalVarNameExistInScope(string name);
            public void ExecInLocalScope(Action action);
            public TResult ExecInLocalScope<TResult>(Func<TResult> func);
            public bool IsLocalVarOutsideLambda(string name);
            public ImmutableArray<R.TypeAndName> GetCapturedLocalVars();
            public bool NeedCaptureThis();
        }




        
        
        class FuncContext : ScopeContext
        {
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            ScopedDictionary<string, LocalVarInfo> localVarsOutsideLambda; // 람다 구문 바깥에 있는 로컬 변수, 캡쳐대상이다
            ScopedDictionary<string, LocalVarInfo> localVarsByName;

            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;

            public FuncContext(TypeValue? retTypeValue, bool bSequence)
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
                switch(lambdaCapture)
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
            
            public bool IsLocalVarOutsideLambda(string name)
            {
                if (localVarsByName.ContainsKey(name))
                    return false;

                if (localVarsOutsideLambda.ContainsKey(name))
                    return true;

                // 아무데서도 못찾았으면 false
                return false;
            }

            public ImmutableArray<R.TypeAndName> GetCapturedLocalVars()
            {
                return localCaptures.Select(localCapture => {
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
    }
}