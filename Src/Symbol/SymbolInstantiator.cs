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
    public class SymbolInstantiator : IDeclSymbolNodeVisitor, ITypeDeclSymbolVisitor
    {
        SymbolFactory factory;        
        ISymbolNode? outer;
        ImmutableArray<IType> typeArgs;

        ISymbolNode? result;

        SymbolInstantiator(SymbolFactory factory, ISymbolNode? outer, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.typeArgs = typeArgs;
            this.result = null;
        }

        public static ITypeSymbol Instantiate(SymbolFactory factory, ISymbolNode outer, ITypeDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            var instantiator = new SymbolInstantiator(factory, outer, typeArgs);
            decl.Accept(instantiator);

            var typeSymbolResult = instantiator.result as ITypeSymbol;
            Debug.Assert(typeSymbolResult != null);
            return typeSymbolResult;
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
            decl.Accept(instantiator);

            Debug.Assert(instantiator.result != null);
            return instantiator.result;
        }

        public void VisitClass(ClassDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            result = factory.MakeClass(outer, decl, typeArgs);
        }

        public void VisitClassConstructor(ClassConstructorDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeClassConstructor(@class, decl);
        }

        public void VisitClassMemberFunc(ClassMemberFuncDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);

            result = factory.MakeClassMemberFunc(@class, decl, typeArgs);
        }

        public void VisitClassMemberVar(ClassMemberVarDeclSymbol decl)
        {
            var @class = outer as ClassSymbol;
            Debug.Assert(@class != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeClassMemberVar(@class, decl);
        }

        public void VisitEnum(EnumDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            result = factory.MakeEnum(outer, decl, typeArgs);
        }

        public void VisitEnumElem(EnumElemDeclSymbol decl)
        {
            var @enum = outer as EnumSymbol;
            Debug.Assert(@enum != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeEnumElem(@enum, decl);
        }

        public void VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol decl)
        {
            var enumElem = outer as EnumElemSymbol;
            Debug.Assert(enumElem != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeEnumElemMemberVar(enumElem, decl);
        }

        public void VisitGlobalFunc(GlobalFuncDeclSymbol decl)
        {
            var topLevelOuter = outer as ITopLevelSymbolNode;
            Debug.Assert(topLevelOuter != null);

            result = factory.MakeGlobalFunc(topLevelOuter, decl, typeArgs);
        }

        public void VisitInterface(InterfaceDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            result = factory.MakeInterface(outer, decl, typeArgs);
        }
        
        public void VisitModule(ModuleDeclSymbol decl)
        {
            Debug.Assert(outer == null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeModule(decl);
        }

        public void VisitNamespace(NamespaceDeclSymbol decl)
        {
            var topLevelOuter = outer as ITopLevelSymbolNode;
            Debug.Assert(topLevelOuter != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeNamespace(topLevelOuter, decl);
        }

        public void VisitStruct(StructDeclSymbol decl)
        {
            Debug.Assert(outer != null);
            result = factory.MakeStruct(outer, decl, typeArgs);
        }

        public void VisitStructConstructor(StructConstructorDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeStructConstructor(@struct, decl);
        }

        public void VisitStructMemberFunc(StructMemberFuncDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);

            result = factory.MakeStructMemberFunc(@struct, decl, typeArgs);
        }

        public void VisitStructMemberVar(StructMemberVarDeclSymbol decl)
        {
            var @struct = outer as StructSymbol;
            Debug.Assert(@struct != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeStructMemberVar(@struct, decl);
        }

        public void VisitLambda(LambdaDeclSymbol declSymbol)
        {
            var outerFunc = outer as IFuncSymbol;
            Debug.Assert(outerFunc != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeLambda(outerFunc, declSymbol);
        }

        public void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol)
        {
            var outerLambda = outer as LambdaSymbol;
            Debug.Assert(outerLambda != null);
            Debug.Assert(typeArgs.IsEmpty);

            result = factory.MakeLambdaMemberVar(outerLambda, declSymbol);
        }
    }
}
