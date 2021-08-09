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

            R.Path.Nested classPath;
            int totalTypeParamCount;
            List<R.Path> memberVarTypes;

            public static void Eval(GlobalContext globalContext, ItemContainer parentContainer, R.Path.Normal curPath, int totalTypeParamCount, R.ClassDecl classDecl)
            {
                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, classDecl.TypeParams.Length);
                var classPath = curPath.Child(classDecl.Name, new R.ParamHash(classDecl.TypeParams.Length, default), typeArgs);

                var classContainer = new ItemContainer();
                var evaluator = new ClassDeclEvaluator(globalContext, classPath, totalTypeParamCount + classDecl.TypeParams.Length, classContainer);

                foreach (var memberDecl in classDecl.MemberDecls)
                {
                    evaluator.EvalMemberDecl(memberDecl);
                }

                parentContainer.AddItemContainer(classDecl.Name, new R.ParamHash(classDecl.TypeParams.Length, default), classContainer);
                parentContainer.AddRuntimeItem(new IR0ClassRuntimeItem(globalContext, classDecl, evaluator.memberVarTypes.ToImmutableArray()));
            }

            ClassDeclEvaluator(GlobalContext globalContext, R.Path.Nested classPath, int totalTypeParamCount, ItemContainer classContainer)
            {
                this.globalContext = globalContext;
                this.classContainer = classContainer;
                this.classPath = classPath;
                this.totalTypeParamCount = totalTypeParamCount;
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
                var runtimeItem = new IR0ConstructorRuntimeItem(globalContext, R.Name.Constructor.Instance, paramHash, constructorDecl.Parameters, constructorDecl.Body);

                var constructorContainer = new ItemContainer();
                classContainer.AddItemContainer(R.Name.Constructor.Instance, paramHash, constructorContainer);
                classContainer.AddRuntimeItem(runtimeItem);

                var constructorPath = classPath.Child(R.Name.Constructor.Instance, paramHash, default);

                DeclEvaluator.EvalDecls(globalContext, constructorContainer, constructorPath, totalTypeParamCount, constructorDecl.Decls);
            }

            void EvalClassMemberFuncDecl(R.ClassMemberFuncDecl funcDecl)
            {
                var paramHash = Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.Parameters);
                var runtimeItem = new IR0ClassFuncRuntimeItem(globalContext, funcDecl);

                var memberFuncContainer = new ItemContainer();
                classContainer.AddItemContainer(funcDecl.Name, paramHash, memberFuncContainer);
                classContainer.AddRuntimeItem(runtimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, funcDecl.TypeParams.Length);
                var funcPath = classPath.Child(funcDecl.Name, paramHash, typeArgs);
                DeclEvaluator.EvalDecls(globalContext, memberFuncContainer, funcPath, totalTypeParamCount + funcDecl.TypeParams.Length, funcDecl.Decls);
            }

            void EvalClassMemberSeqFuncDecl(R.ClassMemberSeqFuncDecl seqFuncDecl)
            {
                var paramHash = Misc.MakeParamHash(seqFuncDecl.TypeParams.Length, seqFuncDecl.Parameters);
                var runtimeItem = new IR0ClassSeqFuncRuntimeItem(globalContext, seqFuncDecl);

                var memberSeqFuncContainer = new ItemContainer();
                classContainer.AddItemContainer(seqFuncDecl.Name, paramHash, memberSeqFuncContainer);
                classContainer.AddRuntimeItem(runtimeItem);

                var typeArgs = MakeTypeArgsByTypeParams(totalTypeParamCount, seqFuncDecl.TypeParams.Length);
                var seqFuncPath = classPath.Child(seqFuncDecl.Name, paramHash, typeArgs);

                DeclEvaluator.EvalDecls(globalContext, memberSeqFuncContainer, seqFuncPath, totalTypeParamCount + seqFuncDecl.TypeParams.Length, seqFuncDecl.Decls);
            }

            void EvalMemberDecl(R.ClassMemberDecl memberDecl)
            {
                switch (memberDecl)
                {
                    case R.ClassMemberVarDecl varDecl:
                        EvalClassMemberVarDecl(varDecl);
                        break;

                    case R.ClassConstructorDecl constructorDecl:
                        EvalClassConstructorDecl(constructorDecl);
                        break;

                    case R.ClassMemberFuncDecl funcDecl:
                        EvalClassMemberFuncDecl(funcDecl);
                        break;

                    case R.ClassMemberSeqFuncDecl seqFuncDecl:
                        EvalClassMemberSeqFuncDecl(seqFuncDecl);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }

        }
    }
}
