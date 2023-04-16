using Citron.Collections;
using Citron.IR0;
using Citron.Symbol;
using System;

namespace Citron.Analysis;

partial struct LocalRefExpVisitor
{
    record struct MemberBinder(string memberName, ImmutableArray<IType> typeArgs, ScopeContext context) : IMemberParentResultVisitor<MemberParentResult>
    {
        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitLocation(MemberParentResult.Location result)
        {
            if (result.Type is StructType structType)
            {
                if (typeArgs.Length == 0)
                {
                    var memberVar = structType.Symbol.GetMemberVar(new Name.Normal(memberName));
                    if (memberVar != null)
                    {
                        // static 인지 체크
                        if (memberVar.IsStatic())
                            throw new NotImplementedException(); // 정적변수를 인스턴스를 사용해서 사용했습니다 T.a (x)

                        return new MemberParentResult.Location(new StructMemberLoc(result.Loc, memberVar), memberVar.GetDeclType());
                    }
                }
            }
            else if (result.Type is ClassType classType)
            {
                if (typeArgs.Length == 0)
                {
                    var memberVar = classType.Symbol.GetMemberVar(new Name.Normal(memberName));
                    if (memberVar != null)
                    {
                        if (memberVar.IsStatic())
                            throw new NotImplementedException(); // 정적변수를 인스턴스를 사용해서 사용했습니다 T.a (x)

                        return new MemberParentResult.BoxRef(new ClassMemberVarBoxRefExp(result.Loc, memberVar), memberVar.GetDeclType());
                    }
                }
            }

            // 나머지는 에러, 레퍼런스로 만들 수 없습니다
            throw new NotImplementedException();
        }

        // 부모 부분이 box T& 타입의 Exp로 평가되는 경우
        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitBoxRef(MemberParentResult.BoxRef result)
        {
            if (result.DerefType is StructType structType)
            {
                if (typeArgs.Length == 0)
                {
                    var memberVar = structType.Symbol.GetMemberVar(new Name.Normal(memberName));
                    if (memberVar != null)
                    {
                        return new MemberParentResult.BoxRef(new StructMemberVarBoxRefExp(result.Exp, memberVar), memberVar.GetDeclType());
                    }
                }
            }
            else if (result.DerefType is ClassType classType) // var& x = (...).x
            {
                if (typeArgs.Length == 0)
                {
                    var memberVar = classType.Symbol.GetMemberVar(new Name.Normal(memberName));
                    if (memberVar != null)
                    {
                        return new MemberParent
                    }

                }
            }
        }

        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitValue(MemberParentResult.Value result)
        {
            throw new System.NotImplementedException();
        }

        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitNamespace(MemberParentResult.Namespace result)
        {
            throw new System.NotImplementedException();
        }

        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitClass(MemberParentResult.Class result)
        {
            var memberResult = ((ITypeSymbol)result.Symbol).QueryMember(new Name.Normal(memberName), typeArgs.Length);

            if (memberResult is SymbolQueryResult.ClassMemberVar classMemberVarResult)
            {
                var symbol = classMemberVarResult.Var;

                if (!symbol.IsStatic())
                    context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                if (!context.CanAccess(symbol))
                    context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                return new ExpResult.IR0Loc(new R.ClassMemberLoc(null, symbol), symbol.GetDeclT ype());
            }
        }

        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitStruct(MemberParentResult.Struct result)
        {
            throw new System.NotImplementedException();
        }

        MemberParentResult IMemberParentResultVisitor<MemberParentResult>.VisitThis(MemberParentResult.ThisVar result)
        {
            throw new System.NotImplementedException();
        }

        
    }
}