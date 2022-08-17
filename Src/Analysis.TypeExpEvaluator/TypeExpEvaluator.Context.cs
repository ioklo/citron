using System;
using System.Diagnostics.CodeAnalysis;
using Citron.Log;
using Citron.Collections;

using Citron.Syntax;
using Citron.Module;
using System.Collections.Generic;

using static Citron.Symbol.DeclSymbolPathExtensions;
using Citron.Symbol;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class Context
        {
            Name internalModuleName;
            ImmutableArray<ModuleDeclSymbol> referenceModules;
            TypeSkeletonRepository skelRepo;
            ILogger logger;

            public Context(Name internalModuleName, ImmutableArray<ModuleDeclSymbol> referenceModules, TypeSkeletonRepository skelRepo, ILogger logger)
            {
                this.internalModuleName = internalModuleName;
                this.referenceModules = referenceModules;
                this.skelRepo = skelRepo;
                this.logger = logger;
            }

            [DoesNotReturn]
            public void Throw(TypeExpErrorCode code, ISyntaxNode node, string msg)
            {
                logger.Add(new TypeExpErrorLog(code, node, msg));
                throw new FatalException();
            }

            public void AddInfo(TypeExp exp, TypeExpInfo info)
            {
                exp.Info = info;
            }

            public IEnumerable<TypeExpInfo> GetTypeExpInfos(SymbolPath path, TypeExp typeExp)
            {
                var declPath = path.GetDeclSymbolPath();

                var typeSkel = skelRepo.GetTypeSkeleton(declPath);
                if (typeSkel != null)
                {
                    yield return new InternalTypeExpInfo(new ModuleSymbolId(internalModuleName, path), typeSkel, typeExp);
                }

                // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
                foreach (var referenceModule in referenceModules)
                {
                    var declSymbol = referenceModule.GetDeclSymbol(declPath);

                    if (declSymbol is ITypeDeclSymbol typeDeclSymbol)
                    {
                        var symbolId = new ModuleSymbolId(referenceModule.GetName(), path);
                        yield return new ModuleSymbolTypeExpInfo(symbolId, typeDeclSymbol, typeExp);
                    }
                }
            }
        }
    }
}
