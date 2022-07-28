using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using R = Citron.IR0;

namespace Citron.Analysis
{
    partial class LambdaBodyContext : BodyContext
    {
        [AutoConstructor]
        partial struct CaptureInfo
        {
            public readonly LambdaMemberVarDeclSymbol MemberVarDecl;
            public readonly R.Argument Arg;
        }
        
        BodyContext outerBodyContext; // Lambda던, FuncBody던
        LocalContext outerLocalContext;

        Holder<LambdaDeclSymbol> lambdaDeclHolder;
        List<CaptureInfo> captureInfos;

        public LambdaBodyContext(BodyContext outerBodyContext, LocalContext outerLocalContext)
            : base(bSeqFunc: false)
        {
            this.outerBodyContext = outerBodyContext;
            this.outerLocalContext = outerLocalContext;

            this.lambdaDeclHolder = new Holder<LambdaDeclSymbol>();
            this.captureInfos = new List<CaptureInfo>();
        }

        public override bool CanAccess(ISymbolNode target)
        {
            return outerBodyContext.CanAccess(target);
        }

        public override ITypeSymbol? GetThisType()
        {
            // TODO: Lambda에 명시적인 캡쳐 구문이 들어가는 경우 ThisType이 필요하다
            return null;
        }

        public override bool IsLambda()
        {
            return true;
        }

        public override bool HasOuterBodyContext()
        {
            return true;
        }

        public override LambdaMemberVarSymbol Capture(IdentifierResult.LambdaMemberVar outsideLambda)
        {
            throw new NotImplementedException();
        }

        public override IdentifierResult ResolveIdentifierOuter(string idName, ImmutableArray<ITypeSymbol> typeArgs, ResolveHint hint, GlobalContext globalContext)
        {
            // 바깥에서 찾는다
            var outerResult = IdExpIdentifierResolver.Resolve(idName, typeArgs, hint, globalContext, outerBodyContext, outerLocalContext);

            switch (outerResult)
            {
                case IdentifierResult.NotFound:
                case IdentifierResult.Error:
                    return outerResult;
                    
                // this는 캡쳐대상. 최외각 lambda에서나 캡쳐대상이고, 다음부터는 lambdaMemberVar로 감싸져서 들어온다
                // TODO: valueType this캡쳐는 명시적이 아니라면 에러.
                case IdentifierResult.ThisVar(var thisType):
                    new IdentifierResult.LambdaMemberVar(() =>
                    {
                        // 캡쳐 정보
                        // 멤버의 타입, 멤버 변수 이름(저장시 사용할), 캡쳐할때 사용할 expression                        
                        var memberVarDecl = Capture(thisType, Name.CapturedThis, new R.Argument.Normal(new R.LoadExp(new R.ThisLoc(), thisType)));

                        
                    });

                    break;

                case IdentifierResult.LocalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName):
                    throw new NotImplementedException();

                case IdentifierResult.LambdaMemberVar(Func<LambdaMemberVarSymbol> SymbolConstructor):
                    throw new NotImplementedException();

                case IdentifierResult.GlobalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName):
                    throw new NotImplementedException();

                case IdentifierResult.GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch):
                    throw new NotImplementedException();

                case IdentifierResult.Class(ClassSymbol Symbol):
                    throw new NotImplementedException();

                case IdentifierResult.ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch):
                    throw new NotImplementedException();

                case IdentifierResult.ClassMemberVar(ClassMemberVarSymbol Symbol):
                    throw new NotImplementedException();

                case IdentifierResult.Struct(StructSymbol Symbol):
                    throw new NotImplementedException();

                case IdentifierResult.StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch):
                    throw new NotImplementedException();

                case IdentifierResult.StructMemberVar(StructMemberVarSymbol Symbol):
                    throw new NotImplementedException();

                case IdentifierResult.Enum(EnumSymbol Symbol):
                    throw new NotImplementedException();

                case IdentifierResult.EnumElem(EnumElemSymbol EnumElemSymbol):
                    throw new NotImplementedException();

                case IdentifierResult.Valid validResult:
                    return new IdentifierResult.LambdaMemberVar(validResult);
            }

            throw new UnreachableCodeException();
        }

        //public override LambdaMemberVarDeclSymbol MarkLocalVarCaptured(Name varName, ITypeSymbol typeSymbol)
        //{
        //    // 같은 이름이 있다면 바로 리턴
        //    foreach (var memberVar in memberVars.AsEnumerable())
        //        if (memberVar.GetName().Equals(varName))
        //            return memberVar;

        //    // 바로 겉에서 찾는다
        //    var localVarInfo = outerLocalContext.GetLocalVarInfo(varName);
        //    if (localVarInfo != null)
        //    {

        //    }

        //    // outerBodyContext.MarkLocalVarCaptured(varName, )
        //    throw new NotImplementedException();
        //}

        public override BodyContext NewLambdaBodyContext(LocalContext outerLocalContext)
        {
            return new LambdaBodyContext(this, outerLocalContext);
        }

        // 암시적 this가 나타났을 때, 
        public override R.Loc MakeThisLoc()
        {
            //var thisMemberVar = outerBbodyContext.MarkCaptured(M.Name.CapturedThis, thisType);
            //return new R.LambdaMemberVarLoc(thisMemberVar);
        }

        public LambdaMemberVarDeclSymbol Capture(ITypeSymbol type, Name name, R.Argument arg)
        {
            var memberVarDecl = new LambdaMemberVarDeclSymbol(lambdaDeclHolder, type, name);
            captureInfos.Add(new CaptureInfo(memberVarDecl, arg));

            return memberVarDecl;
        }
    }
}