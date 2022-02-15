using Citron.Collections;
using Citron.CompileTime;
using System;
using System.Threading.Tasks;

namespace Citron
{
    public interface IModuleDriver
    {
        // ImmutableArray<(M.Name ModuleName, IItemContainer Container)> GetRootContainers();        
        Value Alloc(SymbolPath typePath);

        ValueTask ExecuteGlobalFuncAsync(SymbolPath globalFuncPath, ImmutableArray<Value> args, Value retValue);

        // class services 
        ValueTask ExecuteClassConstructor(SymbolPath constructorPath, ClassValue thisValue, ImmutableArray<Value> args);
        ValueTask ExecuteClassMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue);        
        void InitializeClassInstance(SymbolPath path, ImmutableArray<Value>.Builder builder);
        int GetTotalClassMemberVarCount(SymbolPath classPath);
        Value GetClassStaticMemberValue(SymbolPath path);
        int GetClassMemberVarIndex(SymbolPath path);
        SymbolId? GetBaseClass(SymbolPath path);

        // struct services
        ValueTask ExecuteStructMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue);
        ValueTask ExecuteStructConstructor(SymbolPath constructorPath, StructValue thisValue, ImmutableArray<Value> args);
        int GetStructMemberVarIndex(SymbolPath path);
        Value GetStructStaticMemberValue(SymbolPath path);        
    }
}