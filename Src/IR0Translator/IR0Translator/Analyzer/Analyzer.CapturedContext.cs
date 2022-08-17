using Citron.Collections;
using Citron.Infra;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Citron.Analysis;

using R = Citron.IR0;
using M = Citron.Module;
using System;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        class CapturedContext : ICallableContext
        {
            ITypeSymbol? thisType; // possible,
            LocalContext parentLocalContext;
            ITypeSymbol? retType;
            bool bCaptureThis;
            Dictionary<string, ITypeSymbol> localCaptures;
            ImmutableArray<R.CallableMemberDecl> decls;
            LambdaIdComponent AnonymousIdComponent;

            public CapturedContext(ITypeSymbol? thisType, LocalContext parentLocalContext, ITypeSymbol? retType)
            {
                this.thisType = thisType;
                this.parentLocalContext = parentLocalContext;
                this.retType = retType;
                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, ITypeSymbol>();
            }

            public CapturedContext(CapturedContext other, CloneContext cloneContext)
            {
                this.parentLocalContext = cloneContext.GetClone(other.parentLocalContext);
                this.retType = other.retType;
                this.bCaptureThis = other.bCaptureThis;
                this.localCaptures = new Dictionary<string, ITypeSymbol>(other.localCaptures);
                this.decls = other.decls;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext context)
            {
                return new CapturedContext(this, context);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_callableContext, UpdateContext updateContext)
            {
                var src = (CapturedContext)src_callableContext;
                updateContext.Update(this.parentLocalContext, src.parentLocalContext);

                this.localCaptures.Clear();
                foreach (var keyValue in src.localCaptures)
                    this.localCaptures.Add(keyValue.Key, keyValue.Value);
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            { 
                return parentLocalContext.GetLocalVarInfo(varName);
            }

            public FuncReturn GetReturn()
            {
                if (retType == null)
                    throw new NotImplementedException();

                return new FuncReturn(false, retType);
            }

            public void SetRetType(ITypeSymbol retTypeValue)
            {
                this.retType = retTypeValue;
            }

            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType)
            {
                if (localCaptures.TryGetValue(capturedVarName, out var prevType))
                    Debug.Assert(prevType.Equals(capturedVarType));
                else
                    localCaptures.Add(capturedVarName, capturedVarType);
            }

            public bool IsSeqFunc()
            {
                return false; // 아직 sequence lambda 기능이 없으므로
            }

            public ImmutableArray<(ITypeSymbol DeclType, M.Name VarName)> GetCapturedLocalVars()
            {
                return localCaptures.Select(localCapture => (localCapture.Value, (M.Name)new M.Name.Normal(localCapture.Key))).ToImmutableArray();
            }

            public bool NeedCaptureThis()
            {
                return bCaptureThis;
            }

            public ITypeSymbol? GetThisType() 
            { 
                return thisType;
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl)
            {
                decls = decls.Add(decl);
            }

            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls()
            {
                return decls;
            }

            public R.Name.Anonymous NewAnonymousName()
            {
                return AnonymousIdComponent.NewAnonymousName();
            }

            public ImmutableArray<FuncParameter> GetParameters()
            {
                throw new NotImplementedException();
            }
        }
    }
}