using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 => TODO: 함수 인자 계산할때 backtracking이 생긴다
        class GlobalContext : IMutable<GlobalContext>
        {            
            ItemValueFactory itemValueFactory;
            InternalBinaryOperatorQueryService internalBinOpQueryService;
            GlobalItemValueFactory globalItemValueFactory;

            TypeExpInfoService typeExpInfoService;            
            IErrorCollector errorCollector;
            InternalGlobalVariableRepository internalGlobalVarRepo;

            public GlobalContext(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpInfoService,
                IErrorCollector errorCollector)
            {
                this.itemValueFactory = itemValueFactory;
                this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(itemValueFactory);
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpInfoService = typeExpInfoService;
                this.errorCollector = errorCollector;
                this.internalGlobalVarRepo = new InternalGlobalVariableRepository();
            }

            GlobalContext(
                GlobalContext other,
                CloneContext cloneContext)
            {
                Infra.Misc.EnsurePure(other.itemValueFactory);
                this.itemValueFactory = other.itemValueFactory;

                Infra.Misc.EnsurePure(other.internalBinOpQueryService);
                this.internalBinOpQueryService = other.internalBinOpQueryService;

                Infra.Misc.EnsurePure(other.globalItemValueFactory);
                this.globalItemValueFactory = other.globalItemValueFactory;

                Infra.Misc.EnsurePure(other.typeExpInfoService);
                this.typeExpInfoService = other.typeExpInfoService;

                this.errorCollector = cloneContext.GetClone(other.errorCollector);
                this.internalGlobalVarRepo = cloneContext.GetClone(other.internalGlobalVarRepo);
            }

            public GlobalContext Clone(CloneContext cloneContext)
            {
                return new GlobalContext(this, cloneContext);
            }

            public void Update(GlobalContext src, UpdateContext updateContext)
            {
                Infra.Misc.EnsurePure(src.itemValueFactory);
                this.itemValueFactory = src.itemValueFactory;

                Infra.Misc.EnsurePure(src.internalBinOpQueryService);
                this.internalBinOpQueryService = src.internalBinOpQueryService;

                Infra.Misc.EnsurePure(src.globalItemValueFactory);
                this.globalItemValueFactory = src.globalItemValueFactory;

                Infra.Misc.EnsurePure(src.typeExpInfoService);
                this.typeExpInfoService = src.typeExpInfoService;

                updateContext.Update(this.errorCollector, src.errorCollector);
                updateContext.Update(this.internalGlobalVarRepo, src.internalGlobalVarRepo);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, code.ToString()));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, code.ToString()));
                throw new AnalyzerFatalException();
            }            

            public TypeValue GetVoidType()
            {
                return itemValueFactory.Void;
            }

            public TypeValue GetBoolType()
            {
                return itemValueFactory.Bool;
            }

            public TypeValue GetIntType()
            {
                return itemValueFactory.Int;
            }

            public TypeValue GetStringType()
            {
                return itemValueFactory.String;
            }

            public TypeValue GetListType(TypeValue elemType)
            {
                return itemValueFactory.List(elemType);
            }
            
            public void AddInternalGlobalVarInfo(M.Name name, TypeValue typeValue)
            {
                internalGlobalVarRepo.AddInternalGlobalVariable(name, typeValue);
            }

            public bool IsBoolType(TypeValue typeValue)
            {
                return itemValueFactory.Bool.Equals(typeValue);
            }

            public bool IsIntType(TypeValue typeValue)
            {
                return itemValueFactory.Int.Equals(typeValue);
            }

            public bool IsStringType(TypeValue typeValue)
            {
                return itemValueFactory.String.Equals(typeValue);
            }

            public TypeValue GetTypeValueByMType(M.Type type)
            {
                return itemValueFactory.MakeTypeValueByMType(type);
            }

            public TypeValue GetTypeValueByTypeExp(S.TypeExp typeExp)
            {
                var typeExpInfo = typeExpInfoService.GetTypeExpInfo(typeExp);
                return itemValueFactory.MakeTypeValue(typeExpInfo);
            }

            public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName)
            {
                return internalGlobalVarRepo.GetVariable(idName);
            }

            public bool DoesInternalGlobalVarNameExist(string name)
            {
                return internalGlobalVarRepo.HasVariable(name);
            }

            public LambdaTypeValue GetLambdaTypeValue(R.Path.Nested lambda, TypeValue retType, ImmutableArray<TypeValue> paramTypes)
            {
                return itemValueFactory.MakeLambdaType(lambda, retType, paramTypes);
            }

            public SeqTypeValue GetSeqTypeValue(R.Path.Nested seq, TypeValue yieldType)
            {
                return itemValueFactory.MakeSeqType(seq, yieldType);
            }

            public ItemQueryResult GetGlobalItem(M.NamespacePath namespacePath, string idName, int typeParamCount)
            {
                return globalItemValueFactory.GetGlobal(namespacePath, idName, typeParamCount);
            }

            public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind)
            {
                return internalBinOpQueryService.GetInfos(kind);
            }

            public TypeValue MakeTypeValue(ItemValueOuter outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeTypeValue(outer, typeInfo, typeArgs);
            }

            public FuncValue MakeFuncValue(ItemValueOuter outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeFunc(outer, funcInfo, typeArgs);
            }

            public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, M.MemberVarInfo info)
            {
                return itemValueFactory.MakeMemberVarValue(outer, info);
            }

            public TupleTypeValue GetTupleType(ImmutableArray<(TypeValue Type, string? Name)> elems)
            {
                return itemValueFactory.MakeTupleType(elems);
            }

            public ExpResult.Exp? TryCastExp_Exp(ExpResult.Exp expResult, TypeValue expectType) // nothrow
            {
                // 같으면 그대로 리턴
                if (expResult.TypeValue.Equals(expectType))
                    return expResult;

                // 1. enumElem -> enum
                if (expResult.TypeValue is EnumElemTypeValue enumElemTypeValue)
                {
                    if (expectType is EnumTypeValue expectEnumType)
                    {
                        if (enumElemTypeValue.Outer.Equals(expectType))
                        {
                            var castExp = new R.CastEnumElemToEnumExp(expResult.Result, expectEnumType.GetRPath_Nested());
                            return new ExpResult.Exp(castExp, expectType);
                        }
                    }

                    return null;
                }

                // 2. exp is class type
                else if (expResult.TypeValue is ClassTypeValue classTypeValue)
                {
                    if (expectType is ClassTypeValue expectClassType)
                    {
                        // allows upcast
                        if (expectClassType.IsBaseOf(classTypeValue))
                        {
                            var castExp = new R.CastClassExp(expResult.Result, expectClassType.GetRPath());
                            return new ExpResult.Exp(castExp, expectClassType);
                        }

                        return null;
                    }

                    // TODO: interface
                    // if (expectType is InterfaceTypeValue )
                }

                return null;
            }
        }
    }
}
