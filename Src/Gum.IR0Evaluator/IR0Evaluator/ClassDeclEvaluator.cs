using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0.IR0Factory;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        struct ClassDeclEvaluator
        {
            GlobalContext globalContext;
            ItemContainer classContainer;

            R.ClassDecl classDecl;
            R.Path.Nested classPath;
            int totalTypeParamCount;
            List<R.Path> memberVarTypes;

            public static void Eval(GlobalContext globalContext, ItemContainer parentContainer, R.Path.Normal curPath, int totalTypeParamCount, R.ClassDecl classDecl)
            {
                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, classDecl.TypeParams.Length);
                var classPath = curPath.Child(classDecl.Name, new R.ParamHash(classDecl.TypeParams.Length, default), typeArgs);

                var classContainer = new ItemContainer();
                var evaluator = new ClassDeclEvaluator(globalContext, classPath, totalTypeParamCount + classDecl.TypeParams.Length, classContainer, classDecl);

                foreach (var constructorDecl in classDecl.ConstructorDecls)
                    evaluator.EvalClassConstructorDecl(constructorDecl);

                foreach (var memberFuncDecl in classDecl.MemberFuncDecls)
                    evaluator.EvalClassMemberFuncDecl(memberFuncDecl);

                foreach (var memberVarDecl in classDecl.MemberVarDecls)
                    evaluator.EvalClassMemberVarDecl(memberVarDecl);

                parentContainer.AddItemContainer(classDecl.Name, new R.ParamHash(classDecl.TypeParams.Length, default), classContainer);
                parentContainer.AddRuntimeItem(new IR0ClassRuntimeItem(globalContext, classDecl, evaluator.memberVarTypes.ToImmutableArray()));
            }

            ClassDeclEvaluator(GlobalContext globalContext, R.Path.Nested classPath, int totalTypeParamCount, ItemContainer classContainer, R.ClassDecl classDecl)
            {
                this.globalContext = globalContext;
                this.classContainer = classContainer;
                this.classPath = classPath;
                this.totalTypeParamCount = totalTypeParamCount;
                this.classDecl = classDecl;
                this.memberVarTypes = new List<R.Path>();
            }

            void EvalClassMemberVarDecl(R.ClassMemberVarDecl varDecl)
            {
                foreach (var name in varDecl.Names)
                {
                    var memberVarItem = new IR0ClassMemberVarRuntimeItem(globalContext, classPath, name, memberVarTypes.Count);

                    memberVarTypes.Add(varDecl.Type);
                    classContainer.AddRuntimeItem(memberVarItem);
                }
            }

            void EvalClassConstructorDecl(R.ClassConstructorDecl constructorDecl)
            {
                // NOTICE: Constructor는 typeParamCount가 0 이다
                var paramHash = Misc.MakeParamHash(typeParamCount: 0, constructorDecl.Parameters);

                IR0ConstructorRuntimeItem runtimeItem;
                if (constructorDecl.BaseCallInfo != null)
                {
                    var baseConstructorPath = classDecl.BaseClass!.Child(R.Name.Constructor.Instance, constructorDecl.BaseCallInfo.Value.ParamHash, default);

                    // with base constructor call
                    runtimeItem = new IR0ConstructorRuntimeItem(
                        globalContext, R.Name.Constructor.Instance, paramHash, constructorDecl.Parameters,
                        baseConstructorPath, constructorDecl.BaseCallInfo.Value.Args, constructorDecl.Body);
                }
                else
                {
                    runtimeItem = new IR0ConstructorRuntimeItem(globalContext, R.Name.Constructor.Instance, paramHash, constructorDecl.Parameters, constructorDecl.Body);
                }

                var constructorContainer = new ItemContainer();
                classContainer.AddItemContainer(R.Name.Constructor.Instance, paramHash, constructorContainer);
                classContainer.AddRuntimeItem(runtimeItem);

                var constructorPath = classPath.Child(R.Name.Constructor.Instance, paramHash, default);

                DeclEvaluator.EvalCallableMemberDecls(globalContext, constructorContainer, constructorPath, totalTypeParamCount, constructorDecl.CallableMemberDecls);
            }           

            void EvalClassMemberFuncDecl(R.ClassMemberFuncDecl memberFuncDecl)
            {
                var funcDecl = memberFuncDecl.FuncDecl;
                DeclEvaluator.EvalFuncDecl(globalContext, classContainer, classPath, totalTypeParamCount, funcDecl);
            }
        }
    }
}
