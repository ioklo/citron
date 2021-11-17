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
            public bool IsRef { get; }
            public TypeValue TypeValue { get; }
            public string Name { get; }
            
            public LocalVarInfo(bool bRef, TypeValue typeValue, string name)
            {
                IsRef = bRef;
                Name = name;
                TypeValue = typeValue;
            }

            public LocalVarInfo UpdateTypeValue(TypeValue newTypeValue)
            {
                return new LocalVarInfo(IsRef, newTypeValue, Name);
            }
        }

        // 현재 분석이 진행되는 곳의 컨텍스트
        class LocalContext : IMutable<LocalContext>
        {
            LocalContext? parentLocalContext;
            ImmutableDictionary<string, LocalVarInfo> localVarInfos;
            bool bLoop;

            LocalContext(LocalContext? parentLocalContext, bool bLoop)
            {   
                this.parentLocalContext = parentLocalContext;
                this.localVarInfos = ImmutableDictionary<string, LocalVarInfo>.Empty;
                this.bLoop = bLoop;
            }

            public LocalContext()
                : this(null, false)
            {
            }

            public LocalContext(LocalContext other, CloneContext cloneContext)
            {
                if (other.parentLocalContext != null)
                    this.parentLocalContext = cloneContext.GetClone(other.parentLocalContext);
                else
                    this.parentLocalContext = null;

                this.localVarInfos = other.localVarInfos;
                this.bLoop = other.bLoop;
            }

            public LocalContext Clone(CloneContext context)
            {
                return new LocalContext(this, context);
            }

            public void Update(LocalContext src, UpdateContext context)
            {
                if (this.parentLocalContext != null)
                    context.Update(this.parentLocalContext, src.parentLocalContext!);
                else
                    Debug.Assert(src.parentLocalContext == null);

                this.localVarInfos = src.localVarInfos;
                this.bLoop = src.bLoop;
            }

            public LocalContext NewLocalContext()
            {
                return new LocalContext(this, bLoop);
            }

            public LocalContext NewLocalContextWithLoop()
            {
                return new LocalContext(this, true);
            }

            public void AddLocalVarInfo(bool bRef, TypeValue typeValue, string name)
            {
                localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(bRef, typeValue, name));
            }

            public void SetLocalVarType(string name, TypeValue typeValue)
            {
                var value = localVarInfos[name];
                localVarInfos = localVarInfos.SetItem(name, value.UpdateTypeValue(typeValue));
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                if (localVarInfos.TryGetValue(varName, out var info))
                    return info;

                if (parentLocalContext != null)
                    return parentLocalContext.GetLocalVarInfo(varName);

                return null;
            }                                    

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return localVarInfos.ContainsKey(name);
            }

            public bool IsInLoop()
            {
                return bLoop;
            }

            
        }
    }
}