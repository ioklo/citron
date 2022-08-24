using System;
using System.Diagnostics.CodeAnalysis;
using Citron.Log;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.Module;
using System.Collections.Generic;

using static Citron.Symbol.DeclSymbolPathExtensions;
using Citron.Symbol;
using Citron.Infra;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        // global
        class GlobalContext
        {
            M.Name internalModuleName;
            ImmutableArray<ModuleDeclSymbol> referenceModules;
            Skeleton moduleSkel;
            ILogger logger;

            public GlobalContext(M.Name internalModuleName, ImmutableArray<ModuleDeclSymbol> referenceModules, Skeleton moduleSkel, ILogger logger)
            {
                this.internalModuleName = internalModuleName;
                this.referenceModules = referenceModules;
                this.moduleSkel = moduleSkel;
                this.logger = logger;
            }

            [DoesNotReturn]
            public void Throw(TypeExpErrorCode code, S.ISyntaxNode node, string msg)
            {
                logger.Add(new TypeExpErrorLog(code, node, msg));
                throw new FatalException();
            }

            public void AddInfo(S.TypeExp exp, S.TypeExpInfo info)
            {
                exp.Info = info;
            }

            // 타입에 관련된 Skeleton만 검색한다
            Skeleton GetTypeSkeleton(DeclSymbolPath? path)
            {
                if (path == null)
                    return moduleSkel;

                var outerSkel = GetSkeleton(path.Outer);
                outerSkel.Get
            }

            // path는 fullPath
            public Candidates<Func<S.TypeExp, S.TypeExpInfo>> MakeCandidates(SymbolPath path)
            {
                var candidates = new Candidates<Func<S.TypeExp, S.TypeExpInfo>>();
                var declPath = path.GetDeclSymbolPath();

                var typeSkel = skelRepo.GetTypeSkeleton(declPath);
                if (typeSkel != null)
                {
                    candidates.Add(typeExp => new InternalTypeExpInfo(new ModuleSymbolId(internalModuleName, path), typeSkel, typeExp));
                }

                // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
                foreach (var referenceModule in referenceModules)
                {
                    var declSymbol = referenceModule.GetDeclSymbol(declPath);

                    if (declSymbol is ITypeDeclSymbol typeDeclSymbol)
                    {
                        var symbolId = new ModuleSymbolId(referenceModule.GetName(), path);
                        candidates.Add(typeExp => new ModuleSymbolTypeExpInfo(symbolId, typeDeclSymbol, typeExp));
                    }
                }

                return candidates;
            }
        }
    }
}
