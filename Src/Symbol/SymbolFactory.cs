using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    // DeclSymbolFactory는 따로 만들자
    [ExcludeComparison]
    public class SymbolFactory
    {
        // Decl류는 한개만 존재하고, Class (instance)류는 매번 생성한다.
        // instance류는 속성 변경을 허용하지 않는다(되더라도 지역적으로만 전파되기 때문에 의미없다)

        public SymbolFactory()
        {

        }

        #region Global

        public ModuleSymbol MakeModule(ModuleDeclSymbol decl)
        {
            return new ModuleSymbol(this, decl);
        }

        public NamespaceSymbol MakeNamespace(ITopLevelSymbolNode outer, NamespaceDeclSymbol decl)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());

            return new NamespaceSymbol(this, outer, decl);
        }

        public GlobalFuncSymbol MakeGlobalFunc(ITopLevelSymbolNode outer, GlobalFuncDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new GlobalFuncSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Class

        public ClassSymbol MakeClass(ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new ClassSymbol(this, outer, decl, typeArgs);
        }

        public ClassConstructorSymbol MakeClassConstructor(ClassSymbol @class, ClassConstructorDeclSymbol decl)
        {
            Debug.Assert(@class.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new ClassConstructorSymbol(this, @class, decl);
        }

        public ClassMemberVarSymbol MakeClassMemberVar(ClassSymbol @class, ClassMemberVarDeclSymbol decl)
        {
            Debug.Assert(@class.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new ClassMemberVarSymbol(this, @class, decl);
        }

        public ClassMemberFuncSymbol MakeClassMemberFunc(ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new ClassMemberFuncSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Interface

        public InterfaceSymbol MakeInterface(ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new InterfaceSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Struct

        public StructSymbol MakeStruct(ISymbolNode outer, StructDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new StructSymbol(this, outer, decl, typeArgs);
        }
        
        public StructConstructorSymbol MakeStructConstructor(StructSymbol @struct, StructConstructorDeclSymbol decl)
        {
            Debug.Assert(@struct.GetDeclSymbolNode() == decl.GetOuterDeclNode());

            return new StructConstructorSymbol(this, @struct, decl);
        }

        public StructMemberFuncSymbol MakeStructMemberFunc(StructSymbol @struct, StructMemberFuncDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(@struct.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new StructMemberFuncSymbol(this, @struct, decl, typeArgs);
        }
        
        public StructMemberVarSymbol MakeStructMemberVar(StructSymbol @struct, StructMemberVarDeclSymbol decl)
        {
            Debug.Assert(@struct.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new StructMemberVarSymbol(this, @struct, decl);
        }

        #endregion
        

        #region Enum

        public EnumSymbol MakeEnum(ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new EnumSymbol(this, outer, decl, typeArgs);
        }

        public EnumElemSymbol MakeEnumElem(EnumSymbol @enum, EnumElemDeclSymbol decl)
        {
            Debug.Assert(@enum.GetDeclSymbolNode() == decl.GetOuterDeclNode());

            return new EnumElemSymbol(this, @enum, decl);
        }

        public EnumElemMemberVarSymbol MakeEnumElemMemberVar(EnumElemSymbol enumElem, EnumElemMemberVarDeclSymbol decl)
        {
            Debug.Assert(enumElem.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new EnumElemMemberVarSymbol(this, enumElem, decl);
        }

        #endregion

        #region Lambda
        public LambdaSymbol MakeLambda(IFuncSymbol outer, LambdaDeclSymbol declSymbol)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == declSymbol.GetOuterDeclNode());
            return new LambdaSymbol(this, outer, declSymbol);
        }

        public LambdaMemberVarSymbol MakeLambdaMemberVar(LambdaSymbol outer, LambdaMemberVarDeclSymbol declSymbol)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == declSymbol.GetOuterDeclNode());
            return new LambdaMemberVarSymbol(this, outer, declSymbol);
        }

        #endregion
    }
}
