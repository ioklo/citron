using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public abstract partial class FuncDecl
    {
        public FuncDeclId Id { get; }
        public bool IsThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<string> ParamNames { get; }
        public Stmt Body { get; }

        internal FuncDecl(FuncDeclId id,
                bool bThisCall,
                ImmutableArray<string> typeParams,
                ImmutableArray<string> paramNames,
                Stmt body)
        {
            Id = id;
            IsThisCall = bThisCall;

            TypeParams = typeParams;
            ParamNames = paramNames;
            Body = body;
        }        
    }
    
    public class NormalFuncDecl : FuncDecl, IEquatable<NormalFuncDecl>
    {
        public NormalFuncDecl(
            FuncDeclId id,
            bool bThisCall,
            ImmutableArray<string> typeParams,
            ImmutableArray<string> paramNames,
            Stmt body)
            : base(id, bThisCall, typeParams, paramNames, body)
        {
        }

        public override bool Equals(object? obj) => Equals(obj as NormalFuncDecl);
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(NormalFuncDecl? other)
        {
            return base.Equals(other);
        }
    }

    public class SequenceFuncDecl : FuncDecl
    {
        public Type ElemType { get; }

        public SequenceFuncDecl(FuncDeclId id, Type elemType, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<string> paramNames, Stmt body)
            : base(id, bThisCall, typeParams, paramNames, body)
        {
            ElemType = elemType;
        }

        public override bool Equals(object? obj) => Equals(obj as SequenceFuncDecl);
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(SequenceFuncDecl? other)
        {
            return base.Equals(other);
        }
    }
}
