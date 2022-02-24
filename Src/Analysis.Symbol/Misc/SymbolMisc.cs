using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    class SymbolMisc
    {
        public static ClassConstructorDeclSymbol? GetConstructorHasSameParamWithTrivial(
            ClassConstructorSymbol? baseConstructor,
            ImmutableArray<ClassConstructorDeclSymbol> constructors, 
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            // 생성자 중, 파라미터가 같은 것이 있는지 확인
            foreach (var constructor in constructors)
            {
                if (IsMatchTrivialConstructorParameters(baseConstructor, constructor, memberVars))
                    return constructor;
            }

            return null;
        }
        
        // NOTICE: comparing with baseConstructor and constructor'Decl'
        public static bool IsMatchTrivialConstructorParameters(ClassConstructorSymbol? baseConstructor, ClassConstructorDeclSymbol constructorDecl, ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            int baseParamCount = baseConstructor != null ? baseConstructor.GetParameterCount() : 0;
            int paramCount = constructorDecl.GetParameterCount();
            
            if (memberVars.Length != paramCount) return false;

            // constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
            for (int i = 0; i < baseParamCount; i++)
            {
                Debug.Assert(baseConstructor != null);

                var baseParameter = baseConstructor.GetParameter(i);
                var parameter = constructorDecl.GetParameter(i);

                if (!baseParameter.Equals(parameter)) return false;

                // 추가 조건, normal로만 자동으로 생성한다
                if (baseParameter.Kind != M.FuncParameterKind.Default ||
                    parameter.Kind != M.FuncParameterKind.Default) return false;
            }

            // baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
            for (int i = 0; i < paramCount; i++)
            {
                var memberVarType = memberVars[i].GetDeclType();
                var parameter = constructorDecl.GetParameter(i + baseParamCount);

                // 타입을 비교해서 같지 않다면 제외
                if (!parameter.Type.Equals(memberVarType)) return false;

                // 기본으로만 자동으로 생성한다
                if (parameter.Kind != M.FuncParameterKind.Default) return false;
            }

            return true;
        }

        public static ClassConstructorDeclSymbol MakeTrivialConstructorDecl(
            ClassDeclSymbol outer,
            ClassConstructorSymbol? baseConstructor,
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars)
        {
            int totalParamCount = (baseConstructor?.GetParameterCount() ?? 0) + memberVars.Length;
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

                    // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
                    var newBaseParam = new FuncParameter(baseParam.Kind, baseParam.Type, new M.Name.ConstructorParam(i));
                    builder.Add(newBaseParam);
                }
            }

            foreach(var memberVar in memberVars)
            {
                var type = memberVar.GetDeclType();
                var name = memberVar.GetName();

                var param = new FuncParameter(M.FuncParameterKind.Default, type, name);
                builder.Add(param);
            }

            // trivial constructor를 만듭니다
            return new ClassConstructorDeclSymbol(new Holder<ClassDeclSymbol>(outer), M.AccessModifier.Public, new Holder<ImmutableArray<FuncParameter>>(builder.MoveToImmutable()), true);
        }        

        

        
    }
}
