using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Gum.App.Compiler.Test
{
    /// <summary>
    /// SourceTest의 요약 설명
    /// </summary>
    [TestClass]
    public class SourceTest
    {
        public SourceTest()
        {
            //
            // TODO: 여기에 생성자 논리를 추가합니다.
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///현재 테스트 실행에 대한 정보 및 기능을
        ///제공하는 테스트 컨텍스트를 가져오거나 설정합니다.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set  
            {
                testContextInstance = value;
            }
        }

        #region 추가 테스트 특성
        //
        // 테스트를 작성할 때 다음 추가 특성을 사용할 수 있습니다.
        //
        // ClassInitialize를 사용하여 클래스의 첫 번째 테스트를 실행하기 전에 코드를 실행합니다.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // ClassCleanup을 사용하여 클래스의 테스트를 모두 실행한 후에 코드를 실행합니다.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // TestInitialize를 사용하여 각 테스트를 실행하기 전에 코드를 실행합니다.
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // TestCleanup을 사용하여 각 테스트를 실행하기 전에 코드를 실행합니다.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            string solution_dir = Path.GetDirectoryName(TestContext.TestDir);

            //foreach(var file in Directory.EnumerateFiles())

            Console.WriteLine(solution_dir);
            

            // \Proj\Gum\Src\App.Compiler.Test\1. Scope\AnonymousScope.gum

            // Parser parser = new Parser();
            // parser.ParseProgram()
            //
            // TODO: 테스트 논리를 여기에 추가합니다.
            //
        }
    }
}
