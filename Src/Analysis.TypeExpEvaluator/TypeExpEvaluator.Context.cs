using System;
using System.Diagnostics.CodeAnalysis;
using Citron.Log;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.CompileTime;
using System.Collections.Generic;

using static Citron.CompileTime.DeclSymbolPathExtensions;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class Context
        {
            M.Name internalModuleName;
            ImmutableArray<ModuleDeclSymbol> referenceModules;
            TypeSkeletonRepository skelRepo;
            ILogger logger;
            Dictionary<S.TypeExp, TypeExpInfo> infosByTypeExp;

            public Context(M.Name internalModuleName, ImmutableArray<ModuleDeclSymbol> referenceModules, TypeSkeletonRepository skelRepo, ILogger logger, Dictionary<S.TypeExp, TypeExpInfo> infosByTypeExp)
            {
                this.internalModuleName = internalModuleName;
                this.referenceModules = referenceModules;
                this.skelRepo = skelRepo;
                this.logger = logger;

                this.infosByTypeExp = infosByTypeExp;
            }

            [DoesNotReturn]
            public void Throw(TypeExpErrorCode code, S.ISyntaxNode node, string msg)
            {
                logger.Add(new TypeExpErrorLog(code, node, msg));
                throw new FatalException();
            }

            public void AddInfo(S.TypeExp exp, TypeExpInfo info)
            {
                infosByTypeExp.Add(exp, info);
            }

            public IEnumerable<TypeExpInfo> GetTypeExpInfos(M.SymbolPath path, S.TypeExp typeExp)
            {
                var declPath = path.GetDeclSymbolPath();

                var typeSkel = skelRepo.GetTypeSkeleton(declPath);
                if (typeSkel != null)
                {
                    yield return new InternalTypeExpInfo(new M.ModuleSymbolId(internalModuleName, path), typeSkel, typeExp);
                }

                // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
                foreach (var referenceModule in referenceModules)
                {
                    var declSymbol = referenceModule.GetDeclSymbol(declPath);

                    if (declSymbol is ITypeDeclSymbol typeDeclSymbol)
                    {
                        var symbolId = new M.ModuleSymbolId(referenceModule.GetName(), path);
                        yield return new ModuleSymbolTypeExpInfo(symbolId, typeDeclSymbol, typeExp);
                    }
                }
            }
        }
    }
}
