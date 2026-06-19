#pragma once
#include "SyntaxConfig.h"
#include "Syntaxes.g.h"

#include <vector>
#include <memory>

namespace Citron {

class SSyntax;

class SStmt;
class SStmt_Command;
class SStmt_VarDecl;
class SStmt_If;
class SStmt_For;
class SStmt_Continue;
class SStmt_Break;
class SStmt_Return;
class SStmt_Block;
class SStmt_Blank;
class SStmt_Exp;
class SStmt_Task;
class SStmt_Await;
class SStmt_Async;
class SStmt_Foreach;
class SStmt_Yield;
class SStmt_Directive;

class SExp;
class SExp_Identifier;
class SExp_String;
class SExp_IntLiteral;
class SExp_BoolLiteral;
class SExp_NullLiteral;
class SExp_BinaryOp;
class SExp_UnaryOp;
class SExp_Call;
class SExp_Lambda;
class SExp_Indexer;
class SExp_Member;
class SExp_List;
class SExp_New;
class SExp_Shared;
class SExp_Is;
class SExp_As;

class STypeExp;
class STypeExp_Id;
class STypeExp_Member;
class STypeExp_Nullable;
class STypeExp_Shared; // shared int
class STypeExp_Ptr;
class STypeExp_Local; // local I

class SStringExpElement;
class SStringExpElement_Text;
class SStringExpElement_Exp;

class SLambdaExpBody;
class SLambdaExpBody_Stmts;
class SLambdaExpBody_Exp;

class SEmbeddableStmt;
class SEmbeddableStmt_Single;
class SEmbeddableStmt_Block;

class SForStmtInitializer;
class SForStmtInitializer_Exp;
class SForStmtInitializer_VarDecl;

class SClassMemberDecl;
class SClassFuncDecl;
class SClassCtorDecl;
class SClassVarDecl;

class SStructMemberDecl;
class SStructFuncDecl;
class SStructCtorDecl;
class SStructVarDecl;

class SClassDecl;
class SStructDecl;
class SEnumDecl;
class SEnumElemDecl;
class SEnumElemVarDecl;

class SGlobalFuncDecl;
class SNamespaceDecl;
class SScript;
class SArgument;
class SArguments;

class SFactory
{
    std::vector<std::unique_ptr<SSyntax>> syntaxes;
    std::vector<std::unique_ptr<SArgument>> args;
    std::vector<std::unique_ptr<SArguments>> argsArray;

public:
    SYNTAX_API SFactory();
    SYNTAX_API ~SFactory();

    template<typename TSyntax, typename... TArgs> 
    TSyntax* Make(TArgs&&... args)
    { 
        auto elem = std::make_unique<TSyntax>(std::forward<TArgs>(args)...); 
        auto* pElem = elem.get(); 
        syntaxes.push_back(std::move(elem)); 
        return pElem; 
    }
    
#define MAKE(ITEM) \
    template<typename... TArgs> \
    ITEM* Make##ITEM(TArgs&&... args) \
    { \
        auto elem = std::make_unique<ITEM>(std::forward<TArgs>(args)...); \
        auto* pElem = elem.get(); \
        syntaxes.push_back(std::move(elem)); \
        return pElem; \
    }
    
    MAKE(SStmt_Command)
    MAKE(SStmt_VarDecl)
    MAKE(SStmt_If)
    MAKE(SStmt_For)
    MAKE(SStmt_While)
    MAKE(SStmt_Switch)
    MAKE(SStmt_Continue)
    MAKE(SStmt_Break)
    MAKE(SStmt_Return)
    MAKE(SStmt_Block)
    MAKE(SStmt_Blank)
    MAKE(SStmt_Exp)
    MAKE(SStmt_Task)
    MAKE(SStmt_Await)
    MAKE(SStmt_Async)
    MAKE(SStmt_Foreach)
    MAKE(SStmt_Yield)
    MAKE(SStmt_Directive)
    
    MAKE(SExp_Identifier)
    MAKE(SExp_String)
    MAKE(SExp_IntLiteral)
    MAKE(SExp_BoolLiteral)
    MAKE(SExp_NullLiteral)
    MAKE(SExp_BinaryOp)
    MAKE(SExp_UnaryOp)
    MAKE(SExp_Call)
    MAKE(SExp_Lambda)
    MAKE(SExp_Indexer)
    MAKE(SExp_Member)
    MAKE(SExp_List)
    MAKE(SExp_New)
    MAKE(SExp_Shared)
    MAKE(SExp_Is)
    MAKE(SExp_As)

    MAKE(STypeExp_Id)
    MAKE(STypeExp_Member)
    MAKE(STypeExp_Nullable)
    MAKE(STypeExp_Shared)
    MAKE(STypeExp_Ptr)
    MAKE(STypeExp_Local)

    MAKE(SVarDeclType_Var)
    MAKE(SVarDeclType_VarRef)
    MAKE(SVarDeclType_Ref)
    MAKE(SVarDeclType_Normal)

    MAKE(SStringExpElement_Text)
    MAKE(SStringExpElement_Exp)

    MAKE(SLambdaExpBody_Stmts)
    MAKE(SLambdaExpBody_Exp)

    MAKE(SEmbeddableStmt_Single)
    MAKE(SEmbeddableStmt_Block)
    
    MAKE(SForStmtInitializer_Exp)
    MAKE(SForStmtInitializer_VarDecl)

    MAKE(SClassMemberDecl)
    MAKE(SClassCtorDecl)
    MAKE(SClassVarDecl)

    MAKE(SStructMemberDecl)
    MAKE(SStructCtorDecl)
    MAKE(SStructDtorDecl)
    MAKE(SStructVarDecl)

    MAKE(SNamespaceDeclElement)
    MAKE(SClassDecl)
    MAKE(SStructDecl)
    MAKE(SEnumDecl)
    MAKE(SEnumElemDecl)
    MAKE(SEnumElemVarDecl)
    MAKE(SNamespaceDecl)
    MAKE(SScript)

    template<typename TSSyntax, typename... TArgs> requires std::derived_from<TSSyntax, SSyntax>
    TSSyntax* Make(TArgs&&... args)
    {
        auto elem = std::make_unique<TSSyntax>(std::forward<TArgs>(args)...);
        auto* pElem = elem.get();
        syntaxes.push_back(std::move(elem));
        return pElem;
    }

    template<typename... TArgs> 
    SArgument* MakeSArgument(TArgs&&... args)
    {
        auto elem = std::make_unique<SArgument>(std::forward<TArgs>(args)...);
        auto* pElem = elem.get();
        this->args.push_back(std::move(elem));
        return pElem;
    }

    template<typename... TArgs>
    SArguments* MakeSArguments(TArgs&&... args)
    {
        auto elem = std::make_unique<SArguments>(std::forward<TArgs>(args)...);
        auto* pElem = elem.get();
        this->argsArray.push_back(std::move(elem));
        return pElem;
    }

};


} // namespace Citron