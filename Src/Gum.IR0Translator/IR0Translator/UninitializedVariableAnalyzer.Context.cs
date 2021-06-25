using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {   
        class Context
        {
            Context? parent;       // 최상위라면 null

            InnerContext context;
            bool bNeedCopyToWrite; // 이 컨텍스트를 수정할 때, 복사해야하는지 여부

            public bool IsInitialized(string name)
            {
                return context.IsInitialized(name);
            }

            // Write operation
            public void SetInitialized(string name) // recursively
            {
                // 자기 자신에게만 있는지 확인
                if (context.Contains(name))
                {
                    EnsureWrite();
                    context.SetInitialized(name);
                    return;
                }

                if (parent != null)
                    parent.SetInitialized(name);
            }

            public void AddLocalVar(string name, bool bInitialized)
            {
                EnsureWrite();
                context.AddLocalVar(name, bInitialized);
            }
        }

        // 컨텍스트의 공유 상태, 독점 상태는 외부에서 관리한다
        class InnerContext
        {
            Dictionary<string, bool> localVars; // 필요없을때 할당하지 않는 부분은 추후에, 지금은 컨텍스트를 만들면 무조건 생성한다            
            
            // 실제 오퍼레이션 
            public bool IsInitialized(string name)
            {
                if (localVars.TryGetValue(name, out var bInitialized))
                    return bInitialized;

                if (parent != null)
                    return parent.IsInitialized();

                throw new UnreachableCodeException();
            }

            public void SetInitialized(string varName)
            {   
                if (context.NeedToWriteOnSetInitialized(varName))
                    EnsureWrite();

                context.SetInitialized(varName);
            }            
            
            public void AddLocalVar(string name, bool bInitialized)
            {
                EnsureWrite();
                context.AddLocalVar(name, bInitialized);
            }
        }

        struct CopyOnWriteDictionary
        {
            ; // 필요없으면 할당하지 않는다.
            bool bNeedCopyToWrite;

            public void AddLocalVar(string name, bool initialized)
            {
                EnsureWrite();
                Debug.Assert(localVars != null);

                localVars.Add(name, initialized);
            }

            public CopyOnWriteDictionary Share()
            {
                var result = new CopyOnWriteDictionary();
                result.localVars = localVars;
                result.bNeedCopyToWrite = true;

                this.bNeedCopyToWrite = true; // 지금 부터 나도 바뀌면 복사를 떠야 한다
                return result;
            }

            public bool? IsInitialized(string name)
            {
                if (localVars == null) return null;

                if (localVars.TryGetValue(name, out var initialized))
                    return initialized;

                return null;
            }

            void EnsureWrite()
            {
                if (!bNeedCopyToWrite) return;

                if (localVars != null)
                    localVars = new Dictionary<string, bool>(localVars);
                else
                    localVars = new Dictionary<string, bool>();

                bNeedCopyToWrite = false;
            }

            public bool Contains(string name)
            {
                if (localVars == null) return false;
                return localVars.ContainsKey(name);
            }            

            public void SetInitialized(string varName)
            {
                Debug.Assert(localVars != null);

                if (localVars[varName] == false)
                {
                    EnsureWrite();
                    localVars[varName] = true;
                }
            }
        }

    }
}