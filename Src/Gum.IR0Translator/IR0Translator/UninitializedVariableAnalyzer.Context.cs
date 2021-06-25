using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {   
        partial class Context
        {
            Context? parent;       // 최상위라면 null
            InnerContext context;
            bool bNeedCopyToWrite; // 이 컨텍스트를 수정할 때, 복사해야하는지 여부

            public Context(Context? parent)
            {
                this.parent = parent;
                this.context = new InnerContext();
                this.bNeedCopyToWrite = false;
            }

            Context(Context? parent, InnerContext innerContext, bool bNeedCopyToWrite)
            {
                this.parent = parent;
                this.context = innerContext;
                this.bNeedCopyToWrite = bNeedCopyToWrite;
            }

            void EnsureWrite()
            {
                if (!bNeedCopyToWrite) return;

                context = new InnerContext(context); // 복사
                bNeedCopyToWrite = false;
            }

            public bool? IsInitialized(string name)
            {
                if (context.Contains(name))
                    return context.IsInitialized(name);

                if (parent != null)
                    return parent.IsInitialized(name);

                return null;
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

            // 새로운 클론을 만들면
            public Context Clone()
            {
                var newContext = new Context(parent, context, true);
                this.bNeedCopyToWrite = true; // 둘이 context를 공유하기 때문에, this도 변경하려면 복사 필요

                return newContext;
            }
        }

        partial class Context
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

                public void SetInitialized(string varName)
                {
                    localVars[varName] = true;
                }

                public void AddLocalVar(string varName, bool bInitialized)
                {
                    localVars[varName] = bInitialized;
                }
            }
        }
    }
}