using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Collections;
using Gum.Infra;
using Pretune;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class ModuleSymbol : ITopLevelSymbolNode
    {
        SymbolFactory factory;
        ModuleDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal ModuleSymbol(SymbolFactory factory, ModuleDeclSymbol decl)
        {
            this.factory = factory;
            this.decl = decl;
        }

        public ITopLevelSymbolNode Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath()
        {
            var name = decl.GetName();
            return (name, null);
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
        {
            // NOTICE: 타입, 함수 간의 이름 충돌이 일어나지 않는다고 가정한다, ClassSymbol, StructSymbol도 마찬가지.
            // 타입, 함수순으로 검색하고 검색결과가 나오면 바로 리턴한다
            var memberTypeDecl = decl.GetType(memberName, typeParamCount);
            if (memberTypeDecl != null)
                return SymbolQueryResultBuilder.Build(memberTypeDecl, this, factory);

            var builder = ImmutableArray.CreateBuilder<Func<ImmutableArray<ITypeSymbol>, GlobalFuncSymbol>>();
            foreach (var memberFunc in decl.GetFuncs(memberName, typeParamCount))
            {
                builder.Add(typeArgs => factory.MakeGlobalFunc(this, memberFunc, typeArgs));
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count != 0)
                return new SymbolQueryResult.GlobalFuncs(builder.ToImmutable());

            return SymbolQueryResult.NotFound.Instance;
        }
    }
}
