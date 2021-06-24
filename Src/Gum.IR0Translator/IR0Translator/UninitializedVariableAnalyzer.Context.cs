using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial struct UninitializedVariableAnalyzer
    {
        // never-thread-safe
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
            
            internal CopyOnWriteContext Share()
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

            public Context MakeChild()
            {
                var shareThis = Share();
                return new ChildContext(shareThis);
            }

            void EnsureWrite()
            {
                if (!bNeedCopyToWrite) return;

                context = context.Copy();
                bNeedCopyToWrite = false;
            }

            public void AddLocalVar(string name, bool bInitialized)
            {
                EnsureWrite();
                context.AddLocalVar(name, bInitialized);
            }

            // return ref
            public CopyOnWriteContext GetParentContext()
            {
                return context.GetParentContext();
            }

            // 
            public void Merge(CopyOnWriteContext other)
            {
                EnsureWrite();
                
                // 
            }
        }

        struct CopyOnWriteDictionary
        {
            Dictionary<string, bool>? localVars; // 필요없으면 할당하지 않는다.
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

        // Mutable, copy on demand
        abstract class Context 
        {
            public abstract void AddLocalVar(string varName, bool bInitialized);
            public abstract bool IsInitialized(string name);
            public abstract void SetInitialized(string name);
            public abstract bool NeedToWriteOnSetInitialized(string varName);
            public abstract Context GetParentContext();

            public abstract Context Copy();
        }

        class RootContext : Context
        {
            CopyOnWriteDictionary dictionary;

            public RootContext() { }

            RootContext(CopyOnWriteDictionary dictionary)
            {
                this.dictionary = dictionary;
            }

            public override void AddLocalVar(string varName, bool bInitialized)
            {
                dictionary.AddLocalVar(varName, bInitialized);
            }

            public override Context Copy()
            {
                return new RootContext(dictionary);
            }

            public override Context GetParentContext()
            {
                throw new UnreachableCodeException();
            }

            public override bool IsInitialized(string name) 
            { 
                var result = dictionary.IsInitialized(name);
                if (result != null)
                    return result.Value;

                return false;
            }

            public override bool NeedToWriteOnSetInitialized(string varName)
            {
                return dictionary.Contains(varName);
            }

            public override void SetInitialized(string varName)
            {
                Debug.Assert(dictionary.Contains(varName));
                dictionary.SetInitialized(varName);
            }
        }
        
        class ChildContext : Context
        {
            CopyOnWriteContext parent;
            CopyOnWriteDictionary dictionary;
            
            // 빈 Dictionary로 시작한다
            public ChildContext(CopyOnWriteContext parent)
            {
                this.parent = parent;
                this.dictionary = new CopyOnWriteDictionary();
            }

            public ChildContext(CopyOnWriteContext parent, CopyOnWriteDictionary dictionary)
            {
                this.parent = parent;
                this.dictionary = dictionary;
            }

            public override void AddLocalVar(string name, bool initialized)
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

            public override Context Copy()
            {
                return new ChildContext(parent.Share(), dictionary.Share());
            }

            // 두개 
            public void Merge(Context x, Context y)
            {
                // fast-forward, no modified
                if (this == x && this == y)
                {
                    return;
                }
                else if (this != x && this == y)
                {
                    parent = x.parent;
                    localVars = x.localVars;
                    return;
                }
                else if (this == childX && this != childY)
                {
                    parent = y.parent;
                    localVars = y.localVars;
                    return;
                }
                else
                {
                    if (localVars != null)
                    {
                        Debug.Assert(x.localVars != null && y.localVars != null);

                        foreach (var key in localVars.Keys)
                            localVars[key] = x.localVars[key] && y.localVars[key];
                    }
                }

                if (parent != null)
                {
                    parent.Merge(x, y);
                }
            }

            public void Replace(Context child)
            {
                Debug.Assert(child.parent != null);
                parent = child.parent.parent;
                localVars = child.parent.localVars;
            }

            public override Context GetParentContext()
            {
                return parent.GetContext();
            }
        }
    }
}