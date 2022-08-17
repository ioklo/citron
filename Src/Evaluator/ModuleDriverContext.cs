using Citron.Module;
using Citron.Infra;
using System;
using System.Collections.Generic;

namespace Citron
{
    public class ModuleDriverContext
    {
        Evaluator evaluator;
        List<IModuleDriver> drivers;
        Dictionary<Name, IModuleDriver> moduleDriverSelector;

        public ModuleDriverContext(Evaluator evaluator)
        {
            this.evaluator = evaluator;
            this.drivers = new List<IModuleDriver>();
            this.moduleDriverSelector = new Dictionary<Name, IModuleDriver>();
        }

        public IModuleDriver GetModuleDriver(SymbolId id)
        {
            if (id is ModuleSymbolId moduleSymbolId)
            {
                // module -> moduleDriver 정보가 있어야 한다
                return moduleDriverSelector[moduleSymbolId.ModuleName];
            }

            throw new RuntimeFatalException();
        }

        public void AddDriver(IModuleDriver driver)
        {
            drivers.Add(driver);
        }

        public Evaluator GetEvaluator()
        {
            return evaluator;
        }

        public void AddModule(Name moduleName, IModuleDriver driver)
        {
            moduleDriverSelector.Add(moduleName, driver);
        }
    }
}