using Citron.Infra;
using Citron.Syntax;
using System;

namespace Citron.Analysis
{
    // 하는 일: 문법상 exp가 
    // 1. 무엇을 가리키고 있는지 (클래스/구조체/열거자 이름, 전역 변수, 지역 변수, 전역 함수 등.., expression, location)
    // 2. 가리킨 것의 타입은 어떤지 (이건 expression, location으로 평가될때만 유효하다)
    // 3. Syntax stmt 부분을 IR0로 translation하기
    // 4. 지역변수 이름 구분, 

    // 다른곳으로 분리할 수 있는것들을 나눠보자
    // 4. Access 가능한지 파악하고 에러를 내주기
    // 5. 타입 체킹? (정작 resolve가 다 끝난 후에 타입체킹을 해야 하는 이유가 있는가, if에 bool이 들어가야 하는 걸 체크하는 경우엔 필요하다)

    // ExpResult와 IR0.Exp의 차이점    
    // ClassMember등이 없다
    // 일단 ExpResult를 밀어야 하지 않을까
    // IdentifierResolver랑 약간 겹친다

    // 생각해보니 TypeExp가 먼저 
    struct ExpIdentifierResolver
    {
        void AnalyzeArgument(Argument arg)
        {
            switch(arg)
            {
                case Argument.Normal normalArg:
                    throw new NotImplementedException();
                    // AnalyzeExp(normalArg.Exp, );
                    break;

                case Argument.Params:
                case Argument.Ref:
                    break;
            }
            
        }

        void AnalyzeStringExpElement(StringExpElement elem)
        {
            switch(elem)
            {
                case TextStringExpElement textElem: 
                    break; 

                case ExpStringExpElement expElem:
                    // AnalyzeExp(expElem.Exp, );
                    throw new NotImplementedException();
                    break;
            }
        }

        void AnalyzeIdentifierExp(IdentifierExp idExp, ResolveTypeHint typeHint)
        {
            throw new NotImplementedException();
        }

        void AnalyzeMemberExp(MemberExp memberExp)
        {
            throw new NotImplementedException();
        }

        void AnalyzeCallExp(CallExp callExp, ResolveTypeHint typeHint)
        {
            throw new NotImplementedException();
        }
        
        void AnalyzeLambdaExp(LambdaExp lambdaExp, ResolveTypeHint typeHint)
        {
            // (int p0, int p2) => { }
            // p0, p2가 int 타입이라는 정보를 추가하고 StmtIdentifierResolver를 돌린다
            throw new NotImplementedException();
        }

        // instance[e]
        void AnalyzeIndexerExp(IndexerExp indexerExp)
        {
            throw new NotImplementedException();
        }

        // new C(arg1, arg2)
        void AnalyzeNewExp(NewExp newExp)
        {
            foreach(var arg in newExp.Args)
            {
                AnalyzeArgument(arg);
            }
        }

        // interpolated string
        void AnalyzeStringExp(StringExp stringExp)
        {
            foreach(var elem in stringExp.Elements)
            {
                AnalyzeStringExpElement(elem);
            }
        }

        // list<int> l = []
        void AnalyzeListExp(ListExp listExp, ResolveTypeHint typeHint)
        {
            throw new NotImplementedException();

            foreach(var elem in listExp.Elems)
            {
                // ResolveTypeHint typeHint = ??;
                AnalyzeExp(elem, typeHint);
            }
        }

        // 
        void AnalyzeUnaryOpExp(UnaryOpExp unaryExp)
        {
            // AnalyzeExp(unaryExp.Operand, ResolveTypeHint);
        }

        void AnalyzeBinaryOpExp(BinaryOpExp binaryExp)
        {
            // AnalyzeExp(binaryExp.Operand0);
            // AnalyzeExp(binaryExp.Operand1);
        }

        public void AnalyzeExp(Exp exp, ITypeSymbol? typeHint)
        {
            switch (exp)
            {
                case IdentifierExp idExp: AnalyzeIdentifierExp(idExp, typeHint); break;
                case MemberExp memberExp: AnalyzeMemberExp(memberExp); break;

                case CallExp callExp: AnalyzeCallExp(callExp, typeHint); break;
                case LambdaExp lambdaExp: AnalyzeLambdaExp(lambdaExp, typeHint); break;
                case IndexerExp indexerExp: AnalyzeIndexerExp(indexerExp); break;
                case NewExp newExp: AnalyzeNewExp(newExp); break;

                // literal
                case NullLiteralExp nullExp: break;
                case BoolLiteralExp boolExp: break;
                case IntLiteralExp intExp: break;

                // exp composition
                case StringExp stringExp: AnalyzeStringExp(stringExp); break;
                case ListExp listExp: AnalyzeListExp(listExp, typeHint); break;

                // inrnal
                case UnaryOpExp unaryOpExp: AnalyzeUnaryOpExp(unaryOpExp); break;
                case BinaryOpExp binaryOpExp: AnalyzeBinaryOpExp(binaryOpExp); break;

                default: throw new UnreachableCodeException();
            }
        }
    }
}
