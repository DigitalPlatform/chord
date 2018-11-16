using DigitalPlatform.Z3950;
using DigitalPlatform.Z3950.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTestZ3950
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            BerTree tree = new BerTree();
            BerNode root = null;
            root = tree.m_RootNode.NewChildConstructedNode(
                BerTree.z3950_presentResponse,
                BerNode.ASN1_CONTEXT);
            byte[] baPackage = null;
            root.EncodeBERPackage(ref baPackage);
        }

        [TestMethod]
        public void TestMethod2()
        {
            PresentRequestInfo request = new PresentRequestInfo();
            List<RetrivalRecord> records = new List<RetrivalRecord>();
            DiagFormat diag = null;
            // 编码Present响应包
            int nRet = ZProcessor.Encode_PresentResponse(request,
                records,
                diag,
                0, // e.TotalCount,
                out byte[] baResponsePackage);
            //if (nRet == -1)
            //    goto ERROR1;
        }

        // 检查一个已知的死循环错误
        [TestMethod]
        public void TestMethod3()
        {
            byte[] buffer = Convert.FromBase64String("41YAAAABEKq7zN3u/wARIjNEVWZ3iJkBAAAAJoUGAAAAAgEAAQYAbXlkZWFyAwEAETwAAAADAQD57BfsFwMBAPoeQhM0AwEA/rQBAAADAQDuDOmJFAAAAA==");
            // 分析请求包
            BerTree tree1 = new BerTree();
            tree1.m_RootNode.BuildPartTree(buffer,
                0,
                buffer.Length,
                out int nTotallen);

            BerNode root = tree1.GetAPDuRoot();

            switch (root.m_uTag)
            {
                case BerTree.z3950_initRequest:
                    break;
                case BerTree.z3950_searchRequest:
                    break;
                case BerTree.z3950_presentRequest:
                    break;
            }
        }
    }
}
