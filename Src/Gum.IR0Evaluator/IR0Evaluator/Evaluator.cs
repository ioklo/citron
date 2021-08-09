using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Infra;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;
using Gum.Collections;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public partial class Evaluator
    {
        public static async ValueTask<int> EvalAsync(ImmutableArray<IModuleDriver> moduleDrivers, ICommandProvider commandProvider, R.Script script)
        {   
            var globalContext = new GlobalContext(commandProvider);

            foreach (var moduleDriver in moduleDrivers)
            {
                var containerInfos = moduleDriver.GetRootContainers();
                foreach (var containerInfo in containerInfos)
                    globalContext.AddRootItemContainer(containerInfo.ModuleName, containerInfo.Container);
            }

            var rootItemContainer = new ItemContainer();
            DeclEvaluator.EvalDecls(globalContext, rootItemContainer, new R.Path.Root(script.Name), 0, script.Decls);
            globalContext.AddRootItemContainer(script.Name, rootItemContainer);

            var topLevelRetValue = new IntValue();
            var context = new EvalContext(topLevelRetValue);
            var localContext = new LocalContext(ImmutableDictionary<string, Value>.Empty);
            var localTaskContext = new LocalTaskContext();

            await StmtEvaluator.EvalTopLevelStmtsAsync(globalContext, context, localContext, localTaskContext, script.TopLevelStmts);
            
            return ((IntValue)context.GetRetValue()).GetInt();
        }

        static bool GetBaseType(R.Path type, [NotNullWhen(true)] out R.Path? outBaseType)
        {
            throw new NotImplementedException();
            //var typeInst = context.GetTypeInst(type);

            //if (typeInst is ClassInst classInst)
            //{
            //    var baseType = classInst.GetBaseType();
            //    if (baseType != null)
            //    {
            //        outBaseType = baseType;
            //        return true;
            //    }
            //}

            //outBaseType = null;
            //return false;
        }
        
        // xType이 y타입인가 묻는 것
        static bool IsType(R.Path xType, R.Path yType)
        {
            R.Path? curType = xType;

            while (curType != null)
            {
                if (EqualityComparer<R.Path?>.Default.Equals(curType, yType))
                    return true;

                if (!GetBaseType(curType, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curType = baseTypeValue;
            }

            return false;
        }

        // 캡쳐는 람다 Value안에 값을 세팅한다        
        static void CaptureLocals(EvalContext context, LocalContext localContext, Value? capturedThis, ImmutableDictionary<string, Value> localVars, R.CapturedStatement capturedStatement)
        {
            if (capturedStatement.ThisType != null)
            {
                Debug.Assert(capturedThis != null);
                capturedThis.SetValue(context.GetThisValue()!);
            }

            foreach (var typeAndName in capturedStatement.OuterLocalVars)
            {
                var origValue = localContext.GetLocalValue(typeAndName.Name);
                localVars[typeAndName.Name].SetValue(origValue);
            }
        }        
    }
}