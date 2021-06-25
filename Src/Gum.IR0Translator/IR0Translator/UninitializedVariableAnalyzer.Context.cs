using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {   
        partial struct Context
        {
            Context? parent;       // 최상위라면 null
            InnerContext innerContext;
            bool bNeedCopyToWrite; // 이 컨텍스트를 수정할 때, 복사해야하는지 여부

            public Context(Context? parent)
            {
                this.parent = parent;
                this.innerContext = new InnerContext();
                this.bNeedCopyToWrite = false;
            }

            Context(Context? parent, InnerContext innerContext, bool bNeedCopyToWrite)
            {
                this.parent = parent;
                this.innerContext = innerContext;
                this.bNeedCopyToWrite = bNeedCopyToWrite;
            }

            void EnsureWrite()
            {
                if (!bNeedCopyToWrite) return;

                innerContext = new InnerContext(innerContext); // 복사
                bNeedCopyToWrite = false;
            }

            public bool? IsInitialized(string name)
            {
                if (innerContext.Contains(name))
                    return innerContext.IsInitialized(name);

                if (parent != null)
                    return parent.IsInitialized(name);

                return null;
            }

            // Write operation
            public void SetInitialized(string name) // recursively
            {
                // 자기 자신에게만 있는지 확인
                if (innerContext.Contains(name))
                {
                    EnsureWrite();
                    innerContext.SetInitialized(name, true);
                    return;
                }

                if (parent != null)
                    parent.SetInitialized(name);
            }

            public void AddLocalVar(string name, bool bInitialized)
            {
                EnsureWrite();
                innerContext.AddLocalVar(name, bInitialized);
            }

            // 새로운 클론을 만들면
            public Context Clone()
            {
                var clonedParent = parent == null ? null : parent.Clone();

                var newContext = new Context(clonedParent, innerContext, true);
                this.bNeedCopyToWrite = true; // 둘이 innerContext를 공유하기 때문에, this도 변경하려면 복사 필요

                return newContext;
            }

            public void Merge(Context other)
            {
                // fast-forward, copy가 일어났을 경우만
                if (innerContext != other.innerContext)
                {
                    foreach (var key in innerContext.GetKeys())
                    {
                        bool x = innerContext.IsInitialized(key);
                        bool y = other.innerContext.IsInitialized(key);
                        bool merged = x && y;

                        if (x != merged)
                        {
                            EnsureWrite();
                            innerContext.SetInitialized(key, merged);
                        }
                    }
                }

                if (parent != null)
                {
                    Debug.Assert(other.parent != null);
                    parent.Merge(other.parent);
                }
            }
        }

        partial struct Context
        {
            // 컨텍스트의 공유 상태, 독점 상태는 외부에서 관리한다
            class InnerContext
            {
                Dictionary<string, bool> localVars; // 필요없을때 할당하지 않는 부분은 추후에, 지금은 컨텍스트를 만들면 무조건 생성한다            

                public InnerContext(InnerContext other)
                {
                    localVars = new Dictionary<string, bool>(other.localVars);
                }

                public InnerContext()
                {
                    localVars = new Dictionary<string, bool>();
                }

                // 실제 오퍼레이션 
                public bool IsInitialized(string name)
                {
                    return localVars[name];
                }

                public bool Contains(string varName)
                {
                    return localVars.ContainsKey(varName);
                }

                public void SetInitialized(string varName, bool bInitialized)
                {
                    localVars[varName] = bInitialized;
                }

                public void AddLocalVar(string varName, bool bInitialized)
                {
                    localVars[varName] = bInitialized;
                }

                // foreach 수행중에 localVars가 바뀌어도 상관없도록 list를 만들어서 리턴한다
                public IEnumerable<string> GetKeys()
                {
                    return localVars.Keys.ToList();
                }
            }
        }
    }
}