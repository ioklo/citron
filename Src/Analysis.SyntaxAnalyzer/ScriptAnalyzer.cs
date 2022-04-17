using Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Collections;
using Citron.Infra;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;

namespace Citron.Analysis
{
    struct FuncDeclSymbolSyntaxInfo
    {
        public readonly IFuncDeclSymbol Symbol;
        public readonly S.ISyntaxNode Syntax;

        public FuncDeclSymbolSyntaxInfo(IFuncDeclSymbol symbol, S.ISyntaxNode syntax)
        {
            this.Symbol = symbol;
            this.Syntax = syntax;
        }
    }

    // S.Script Analyzer
    class ScriptAnalyzer
    {
        class GlobalVarDeclComponent : VarDeclComponent
        {
            GlobalContext globalContext;
            ImmutableArray<R.Stmt>.Builder topLevelStmtsBuilder;

            ImmutableArray<R.VarDeclElement>.Builder builder;

            public GlobalVarDeclComponent(GlobalContext globalContext, StmtAndExpAnalyzer stmtAndExpAnalyzer, ImmutableArray<R.Stmt>.Builder topLevelStmtsBuilder)
                : base(globalContext, stmtAndExpAnalyzer, false)
            {
                this.globalContext = globalContext;
                this.topLevelStmtsBuilder = topLevelStmtsBuilder;
                this.builder = ImmutableArray.CreateBuilder<R.VarDeclElement>();
            }

            public override void OnElemCreated(ITypeSymbol type, string name, S.VarDeclElement selem, R.VarDeclElement elem)
            {
                if (globalContext.DoesInternalGlobalVarNameExist(name))
                    globalContext.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, selem);

                globalContext.AddInternalGlobalVarInfo(elem is R.VarDeclElement.Ref, type, name);

                builder.Add(elem);
            }

            public override void OnCompleted()
            {
                topLevelStmtsBuilder.Add(new R.GlobalVarDeclStmt(builder.ToImmutable()));
            }
        }

        GlobalContext globalContext;
        StmtAndExpAnalyzer stmtAndExpAnalyzer;
        
        ImmutableArray<R.StmtBody>.Builder stmtBodiesBuilder;        

        public static R.Script Analyze(GlobalContext globalContext, S.Script script, ModuleDeclSymbol moduleDecl, ImmutableArray<FuncDeclSymbolSyntaxInfo> funcDeclSymbolSyntaxInfos)
        {
            var localContext = new LocalContext();
            var stmtAndExpAnalyzer = new StmtAndExpAnalyzer(globalContext, bodyContext, localContext);
            
            var scriptAnalyzer = new ScriptAnalyzer(globalContext, stmtAndExpAnalyzer);
            scriptAnalyzer.Analyze(script, funcDeclSymbolSyntaxInfos);
            return scriptAnalyzer.MakeScript(moduleDecl);
        }

        ScriptAnalyzer(GlobalContext globalContext, StmtAndExpAnalyzer stmtAndExpAnalyzer)
        {
            this.globalContext = globalContext;
            this.stmtAndExpAnalyzer = stmtAndExpAnalyzer;

            this.stmtBodiesBuilder = ImmutableArray.CreateBuilder<R.StmtBody>();
        }

        void AnalyzeTopLevelStmts(S.Script script)
        {
            var topLevelStmtsBuilder = ImmutableArray.CreateBuilder<R.Stmt>();
            var globalVarDeclComponent = new GlobalVarDeclComponent(globalContext, stmtAndExpAnalyzer, topLevelStmtsBuilder);

            foreach (var elem in script.Elements)
            {
                if (elem is S.StmtScriptElement stmtElem)
                {
                    if (stmtElem.Stmt is S.VarDeclStmt varDeclStmt)
                    {
                        globalVarDeclComponent.AnalyzeVarDecl(varDeclStmt.VarDecl);
                        continue;
                    }

                    var stmtResult = stmtAndExpAnalyzer.AnalyzeStmt(stmtElem.Stmt);
                    topLevelStmtsBuilder.Add(stmtResult.Stmt);
                }   
            }

            stmtBodiesBuilder.Add(new R.StmtBody(new DeclSymbolPath(null, Name.TopLevel), topLevelStmtsBuilder.ToImmutable()));
        }

        void AnalyzeGlobalFuncDecl(GlobalFuncDeclSymbol symbol, S.GlobalFuncDecl syntax)
        {   
            var retTypeValue = globalContext.GetSymbolByTypeExp(syntax.RetType);

            var funcContext = new FuncBodyContext(symbol, null, symbol.GetReturn(), symbol.IsSequence);
            var localContext = new LocalContext();
            var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

            // 파라미터를 로컬에 순서대로 추가
            int paramCount = symbol.GetParameterCount();
            for(int i = 0; i < paramCount; i++)
            {
                var param = symbol.GetParameter(i);
                localContext.AddLocalVarInfo(param.Kind == FuncParameterKind.Ref, param.Type, param.Name);
            }
            
            analyzer.AnalyzeBody(syntax.Body);

            //var declSymbolId = symbol.GetDeclSymbolId();            
            // funcContext에 심어져 있는 Lambda가져와서 넣기
            //symbol.AddLambda(...)
            //stmtBodiesBuilder.Add(new R.StmtBody(declSymbolId.Path, analyzer.MakeStmts()));
            throw new NotImplementedException();
        }

        void Analyze(S.Script script, ImmutableArray<FuncDeclSymbolSyntaxInfo> funcDeclSymbolSyntaxInfos)
        {
            // 첫번째 페이즈, global var를 검사하는 겸 
            AnalyzeTopLevelStmts(script);

            // 두번째 페이즈, declaration을 훑는다                
            foreach (var info in funcDeclSymbolSyntaxInfos)
            {
                switch(info.Symbol)
                {
                    case GlobalFuncDeclSymbol globalFuncDeclSymbol:
                        AnalyzeGlobalFuncDecl(globalFuncDeclSymbol, (S.GlobalFuncDecl)info.Syntax);
                        break;

                    case ClassConstructorSymbol classConstructorSymbol: 
                        AnalyzeClassConstructor(classConsturctorSymbol, (S.ClassConstructorDecl)info.Syntax);
                        break;

                    case ClassMemberFuncDeclSymbol classMemberFuncSymbol: 
                        AnalyzeClassMemberFunc(classMemberFuncSymbol, (S.ClassMemberFuncDecl)info.Syntax);
                        break;

                    case StructConstructorSymbol structConstructorSymbol:
                        AnalyzeStructConstructor(structConstructorSymbol, (S.StructConstructorDecl)info.Syntax);
                        break;

                    case StructMemberFuncDeclSymbol structMemberFuncSymbol:
                        AnalyzeStructMemberFuncDecl(structMemberFuncSymbol, (S.StructMemberFuncDecl)info.Syntax);
                        break;

                    default: 
                        throw new UnreachableCodeException();
                }
            }
        }

        public R.Script MakeScript(ModuleDeclSymbol moduleDecl)
        {
            return new R.Script(moduleDecl, stmtBodiesBuilder.ToImmutable());
        }
    }
}
