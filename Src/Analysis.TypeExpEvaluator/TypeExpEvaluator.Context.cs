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

            public IEnumerable<(ModuleSymbolId, ITypeDeclSymbol)> QuerySymbolsOnReference(SymbolPath path)
            {
                var declPath = path.GetDeclSymbolPath();

                foreach (var referenceModule in referenceModules)
                {
                    var declSymbol = referenceModule.GetDeclSymbol(declPath);
                    if (declSymbol is ITypeDeclSymbol typeDeclSymbol)
                    {
                        var symbolId = new ModuleSymbolId(referenceModule.GetName(), path);
                        yield return (symbolId, typeDeclSymbol);
                    }
                }
            }
        }
    }
}
