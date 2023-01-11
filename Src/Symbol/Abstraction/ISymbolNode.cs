using Citron.Collections;
using Citron.Infra;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Citron.Symbol
{
    // 자식으로 가는 줄기는 없다
    public interface ISymbolNode : ICyclicEqualityComparableClass<ISymbolNode>
    {
        // 순회
        ISymbolNode? GetOuter();
        IDeclSymbolNode GetDeclSymbolNode();
        
        TypeEnv GetTypeEnv();
        ISymbolNode Apply(TypeEnv typeEnv);        
        IType GetTypeArg(int i);

        void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor;
    }

    public static class SymbolNodeExtensions
    {
        public static int GetTotalTypeParamCount(this ISymbolNode symbol)
        {
            var decl = symbol.GetDeclSymbolNode();
            return decl.GetTotalTypeParamCount();
        }

        public static bool CanAccess(this ISymbolNode user, ISymbolNode target)
        {
            var targetDecl = target.GetDeclSymbolNode();
            var userDecl = user.GetDeclSymbolNode();

            return userDecl.CanAccess(targetDecl);
        }

        // declSymbol을 가지고 type var를 그대로 인자에 넣는 symbol을 생성한다
        public static ISymbolNode MakeOpenSymbol(this IDeclSymbolNode declSymbol, SymbolFactory factory)
        {
            var outerDeclSymbol = declSymbol.GetOuterDeclNode();
            var outerSymbol = (outerDeclSymbol == null) ? null : MakeOpenSymbol(outerDeclSymbol, factory);

            return SymbolInstantiator.InstantiateOpen(factory, outerSymbol, declSymbol);
        }

        // TODO: 
        public static SymbolId GetSymbolId(this ISymbolNode symbol)
        {
            var outer = symbol.GetOuter();
            if (outer != null)
            {
                var outerId = outer.GetSymbolId();
                var decl = symbol.GetDeclSymbolNode();
                var declName = decl.GetNodeName();

                int typeParamCount = decl.GetTypeParamCount();
                var typeArgIdsBuilder = ImmutableArray.CreateBuilder<TypeId>(typeParamCount);
                for (int i = 0; i < typeParamCount; i++)
                {
                    var typeArg = symbol.GetTypeArg(i);
                    typeArgIdsBuilder.Add(typeArg.GetTypeId());
                }

                return outerId.Child(declName.Name, typeArgIdsBuilder.MoveToImmutable(), declName.ParamIds);
            }
            else
            {
                Debug.Assert(symbol is ModuleSymbol);
                var declSymbol = symbol.GetDeclSymbolNode();
                var nodeName = declSymbol.GetNodeName();

                Debug.Assert(nodeName.TypeParamCount == 0 && nodeName.ParamIds.Length == 0);
                return new SymbolId(nodeName.Name, null);
            }
        }
    }
}