using Citron.Module;
using Citron.IR0;
using Citron.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Citron.Runtime
{
    public class UnitTest1
    {
        [Fact]
        public static void Temp()
        {
            //var runtimeModule = new RuntimeModule(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Directory.GetCurrentDirectory());

            //ModuleInfoService moduleInfoService = new ModuleInfoService(new IModuleInfo[] { runtimeModule });

            //DomainService domainService = new DomainService();

            //domainService.LoadModule(runtimeModule);

            //var intTypeId = RuntimeModule.IntId;
            //var listTypeId = RuntimeModule.ListId;

            //// int
            //var intTypeValue = new NormalTypeValue(intTypeId, TypeArgumentList.Empty);           
            

            //// List<int>.Add
            //var listAddFuncId = listTypeId.Append("Add", 0);
            //var listAddFuncTypeArgList = TypeArgumentList.Make(new[] { intTypeValue }, new TypeValue[] { }); // 첫번째는 List, 두번째는 Add에 대한 typeArgs
            //var funcInst = domainService.GetFuncInst(new FuncValue(listAddFuncId, listAddFuncTypeArgList));
            

            //// list = [1, 2]
            //var list = runtimeModule.MakeList(domainService, intTypeValue, new List<Value> { runtimeModule.MakeInt(1), runtimeModule.MakeInt(2) });

            //// List<int>.Add(list, 3)
            //if( funcInst is NativeFuncInst nativeFuncInst )
            //    nativeFuncInst.CallAsync(list, new Value[] { runtimeModule.MakeInt(3) }, VoidValue.Instance);

            //// [1, 2, 3]
            //Assert.True(list is ObjectValue objValue && 
            //    objValue.Object is ListObject lstObj &&
            //    runtimeModule.GetInt(lstObj.Elems[0]) == 1 &&
            //    runtimeModule.GetInt(lstObj.Elems[1]) == 2 &&
            //    runtimeModule.GetInt(lstObj.Elems[2]) == 3);

            //// List<int>
            ////var listIntTypeValue = QsTypeValue.MakeNormal(null, listType.TypeId, QsTypeValue.MakeNormal(null, intType.TypeId));
            ////var listIntAddFuncValue = new QsFuncValue(listIntTypeValue, funcInfo.Value.FuncId);

            ////// List<T>.Add
            ////// (List<>.Add), (T(List) => int)

            ////// 누가 TypeValue를 TypeInst로 만들어주나.. DomainService
            ////var typeInstEnv = domainService.MakeTypeInstEnv(listIntAddFuncValue);

            ////var funcInst = domainService.GetFuncInst(listIntAddFuncValue) as QsNativeFuncInst;

            //// FuncValue를 만들어 보자 
        }
    }
}
