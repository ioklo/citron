using System;
using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using static Citron.Symbol.Name;
using static Citron.Symbol.SymbolQueryResult;
using R = Citron.IR0;

namespace Citron.Analysis;

// Sytnax.Exp를 바로 번역했을 경우 생기는 날것의 결과 
abstract record class ExpResult
{
    // 카테고리
    public record class Valid : ExpResult;
    public record class NotFound : ExpResult
    {
        internal NotFound() { }
    }

    public record class Error : ExpResult
    { 
        public string Text { get; init; }
        internal Error(string text) { this.Text = text; }
    }

    #region TopLevel
    public record class Namespace : Valid;
    public record class GlobalVar(bool IsRef, IType Type, Name VarName) : Valid;

    // TypeArgsForMatch: partial
    public record class GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos, ImmutableArray<IType> TypeArgsForMatch) : Valid;
    #endregion

    #region Body
    public record class TypeVar(TypeVarType Type) : Valid;
    public record class ThisVar(IType Type) : Valid;
    public record class LocalVar(bool IsRef, IType Type, Name Name) : Valid;
    public record class LambdaMemberVar(LambdaMemberVarSymbol MemberVar) : Valid;
    #endregion

    #region Class
    public record class Class(ClassSymbol Symbol) : Valid;

    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    public record ClassMemberFuncs(
        ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos, 
        ImmutableArray<IType> ParitalTypeArgs, 
        bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    public record ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;

    #endregion

    #region Struct
    public record class Struct(StructSymbol Symbol) : Valid;
    public record class StructMemberFuncs(
        ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos,
        ImmutableArray<IType> ParitalTypeArgs, 
        bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, R.Loc? ExplicitInstance) : Valid;
    #endregion

    #region Enum
    public record class Enum(EnumSymbol Symbol) : Valid;
    public record class EnumElem(EnumElemSymbol Symbol) : Valid;
    public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, R.Loc Instance) : Valid; // e.x
    #endregion

    // 기타의 경우
    public record class IR0Exp(R.Exp Exp) : ExpResult;
    public record class IR0Loc(R.Loc Loc, IType Type) : ExpResult;
}

static class ExpResults
{
    public readonly static ExpResult NotFound = new ExpResult.NotFound();

    // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
    public readonly static ExpResult MultipleCandiates = new ExpResult.Error("MultipleCandidates");
    public readonly static ExpResult VarWithTypeArg = new ExpResult.Error("VarWithTypeArg");
    public readonly static ExpResult CantGetStaticMemberThroughInstance = new ExpResult.Error("CantGetStaticMemberThroughInstance");
    public readonly static ExpResult CantGetTypeMemberThroughInstance = new ExpResult.Error("CantGetTypeMemberThroughInstance");
    public readonly static ExpResult CantGetInstanceMemberThroughType = new ExpResult.Error("CantGetInstanceMemberThroughType");
    public readonly static ExpResult FuncCantHaveMember = new ExpResult.Error("FuncCantHaveMember");
    public readonly static ExpResult CantGetThis = new ExpResult.Error("CantGetThis");

    public static ExpResult ToErrorIdentifierResult(this SymbolQueryResult.Error errorResult)
    {
        switch (errorResult)
        {
            case SymbolQueryResult.Error.MultipleCandidates:
                return ExpResults.MultipleCandiates;

            case SymbolQueryResult.Error.VarWithTypeArg:
                return ExpResults.VarWithTypeArg;
        }

        throw new UnreachableCodeException();
    }

    // ExpResult를 R.Exp로 바꾸는 작업
    public static R.Exp MakeIR0Exp(this ExpResult result)
    {
        Debug.Assert(result is not ExpResult.NotFound);
        Debug.Assert(result is not ExpResult.Error);

        // 후처리, 지금 해야하나
        switch (result)
        {


            // 실제 this인 경우만 해당된다. 람다에서 참조한 this는 LambdaMemberVar로 들어온다
            case ExpResult.ThisVar thisVarResult:
                return new ExpResult.IR0Loc(new R.ThisLoc(), thisVarResult.Type);

            case IdentifierResult.LocalVar localVarResult:
                {
                    R.Loc loc = new R.LocalVarLoc(localVarResult.VarName);

                    if (localVarResult.IsRef)
                        loc = new R.DerefLocLoc(loc);

                    return new ExpResult.Loc(loc, localVarResult.TypeSymbol);
                }

            // 캡쳐 되었습니다
            case IdentifierResult.LambdaMemberVar lambdaMemberVarResult:
                return new ExpResult.Loc(new R.LambdaMemberVarLoc(lambdaMemberVarResult.Symbol), lambdaMemberVarResult.Symbol.GetDeclType());

            case IdentifierResult.GlobalVar globalVarResult:
                {
                    R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);

                    if (globalVarResult.IsRef)
                        loc = new R.DerefLocLoc(loc);

                    return new ExpResult.Loc(loc, globalVarResult.TypeSymbol);
                }

            case IdentifierResult.GlobalFuncs globalFuncsResult:
                return new ExpResult.GlobalFuncs(globalFuncsResult.QueryResult, globalFuncsResult.TypeArgsForMatch);

            // 'C'
            case IdentifierResult.Class classResult:
                return new ExpResult.Class(classResult.Symbol);

            // 'F' (inside class context C)
            case IdentifierResult.ClassMemberFuncs classMemberFuncsResult:
                {
                    // this.F(); 혹은 C.F();
                    return new ExpResult.ClassMemberFuncs(classMemberFuncsResult.QueryResult, classMemberFuncsResult.TypeArgsForMatch, HasExplicitInstance: false, null);
                }

            // 'x' (inside class context C)
            case IdentifierResult.ClassMemberVar classMemberVarResult: // 람다라면 내부에서 this capture되었을
                {
                    var classMemberVar = classMemberVarResult.Symbol;

                    // this.x, 람다인 경우 캡쳐가 필요하다
                    if (!classMemberVar.IsStatic())
                    {
                        var thisLoc = bodyContext.MakeThisLoc(); // lambda 내부라면 CapturedThis멤버, 일반 함수라면 this 가져오기
                        return new ExpResult.Loc(new R.ClassMemberLoc(thisLoc, classMemberVar), classMemberVar.GetDeclType());
                    }
                    else // C.x
                    {
                        return new ExpResult.Loc(new R.ClassMemberLoc(null, classMemberVar), classMemberVar.GetDeclType());
                    }
                }

            // 'S'
            case IdentifierResult.Struct structResult:
                return new ExpResult.Struct(structResult.Symbol);

            // 'F' (inside struct context S)
            case IdentifierResult.StructMemberFuncs structMemberFuncsResult:
                return new ExpResult.StructMemberFuncs(structMemberFuncsResult.QueryResult, structMemberFuncsResult.TypeArgsForMatch, HasExplicitInstance: false, null);

            // 'x' (inside struct context S)
            case IdentifierResult.StructMemberVar structMemberVarResult:
                {
                    var structMemberVar = structMemberVarResult.Symbol;

                    if (!structMemberVar.IsStatic())
                    {
                        var thisLoc = bodyContext.MakeThisLoc();
                        return new ExpResult.Loc(new R.StructMemberLoc(thisLoc, structMemberVar), structMemberVar.GetDeclType());
                    }
                    else // S.x
                    {
                        return new ExpResult.Loc(new R.StructMemberLoc(null, structMemberVar), structMemberVar.GetDeclType());
                    }
                }

            // 'E'
            case IdentifierResult.Enum enumResult:
                return new ExpResult.Enum(enumResult.Symbol);

            // 'First' (with type hint enum E)
            case IdentifierResult.EnumElem enumElemResult:
                return new ExpResult.EnumElem(enumElemResult.EnumElemSymbol);
        }

