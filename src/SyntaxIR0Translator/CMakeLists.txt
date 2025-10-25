
add_library(SyntaxIR0Translator SHARED)

target_compile_options(SyntaxIR0Translator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(SyntaxIR0Translator
        PRIVATE Public/SyntaxIR0Translator
        PUBLIC Public)

target_link_libraries(SyntaxIR0Translator PRIVATE Infra Syntax Logging ESymbol RSymbol NSymbol MIR)

target_sources(SyntaxIR0Translator
        PRIVATE
        BinOpQueryService.cpp
        BodyPhaseContext.cpp
        CommonTranslation.cpp
        DeclTypeInfo.cpp
        DesignatedDiagnostic.cpp
        EnumTranslation.cpp
        FuncContext.cpp
        FuncMatching.cpp
        FuncsWithPartialTypeArgsComponent.cpp
        ImCallableAndSArgsToMExpTranslation.cpp
        ImExp.cpp
        ImExpAndMemberNameToImExpTranslation.cpp
        ImExpToIrExpTranslation.cpp
        ImExpToReExpTranslation.cpp
        IrExp.cpp
        IrExpAndMemberNameToIrExpTranslation.cpp
        IrExpToMExpTranslation.cpp
        MemberDeclPhaseContext.cpp
        Misc.cpp
        ModuleDecls.cpp
        RFuncAndRArgsToMExpTranslation.cpp
        ReExp.cpp
        ReExpToMExpTranslation.cpp
        ReExpToMLocTranslation.cpp
        ResolveIdentifierError.cpp
        SExpRefToIrExpTranslation.cpp
        SExpRefToMExpTranslation.cpp
        SExpToImExpTranslation.cpp
        SExpToMExpTranslation.cpp
        SExpToMLocTranslation.cpp
        SExpToReExpTranslation.cpp
        SStmtToMStmtTranslation.cpp
        SVarDeclToMStmtsTranslation.cpp
        ScopeContext.cpp
        SkeletonPhaseContext.cpp
        SRTFactory.cpp
        StructTranslation.cpp
        SyntaxIR0Translator.cpp
        TranslationContext.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        BinOpQueryService.h
        BodyPhaseContext.h
        CommonTranslation.h
        DeclTypeInfo.h
        DesignatedDiagnostic.h
        EnumTranslation.h
        FuncContext.h
        FuncMatching.h
        FuncsWithPartialTypeArgsComponent.h
        ImCallableAndSArgsToMExpTranslation.h
        ImExp.h
        ImExpAndMemberNameToImExpTranslation.h
        ImExpToIrExpTranslation.h
        ImExpToReExpTranslation.h
        IrExp.h
        IrExpAndMemberNameToIrExpTranslation.h
        IrExpToMExpTranslation.h
        MemberDeclPhaseContext.h
        Misc.h
        ModuleDecls.h
        RFuncAndRArgsToMExpTranslation.h
        ReExp.h
        ReExpToMExpTranslation.h
        ReExpToMLocTranslation.h
        ResolveIdentifierError.h
        SExpRefToIrExpTranslation.h
        SExpRefToMExpTranslation.h
        SExpToImExpTranslation.h
        SExpToMExpTranslation.h
        SExpToMLocTranslation.h
        SExpToReExpTranslation.h
        SStmtToMStmtTranslation.h
        SVarDeclToMStmtsTranslation.h
        ScopeContext.h
        SkeletonPhaseContext.h
        SRTFactory.h
        StructTranslation.h
        SyntaxIR0Translator.h
        SyntaxIR0TranslatorConfig.h
        TranslationContext.h

)