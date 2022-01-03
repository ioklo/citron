using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Analysis
{
    // DeclSymbolFactory는 따로 만들자
    class SymbolFactory
    {
        // Decl류는 한개만 존재하고, Class (instance)류는 매번 생성한다.
        // instance류는 속성 변경을 허용하지 않는다(되더라도 지역적으로만 전파되기 때문에 의미없다)

        public ClassSymbol MakeClass(ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new ClassSymbol(this, outer, decl, typeArgs);
        }

        public ClassConstructorSymbol MakeClassConstructor(ClassSymbol @class, ClassConstructorDeclSymbol decl)
        {
            return new ClassConstructorSymbol(this, @class, decl);
        }

        internal ClassMemberVarSymbol MakeClassMemberVar(ClassSymbol @class, ClassMemberVarDeclSymbol decl)
        {
            return new ClassMemberVarSymbol(this, @class, decl);
        }

        public ClassMemberFuncSymbol MakeClassMemberFunc(ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new ClassMemberFuncSymbol(this, outer, decl, typeArgs);
        }

        public StructSymbol MakeStruct(ISymbolNode outer, StructDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new StructSymbol(this, outer, decl, typeArgs);
        }

        public StructConstructorSymbol MakeStructConstructor(StructSymbol @struct, StructConstructorDeclSymbol decl)
        {
            return new StructConstructorSymbol(this, @struct, decl);
        }

        public EnumSymbol MakeEnum(ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new EnumSymbol(this, outer, decl, typeArgs);
        }

        public GlobalFuncSymbol MakeGlobalFunc(ITopLevelSymbolNode outer, GlobalFuncDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new GlobalFuncSymbol(this, outer, decl, typeArgs);
        }

        public StructMemberFuncSymbol MakeStructMemberFunc(StructSymbol @struct, StructMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new StructMemberFuncSymbol(this, @struct, decl, typeArgs);
        }

        public EnumElemSymbol MakeEnumElem(EnumSymbol @enum, EnumElemDeclSymbol decl)
        {
            return new EnumElemSymbol(this, @enum, decl);
        }

        public EnumElemMemberVarSymbol MakeEnumElemMemberVar(EnumElemSymbol enumElem, EnumElemMemberVarDeclSymbol decl)
        {
            return new EnumElemMemberVarSymbol(this, enumElem, decl);
        }

        public StructMemberVarSymbol MakeStructMemberVar(StructSymbol @struct, StructMemberVarDeclSymbol decl)
        {
            return new StructMemberVarSymbol(this, @struct, decl);
        }
    }
}
