using Gum.CompileTime;
using Gum.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gum;

namespace Gum.Runtime
{
    // TODO: 이 단계에서 Module이 필요 없을 것 같다. 일단 주석처리
    //internal class ScriptModule : IModule
    //{
    //    private ScriptModuleInfo moduleInfo;
    //    private ImmutableDictionary<ModuleItemId, ScriptTemplate> templatesById;
    //    private TypeValueApplier typeValueApplier;

    //    public ScriptModule(
    //        ScriptModuleInfo moduleInfo, 
    //        Func<ScriptModule, TypeValueApplier> typeValueApplierConstructor, 
    //        IEnumerable<ScriptTemplate> templates)
    //    {
    //        this.moduleInfo = moduleInfo;
    //        this.typeValueApplier = typeValueApplierConstructor.Invoke(this);
    //        this.templatesById = templates.ToImmutableDictionary(templ => templ.Id);
    //    }

    //    public string ModuleName => moduleInfo.ModuleName;

    //    public bool GetFuncInfo(ModuleItemId id, [NotNullWhen(true)] out FuncInfo? funcInfo)
    //    {
    //        return moduleInfo.GetFuncInfo(id, out funcInfo);
    //    }       

    //    public bool GetTypeInfo(ModuleItemId id, [NotNullWhen(true)] out ITypeInfo? typeInfo)
    //    {
    //        return moduleInfo.GetTypeInfo(id, out typeInfo);
    //    }

    //    public bool GetVarInfo(ModuleItemId id, [NotNullWhen(true)] out VarInfo? varInfo)
    //    {
    //        return moduleInfo.GetVarInfo(id, out varInfo);
    //    }

    //    public FuncInst GetFuncInst(DomainService domainService, FuncValue funcValue)
    //    {
    //        var templ = templatesById[funcValue.FuncId];

    //        if (templ is ScriptTemplate.Func funcTempl)
    //        {
    //            if (funcValue.TypeArgList.GetTotalLength() != 0)
    //                throw new NotImplementedException();

    //            return new ScriptFuncInst(
    //                funcTempl.SeqElemTypeValue, 
    //                funcTempl.bThisCall, 
    //                null, ImmutableArray<Value>.Empty, funcTempl.LocalVarCount, funcTempl.Body);
    //        }

    //        throw new InvalidOperationException();
    //    }

    //    public TypeInst GetTypeInst(DomainService domainService, TypeValue.Normal typeValue)
    //    {
    //        var templ = templatesById[typeValue.TypeId];

    //        if (templ is ScriptTemplate.Enum enumTempl)
    //        {
    //            // E<int>
    //            var defaultFieldInsts = enumTempl.DefaultFields.Select(field =>
    //            {
    //                var fieldType = typeValueApplier.Apply(typeValue, field.TypeValue);
    //                return (field.Name, domainService.GetTypeInst(fieldType));
    //            });

    //            return new EnumTypeInst(typeValue, enumTempl.DefaultElemName, defaultFieldInsts);
    //        }

    //        throw new InvalidOperationException();
    //    }

    //    public void OnLoad(DomainService domainService)
    //    {
    //    }
    //}
}