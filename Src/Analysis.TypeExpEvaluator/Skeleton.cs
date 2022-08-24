﻿using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using S = Citron.Syntax;
using M = Citron.Module;
using Pretune;
using Citron.Symbol;

namespace Citron.Analysis
{
    // TypeExpEvaluator에서 사용할 임시 구조체    

    // mutable
    class Skeleton
    {
        public SkeletonKind Kind { get; }
        public M.Name Name { get; }
        public int TypeParamCount { get; }
        
        public int FuncIndex { get; } // 같은 이름의 함수일 경우 구분자, 나머지는 0이다
        public S.ISyntaxNode? FuncNode { get; } // Kind가 func 종류일때만 유효하다
        
        Dictionary<(M.Name, int), List<Skeleton>> membersByName;
        Dictionary<S.ISyntaxNode, Skeleton> membersByFuncDecl;

        public Skeleton(SkeletonKind kind, M.Name name, int typeParamCount, int funcIndex, S.ISyntaxNode? funcNode)
        {
            Kind = kind;

            Name = name;
            TypeParamCount = typeParamCount;

            FuncIndex = funcIndex;
            FuncNode = funcNode;

            membersByName = new Dictionary<(M.Name, int), List<Skeleton>>();
            membersByFuncDecl = new Dictionary<S.ISyntaxNode, Skeleton>();
        }

        // TODO: 같은 이름이 추가되면 에러를 내야 한다, 테스트 작성도 하자
        // 근데 구현 장소는 여기가 아니다 (타입, 함수 등 한꺼번에 검사)
        public Skeleton AddMember(SkeletonKind kind, M.Name name, int typeParamCount, S.ISyntaxNode? funcNode = null)
        {
            var key = (name, typeParamCount);
            
            if (!membersByName.TryGetValue(key, out var members))
            {
                members = new List<Skeleton>();
                membersByName.Add(key, members);
            }
            
            var skel = new Skeleton(kind, name, typeParamCount, members.Count, funcNode);
            members.Add(skel);

            if (funcNode != null)
                membersByFuncDecl.Add(funcNode, skel);

            return skel;
        }

        // get member
        public Skeleton? GetMember(M.Name name, int typeParamCount, int funcIndex)
        {
            var key = (name, typeParamCount);

            if (membersByName.TryGetValue(key, out var members))
                if (funcIndex < members.Count)
                    return members[funcIndex];

            return null;
        }

        public Skeleton? GetFuncDeclMember(S.ISyntaxNode node)
        {
            return membersByFuncDecl.GetValueOrDefault(node);
        }
    }
}
