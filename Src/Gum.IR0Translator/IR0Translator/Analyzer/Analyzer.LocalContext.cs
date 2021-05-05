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
            public string Name { get; }
            public TypeValue TypeValue { get; }
            public LocalVarInfo(string name, TypeValue typeValue)
            {
                Name = name;
                TypeValue = typeValue;
            }
        }        

        // 현재 분석이 진행되는 곳의 컨텍스트
        class LocalContext
        {
            CallableContext callableContext;
            LocalContext? parentLocalContext;
            ImmutableDictionary<string, LocalVarInfo> localVarInfos;
            bool bLoop;

            LocalContext(CallableContext callableContext, LocalContext? parentLocalContext, bool bLoop)
            {
                this.callableContext = callableContext;
                this.parentLocalContext = parentLocalContext;
                this.localVarInfos = ImmutableDictionary<string, LocalVarInfo>.Empty;
                this.bLoop = bLoop;
            }

            public LocalContext(CallableContext callableContext)
                : this(callableContext, null, false)
            {
            }

            public LocalContext NewLocalContext()
            {
                return new LocalContext(callableContext, this, bLoop);
            }

            public LocalContext NewLocalContextWithLoop()
            {
                return new LocalContext(callableContext, this, true);
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarInfos.SetItem(name, new LocalVarInfo(name, typeValue));
            }

            public LocalVarInfo? GetLocalVarInfo(string varName)
            {
                if (localVarInfos.TryGetValue(varName, out var info))
                    return info;

                if (parentLocalContext != null)
                    return parentLocalContext.GetLocalVarInfo(varName);

                return null;
            }                        
            
            public bool IsLocalVarOutsideLambda(string name)
            {
                // 현재 로컬에 존재하면 false
                if (localVarInfos.ContainsKey(name))
                    return false;

                var localVarInfo = callableContext.GetLocalVarOutsideLambda(name);
                return localVarInfo != null;
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