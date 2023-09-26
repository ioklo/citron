using System.Diagnostics;
using System;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;
using Citron.IR0;

// for conflict
using SForeachStmt = Citron.Syntax.ForeachStmt;

using static Citron.Infra.Misc;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Pretune;

namespace Citron.Analysis;

partial struct StmtVisitor
{   
    partial struct ForeachStmtBuilder
    {
        SForeachStmt stmt;
        ScopeContext context;
        
        Name itemVarName;

        static Name nextName = new Name.Normal("Next");
        static Name getEnumeratorName = new Name.Normal("GetEnumerator");

        public ForeachStmtBuilder(SForeachStmt stmt, ScopeContext context)
        {
            this.stmt = stmt;
            this.context = context;

            this.itemVarName = new Name.Normal(stmt.VarName);
        }

        // syntax의 enumerableExp를 사용해서 enumerator를 가져오는 Exp를 생성한다
        TranslationResult<(Exp Exp, IType ExpType)> MakeEnumeratoExpAndType()
        {
            TranslationResult<(Exp, IType)> Error() => TranslationResult.Error<(Exp, IType)>();

            var enumerableResult = ExpIR0LocTranslator.Translate(stmt.Enumerable, context, hintType: null, bWrapExpAsLoc: true, A2015_ResolveIdentifier_ExpressionIsNotLocation);
            if (!enumerableResult.IsValid(out var enumerableLocResult))
                return Error();

            // GetEnumerator함수를 손으로 찾는다
            var sqr = enumerableLocResult.LocType.QueryMember(getEnumeratorName, explicitTypeArgCount: 0);
            if (sqr == null)
            {
                // TODO: [15] foreach 에러 처리
                throw new NotImplementedException();
            }

            var candidates = new Candidates<(ISymbolNode, IFuncDeclSymbol)>();
            foreach(var (outer, declSymbol) in sqr.AsOuterAndDeclSymbols())
            {
                // 파라미터가 없어야 한다
                if (declSymbol.GetParameterCount() != 0) continue;

                // 따라서 Type Parameter도 없어야 한다
                if (declSymbol.GetTypeParamCount() != 0) continue;

                // instance함수여야 한다
                if (declSymbol.IsStatic()) continue;

                candidates.Add((outer, declSymbol));
            }

            if (candidates.GetCount() == 0)
            {
                // TODO: [15] foreach 에러 처리
                return Error();
            }

            if (candidates.GetCount() != 1)
            {
                // TODO: [15] foreach 에러 처리
                throw new NotImplementedException();
            }

            {
                var (outer, declSymbol) = candidates.GetAt(0);

                // 아까 갯수가 0인지 체크를 했으니 typeArgs는 default이다
                var funcS = (IFuncSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs: default);
                var funcSRet = funcS.GetReturn();
                Debug.Assert(funcSRet != null);

                var enumeratoExp = InstanceFuncSymbolBinder.Bind(funcS, enumerableLocResult.Loc, args: default);
                return TranslationResult.Valid<(Exp, IType)>((enumeratoExp, funcSRet.Value.Type));
            }
        }

        TranslationResult<(Exp NextExp, IType ItemVarType)> MakeNextExpAndInferredItemVarType(IType enumeratorType)
        {
            var sqr = enumeratorType.QueryMember(nextName, explicitTypeArgCount: 0);
            if (sqr == null) return TranslationResult.Error<(Exp, IType)>();

            var candidates = new Candidates<(Exp, IType)>();
            foreach (var (outer, declSymbol) in sqr.AsOuterAndDeclSymbols())
            {
                // TODO: [16] TypeResolver적용
                if (declSymbol.GetTypeParamCount() != 0) continue;

                var symbol = (IFuncSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs: default);

                // 파라미터는 1개
                if (symbol.GetParameterCount() != 1) continue;

                // 리턴 타입은 bool
                var ret = symbol.GetReturn();
                Debug.Assert(ret.HasValue);

                if (!BodyMisc.TypeEquals(ret.Value.Type, context.GetBoolType())) continue;

                // 인자는 out T*꼴이어야 한다
                var param = symbol.GetParameter(0);
                if (!param.bOut) continue;
                if (param.Type is not LocalPtrType localPtrParamType) continue;

                var itemTypeFromNextParam = localPtrParamType.GetInnerType();
                
                // $enumerator.GetNext(&i);
                var arg = new Argument.Normal(new LocalRefExp(new LocalVarLoc(itemVarName)));
                var enumeratorLoc = new LocalVarLoc(Names.Enumerator);

                var nextExp = InstanceFuncSymbolBinder.Bind(symbol, enumeratorLoc, Arr<Argument>(arg));
                candidates.Add((nextExp, itemTypeFromNextParam));
            }

            if (candidates.GetCount() == 1)
            {
                var one = candidates.GetAt(0);
                return TranslationResult.Valid(one);
            }
            else
            {
                // TODO: [17] NextFunc가 여러개일때 처리
                return TranslationResult.Error<(Exp, IType)>();
            }
        }

