using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FourOver;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FourOverTest
{
    [TestClass]
    public class FourOverClientTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            FourOverClient client = new FourOverClient();
            Assert.IsNotNull(client);
            List<FrontModel> models = client.Rip("postcards","PostCards");
        }

        [TestMethod]
        public void ParseTest()
        {
            string html =
                new StreamReader(
                    this.GetType().Assembly.GetManifestResourceStream("FourOverTest.TestFiles.NotePadsPage1.html"))
                    .ReadToEnd();
            Assert.IsNotNull(html);
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            HtmlNodeCollection nodes = hdoc.DocumentNode.SelectNodes("//div[@class=\"prodname\"]/..");
            Assert.IsTrue(nodes.Count > 0);
        }

        [TestMethod]
        public void GetFrontModelsTest()
        {
            string html =
                new StreamReader(
                    this.GetType().Assembly.GetManifestResourceStream("FourOverTest.TestFiles.NotePadsPage1.html"))
                    .ReadToEnd();
            Assert.IsNotNull(html);
            FourOverClient client = new FourOverClient();
            List<FrontModel> models = client.GetFrontModels(html);
            Assert.IsNotNull(models);
        }
    }
}
