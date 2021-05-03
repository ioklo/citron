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
            // 로컬 관련
            public abstract void AddLocalVarInfo(string name, TypeValue typeValue);
            public abstract LocalVarInfo? GetLocalVarInfo(string varName);
            public abstract bool DoesLocalVarNameExistInScope(string name); // 이 스코프 안에 존재하는가
            public abstract bool IsLocalVarOutsideLambda(string name);

            // 함수/람다 관련 
            //public abstract TypeValue? GetRetTypeValue();
            //public abstract void SetRetTypeValue(TypeValue retTypeValue);
            //public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            //public abstract bool IsSeqFunc();            

            public abstract TResult ExecInLambdaScope<TResult>(TypeValue? lambdaRetTypeValue, Func<TResult> action);
            public abstract void ExecInLocalScope(Action action);
            public abstract TResult ExecInLocalScope<TResult>(Func<TResult> func);
            
        }

        abstract class FuncScopeContext
        {
            public abstract LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            public abstract TypeValue? GetRetTypeValue();
            public abstract void SetRetTypeValue(TypeValue retTypeValue);
            public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            public abstract bool IsSeqFunc();            
        }

        class LambdaScopeContext : FuncScopeContext
        {
            ScopeContext parentContext;
            TypeValue? retTypeValue;
            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;

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

            public override bool IsLocalVarOutsideLambda(string name)
            {
                var localVarInfo = parentContext.GetLocalVarInfo(name);
                return localVarInfo != null;

                if (localVarsByName.ContainsKey(name))
                    return false;

                if (localVarsOutsideLambda.ContainsKey(name))
                    return true;

                // 아무데서도 못찾았으면 false
                return false;
            }

        }

        // 로컬 변수를 
        class LocalContext : ScopeContext
        {
            FuncScopeContext funcScopeContext; // 이 로컬에서 가장 가까운 함수 컨텍스트
            ScopeContext parentContext;

            ImmutableDictionary<string, LocalVarInfo> localVarInfos;

            public override void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarInfos.SetItem(name, new LocalVarInfo(name, typeValue));
            }

            public override LocalVarInfo? GetLocalVarInfo(string varName)
            {
                return localVarInfos.GetValueOrDefault(varName);
            }
            
            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return funcScopeContext.GetLocalVarOutsideLambda(varName);
            }

            public TypeValue? GetRetTypeValue()
            {
                return funcScopeContext.GetRetTypeValue();
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                funcScopeContext.SetRetTypeValue(retTypeValue);
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                parentContext.AddLambdaCapture(lambdaCapture);
            }
        }

        // 람다 안쪽        
        class LambdaContext : ScopeContext
        {
            ScopeContext parentContext;
            TypeValue? retTypeValue;            

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
            
            public override TResult ExecInLambdaScope<TResult>(TypeValue? lambdaRetTypeValue, Func<TResult> action);
            public override bool DoesLocalVarNameExistInScope(string name)
            {
                return localVarInfos.ContainsKey(name);
            }

            public override void ExecInLocalScope(Action action);
            public override TResult ExecInLocalScope<TResult>(Func<TResult> func);

            public override bool IsLocalVarOutsideLambda(string name)
            {
                // 현재 로컬에 존재하면 false
                if (localVarsByName.ContainsKey(name))
                    return false;

                var localVarInfo = parentContext.GetLocalVarInfo(name);
                return localVarInfo != null;
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
        
        class FuncContext : ScopeContext
        {
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            ScopedDictionary<string, LocalVarInfo> localVarsOutsideLambda; // 람다 구문 바깥에 있는 로컬 변수, 캡쳐대상이다
            ScopedDictionary<string, LocalVarInfo> localVarsByName;

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
        }
    }
}