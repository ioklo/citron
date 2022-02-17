using System;
using Citron.CompileTime;

namespace Citron
{
    public class EvalThread
    {
        Evaluator evaluator;

        // stack정보 => increasing
        // | ... | v | ... | args ... | savedBase | savedRet | locals ... | temps ... |
        //      ret                  base                                            cur
        //const int STACK_SIZE = 1024 * 1024;

        //unsafe void* stackPtr;
        //unsafe void* basePtr;
        //unsafe void* retPtr;
        //unsafe byte* curPtr; // increase 하기 위해서 byte

        public EvalThread()
        {   
            //var stack = Marshal.AllocHGlobal(STACK_SIZE);
            //GC.AddMemoryPressure(STACK_SIZE);

            //unsafe
            //{
            //    byte* stackPtr = (byte*)stack.ToPointer();

            //    // -------------------------------
            //    // |retPtr|prevBasePtr|prevRetPtr|
            //    // -------------------------------      
            //    // stackPtr
            //    //        basePtr                curPtr
            //    int* retPtr = (int*)stackPtr; // int 할당
            //    void** prevBasePtr = (void**)(stackPtr + sizeof(int));
            //    void** prevRetPtr = (void**)(stackPtr + sizeof(int) + sizeof(byte*));

            //    basePtr = stackPtr + sizeof(int);
            //    curPtr = stackPtr + sizeof(int) + sizeof(byte*) * 2;

            //    *prevBasePtr = null;
            //    *prevRetPtr = null;
            //}
        }

        public TValue StackAlloc<TValue>(SymbolId typeId)
            where TValue : Value
        {
            return evaluator.AllocValue<TValue>(typeId);

            //unsafe
            //{
            //    byte* result = curPtr;
            //    curPtr += size;
            //    return (IntPtr)result;
            //}
        }

        public void NewFrame(Value newRetValue)
        {
            throw new NotImplementedException();

            //// base ret cur
            //unsafe
            //{
            //    void** savedBasePtr = (void**)curPtr;
            //    void** savedRetPtr = (void**)(curPtr + sizeof(void*));

            //    // savedBase, savedRet
            //    *savedBasePtr = basePtr;
            //    *savedRetPtr = retPtr;

            //    basePtr = curPtr;
            //    curPtr += sizeof(void*) * 2; // savedBase, savedRet
            //    retPtr = newRetPtr.ToPointer();
            //}
        }

        // 언제 사라질지 정하지 못한다
        //~EvalThread()
        //{
        //    unsafe
        //    {
        //        Marshal.FreeHGlobal(new IntPtr(stackPtr));
        //        GC.RemoveMemoryPressure(STACK_SIZE);
        //    }
        //}
    }
}
