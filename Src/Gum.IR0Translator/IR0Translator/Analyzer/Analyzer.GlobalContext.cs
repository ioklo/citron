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
using Gum.Log;

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
            ILogger logger;
            InternalGlobalVariableRepository internalGlobalVarRepo;

            public GlobalContext(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpInfoService,
                ILogger logger)
            {
                this.itemValueFactory = itemValueFactory;
                this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(itemValueFactory);
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpInfoService = typeExpInfoService;
                this.logger = logger;
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

                this.logger = cloneContext.GetClone(other.logger);
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

                updateContext.Update(this.logger, src.logger);
                updateContext.Update(this.internalGlobalVarRepo, src.internalGlobalVarRepo);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                logger.Add(new AnalyzeErrorLog(code, node, code.ToString()));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                logger.Add(new AnalyzeErrorLog(code, node, code.ToString()));
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
                return itemValueFactory.MakeListType(elemType);
            }
            
            public void AddInternalGlobalVarInfo(bool bRef, TypeValue typeValue, string name)
            {
                internalGlobalVarRepo.AddInternalGlobalVariable(bRef, typeValue, name);
            }

            public bool IsBoolType(TypeValue typeValue)
            {
                return itemValueFactory.Bool.Equals(typeValue);
            }

            public bool IsIntType(TypeValue typeValue)
            {
                return itemValueFactory.Int.Equals(typeValue);
            }

            public EnumElemTypeValue MakeEnumElemTypeValue(EnumTypeValue outer, IModuleEnumElemInfo elemInfo)
            {
                return itemValueFactory.MakeEnumElemTypeValue(outer, elemInfo);
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

            public LambdaTypeValue GetLambdaTypeValue(R.Path.Nested lambda, TypeValue retType, ImmutableArray<ParamInfo> paramInfos)
            {
                return itemValueFactory.MakeLambdaType(lambda, retType, paramInfos);
            }

            public SeqTypeValue GetSeqTypeValue(R.Path.Nested seq, TypeValue yieldType)
            {
                return itemValueFactory.MakeSeqType(seq, yieldType);
            }

            public ItemQueryResult GetGlobalItem(M.NamespacePath namespacePath, M.Name name, int typeParamCount)
            {
                return globalItemValueFactory.GetGlobal(namespacePath, name, typeParamCount);
            }

            public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind)
            {
                return internalBinOpQueryService.GetInfos(kind);
            }

            public TypeValue MakeTypeValue(ItemValueOuter outer, IModuleTypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeTypeValue(outer, typeInfo, typeArgs);
            }

            public FuncValue MakeFuncValue(ItemValueOuter outer, IModuleFuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeFunc(outer, funcInfo, typeArgs);
            }

            public ConstructorValue MakeConstructorValue(NormalTypeValue outer, IModuleConstructorInfo info)
            {
                return itemValueFactory.MakeConstructorValue(outer, info);
            }

            public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, IModuleMemberVarInfo info)
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

                // TODO: TypeValue에 TryCast를 각각 넣기
                // expectType.TryCast(expResult); // expResult를 넣는것도 이상하다.. 그건 그때가서

                // 1. enumElem -> enum
                if (expResult.TypeValue is EnumElemTypeValue enumElemTypeValue)
                {
                    if (expectType is EnumTypeValue expectEnumType)
                    {
                        if (enumElemTypeValue.Outer.Equals(expectType))
                        {
                            var castExp = new R.CastEnumElemToEnumExp(expResult.Result, enumElemTypeValue.GetRPath_Nested());
                            return new ExpResult.Exp(castExp, expectType);
                        }
                    }

                    return null;
                }

                // 2. exp is class type
                if (expResult.TypeValue is ClassTypeValue classTypeValue)
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

                // 3. T -> T? 는 허용
                if (expectType is NullableTypeValue expectNullableType)
                {
                    // C -> B -> B? 허용
                    var expectInnerType = expectNullableType.GetInnerTypeValue();

                    // C -> B 시도
                    var castToInnerTypeResult = TryCastExp_Exp(expResult, expectInnerType);
                    if (castToInnerTypeResult != null)
                    {
                        // B -> B?
                        var castExp = new R.NewNullableExp(expectInnerType.GetRPath(), castToInnerTypeResult.Result);
                        return new ExpResult.Exp(castExp, expectType);
                    }
                }

                return null;
            }

            public TypeVarTypeValue MakeTypeVarTypeValue(int index)
            {
                return itemValueFactory.MakeTypeVar(index);
            }

            public StructTypeValue MakeStructTypeValue(ItemValueOuter outer, IModuleStructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeStructValue(outer, structInfo, typeArgs);
            }

            public ClassTypeValue MakeClassTypeValue(ItemValueOuter outer, IModuleClassInfo classInfo, ImmutableArray<TypeValue> typeArgs)
            {
                return itemValueFactory.MakeClassValue(outer, classInfo, typeArgs);
            }
        }
    }
}
