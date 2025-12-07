
add_library(SyntaxIR0Translator SHARED)

set_target_properties(SyntaxIR0Translator PROPERTIES DEFINE_SYMBOL SYNTAXIR0TRANSLATOR_EXPORTS)

target_include_directories(SyntaxIR0Translator
        PRIVATE Public/SyntaxIR0Translator
        PUBLIC Public)

target_link_libraries(SyntaxIR0Translator PRIVATE
        Common
        Infra
        Syntax
        Logging
        ESymbol
        RSymbol
        NSymbol
        MIR
)
target_sources(SyntaxIR0Translator
        PRIVATE
        BinOpQueryService.cpp
        BuildTypeDependentSymbolContext.cpp
        CommonTranslation.cpp
        DeclTypeInfo.cpp
        DesignatedDiagnostic.cpp
        EnumElemVarTask.cpp
        FuncContext.cpp
        FuncMatching.cpp
        FuncsWithPartialTypeArgsComponent.cpp
        GlobalContext.cpp
        GlobalFuncTask.cpp
        ImCallableAndSArgsToMExpTranslation.cpp
        ImExp.cpp
        ImExpAndMemberNameToImExpTranslation.cpp
        ImExpToIrExpTranslation.cpp
        ImExpToReExpTranslation.cpp
        IrExp.cpp
        IrExpAndMemberNameToIrExpTranslation.cpp
        IrExpToMExpTranslation.cpp
        Misc.cpp
        ModuleDecls.cpp
        PhaseManager.cpp
        ReExp.cpp
        ReExpToMExpTranslation.cpp
        ReExpToMLocTranslation.cpp
        ResolveTypeHierarchyContext.cpp
        RFuncAndRArgsToMExpTranslation.cpp
        ScopeContext.cpp
        SExpRefToIrExpTranslation.cpp
        SExpRefToMExpTranslation.cpp
        SExpToImExpTranslation.cpp
        SExpToMExpTranslation.cpp
        SExpToMLocTranslation.cpp
        SExpToReExpTranslation.cpp
        SRTFactory.cpp
        SStmtToMStmtTranslation.cpp
        StructCtorTask.cpp
        StructFuncTask.cpp
        StructTask.cpp
        StructVarTask.cpp
        SVarDeclToMStmtsTranslation.cpp
        SyntaxIR0Translator.cpp
        TranslateBodyContext.cpp
        TranslationContext.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/SyntaxIR0Translator/SyntaxIR0Translator.h
        Public/SyntaxIR0Translator/SyntaxIR0TranslatorConfig.h
        BinOpQueryService.h
        BuildTypeDependentSymbolContext.h
        CommonTranslation.h
        DeclTypeInfo.h
        DesignatedDiagnostic.h
        EnumElemVarTask.h
        FuncContext.h
        FuncMatching.h
        FuncsWithPartialTypeArgsComponent.h
        GlobalContext.h
        GlobalFuncTask.h
        ImCallableAndSArgsToMExpTranslation.h
        ImExp.h
        ImExpAndMemberNameToImExpTranslation.h
        ImExpToIrExpTranslation.h
        ImExpToReExpTranslation.h
        IrExp.h
        IrExpAndMemberNameToIrExpTranslation.h
        IrExpToMExpTranslation.h
        Misc.h
        ModuleDecls.h
        PhaseManager.h
        ReExp.h
        ReExpToMExpTranslation.h
        ReExpToMLocTranslation.h
        ResolveTypeHierarchyContext.h
        RFuncAndRArgsToMExpTranslation.h
        ScopeContext.h
        SExpRefToIrExpTranslation.h
        SExpRefToMExpTranslation.h
        SExpToImExpTranslation.h
        SExpToMExpTranslation.h
        SExpToMLocTranslation.h
        SExpToReExpTranslation.h
        SRTFactory.h
        SStmtToMStmtTranslation.h
        StructCtorTask.h
        StructFuncTask.h
        StructTask.h
        StructVarTask.h
        SVarDeclToMStmtsTranslation.h
        TranslateBodyContext.h
        TranslationContext.h
        TranslationTasks.h

)