
using Citron.Infra;

using Citron.Symbol;

using S = Citron.Syntax;
using System.Diagnostics;
using Citron.Collections;
using System;
using Pretune;

namespace Citron.Analysis
{
    [AutoConstructor]
    partial struct StructVisitor_BuildingMemberDeclPhase
    {
        StructDeclSymbol structDeclSymbol;
        BuildingMemberDeclPhaseContext context;        

        void VisitStructMemberVarDecl(S.StructMemberVarDecl syntax)
        {
            foreach (var name in syntax.VarNames)
            {
                var accessModifier = BuilderMisc.MakeStructMemberAccessor(syntax.AccessModifier);
                var declType = context.MakeType(syntax.VarType, structDeclSymbol);

                // TODO: static지원
                var symbol = new StructMemberVarDeclSymbol(structDeclSymbol, accessModifier, bStatic: false, declType, new Name.Normal(name));
                structDeclSymbol.AddMemberVar(symbol);
            }
        }

        void VisitStructMemberFuncDecl(S.StructMemberFuncDecl syntax)
        {
            var accessor = BuilderMisc.MakeStructMemberAccessor(syntax.AccessModifier);
            var typeParams = BuilderMisc.VisitTypeParams(syntax.TypeParams);
            var declSymbol = new StructMemberFuncDeclSymbol(
                structDeclSymbol, accessor, syntax.IsStatic, new Name.Normal(syntax.Name), typeParams);

            var (funcReturn, funcParams)= context.MakeFuncReturnAndParams(declSymbol, syntax.RetType, syntax.Parameters);
            declSymbol.InitFuncReturnAndParams(funcReturn, funcParams);

            structDeclSymbol.AddFunc(declSymbol);

            context.AddBuildingBodyPhaseTask(context =>
            {   
                return StructVisitor_BuildingBodyPhase.VisitStructMemberFuncDecl(syntax.Body, context, declSymbol, bSeqFunc: syntax.IsSequence);
            });
        }

        void VisitStructConstructorDecl(S.StructConstructorDecl syntax)
        {
            var accessModifier = BuilderMisc.MakeStructMemberAccessor(syntax.AccessModifier);

            // Constructor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(struct) declSymbol을 넘긴다
            var parameters = BuilderMisc.MakeParameters(structDeclSymbol, context, syntax.Parameters);

            // TODO: 타이프 쳐서 만들어진 constructor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다
            // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
            var declSymbol = new StructConstructorDeclSymbol(structDeclSymbol, accessModifier, parameters, bTrivial: false, bLastParameterVariadic: false);
            structDeclSymbol.AddConstructor(declSymbol);

            context.AddBuildingBodyPhaseTask(context =>
            {   
                return StructVisitor_BuildingBodyPhase.VisitStructConstructorDecl(syntax.Body, context, declSymbol);
            });
        }

