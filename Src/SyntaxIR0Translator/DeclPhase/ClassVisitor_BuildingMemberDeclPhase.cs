using System;

using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;
using System.Diagnostics;

namespace Citron.Analysis
{
    struct ClassVisitor_BuildingMemberDeclPhase
    {
        ClassDeclSymbol declSymbol;
        BuildingMemberDeclPhaseContext context;

        public ClassVisitor_BuildingMemberDeclPhase(ClassDeclSymbol declSymbol, BuildingMemberDeclPhaseContext context)
        {
            this.declSymbol = declSymbol;
            this.context = context;
        }

        void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl syntax)
        {
            var accessModifier = BuilderMisc.MakeClassMemberAccessor(syntax.AccessModifier);
            var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);

            var func = new ClassMemberFuncDeclSymbol(declSymbol, accessModifier, new Name.Normal(syntax.Name), typeParams, syntax.IsStatic);
            declSymbol.AddFunc(func);

            var (funcReturn, funcParams) = context.MakeFuncReturnAndParams(func, syntax.IsRefReturn, syntax.RetType, syntax.Parameters);
            func.InitFuncReturnAndParams(funcReturn, funcParams);
        }

        void VisitClassMemberVarDecl(S.ClassMemberVarDecl syntax)
        {
            foreach (var name in syntax.VarNames)
            {
                var accessModifier = BuilderMisc.MakeClassMemberAccessor(syntax.AccessModifier);
                
                var declType = context.MakeType(syntax.VarType, declSymbol);

                // TODO: bStatic 지원
                var varDecl = new ClassMemberVarDeclSymbol(declSymbol, accessModifier, bStatic: false, declType, new Name.Normal(name));
                declSymbol.AddMemberVar(varDecl);
            }
        }

        void VisitClassConstructorDecl(S.ClassConstructorDecl syntax)
        {
            var accessor = BuilderMisc.MakeClassMemberAccessor(syntax.AccessModifier);

            // Constructor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(class) declSymbol을 넘긴다
            var parameters = BuilderMisc.MakeParameters(declSymbol, context, syntax.Parameters);
            
            // TODO: syntax에 trivial 마킹하면 검사하고 trivial로 만든다
            var constructorDeclSymbol = new ClassConstructorDeclSymbol(declSymbol, accessor, parameters, bTrivial: false);
            declSymbol.AddConstructor(constructorDeclSymbol);
        }

        static bool IsMatchClassTrivialConstructorParameters(ClassConstructorSymbol? baseConstructor, ClassDeclSymbol classDeclSymbol, ClassConstructorDeclSymbol constructorDecl)
        {
            int baseParamCount = baseConstructor != null ? baseConstructor.GetParameterCount() : 0;
            int paramCount = constructorDecl.GetParameterCount();
            int memberVarCount = classDeclSymbol.GetMemberVarCount();

            if (memberVarCount != paramCount) return false;

            // constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
            for (int i = 0; i < baseParamCount; i++)
            {
                Debug.Assert(baseConstructor != null);

                var baseParameter = baseConstructor.GetParameter(i);
                var parameter = constructorDecl.GetParameter(i);

                if (!baseParameter.Equals(parameter)) return false;

                // 추가 조건, normal로만 자동으로 생성한다
                if (baseParameter.Kind != FuncParameterKind.Default ||
                    parameter.Kind != FuncParameterKind.Default) return false;
            }

            // baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
            for (int i = 0; i < paramCount; i++)
            {
                var memberVarType = classDeclSymbol.GetMemberVar(i).GetDeclType();
                var parameter = constructorDecl.GetParameter(i + baseParamCount);

                // 타입을 비교해서 같지 않다면 제외
                if (!parameter.Type.Equals(memberVarType)) return false;

                // 기본으로만 자동으로 생성한다
                if (parameter.Kind != FuncParameterKind.Default) return false;
            }

            return true;
        }

        // baseConstructor가 만들어졌다는 가정하에 짜여짐 (인자에서 요구하는 형태)
        static ClassConstructorDeclSymbol MakeClassTrivialConstructorDecl(
            ClassDeclSymbol outer,
            ClassConstructorSymbol? baseConstructor)
        {
            int memberVarCount = outer.GetMemberVarCount();
            int totalParamCount = (baseConstructor?.GetParameterCount() ?? 0) + memberVarCount;
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(totalParamCount);

            // to prevent conflict between parameter names, using special name $'base'_<name>_index
            // class A { A(int x) {} }
            // class B : A { B(int $base_x0, int x) : base($base_x0) { } }
            // class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
            if (baseConstructor != null)
            {
                for (int i = 0; i < baseConstructor.GetParameterCount(); i++)
                {
                    var baseParam = baseConstructor.GetParameter(i);
                    var paramName = BuilderMisc.MakeBaseConstructorParamName(i, baseParam.Name);

                    // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
                    var newBaseParam = new FuncParameter(baseParam.Kind, baseParam.Type, paramName);
                    builder.Add(newBaseParam);
                }
            }

            for(int i = 0; i < memberVarCount; i++)            
            {
                var memberVar = outer.GetMemberVar(i);
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new FuncParameter(FuncParameterKind.Default, type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new ClassConstructorDeclSymbol(outer, Accessor.Public, builder.MoveToImmutable(), bTrivial: true);
        }

        static ClassConstructorDeclSymbol? GetClassConstructorHasSameParamWithTrivial(
            ClassConstructorSymbol? baseConstructor,
            ClassDeclSymbol classDeclSymbol)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            int count = classDeclSymbol.GetConstructorCount();

            for(int i = 0; i < count; i++)
            {
                var constructor = classDeclSymbol.GetConstructor(i);
            
                if (IsMatchClassTrivialConstructorParameters(baseConstructor, classDeclSymbol, constructor))
                    return constructor;
            }

            return null;
        }


        public void VisitClassDecl(S.ClassDecl syntax)
        {
            ClassType? uniqueBaseClass = null; // TODO: Object로 대체
            var interfacesBuilder = ImmutableArray.CreateBuilder<InterfaceType>();

            foreach (var baseTypeSyntax in syntax.BaseTypes)
            {
                var baseType = context.MakeType(baseTypeSyntax, declSymbol);
                if (baseType is ClassType baseClassType)
                {
                    if (uniqueBaseClass != null)
                        throw new NotImplementedException(); // 베이스 클래스는 하나여야 합니다

                    uniqueBaseClass = baseClassType;
                }
                else if (baseType is InterfaceType interfaceBaseType)
                {
                    interfacesBuilder.Add(interfaceBaseType);
                }
                else
                {
                    throw new NotImplementedException(); // 클래스 또는 인터페이스가 와야 합니다
                }
            }

            declSymbol.InitBaseTypes(uniqueBaseClass, interfacesBuilder.ToImmutable());

            foreach(var memberDecl in syntax.MemberDecls)
            {
                switch(memberDecl)
                {
                    case S.ClassMemberFuncDecl funcMember:                        
                        VisitClassMemberFuncDecl(funcMember);
                        break;
                        

                    case S.ClassMemberVarDecl varMember:
                        VisitClassMemberVarDecl(varMember);
                        break;

                    case S.ClassConstructorDecl constructorMember:
                        VisitClassConstructorDecl(constructorMember);
                        break;
                }
            }

            var capturedDeclSymbol = this.declSymbol;
            context.AddBuildingTrivialConstructorPhaseTask(uniqueBaseClass?.Symbol.GetDecl(), declSymbol, () =>
            {
                var baseTrivialConstructor = uniqueBaseClass?.Symbol.GetTrivialConstructor();
                ClassConstructorDeclSymbol? trivialConstructor = null;

                // baseStruct가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
                // baseStruct가 있고, TrivialConstructor가 있는 경우 => 진행
                // baseStruct가 없는 경우 => 없이 만들고 진행 
                if (baseTrivialConstructor != null || uniqueBaseClass == null)
                {
                    // 같은 인자의 생성자가 없으면 Trivial을 만든다
                    if (GetClassConstructorHasSameParamWithTrivial(baseTrivialConstructor, capturedDeclSymbol) == null)
                    {
                        trivialConstructor = MakeClassTrivialConstructorDecl(capturedDeclSymbol, baseTrivialConstructor);
                        capturedDeclSymbol.AddConstructor(trivialConstructor);
                    }
                }
            });
        }
    }
}