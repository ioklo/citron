using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gum.Runtime
{
    //class VoidTypeInst : TypeInst
    //{
    //    public static VoidTypeInst Instance { get; } = new VoidTypeInst();

    //    private VoidTypeInst() { }

    //    public override TypeValue GetTypeValue()
    //    {
    //        return TypeValue.Void.Instance;
    //    }

    //    public override Value MakeDefaultValue()
    //    {
    //        return VoidValue.Instance;
    //    }
    //}

    //class FuncTypeInst : TypeInst
    //{
    //    private TypeValue.Func typeValue;

    //    public FuncTypeInst(TypeValue.Func typeValue) { this.typeValue = typeValue; }

    //    public override TypeValue GetTypeValue()
    //    {
    //        return typeValue;
    //    }

    //    public override Value MakeDefaultValue()
    //    {
    //        return new LambdaValue();
    //    }
    //}

    //// 도메인: 프로그램 실행에 대한 격리 단위   
    //public class DomainService
    //{
    //    // 모든 모듈의 전역 변수
    //    public Dictionary<ModuleItemId, Value> globalValues { get; }
    //    List<IModule> modules;

    //    public DomainService()
    //    {
    //        globalValues = new Dictionary<ModuleItemId, Value>();
    //        modules = new List<IModule>();
    //    }

    //    public Value GetGlobalValue(ModuleItemId varId)
    //    {
    //        return globalValues[varId];
    //    }
        
    //    public void LoadModule(IModule module)
    //    {
    //        modules.Add(module);
    //        module.OnLoad(this);
    //    }
        
    //    public FuncInst GetFuncInst(FuncValue funcValue)
    //    {
    //        // TODO: caching
    //        foreach (var module in modules)
    //        {
    //            var funcItemInfo = module.GetModuleItemInfo<ModuleItemInfo.Func>(funcValue.FuncId);
    //            if( funcItemInfo != null)
    //                return module.GetFuncInst(this, funcValue);
    //        }

    //        throw new InvalidOperationException();
    //    }
        
    //    // 실행중 TypeValue는 모두 Apply된 상태이다
    //    public TypeInst GetTypeInst(TypeValue typeValue)
    //    {
    //        // X<int>.Y<short> => Tx -> int, Ty -> short
    //        switch (typeValue)
    //        {
    //            case TypeValue.TypeVar tvtv:
    //                Debug.Fail("실행중에 바인드 되지 않은 타입 인자가 나왔습니다");
    //                throw new InvalidOperationException();

    //            case TypeValue.Normal ntv:
    //                {
    //                    foreach (var module in modules)
    //                        if (module.GetTypeInfo(ntv.TypeId, out var _))
    //                            return module.GetTypeInst(this, ntv);

    //                    throw new InvalidOperationException();
    //                }

    //            case TypeValue.Void vtv:
    //                return VoidTypeInst.Instance;
                
    //            case TypeValue.Func ftv:
    //                return new FuncTypeInst(ftv);

    //            default:
    //                throw new NotImplementedException();
    //        }
    //    }
        
        
    //    public void InitGlobalValue(ModuleItemId varId, Value value)
    //    {
    //        Debug.Assert(!globalValues.ContainsKey(varId));
    //        globalValues[varId] = value;
    //    }
    //}
}
