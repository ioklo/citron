using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // syntax를 다 돌고 난 뒤의 후처리
    class TypeBuilder : IQueryModuleTypeDecl
    {
        Dictionary<M.TypeDeclId, ClassDeclSymbol> classes;
        Dictionary<M.TypeDeclId, StructDeclSymbol> structs;

        public TypeBuilder()
        {
            classes = new Dictionary<M.TypeDeclId, ClassDeclSymbol>();
            structs = new Dictionary<M.TypeDeclId, StructDeclSymbol>();
        }

        public void AddClass(M.TypeDeclId declId, ClassDeclSymbol classInfo)
        {   
            classes.Add(declId, classInfo);
        }

        public void AddStruct(M.TypeDeclId declId, StructDeclSymbol structInfo)
        {
            structs.Add(declId, structInfo);
        }

        ClassSymbol IQueryModuleTypeDecl.GetClass(M.TypeId typeId)
        {
            var cls = classes[declId];
            cls.EnsureSetBaseAndBuildTrivialConstructor(this);

            // TODO: external 처리
            return cls;
        }

        StructSymbol IQueryModuleTypeDecl.GetStruct(M.TypeId declId)
        {
            var s = structs[declId];
            s.EnsureSetBaseAndBuildTrivialConstructor(this);

            return s;
        }
        
        public void SetBasesAndBuildTrivialConstructors()
        {
            foreach(var c in classes.Values)
                c.EnsureSetBaseAndBuildTrivialConstructor(this);

            foreach (var s in structs.Values)
                s.EnsureSetBaseAndBuildTrivialConstructor(this);
        }
    }

    
}