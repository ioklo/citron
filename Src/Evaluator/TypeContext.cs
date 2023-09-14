using System;

using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

namespace Citron
{
    // 현재 실행하고 있는 도중의 타입 environment
    // body-space에서 존재, 
    // 최종적으로 TypeVar가 모두 치환되는 형태여야 한다
    // call할때 만들어질 것 같다(call할 대상의 SymbolId와, 타입인자로 확정)
    // 
    // TODO: IR0Evaluator로 옮긴다? => IR0한정 타입일 수도 있으므로?
    public class TypeContext
    {
        // index -> type
        ImmutableArray<TypeId> env;
        public static readonly TypeContext Empty = new TypeContext(default);

        TypeContext(ImmutableArray<TypeId> env)
        {
            this.env = env;
        }

        static void InnerMake(SymbolPath? path, ImmutableArray<TypeId>.Builder builder)
        {
            if (path == null) return;

            InnerMake(path.Outer, builder);
            
            foreach (var typeArg in path.TypeArgs)
                builder.Add(typeArg);
        }

        public static TypeContext Make(SymbolPath? path)
        {
            var builder = ImmutableArray.CreateBuilder<TypeId>();
            InnerMake(path, builder);
            return new TypeContext(builder.ToImmutable());
        }

        // G<List<T>>, T => int
        // Make((G<>, [(List<>, [T])]), [T => int])
        // => [(List<>, [int])]
        public static TypeContext Make(TypeId typeId, TypeContext typeContext)
        {
            return Make(typeContext.Apply(typeId));
        }
        
        // Module.System.NS1.NS2.Type<int, bool>.Type2<short>.Func<string>
        public static TypeContext Make(TypeId typeId)
        {
            switch(typeId)
            {
                case SymbolTypeId symbolTypeId: return Make(symbolTypeId.SymbolId.Path);
                default: throw new NotImplementedException();
            }
        }

        public TypeId Apply(TypeId typeId)
        {
            var applier = new Applier(env);
            return typeId.Accept<Applier, TypeId>(ref applier);
        }

        public SymbolTypeId ApplySymbol(SymbolId symbolId)
        {
            return new Applier(env).ApplySymbol(symbolId);
        }

        struct Applier : ITypeIdVisitor<TypeId>
        {
            ImmutableArray<TypeId> env;

            public Applier(ImmutableArray<TypeId> env)
            {
                this.env = env;
            }

            public SymbolTypeId ApplySymbol(SymbolId typeId)
            {
                var appliedPath = ApplySymbolPath(typeId.Path);
                return new SymbolTypeId(new SymbolId(typeId.ModuleName, appliedPath));
            }

            SymbolPath? ApplySymbolPath(SymbolPath? path)
            {
                if (path == null) return null;

                var appliedOuter = ApplySymbolPath(path.Outer);

                var builder = ImmutableArray.CreateBuilder<TypeId>(path.TypeArgs.Length);
                foreach(var typeArg in path.TypeArgs)
                {
                    var appliedTypeArg = typeArg.Accept<Applier, TypeId>(ref this); // 뭘 저장안하니까 그냥 쓰자
                    builder.Add(appliedTypeArg);
                }

                var appliedTypeArgs = builder.MoveToImmutable();
                return new SymbolPath(appliedOuter, path.Name, appliedTypeArgs, path.ParamIds);
            }
            
            TypeId ITypeIdVisitor<TypeId>.VisitFunc(FuncTypeId typeId)
            {
                throw new NotImplementedException();
            }

            TypeId ITypeIdVisitor<TypeId>.VisitLambda(LambdaTypeId typeId)
            {
                throw new NotImplementedException();
            }

            TypeId ITypeIdVisitor<TypeId>.VisitLocalPtr(LocalPtrTypeId typeId)
            {
                var appliedId = typeId.InnerTypeId.Accept<Applier, TypeId>(ref this);
                return new LocalPtrTypeId(appliedId);
            }

            TypeId ITypeIdVisitor<TypeId>.VisitBoxPtr(BoxPtrTypeId typeId)
            {
                var appliedId = typeId.InnerTypeId.Accept<Applier, TypeId>(ref this);
                return new BoxPtrTypeId(appliedId);
            }
            
            TypeId ITypeIdVisitor<TypeId>.VisitNullable(NullableTypeId typeId)
            {
                var appliedId = typeId.InnerTypeId.Accept<Applier, TypeId>(ref this);
                return new NullableTypeId(appliedId);
            }

            TypeId ITypeIdVisitor<TypeId>.VisitSymbol(SymbolTypeId typeId)
            {
                return ApplySymbol(typeId.SymbolId);
            }

            TypeId ITypeIdVisitor<TypeId>.VisitTuple(TupleTypeId typeId)
            {
                var builder = ImmutableArray.CreateBuilder<TupleMemberVarId>(typeId.MemberVarIds.Length);
                foreach (var memberVarId in typeId.MemberVarIds)
                {
                    var appliedMemberVarId = new TupleMemberVarId(memberVarId.TypeId.Accept<Applier, TypeId>(ref this), memberVarId.Name);
                    builder.Add(appliedMemberVarId);
                }

                return new TupleTypeId(builder.MoveToImmutable());
            }

            TypeId ITypeIdVisitor<TypeId>.VisitTypeVar(TypeVarTypeId typeId)
            {
                return env[typeId.Index];
            }

            TypeId ITypeIdVisitor<TypeId>.VisitVoid(VoidTypeId typeId)
            {
                return typeId;
            }
        }
    }
}