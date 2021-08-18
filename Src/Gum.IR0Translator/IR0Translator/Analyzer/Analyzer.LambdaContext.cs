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
        class LambdaContext : ICallableContext
        {
            R.Path.Nested path;
            NormalTypeValue? thisTypeValue; // possible,
            LocalContext parentLocalContext;
            TypeValue? retTypeValue;
            bool bCaptureThis;
            Dictionary<string, TypeValue> localCaptures;
            ImmutableArray<R.CallableMemberDecl> decls;
            AnonymousIdComponent AnonymousIdComponent;

            public LambdaContext(R.Path.Nested path, NormalTypeValue? thisTypeValue, LocalContext parentLocalContext, TypeValue? retTypeValue)
            {
                this.path = path;
                this.thisTypeValue = thisTypeValue;
                this.parentLocalContext = parentLocalContext;
                this.retTypeValue = retTypeValue;
                this.bCaptureThis = false;
                this.localCaptures = new Dictionary<string, TypeValue>();
            }

            public LambdaContext(LambdaContext other, CloneContext cloneContext)
            {
                this.path = other.path;
                this.parentLocalContext = cloneContext.GetClone(other.parentLocalContext);
                this.retTypeValue = other.retTypeValue;
                this.bCaptureThis = other.bCaptureThis;
                this.localCaptures = new Dictionary<string, TypeValue>(other.localCaptures);
                this.decls = other.decls;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext context)
            {
                return new LambdaContext(this, context);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_callableContext, UpdateContext updateContext)
            {
                var src = (LambdaContext)src_callableContext;
                updateContext.Update(this.parentLocalContext, src.parentLocalContext);

                this.localCaptures.Clear();
                foreach (var keyValue in src.localCaptures)
                    this.localCaptures.Add(keyValue.Key, keyValue.Value);
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            { 
                return parentLocalContext.GetLocalVarInfo(varName);
            }

            public TypeValue? GetRetTypeValue()
            {
                return retTypeValue;
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                this.retTypeValue = retTypeValue;
            }

            public void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType)
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

            public ImmutableArray<R.OuterLocalVarInfo> GetCapturedLocalVars()
            {
                return localCaptures.Select(localCapture =>
                {
                    var name = localCapture.Key;
                    var type = localCapture.Value.GetRPath();
                    return new R.OuterLocalVarInfo(type, name);
                }).ToImmutableArray();
            }

            public bool NeedCaptureThis()
            {
                return bCaptureThis;
            }

            public R.Path.Normal GetPath()
            {   
                return path;
            }

            public NormalTypeValue? GetThisTypeValue() 
            { 
                return thisTypeValue; 
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
        }
    }
}