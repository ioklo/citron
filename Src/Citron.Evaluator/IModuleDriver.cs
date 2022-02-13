using Gum.Collections;
using Gum.CompileTime;
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
        void ExecuteClassConstructor(SymbolPath constructorPath, ClassValue thisValue, ImmutableArray<Value> args);
        ValueTask ExecuteClassMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue);        
        void InitializeClassInstance(SymbolPath path, ImmutableArray<Value>.Builder builder);
        int GetTotalClassMemberVarCount(SymbolPath classPath);
        int GetClassMemberVarIndex(SymbolPath path);

        // struct services
        ValueTask ExecuteStructMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue);
        void ExecuteStructConstructor(SymbolPath constructorPath, StructValue thisValue, ImmutableArray<Value> args);
        
    }
}