        throw new UnreachableCodeException();
    }

    // ExpResult를 R.Loc으로 바꾸는 작업
    public static (R.Loc Loc, IType Type)? MakeIR0Loc(this ExpResult result, bool bWrapExpAsLoc)
    {
        switch (result)
        {
            case ExpResult.NotFound: return null;
            case ExpResult.Error: return null;

            // Valid
            #region TopLevel
            case ExpResult.Namespace: return null;
            case ExpResult.GlobalVar globalVarResult: // ref i; 처리는 어떻게 하는건가?
                {
                    R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);
                    if (globalVarResult.IsRef)
                        loc = new R.DerefLocLoc(loc);

                    return (loc, globalVarResult.Type);
                }

            case ExpResult.GlobalFuncs: return null;

            #endregion

            #region Body
            case ExpResult.TypeVar: return null;

            case ExpResult.ThisVar thisVarResult:
                return (new R.ThisLoc(), thisVarResult.Type);

            // Body에 직접 있는 로컬 
            case ExpResult.LocalVar localVarResult:
                return (new R.LocalVarLoc(localVarResult.Name), localVarResult.Type);

            // Lambda안에 있는 외부 참조
            case ExpResult.LambdaMemberVar lambdaMemberVarResult:
                var lambdaMemberVar = new R.LambdaMemberVarLoc(lambdaMemberVarResult.MemberVar);
                return (lambdaMemberVar, lambdaMemberVarResult.MemberVar.GetDeclType());

            #endregion

            #region Class
            case ExpResult.Class: return null;

            // loc으로 만들 수 있는가
            // var x = F; // F는 여러개인데;
            case ExpResult.ClassMemberFuncs: throw new NotImplementedException();

            case ExpResult.ClassMemberVar classMemberVarResult:

                if (classMemberVarResult.HasExplicitInstance) // c.x, C.x 둘다 해당
                {
                    return (new R.ClassMemberLoc(classMemberVarResult.ExplicitInstance, classMemberVarResult.Symbol), classMemberVarResult.Symbol.GetDeclType());
                }
                else // x, x (static) 둘다 해당
                {
                    var instanceLoc = classMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
                    return (new R.ClassMemberLoc(instanceLoc, classMemberVarResult.Symbol), classMemberVarResult.Symbol.GetDeclType());
                }

            #endregion

            #region Struct
            case ExpResult.Struct: return null;
            case ExpResult.StructMemberFuncs: return null;
            case ExpResult.StructMemberVar structMemberVarResult:
                if (structMemberVarResult.HasExplicitInstance) // c.x, C.x 둘다 해당
                {
                    return (new R.StructMemberLoc(structMemberVarResult.ExplicitInstance, structMemberVarResult.Symbol), structMemberVarResult.Symbol.GetDeclType());
                }
                else // x, x (static) 둘다 해당
                {
                    var instanceLoc = structMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
                    return (new R.StructMemberLoc(instanceLoc, structMemberVarResult.Symbol), structMemberVarResult.Symbol.GetDeclType());
                }

            #endregion

            #region Enum
            case ExpResult.Enum: return null;

            // First
            case ExpResult.EnumElem: return null;
            #endregion

            case ExpResult.IR0Exp rexp:
                if (bWrapExpAsLoc)
                    return (new R.TempLoc(rexp.Exp), rexp.Exp.GetExpType());
                else
                    return null;

            case ExpResult.IR0Loc rloc:
                return (rloc.Loc, rloc.Type);

            default:
                throw new UnreachableCodeException();
        }
    }
}
