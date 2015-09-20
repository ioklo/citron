using Gum.Lang.AbstractSyntax;
using Gum.Test.Metadata;
using Gum.Test.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test
{
    class BuildMetadata // : IFileUnitComponentVisitor, INamespaceComponentVisitor, IMemberComponentVisitor
    {   
        Environment namespaceEnv;        

        internal static void Build(GumMetadata metadata, IEnumerable<IMetadata> externMetadatas, FileUnit fileUnit)
        {
            var builder = new BuildMetadata(metadata, externMetadatas);
            builder.Visit(fileUnit);
        }

        public BuildMetadata(GumMetadata metadata, IEnumerable<IMetadata> externMetadatas)
        {
            this.namespaceEnv = new Environment(metadata, externMetadatas);            
        }

        public void Visit(FileUnit fileUnit)
        {
            foreach (var comp in fileUnit.Comps)
                Visit(comp as dynamic);
        }

        public void Visit(UsingDirective usingDirective)
        {
            // using이 있으면 검색할 네임스페이스를 늘려주는 기능을 합니다
            namespaceEnv.AddUsing(usingDirective.Names);
        }

        // 네임스페이스 declaration입니다
        public void Visit(NamespaceDecl namespaceDecl)
        {
            namespaceEnv.PushNamespace(namespaceDecl.Names);            

            foreach (var comp in namespaceDecl.Comps)
                Visit(comp as dynamic);

            namespaceEnv.PopNamespace();
        }

        public void Visit(VarDecl varDecl)
        {            
            // VarDef 생성
            foreach (var nameAndExp in varDecl.NameAndExps)
            {
                // idWithTypeVar로 Type만들기
                var type = namespaceEnv.GetType(varDecl.Type);
                namespaceEnv.CurNamespace.AddVariable(type, nameAndExp.VarName);
            }
        }

        public void Visit(FuncDecl funcDecl)
        {
            namespaceEnv.PushFuncTypeVars(funcDecl.TypeVars);

            var returnType = namespaceEnv.GetType(funcDecl.ReturnType);
            var argTypes = funcDecl.Parameters.Select(arg => namespaceEnv.GetType(arg.Type));

            namespaceEnv.CurNamespace.AddFunction(funcDecl.Name, funcDecl.TypeVars.Count(), returnType, argTypes);

            namespaceEnv.PopTypeArgs();
        }

        public void Visit(ClassDecl classDecl)
        {
            // 미리 만들어져 있었다
            var typeDef = namespaceEnv.CurNamespace.GetTypeDef(classDecl.Name, classDecl.TypeVars.Count());

            namespaceEnv.PushObjectTypeVars(classDecl.TypeVars);                

            foreach (var memberComp in classDecl.Components)
                Visit(typeDef, memberComp as dynamic);

            namespaceEnv.PopTypeArgs();
        }

        public void Visit(StructDecl structDecl)
        {
            var typeDef = namespaceEnv.CurNamespace.GetTypeDef(structDecl.Name, structDecl.TypeVars.Count());

            namespaceEnv.PushObjectTypeVars(structDecl.TypeVars);            

            foreach (var memberComp in structDecl.Components)
                Visit(typeDef, memberComp as dynamic);

            namespaceEnv.PopTypeArgs();
        }

        public void Visit(TypeDef typeDef, MemberFuncDecl memberFuncDecl)
        {
            // TODO: static function 지원은 추후에
            var returnType = namespaceEnv.GetType(memberFuncDecl.ReturnType);
            var paramTypes = memberFuncDecl.Parameters.Select(arg => namespaceEnv.GetType(arg.Type));
            
            typeDef.AddFunc(memberFuncDecl.Name, memberFuncDecl.TypeParams.Count(), returnType, paramTypes);
        }

        public void Visit(TypeDef typeDef, MemberVarDecl memberVarDecl)
        {
            var varType = namespaceEnv.GetType(memberVarDecl.Type);
            foreach( var name in memberVarDecl.Names)
                typeDef.AddVar(varType, name);
        }
    }
}
