using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    // DeclSymbolFactory는 따로 만들자
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

        public GlobalFuncSymbol MakeGlobalFunc(ITopLevelSymbolNode outer, GlobalFuncDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new GlobalFuncSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Class

        public ClassSymbol MakeClass(ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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

        public ClassMemberFuncSymbol MakeClassMemberFunc(ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new ClassMemberFuncSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Tuple
        public TupleSymbol MakeTuple(ImmutableArray<TupleMemberVarSymbol> memberVars)
        {
            return new TupleSymbol(this, memberVars);
        }

        public TupleMemberVarSymbol MakeTupleMemberVar(IHolder<TupleSymbol> outerHolder, ITypeSymbol declType, string? name, int index)
        {
            return new TupleMemberVarSymbol(this, outerHolder, declType, name, index);
        }
        #endregion

        #region Interface

        public InterfaceSymbol MakeInterface(ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            Debug.Assert(decl.GetTypeParamCount() == typeArgs.Length);

            return new InterfaceSymbol(this, outer, decl, typeArgs);
        }

        #endregion

        #region Struct

        public StructSymbol MakeStruct(ISymbolNode outer, StructDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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

        public StructMemberFuncSymbol MakeStructMemberFunc(StructSymbol @struct, StructMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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

        public EnumSymbol MakeEnum(ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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

        public LambdaSymbol MakeLambda(ISymbolNode outer, LambdaDeclSymbol decl)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new LambdaSymbol(this, outer, decl);
        }

        public LambdaMemberVarSymbol MakeLambdaMemberVar(LambdaSymbol outer, LambdaMemberVarDeclSymbol decl)
        {
            Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new LambdaMemberVarSymbol(this, outer, decl);
        }
        #endregion

        #region TypeVar
        public TypeVarSymbol MakeTypeVar(TypeVarDeclSymbol decl)
        {
            // outer가 확정이 안되었을 수 있어서 assert를 시킬 수 없다
            // Debug.Assert(outer.GetDeclSymbolNode() == decl.GetOuterDeclNode());
            return new TypeVarSymbol(decl);
        }

        #endregion

        #region Special

        public VarSymbol MakeVar()
        {
            return new VarSymbol();
        }

        public VoidSymbol MakeVoid()
        {
            return new VoidSymbol();
        }

        public NullableSymbol MakeNullable(ITypeSymbol innerType)
        {
            return new NullableSymbol(this, innerType);
        }

        #endregion
    }
}
