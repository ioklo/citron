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
        class GlobalContext
        {            
            ItemValueFactory itemValueFactory;
            InternalBinaryOperatorQueryService internalBinOpQueryService;
            GlobalItemValueFactory globalItemValueFactory;

            TypeExpInfoService typeExpTypeValueService;            
            IErrorCollector errorCollector;
            InternalGlobalVariableRepository internalGlobalVarRepo;

            public GlobalContext(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector)
            {
                this.itemValueFactory = itemValueFactory;
                this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(itemValueFactory);
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpTypeValueService = typeExpTypeValueService;
                this.errorCollector = errorCollector;                
                this.internalGlobalVarRepo = new InternalGlobalVariableRepository();
            }

            GlobalContext(
                ItemValueFactory itemValueFactory,
                InternalBinaryOperatorQueryService internalBinOpQueryService,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector,
                InternalGlobalVariableRepository internalGlobalVarRepo)
            {
                this.itemValueFactory = itemValueFactory;
                this.internalBinOpQueryService = internalBinOpQueryService;
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpTypeValueService = typeExpTypeValueService;
                this.errorCollector = errorCollector;
                this.internalGlobalVarRepo = internalGlobalVarRepo;
            }

            public GlobalContext Clone(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector)
            {
                // pure하지 않은 것들을 clone시켜야 하는데, 나중에 pure -> impure로 바뀌었을 때, 알게 될 방법이 있는가
                // 모두 Clone한다
                return new GlobalContext(
                    itemValueFactory,
                    internalBinOpQueryService.Clone(itemValueFactory),
                    globalItemValueFactory,
                    typeExpTypeValueService,
                    errorCollector,
                    internalGlobalVarRepo.Clone());
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, ""));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, ""));
                throw new FatalAnalyzeException();
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

            public bool IsAssignable(TypeValue toTypeValue, TypeValue fromTypeValue)
            {   
                // B <- D
                // 지금은 fromType의 base들을 찾아가면서 toTypeValue와 맞는 것이 있는지 본다
                // TODO: toTypeValue가 interface라면, fromTypeValue의 interface들을 본다

                TypeValue? curType = fromTypeValue;
                while (curType != null)
                {
                    if (toTypeValue.Equals(curType))
                        return true;

                    curType = curType.GetBaseType();
                }

                return false;
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
                return itemValueFactory.MakeTypeValue(type);
            }

            public TypeValue GetTypeValueByTypeExp(S.TypeExp typeExp)
            {
                var typeExpInfo = typeExpTypeValueService.GetTypeExpInfo(typeExp);

                if (typeExpInfo is MTypeTypeExpInfo mtypeInfo)
                    return itemValueFactory.MakeTypeValue(mtypeInfo.Type);
                else if (typeExpInfo is VarTypeExpInfo)
                    return itemValueFactory.MakeVarTypeValue();
                else
                    throw new UnreachableCodeException();
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
            
        }
    }
}