        // NextExp를 만드는데, 캐스팅이 필요하면 CastInfo를 같이 돌려준다
        TranslationResult<(Exp NextExp, (IType RawItemType, Exp CastExp)? CastInfo)> MakeNextExpAndCastExp(IType enumeratorType, IType itemTypeFromSyntax)
        {
            var sqr = enumeratorType.QueryMember(nextName, explicitTypeArgCount: 0);
            if (sqr == null) return TranslationResult.Error<(Exp, (IType, Exp)?)>();

            var candidates = new Candidates<(Exp, (IType, Exp)?)>();
            foreach (var (outer, declSymbol) in sqr.AsOuterAndDeclSymbols())
            {
                if (declSymbol.GetParameterCount() != 1) continue;

                // 리턴 타입은 bool
                var ret = declSymbol.GetReturn();
                Debug.Assert(ret.HasValue);

                if (!BodyMisc.TypeEquals(ret.Value.Type, context.GetBoolType())) continue;

                // TODO: [16] TypeResolver적용
                if (declSymbol.GetTypeParamCount() != 0) continue;
                
                var symbol = (IFuncSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs: default);

                // 인자는 out T*꼴이어야 한다
                var param = symbol.GetParameter(0);
                if (!param.bOut) continue;
                if (param.Type is not LocalPtrType localPtrParamType) continue;

                var itemTypeFromNextParam = localPtrParamType.GetInnerType();

                if (BodyMisc.TypeEquals(itemTypeFromNextParam, itemTypeFromSyntax))
                {
                    // $enumerator.GetNext(&i);
                    var arg = new Argument.Normal(new LocalRefExp(new LocalVarLoc(itemVarName)));
                    var enumeratorLoc = new LocalVarLoc(Names.Enumerator);

                    var nextExp = InstanceFuncSymbolBinder.Bind(symbol, enumeratorLoc, Arr<Argument>(arg));

                    return TranslationResult.Valid<(Exp, (IType, Exp)?)>((nextExp, null));
                }
                else // 캐스팅
                {   
                    var rawItemType = itemTypeFromNextParam;
                    var arg = new Argument.Normal(new LocalRefExp(new LocalVarLoc(Names.RawItem)));
                    var enumeratorLoc = new LocalVarLoc(Names.Enumerator);

                    // $enumerator.GetNext(&$rawItem)
                    var nextExp = InstanceFuncSymbolBinder.Bind(symbol, enumeratorLoc, Arr<Argument>(arg));

                    // $rawItem
                    var rawItemExp = new LoadExp(new LocalVarLoc(Names.RawItem), rawItemType);
                    var castExp = BodyMisc.TryCastExp_Exp(rawItemExp, rawItemType, expectedType: itemTypeFromSyntax, context);
                    if (castExp != null) // 캐스팅이 성공할때만 candidates에 넣기
                    {
                        candidates.Add((nextExp, (rawItemType, castExp)));
                    }
                }
            }

            int count = candidates.GetCount();

            if (count == 0)
            {
                // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                return TranslationResult.Error<(Exp, (IType, Exp)?)>();
            }
            else if (count == 1)
            {
                var one = candidates.GetAt(0);
                return TranslationResult.Valid(one);
            }
            else
            {
                // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                return TranslationResult.Error<(Exp, (IType, Exp)?)>();
            }
        }

        TranslationResult<ImmutableArray<Stmt>> MakeBody(IType itemVarType)
        {
            // 루프 컨텍스트를 하나 열고
            var bodyContext = context.MakeLoopNestedScopeContext();

            // 루프 컨텍스트에 로컬을 하나 추가하고 (enumerator는 추가해야 할까)
            bodyContext.AddLocalVarInfo(itemVarType, itemVarName);

            // 본문 분석
            return StmtVisitor.TranslateEmbeddable(stmt.Body, bodyContext);
        }

        public TranslationResult<ImmutableArray<Stmt>> Build()
        {
            var enumeratorExpAndTypeResult = MakeEnumeratoExpAndType();
            if (!enumeratorExpAndTypeResult.IsValid(out var enumeratorExpAndType))
                return Error();

            var (enumeratorExp, enumeratorType) = enumeratorExpAndType;

            if (!BodyMisc.IsVarType(stmt.Type))
            {
                var itemVarType = context.MakeType(stmt.Type);

                var nextExpCastInfoResult = MakeNextExpAndCastExp(enumeratorType, itemVarType);
                if (!nextExpCastInfoResult.IsValid(out var nextExpCastInfo)) return Error();
                var (nextExp, castInfo) = nextExpCastInfo;

                var bodyResult = MakeBody(itemVarType);
                if (bodyResult.IsValid(out var body)) return Error();

                if (castInfo == null)
                {
                    return Stmts(new ForeachStmt(enumeratorType, enumeratorExp, itemVarType, itemVarName, nextExp, body));
                }
                else
                {
                    var (rawTypeItem, castExp) = castInfo.Value;
                    return Stmts(new ForeachCastStmt(enumeratorType, enumeratorExp, itemVarType, itemVarName, rawTypeItem, nextExp, castExp, body));
                }
            }
            else // var 일 경우
            {
                var nextExpItemVarTypeResult = MakeNextExpAndInferredItemVarType(enumeratorType);
                if (!nextExpItemVarTypeResult.IsValid(out var nextExpItemVarType)) return Error();
                var (nextExp, itemVarType) = nextExpItemVarType;

                var bodyResult = MakeBody(itemVarType);
                if (!bodyResult.IsValid(out var body)) return Error();

                return Stmts(new ForeachStmt(enumeratorType, enumeratorExp, itemVarType, itemVarName, nextExp, body));
            }

        }
    }
}