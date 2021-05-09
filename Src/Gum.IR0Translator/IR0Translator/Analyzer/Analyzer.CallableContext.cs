﻿using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;
using R = Gum.IR0;
using System.Linq;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // 현재 분석중인 스코프 정보
        abstract class CallableContext : DeclContext, ICloneable<CallableContext>
        {
            int anonymousCount;

            // TODO: 이름 수정, Lambda가 아니라 Callable
            public abstract LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            public abstract TypeValue? GetRetTypeValue();
            public abstract void SetRetTypeValue(TypeValue retTypeValue);
            public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            public abstract bool IsSeqFunc();

            public CallableContext()
            {
                anonymousCount = 0;
            }

            protected CallableContext(CallableContext other, CloneContext cloneContext)
            {
                this.anonymousCount = other.anonymousCount;
            }

            public abstract CallableContext Clone_CallableContext(CloneContext context);

            public sealed override DeclContext Clone_DeclContext(CloneContext context)
                => Clone_CallableContext(context);            

            CallableContext ICloneable<CallableContext>.Clone(CloneContext context)
                => Clone_CallableContext(context);

            public R.AnonymousId NewAnonymousId()
            {
                var anonymousIdCount = new R.AnonymousId(anonymousCount);
                anonymousCount++;

                return anonymousIdCount;
            }

            
        }

        // 최상위 레벨 컨텍스트
        class RootContext : CallableContext
        {
            R.ModuleName moduleName;
            ItemValueFactory itemValueFactory;
            List<R.Stmt> topLevelStmts;

            public RootContext(R.ModuleName moduleName, ItemValueFactory itemValueFactory)
            {
                this.moduleName = moduleName;
                this.itemValueFactory = itemValueFactory;
                this.topLevelStmts = new List<R.Stmt>();
            }
            
            public RootContext(RootContext other, CloneContext cloneContext)
                : base(other, cloneContext)
            {
                this.moduleName = other.moduleName;
                this.itemValueFactory = cloneContext.GetClone(other.itemValueFactory);
                this.topLevelStmts = new List<R.Stmt>(other.topLevelStmts);
            }
            
            // moduleName, itemValueFactory
            // Clone호출하는 쪽에서 ItemValueFactory를 어떻게 찾는가!
            public override CallableContext Clone_CallableContext(CloneContext cloneContext)
            {
                return new RootContext(this, cloneContext);
            }

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return null;
            }

            public override TypeValue? GetRetTypeValue()
            {
                return itemValueFactory.Int;
            }

            public override void SetRetTypeValue(TypeValue retTypeValue)
            {
                throw new UnreachableCodeException();
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                throw new UnreachableCodeException();
            }

            public override bool IsSeqFunc()
            {
                return false;
            }

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts.Add(stmt);
            }

            public R.Script MakeScript()
            {
                return new R.Script(moduleName, GetDecls(), topLevelStmts.ToImmutableArray());
            }            

            public override R.Path.Normal GetPath()
            {
                return new R.Path.Root(moduleName);
            }
        }
        
        class FuncContext : CallableContext
        {
            DeclContext parentContext;
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            R.Name name;
            R.ParamHash paramHash;
            ImmutableArray<R.Path> typeArgs;

            public FuncContext(DeclContext parentContext, TypeValue? retTypeValue, bool bSequence, R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
            {
                this.parentContext = parentContext;
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;

                this.name = name;
                this.paramHash = paramHash;
                this.typeArgs = typeArgs;
            }

            public FuncContext(FuncContext other, CloneContext cloneContext)
            {
                this.parentContext = cloneContext.GetClone(other.parentContext);
                this.retTypeValue = other.retTypeValue;
                this.bSequence = other.bSequence;
                this.name = other.name;
                this.paramHash = other.paramHash;
                this.typeArgs = other.typeArgs;
            }

            public override CallableContext Clone_CallableContext(CloneContext cloneContext)
            {
                return new FuncContext(this, cloneContext);
            }            

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                // TODO: 지금은 InnerFunc를 구현하지 않으므로, Outside가 없다. 나중에 지원
                return null;
            }

            public override TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public override void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                throw new UnreachableCodeException();
            }

            public override bool IsSeqFunc()
            {
                return bSequence;
            }

            public override R.Path.Normal GetPath()
            {
                return parentContext.GetPath(name, paramHash, typeArgs);
            }
        }

        class LambdaContext : CallableContext
        {
            DeclContext parentDeclContext;            
            LocalContext parentLocalContext;
            R.AnonymousId anonymousId;
            TypeValue? retTypeValue;
            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;

            public LambdaContext(DeclContext parentDeclContext, LocalContext parentLocalContext, R.AnonymousId anonymousId, TypeValue? retTypeValue)
            {
                this.parentDeclContext = parentDeclContext;
                this.parentLocalContext = parentLocalContext;
                this.anonymousId = anonymousId;
                this.retTypeValue = retTypeValue;
                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, TypeValue>();
            }

            public LambdaContext(LambdaContext other, CloneContext cloneContext)
            {
                this.parentDeclContext = cloneContext.GetClone(other.parentDeclContext);
                this.parentLocalContext = cloneContext.GetClone(other.parentLocalContext);
                this.anonymousId = other.anonymousId;
                this.retTypeValue = other.retTypeValue;
                this.bCaptureThis = other.bCaptureThis;
                this.localCaptures = new Dictionary<string, TypeValue>(other.localCaptures);
            }

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return parentLocalContext.GetLocalVarInfo(varName);
            }

            public override TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public override void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public override void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                switch (lambdaCapture)
                {
                    case NoneLambdaCapture: break;
                    case ThisLambdaCapture: bCaptureThis = true; break;
                    case LocalLambdaCapture localCapture:
                        if (localCaptures.TryGetValue(localCapture.Name, out var prevType))
                            Debug.Assert(prevType.Equals(localCapture.Type));
                        else
                            localCaptures.Add(localCapture.Name, localCapture.Type);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }

            public override bool IsSeqFunc()
            {
                return false; // 아직 sequence lambda 기능이 없으므로 
            }

            public ImmutableArray<R.TypeAndName> GetCapturedLocalVars()
            {
                return localCaptures.Select(localCapture =>
                {
                    var name = localCapture.Key;
                    var type = localCapture.Value.GetRPath();
                    return new R.TypeAndName(type, name);
                }).ToImmutableArray();
            }

            public bool NeedCaptureThis()
            {
                return bCaptureThis;
            }

            public override R.Path.Normal GetPath()
            {
                return parentDeclContext.GetPath(new R.Name.Anonymous(anonymousId), R.ParamHash.None, default);
            }

            public override CallableContext Clone_CallableContext(CloneContext context)
            {
                return new LambdaContext(this, context);
            }
        }
    }
}