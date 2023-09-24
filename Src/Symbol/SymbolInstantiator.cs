using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    public struct SymbolInstantiator : IDeclSymbolNodeVisitor<ISymbolNode>
    {
        SymbolFactory factory;        
        ISymbolNode? outer;
        ImmutableArray<IType> typeArgs;

        SymbolInstantiator(SymbolFactory factory, ISymbolNode? outer, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.typeArgs = typeArgs;
        }

        public static ITypeSymbol Instantiate(SymbolFactory factory, ISymbolNode outer, ITypeDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            var instantiator = new SymbolInstantiator(factory, outer, typeArgs);
            return (ITypeSymbol)((IDeclSymbolNode)decl).Accept<SymbolInstantiator, ISymbolNode>(ref instantiator);
        }

        public static ISymbolNode InstantiateOpen(SymbolFactory factory, ISymbolNode? outerSymbol, IDeclSymbolNode declSymbol)
        {
            int baseTypeParamCount = outerSymbol?.GetTotalTypeParamCount() ?? 0;

            int typeParamCount = declSymbol.GetTypeParamCount();
            var typeArgsBuilder = ImmutableArray.CreateBuilder<IType>(typeParamCount);
            for (int i = 0; i < typeParamCount; i++)
            {   
                var typeVar = new TypeVarType(baseTypeParamCount + i, declSymbol.GetTypeParam(i));
                typeArgsBuilder.Add(typeVar);
            }
            var typeArgs = typeArgsBuilder.MoveToImmutable();

            return Instantiate(factory, outerSymbol, declSymbol, typeArgs);
        }
        
        public static ISymbolNode Instantiate(SymbolFactory factory, ISymbolNode? outer, IDeclSymbolNode decl, ImmutableArray<IType> typeArgs)
        {
            var instantiator = new SymbolInstantiator(factory, outer, typeArgs);
            return decl.Accept<SymbolInstantiator, ISymbolNode>(ref instantiator);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitClass(ClassDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            return factory.MakeClass(outer, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitClassConstructor(ClassConstructorDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeClassConstructor(@class, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitClassMemberFunc(ClassMemberFuncDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);

            return factory.MakeClassMemberFunc(@class, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitClassMemberVar(ClassMemberVarDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeClassMemberVar(@class, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitEnum(EnumDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            return factory.MakeEnum(outer, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitEnumElem(EnumElemDeclSymbol decl)
        {
            var @enum = outer as EnumSymbol;
            Debug.Assert(@enum != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeEnumElem(@enum, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol decl)
        {
            var enumElem = outer as EnumElemSymbol;
            Debug.Assert(enumElem != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeEnumElemMemberVar(enumElem, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitGlobalFunc(GlobalFuncDeclSymbol decl)
        {
            var topLevelOuter = outer as ITopLevelSymbolNode;
            Debug.Assert(topLevelOuter != null);

            return factory.MakeGlobalFunc(topLevelOuter, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitInterface(InterfaceDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            return factory.MakeInterface(outer, decl, typeArgs);
        }
        
        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitModule(ModuleDeclSymbol decl)
        {
            Debug.Assert(outer == null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeModule(decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitNamespace(NamespaceDeclSymbol decl)
        {
            var topLevelOuter = outer as ITopLevelSymbolNode;
            Debug.Assert(topLevelOuter != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeNamespace(topLevelOuter, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitStruct(StructDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            return factory.MakeStruct(outer, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitStructConstructor(StructConstructorDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeStructConstructor(@struct, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitStructMemberFunc(StructMemberFuncDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);

            return factory.MakeStructMemberFunc(@struct, decl, typeArgs);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitStructMemberVar(StructMemberVarDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeStructMemberVar(@struct, decl);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitLambda(LambdaDeclSymbol declSymbol)
        {
            var outerFunc = outer as IFuncSymbol;
            Debug.Assert(outerFunc != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeLambda(outerFunc, declSymbol);
        }

        ISymbolNode IDeclSymbolNodeVisitor<ISymbolNode>.VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol)
        {
            var outerLambda = outer as LambdaSymbol;
            Debug.Assert(outerLambda != null);
            Debug.Assert(typeArgs.IsEmpty);

            return factory.MakeLambdaMemberVar(outerLambda, declSymbol);
        }
    }
}
