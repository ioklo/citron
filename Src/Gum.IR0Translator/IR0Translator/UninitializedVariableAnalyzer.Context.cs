using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {
        struct CopyOnWriteContext
        {
            Context? origParent;    // 원본 parent
            Context? parent;

            public CopyOnWriteContext(Context? parent)
            {
                this.origParent = parent;
                this.parent = null;
            }
        }

        // Mutable, copy on demand
        class Context
        {
            CopyOnWriteContext parent;
            Dictionary<string, bool>? localVars; // 필요없으면 할당하지 않는다.

            Context(parent, Dictionary<string, bool>? localVars)
            {
                this.parent = parent;
                this.localVars = localVars;
            }

            public Context(Context? parent)
            {
                this.parent = new CopyOnWriteContext(parent);
                this.localVars = null;
            }            

            public void AddLocalVar(string name, bool initialized)
            {
                if (localVars == null)
                    localVars = new Dictionary<string, bool>();

                localVars.Add(name, initialized);
            }

            public bool IsInitialized(string name)
            {
                if (localVars != null)
                    if (localVars.TryGetValue(name, out var initialized))
                        return initialized;

                if (parent != null)
                    return parent.IsInitialized(name);

                // 모든 localvariable이 등록되게 되어있으므로 여기에 오면 안된다
                throw new UnreachableCodeException();
            }

            public void SetInitialized(string name)
            {
                if (localVars != null && localVars.ContainsKey(name))
                {
                    localVars[name] = true;
                    return;
                }
                
                if (parent != null)
                {
                    // 아직 복사 전이라면 
                    EnsureCloneParent();
                    parent.SetInitialized(name);
                    return;
                }

                throw new UnreachableCodeException();
            }

            void EnsureCloneParent()
            {
                if (origParent == null) return;
                if (origParent != parent) return;

                parent = new Context(origParent.origParent, origParent.parent, origParent.localVars != null ? new Dictionary<string, bool>(origParent.localVars) : null);
            }

            public void Merge(Context childX, Context childY)
            {
                Debug.Assert(childX.parent != null && childY.parent != null);

                // fast-forward, no modified
                if (this == childX.parent && this == childY.parent)
                {
                    return;
                }
                else if (this != childX.parent && this == childY.parent)
                {
                    parent = childX.parent.parent;
                    localVars = childX.parent.localVars;
                    return;
                }
                else if (this == childX && this != childY)
                {
                    parent = childY.parent.parent;
                    localVars = childY.parent.localVars;
                    return;
                }
                else
                {
                    if (localVars != null)
                    {
                        Debug.Assert(childX.parent.localVars != null && childY.parent.localVars != null);

                        foreach (var key in localVars.Keys)
                            localVars[key] = childX.parent.localVars[key] && childY.parent.localVars[key];
                    }
                }

                if (parent != null)
                {
                    parent.Merge(childX.parent, childY.parent);
                }
            }

            public void Replace(Context child)
            {
                Debug.Assert(child.parent != null);
                parent = child.parent.parent;
                localVars = child.parent.localVars;
            }
        }
    }
}