using Gum.Collections;
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
        abstract class CallableContext : DeclContext
        {
            int lambdaCount;

            // TODO: 이름 수정, Lambda가 아니라 Callable
            public abstract LocalVarInfo? GetLocalVarOutsideLambda(string varName);
            public abstract TypeValue? GetRetTypeValue();
            public abstract void SetRetTypeValue(TypeValue retTypeValue);
            public abstract void AddLambdaCapture(LambdaCapture lambdaCapture);
            public abstract bool IsSeqFunc();

            public CallableContext()
            {
                lambdaCount = 0;
            }

            public void AddLambdaDecl(R.Path? capturedThisType, ImmutableArray<R.TypeAndName> capturedLocalVars, ImmutableArray<R.ParamInfo> paramInfos, R.Stmt body)
            {
                var lambdaId = new R.LambdaId(lambdaCount);
                var capturedStmt = new R.CapturedStatement(capturedThisType, capturedLocalVars, body);
                var decl = new R.LambdaDecl(lambdaId, capturedStmt, paramInfos);

                AddDecl(decl);

                lambdaCount++;
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
        }
        
        class FuncContext : CallableContext
        {
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부

            public FuncContext(TypeValue? retTypeValue, bool bSequence)
            {
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;
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
        }

        class LambdaContext : CallableContext
        {
            LocalContext parentContext;
            TypeValue? retTypeValue;
            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;

            public LambdaContext(LocalContext parentContext, TypeValue? retTypeValue)
            {
                this.parentContext = parentContext;
                this.retTypeValue = retTypeValue;
                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, TypeValue>();
            }

            public override LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return parentContext.GetLocalVarInfo(varName);
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
                    var type = localCapture.Value.GetRType();
                    return new R.TypeAndName(type, name);
                }).ToImmutableArray();
            }

            public bool NeedCaptureThis()
            {
                return bCaptureThis;
            }
        }
    }
}