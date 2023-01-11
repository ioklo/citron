using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Citron.Symbol;
using Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

struct FuncBodyVisitor
{
    GlobalContext globalContext;

    public void VisitGlobalFuncDecl(GlobalFuncDeclSymbol symbol, GlobalFuncDecl syntax)
    {
        var retType = globalContext.MakeType(funcDecl.RetType);

        var rname = new R.Name.Normal(funcDecl.Name);
        var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
        var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: Global이므로 상위에 type parameter가 없다

        var funcContext = new BodyContext(null, retTypeValue, true, false, MakePath(rname, rparamHash, rtypeArgs));
        var localContext = new LocalContext();

        int paramCount = symbol.GetParameterCount();
        for(int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            localContext.AddLocalVarInfo(parameter);
        }

        var stmtVisitor = new StmtVisitor(globalContext, funcContext, localContext);

        // 파라미터 순서대로 추가
        foreach (var param in funcDecl.Parameters)
        {
            var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
            localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
        }

        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
        var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

        var decls = funcContext.GetCallableMemberDecls();
        var normalFuncDecl = new R.NormalFuncDecl(decls, rname, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
        rootContext.AddGlobalFuncDecl(normalFuncDecl);
    }
}
