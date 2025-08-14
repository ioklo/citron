
add_library(SyntaxIR0Translator SHARED)

target_compile_options(SyntaxIR0Translator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(SyntaxIR0Translator
        PRIVATE Public/SyntaxIR0Translator
        PUBLIC Public)

target_link_libraries(SyntaxIR0Translator PRIVATE Infra Syntax Logging Symbol IR0)

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
        ImCallableAndSArgsToNExpTranslation.cpp
        ImExp.cpp
        ImExpAndMemberNameToImExpTranslation.cpp
        ImExpToIrExpTranslation.cpp
        ImExpToReExpTranslation.cpp
        IrExp.cpp
        IrExpAndMemberNameToIrExpTranslation.cpp
        IrExpToNExpTranslation.cpp
        MemberDeclPhaseContext.cpp
        Misc.cpp
        ModuleDecls.cpp
        RFuncAndRArgsToNExpTranslation.cpp
        ReExp.cpp
        ReExpToNExpTranslation.cpp
        ReExpToNLocTranslation.cpp
        ResolveIdentifierError.cpp
        SExpRefToIrExpTranslation.cpp
        SExpRefToNExpTranslation.cpp
        SExpToImExpTranslation.cpp
        SExpToNExpTranslation.cpp
        SExpToNLocTranslation.cpp
        SExpToReExpTranslation.cpp
        SStmtToNStmtTranslation.cpp
        SVarDeclToNStmtsTranslation.cpp
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
        ImCallableAndSArgsToNExpTranslation.h
        ImExp.h
        ImExpAndMemberNameToImExpTranslation.h
        ImExpToIrExpTranslation.h
        ImExpToReExpTranslation.h
        IrExp.h
        IrExpAndMemberNameToIrExpTranslation.h
        IrExpToNExpTranslation.h
        MemberDeclPhaseContext.h
        Misc.h
        ModuleDecls.h
        RFuncAndRArgsToNExpTranslation.h
        ReExp.h
        ReExpToNExpTranslation.h
        ReExpToNLocTranslation.h
        ResolveIdentifierError.h
        SExpRefToIrExpTranslation.h
        SExpRefToNExpTranslation.h
        SExpToImExpTranslation.h
        SExpToNExpTranslation.h
        SExpToNLocTranslation.h
        SExpToReExpTranslation.h
        SStmtToNStmtTranslation.h
        SVarDeclToNStmtsTranslation.h
        ScopeContext.h
        SkeletonPhaseContext.h
        SRTFactory.h
        StructTranslation.h
        SyntaxIR0Translator-HeaderUnits.h
        SyntaxIR0Translator.h
        SyntaxIR0TranslatorConfig.h
        TranslationContext.h

)