        public void VisitStructDecl(S.StructDecl syntax)
        {
            foreach (var memberDecl in syntax.MemberDecls)
            {
                switch(memberDecl)
                {
                    case S.StructMemberFuncDecl memberFuncDecl:
                        VisitStructMemberFuncDecl(memberFuncDecl);
                        break;

                    case S.StructMemberVarDecl memberVarDecl:
                        VisitStructMemberVarDecl(memberVarDecl);
                        break;

                    case S.StructConstructorDecl constructorDecl:
                        VisitStructConstructorDecl(constructorDecl);
                        break;
                }
            }

            StructType? uniqueBaseType = null; // 없다면 null일수도 있다. init으로는 넘겨줘야 한다
            var interfaceTypesBuilder = ImmutableArray.CreateBuilder<InterfaceType>();
            foreach(var baseTypeSyntax in syntax.BaseTypes)
            {
                var baseType = context.MakeType(baseTypeSyntax, this.structDeclSymbol);
                
                if (baseType is StructType structBaseType)
                {
                    if (uniqueBaseType != null)
                        throw new NotImplementedException(); // 두개 이상의 struct를 상속받았다면

                    uniqueBaseType = structBaseType;
                }
                else if (baseType is InterfaceType interfaceBaseType)
                {
                    interfaceTypesBuilder.Add(interfaceBaseType);
                }
                else
                {
                    // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
                    throw new NotImplementedException();
                }
            }

            this.structDeclSymbol.InitBaseTypes(uniqueBaseType, interfaceTypesBuilder.ToImmutable());

            // 다음은 constructor 빌드 단계
            var structDeclSymbol = this.structDeclSymbol;

            context.AddBuildingTrivialConstructorPhaseTask(uniqueBaseType?.GetDeclSymbol(), structDeclSymbol, () =>
            {
                var baseTrivialConstructor = uniqueBaseType?.GetTrivialConstructor();

                // baseStruct가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
                // baseStruct가 있고, TrivialConstructor가 있는 경우 => 진행
                // baseStruct가 없는 경우 => 없이 만들고 진행 
                if (baseTrivialConstructor != null || uniqueBaseType == null)
                {
                    // 같은 인자의 생성자가 없으면 Trivial을 만든다
                    if (GetStructConstructorHasSameParamWithTrivial(baseTrivialConstructor, structDeclSymbol) == null)
                    {
                        var trivialConstructor = MakeStructTrivialConstructorDecl(structDeclSymbol, baseTrivialConstructor);
                        structDeclSymbol.AddConstructor(trivialConstructor);
                    }
                }
            });
        }

        // NOTICE: comparing with baseConstructor and constructor'Decl'
        static bool IsMatchStructTrivialConstructorParameters(StructConstructorSymbol? baseConstructor, StructDeclSymbol declSymbol, StructConstructorDeclSymbol constructorDecl)
        {
            int baseParamCount = baseConstructor != null ? baseConstructor.GetParameterCount() : 0;
            int paramCount = constructorDecl.GetParameterCount();
            int memberVarCount = declSymbol.GetMemberVarCount();

            if (memberVarCount != paramCount) return false;

            // constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
            for (int i = 0; i < baseParamCount; i++)
            {
                Debug.Assert(baseConstructor != null);

                var baseParameter = baseConstructor.GetParameter(i);
                var parameter = constructorDecl.GetParameter(i);

                if (!baseParameter.Equals(parameter)) return false;
            }

            // baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
            for (int i = 0; i < paramCount; i++)
            {
                var memberVarType = declSymbol.GetMemberVar(i).GetDeclType();
                var parameter = constructorDecl.GetParameter(i + baseParamCount);

                // 타입을 비교해서 같지 않다면 제외
                if (!parameter.Type.Equals(memberVarType)) return false;
            }

            return true;
        }

        static StructConstructorDeclSymbol? GetStructConstructorHasSameParamWithTrivial(
            StructConstructorSymbol? baseConstructor,
            StructDeclSymbol decl)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            int constructorCount = decl.GetConstructorCount();
            for (int i = 0; i < constructorCount; i++)
            {
                var constructor = decl.GetConstructor(i);
            
                if (IsMatchStructTrivialConstructorParameters(baseConstructor, decl, constructor))
                    return constructor;
            }

            return null;
        }

        // baseConstructor가 만들어졌다는 가정하에 짜여짐 (인자에서 요구하는 형태)
        static StructConstructorDeclSymbol MakeStructTrivialConstructorDecl(
            StructDeclSymbol declSymbol,
            StructConstructorSymbol? baseConstructor)
        {
            int memberVarCount = declSymbol.GetMemberVarCount();
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
                    var newBaseParam = new FuncParameter(baseParam.Type, paramName);
                    builder.Add(newBaseParam);
                }
            }

            for(int i = 0; i < memberVarCount; i++)            
            {
                var memberVar = declSymbol.GetMemberVar(i);
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new FuncParameter(type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new StructConstructorDeclSymbol(declSymbol, Accessor.Public, builder.MoveToImmutable(), bTrivial: true, bLastParameterVariadic: false);
        }
    }
}