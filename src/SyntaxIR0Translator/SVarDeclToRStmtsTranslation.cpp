#include "pch.h"
#include "SVarDeclToRStmtsTranslation.h"

#include <Infra/Ptr.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>
#include <IR0/RType.h>
#include <IR0/RStmt.h>

#include "DeclTypeInfo.h"
#include "TranslationContext.h"
#include "SExpToRExpTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class VarDeclElemTranslator
{
    SVarDeclElement& elem;
    DeclTypeInfo& declTypeInfo;
    vector<RStmtPtr>* outResult; 
    TranslationContext& context;

public:
    VarDeclElemTranslator(SVarDeclElement& elem, DeclTypeInfo& declTypeInfo, vector<RStmtPtr>* outResult, TranslationContext& context)
        : elem(elem), declTypeInfo(declTypeInfo), outResult(outResult), context(context)
    {
    }

private:
    template<typename TFunc>
    bool Error(TFunc&& f)
    {                                     
        context.Log(std::forward<TFunc>(f));
        return false;
    }

    bool CheckVarConsistencyPlainVar(RType* initExpType)
    {
        if (auto* interfaceType = dynamic_cast<RType_Interface*>(initExpType))
        {
            if (interfaceType->bLocal)
                return Error(&Logger::Fatal_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface);

            return true;
        }

        if (dynamic_cast<RType_BoxPtr*>(initExpType))
            return Error(&Logger::Fatal_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr);

        if (dynamic_cast<RType_LocalPtr*>(initExpType))
            return Error(&Logger::Fatal_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr);

        if (dynamic_cast<RType_NullableValue*>(initExpType))
            return Error(&Logger::Fatal_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr);

        if (dynamic_cast<RType_NullableRef*>(initExpType))
            return Error(&Logger::Fatal_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr);

        return true;
    }

    bool CheckVarConsistency(RType* initExpType)
    {
        // var 꼴별로 에러 체크
        switch (declTypeInfo.kind)
        {
        case DeclTypeInfoKind::Normal:
            assert(false);
            return false;

            // local, boxptr, localptr, nullable 인지 체크한다 
        case DeclTypeInfoKind::PlainVar:
            return CheckVarConsistencyPlainVar(initExpType);
            
        case DeclTypeInfoKind::LocalInterfaceVar:
            if (!dynamic_cast<RType_Interface*>(initExpType))
                return Error(&Logger::Fatal_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface);

        case DeclTypeInfoKind::BoxPtrVar:
            if (!dynamic_cast<RType_BoxPtr*>(initExpType))
                return Error(&Logger::Fatal_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr);

        case DeclTypeInfoKind::LocalPtrVar:
            if (!dynamic_cast<RType_LocalPtr*>(initExpType))
                return Error(&Logger::Fatal_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr);

        case DeclTypeInfoKind::NullableVar:
            if (!dynamic_cast<RType_NullableRef*>(initExpType) || !dynamic_cast<RType_NullableValue*>(initExpType))
                return Error(&Logger::Fatal_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable);
        }

        return true;
    }

    bool HandleVarDeclType()
    {
        if (!elem.initExp)
            return Error(&Logger::Fatal_VarDecl_LocalVarDeclNeedInitializer);

        // var꼴로 나오는 경우 hintType은 없다
        auto rInitExp = TranslateSExpToRExp(*elem.initExp, /*hintType*/ nullptr, context);
        if (!rInitExp)
            return false;

        auto rInitExpType = context.GetType(*rInitExp);

        if (!CheckVarConsistency(rInitExpType.get()))
            return false;

        context.AddLocalVarInfo(rInitExpType, RName_Normal(elem.varName));
        outResult->push_back(MakePtr<RStmt_LocalVarDecl>(rInitExpType, elem.varName, std::move(rInitExp)));

        return true;
    }

    bool HandleExplicitDeclType()
    {
        assert(declTypeInfo.kind == DeclTypeInfoKind::Normal);
        auto& declType = declTypeInfo.type;

        RExpPtr rInitExp;
        if (elem.initExp)
        {
            rInitExp = TranslateSExpToRExp(*elem.initExp, declType, context);
            if (!rInitExp) return false;

            rInitExp = CastRExp(std::move(rInitExp), declType, context);
            if (!rInitExp)
                return Error(&Logger::Fatal_VarDecl_InitExpTypeMismatch);
        }

        context.AddLocalVarInfo(declType, RName_Normal(elem.varName));
        outResult->push_back(MakePtr<RStmt_LocalVarDecl>(declType, elem.varName, std::move(rInitExp)));

        return true;
    }

public:
    bool Translate()
    {
        if (context.DoesLocalVarNameExistInScope(elem.varName))
            return Error(&Logger::Fatal_VarDecl_LocalVarNameShouldBeUniqueWithinScope);

        if (declTypeInfo.kind != DeclTypeInfoKind::Normal)
        {
            return HandleVarDeclType();
        }
        else
        {
            return HandleExplicitDeclType();
        }
    }
};

} // namespace 


bool TranslateSVarDeclToRStmts(SVarDecl& varDecl, vector<RStmtPtr>* outResult, TranslationContext& context)
{
    DeclTypeInfo declTypeInfo = context.GetDeclTypeInfo(*varDecl.type);

    for (auto& elem : varDecl.elements)
    {
        VarDeclElemTranslator translator(elem, declTypeInfo, outResult, context);

        if (!translator.Translate())
            return false;
    }

    return true;
}

} // namespace Citron::SyntaxIR0Translator