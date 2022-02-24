using Citron.Collections;
using M = Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pretune;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class NamespaceSymbol : ITopLevelSymbolNode
    {
        SymbolFactory factory;
        ITopLevelSymbolNode outer;
        NamespaceDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

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
            return outer;
        }        

        public (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath()
        {
            var (module, outerNamespacePath) = outer.GetRootPath();
            var name = decl.GetName();

            return (module, new M.NamespacePath(outerNamespacePath, name));
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

            var builder = ImmutableArray.CreateBuilder<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>>();
            foreach (var memberFunc in decl.GetFuncs(memberName, typeParamCount))
            {
                var dac = new DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>(
                    memberFunc,
                    typeArgs => factory.MakeGlobalFunc(this, memberFunc, typeArgs)
                );

                builder.Add(dac);
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count != 0)
                return new SymbolQueryResult.GlobalFuncs(builder.ToImmutable());

            return SymbolQueryResult.NotFound.Instance;
        }
    }
}
