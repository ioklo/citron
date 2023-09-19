using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class ModuleSymbol : ITopLevelSymbolNode, ICyclicEqualityComparableClass<ModuleSymbol>
    {
        SymbolFactory factory;
        ModuleDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITopLevelSymbolNode ITopLevelSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal ModuleSymbol(SymbolFactory factory, ModuleDeclSymbol decl)
        {
            this.factory = factory;
            this.decl = decl;
        }

        public ModuleSymbol Apply(TypeEnv typeEnv)
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

        //public (Name Module, NamespacePath? NamespacePath) GetRootPath()
        //{
        //    var name = decl.GetName();
        //    return (name, null);
        //}

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        SymbolQueryResult? ISymbolNode.QueryMember(Name memberName, int typeParamCount)
        {
            // NOTICE: 네임스페이스, 타입, 함수 간의 이름 충돌이 일어나지 않는다고 가정한다, ClassSymbol, StructSymbol도 마찬가지.
            // 네임스페이스, 타입, 함수순으로 검색하고 검색결과가 나오면 바로 리턴한다
            if (typeParamCount == 0)
            {
                var nsDSymbol = decl.GetNamespace(memberName);
                if (nsDSymbol != null)                
                    return new SymbolQueryResult.Namespace(factory.MakeNamespace(this, nsDSymbol));
            }
            
            var memberTypeDecl = decl.GetType(memberName, typeParamCount);
            if (memberTypeDecl != null)
                return SymbolQueryResultBuilder.Build(memberTypeDecl, this, factory);

            var builder = ImmutableArray.CreateBuilder<(ISymbolNode, GlobalFuncDeclSymbol)>();
            foreach (var memberFunc in decl.GetFuncs(memberName, typeParamCount))
            {
                builder.Add((this, memberFunc));
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count != 0)
                return new SymbolQueryResult.GlobalFuncs(builder.ToImmutable());

            return null;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ModuleSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITopLevelSymbolNode>.CyclicEquals(ITopLevelSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ModuleSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ModuleSymbol>.CyclicEquals(ModuleSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ModuleSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(decl, other.decl))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(decl), decl);
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitModule(this);
        }

       
    }
}
