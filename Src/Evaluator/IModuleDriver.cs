using Citron.Collections;
using Citron.Module;
using System;
using System.Threading.Tasks;

namespace Citron
{
    public interface IModuleDriver
    {
        // ImmutableArray<(M.Name ModuleName, IItemContainer Container)> GetRootContainers();        
        Value Alloc(SymbolId type);

        ValueTask ExecuteGlobalFuncAsync(SymbolId globalFunc, ImmutableArray<Value> args, Value retValue);

        // class services 
        ValueTask ExecuteClassConstructor(SymbolId constructor, ClassValue thisValue, ImmutableArray<Value> args);
        ValueTask ExecuteClassMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue);        
        void InitializeClassInstance(SymbolId @class, ImmutableArray<Value>.Builder builder);
        int GetTotalClassMemberVarCount(SymbolId @class);
        Value GetClassStaticMemberValue(SymbolId memberVar);
        int GetClassMemberVarIndex(SymbolId memberVar);
        SymbolId? GetBaseClass(SymbolId baseClass);

        // struct services
        ValueTask ExecuteStructMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue);
        ValueTask ExecuteStructConstructor(SymbolId constructor, StructValue thisValue, ImmutableArray<Value> args);
        int GetStructMemberVarIndex(SymbolId memberVar);
        Value GetStructStaticMemberValue(SymbolId memberVar);

        int GetEnumElemMemberVarIndex(SymbolId memberVar);
    }
}