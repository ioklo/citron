using Gum.CompileTime;
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.IR0
{
    partial class Analyzer
    {
        // 현재 분석되고 있는 함수 정보
        class FuncContext
        {
            ItemPath? funcPath;      // null 이면 최상위
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            ScopedDictionary<string, LocalVarOutsideLambdaInfo> localVarsOutsideLambda; // 람다 구문 바깥에 있는 로컬 변수, 캡쳐대상이다
            ScopedDictionary<string, LocalVarInfo> localVarsByName;

            public FuncContext(ItemPath? funcPath, TypeValue? retTypeValue, bool bSequence)
            {
                this.funcPath = funcPath;
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;

                this.localVarsOutsideLambda = new ScopedDictionary<string, LocalVarOutsideLambdaInfo>();
                this.localVarsByName = new ScopedDictionary<string, LocalVarInfo>();
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarsByName.Add(name, new LocalVarInfo(name, typeValue));
            }            

            public ItemPath? GetFuncPath()
            {
                return funcPath;
            }

            public LocalVarOutsideLambdaInfo? GetLocalVarOutsideLambdaInfo(string varName)
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

            public bool IsSeqFunc()
            {
                return bSequence;
            }

            public void ExecInLambdaScope(TypeValue? lambdaRetTypeValue, Action action)
            {
                localVarsOutsideLambda.Push();
                var prevLocalVarsByName = localVarsByName;
                var prevRetTypeValue = retTypeValue;
                retTypeValue = lambdaRetTypeValue;

                // Merge
                foreach (var (name, info) in localVarsByName)
                    localVarsOutsideLambda.Add(name, new LocalVarOutsideLambdaInfo(info));
                
                localVarsByName = new ScopedDictionary<string, LocalVarInfo>();

                try
                {
                    action.Invoke();                    
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

            public IEnumerable<LocalVarOutsideLambdaInfo> GetLocalVarsOutsideLambda()
            {
                return localVarsOutsideLambda.Select(kv => kv.Value);
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