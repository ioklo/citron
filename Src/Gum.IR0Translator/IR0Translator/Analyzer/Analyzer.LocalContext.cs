﻿using Gum.Infra;
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
        class LocalContext : ICloneable<LocalContext>
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

            public LocalContext NewLocalContext()
            {
                return new LocalContext(this, bLoop);
            }

            public LocalContext NewLocalContextWithLoop()
            {
                return new LocalContext(this, true);
            }

            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(name, typeValue));
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

            public LocalContext Clone(CloneContext context)
            {
                return new LocalContext(this, context);
            }
        }
    }
}