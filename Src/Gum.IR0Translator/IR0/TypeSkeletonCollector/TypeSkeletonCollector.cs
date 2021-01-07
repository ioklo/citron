using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using static Gum.IR0.AnalyzeErrorCode;
using S = Gum.Syntax;

namespace Gum.IR0
{
    class TypeSkeletonCollector
    {
        List<Skeleton> globalSkeletons;
        Skeleton.Type? curSkeleton;

        public TypeSkeletonCollector()
        {
            globalSkeletons = new List<Skeleton>();
            curSkeleton = null;
        }
        public void ExecInNewEnumScope(S.EnumDecl enumDecl, Action action)
            => ExecInNewTypeScope(enumDecl, path => new Skeleton.Enum(path, enumDecl), action);

        public void ExecInNewStructScope(S.StructDecl structDecl, Action action)
            => ExecInNewTypeScope(structDecl, path => new Skeleton.Struct(path, structDecl), action);

        void ExecInNewTypeScope<TSkeleton>(S.TypeDecl typeDecl, Func<ItemPath, TSkeleton> constructor, Action action)
            where TSkeleton : Skeleton.Type
        {
            var prevSkeleton = curSkeleton;
            curSkeleton = AddType(typeDecl, constructor);

            try
            {
                action.Invoke();
            }
            finally
            {
                curSkeleton = prevSkeleton;
            }
        }

        public void AddFunc(S.FuncDecl funcDecl)
        {
            // NOTICE: paramHash는 추후에 계산이 된다 => 테스트 추가할 것
            if (curSkeleton != null)
            {
                var funcPath = curSkeleton.Path.Append(funcDecl.Name, funcDecl.TypeParams.Length);
                var funcSkeleton = new Skeleton.Func(funcPath, funcDecl);
                curSkeleton.AddMember(funcSkeleton);
            }
            else
            {
                // TODO: NamespaceRoot가 아니라 namespace 선언 상황에 따라 달라진다
                var funcPath = new ItemPath(NamespacePath.Root, new ItemPathEntry(funcDecl.Name, funcDecl.TypeParams.Length));
                var funcSkeleton = new Skeleton.Func(funcPath, funcDecl);
                globalSkeletons.Add(funcSkeleton);
            }
        }

        public void AddVar(Name name, S.VarDecl varDecl, int elemIndex)
        {
            if (curSkeleton != null)
            {
                var varPath = curSkeleton.Path.Append(name);
                var varSkeleton = new Skeleton.Var(varPath, varDecl, elemIndex);
                curSkeleton.AddMember(varSkeleton);
            }
            else
            {
                var varPath = new ItemPath(NamespacePath.Root, new ItemPathEntry(name));
                var varSkeleton = new Skeleton.Var(varPath, varDecl, elemIndex);
                globalSkeletons.Add(varSkeleton);
            }
        }
        
        TSkeleton AddType<TSkeleton>(S.TypeDecl decl, Func<ItemPath, TSkeleton> constructor)
            where TSkeleton : Skeleton.Type
        {
            ItemPath typePath = (curSkeleton != null)
                ? curSkeleton.Path.Append(decl.Name, decl.TypeParamCount)
                : new ItemPath(NamespacePath.Root, new ItemPathEntry(decl.Name, decl.TypeParamCount)); // TODO: NamespaceRoot가 아니라 namespace 선언 상황에 따라 달라진다

            var typeSkeleton = constructor.Invoke(typePath);

            if (curSkeleton != null)
                curSkeleton.AddMember(typeSkeleton);
            else
                globalSkeletons.Add(typeSkeleton);

            return typeSkeleton;
        }

        public IEnumerable<Skeleton> GetGlobalSkeletons()
        {
            return globalSkeletons;
        }
    }
}
