using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {
        struct CopyOnWriteContext
        {
            Context context;
            bool bNeedCopyToWrite;

            public CopyOnWriteContext(Context context)
            {
                this.context = context;
                this.bNeedCopyToWrite = true;
            }

            public bool IsInitialized(string name)
            {
                return context.IsInitialized(name);
            }

            public CopyOnWriteContext Share()
            {
                var result = new CopyOnWriteContext(context);
                this.bNeedCopyToWrite = true;
                return result;
            }

            public void SetInitialized(string varName)
            {   
                if (context.NeedToWriteOnSetInitialized(varName))
                    EnsureWrite();

                context.SetInitialized(varName);
            }

            void EnsureWrite()
            {
                if (!bNeedCopyToWrite) return;

                context = context.Copy();
                bNeedCopyToWrite = false;
            }
        }

        struct CopyOnWriteDictionary
        {
            Dictionary<string, bool>? localVars; // 필요없으면 할당하지 않는다.
            bool bModified;            

            public void AddLocalVar(string name, bool initialized)
            {
                EnsureModify();
                Debug.Assert(localVars != null);

                localVars.Add(name, initialized);
            }

            public CopyOnWriteDictionary Share()
            {
                var result = new CopyOnWriteDictionary();
                result.localVars = localVars;
                result.bModified = false;

                this.bModified = false; // 지금 부터 나도 바뀌면 복사를 떠야 한다
                return result;
            }

            public bool? IsInitialized(string name)
            {
                if (localVars == null) return null;

                if (localVars.TryGetValue(name, out var initialized))
                    return initialized;

                return null;
            }

            void EnsureModify()
            {
                if (bModified) return;

                if (localVars != null)
                    localVars = new Dictionary<string, bool>(localVars);
                else
                    localVars = new Dictionary<string, bool>();

                bModified = true;
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
                    EnsureModify();
                    localVars[varName] = true;
                }

            }
        }

        // Mutable, copy on demand
        abstract class Context 
        {
            public abstract bool IsInitialized(string name);
            public abstract void SetInitialized(string name);
            public abstract bool NeedToWriteOnSetInitialized(string varName);

            public abstract Context Copy();
        }

        class RootContext : Context 
        {
            CopyOnWriteDictionary dictionary;

            public override bool IsInitialized(string name) 
            { 
                var result = dictionary.IsInitialized(name);
                if (result != null)
                    return result.Value;

                return false;
            }
        }
        
        class ChildContext : Context
        {
            CopyOnWriteContext parent;
            CopyOnWriteDictionary dictionary;
            
            public ChildContext(CopyOnWriteContext parent)
            {
                this.parent = parent;
                this.dictionary = new CopyOnWriteDictionary();
            }            

            public void AddLocalVar(string name, bool initialized)
            {
                dictionary.AddLocalVar(name, initialized);                
            }

            public override bool IsInitialized(string name)
            {
                var result = dictionary.IsInitialized(name);
                if (result != null)
                    return result.Value;
                
                return parent.IsInitialized(name);
            }

            public override bool NeedToWriteOnSetInitialized(string varName)
            {
                return dictionary.Contains(varName);
            }

            public override void SetInitialized(string name)
            {
                if (dictionary.Contains(name))
                {
                    dictionary.SetInitialized(name);
                    return;
                }

                parent.SetInitialized(name);
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