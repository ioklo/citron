using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{    
    // VM이 수행할 프로그램은 cfg와 시작점
    public class Program
    {
        // 타입 정보
        Dictionary<string, TypeInfo> types = new Dictionary<string, TypeInfo>();

        // Functions
        Dictionary<string, FuncInfo> funcs = new Dictionary<string, FuncInfo>();

        public IEnumerable<FuncInfo> Funcs { get { return funcs.Values; } }
        public IDictionary<string, TypeInfo> Types { get { return types; } }        

        public FuncInfo GetFuncInfo(string name)
        {
            FuncInfo info;
            if (funcs.TryGetValue(name, out info))
                return info;

            return null;
        }

        public void AddExternFunc(string name, int args, int returns)
        {
            funcs.Add(name, new FuncInfo(name, args, returns));
        }

        public void AddFunc(string name, int args, int locals, int returns, IEnumerable<Core.IL.ICommand> commands, IEnumerable<int> jumpTable)
        {
            funcs.Add(name, new FuncInfo(name, args, locals, returns, commands, jumpTable));
        }
    }
}
