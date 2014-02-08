using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using WatiN.Core;
using WatiN.Extensions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace FourOver
{
    public class FourOverClient
    {

        public FourOverClient()
        {
        }

        public List<FrontModel> Rip(string product)
        {
            string html;
            using (var browser = new IE("https://trade.4over.com"))
            {
                TextField login = browser.TextField(Find.ById("topLogin"));
                if (login.Exists)
                {
                    login.SetText("bobk@newenglandrepro.com");
                    TextField password = browser.TextField(Find.ById("topPassword"));
                    password.SetText("nerepro");
                    Image img = browser.Image(Find.ByClass("button"));
                    img.Click();
                }
//                browser.GoTo("https://trade.4over.com/products/notepads");
                browser.GoTo("https://trade.4over.com/products/"+product);
                browser.WaitForComplete();
                html = browser.Html;
            }
            List<FrontModel> models = GetFrontModels(html);
            string filename = "4OverPostcards.txt";
            if (File.Exists(filename)) File.Delete(filename);
            foreach (FrontModel model in models)
            {
                if (model.Title.Contains("Direct  Mail")) continue;
                try
                {
                    GetProductData(model);
                    Console.WriteLine("------------ Record Data");
                    Console.WriteLine(model.ToString());
                    File.AppendAllText(filename,model.ToString());
                    Console.WriteLine("------------ End Record Data");
                }
                catch (Exception)
                {
                    Console.WriteLine("=========== Exception ===========================");
                    Console.WriteLine(model.ToString());
                    Console.WriteLine("Error getting data for model :"+model.Title + " == "+model.Link);
                    Console.WriteLine("=========== End Exception ===========================");
                }
                //break;
            }

            return models;
        }

        public List<FrontModel> GetFrontModels(string html)
        {
            HtmlAgilityPack.HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);
            List<FrontModel> retval = new List<FrontModel>();
            HtmlNodeCollection nodes = hdoc.DocumentNode.SelectNodes("//div[@class=\"alt\" or @class=\"reg\"]");
            foreach (HtmlNode node in nodes)
            {
                FrontModel model = new FrontModel();
                HtmlNodeCollection titleNode = node.SelectNodes("./div[@class=\"prodname\"]/a");
                model.Title = titleNode[0].Attributes["title"].Value;
                model.Link = titleNode[0].Attributes["href"].Value;

                HtmlNode priceNode = node.SelectSingleNode("./div[@class=\"prodprice\"]");
                model.Price = priceNode.InnerText.Replace(" ", "");
                retval.Add(model);
            }
            return retval;
        }

        public void GetProductData(FrontModel model)
        {
            using (var browser = new IE("https://trade.4over.com" + model.Link))
            {
                browser.WaitForComplete();
                SelectList runsizeList = browser.SelectList(Find.ById("runsize"));
                StringCollection items = runsizeList.AllContents;
                items.RemoveAt(0);
                foreach (string runsize in items)
                {
                    int parsed = Convert.ToInt32(runsize);
                    if (parsed >= 10000) continue;
                    runsizeList.Select(runsize);
                    browser.WaitForComplete();
                    SelectList colorList = browser.SelectList(Find.ById("color"));
                    StringCollection colorItems = colorList.AllContents;
                    colorItems.RemoveAt(0);
                    foreach (string color in colorItems)
                    {
                        colorList.Select(color);
                        browser.WaitForComplete();
                        SelectList turnAroundTimeList = browser.SelectList(Find.ById("tat"));
                        StringCollection turnAroundTimeItems = turnAroundTimeList.AllContents;
                        turnAroundTimeItems.RemoveAt(0);
                        foreach (string turnAroundTime in turnAroundTimeItems)
                        {
                            turnAroundTimeList.Select(turnAroundTime);
                            browser.WaitForComplete();

                            browser.RadioButton(Find.ById("job_1_ship_type_EQ_BILL")).Checked = true;
                            browser.WaitUntilContainsText("Job will be shipped to:");

                            SelectList shippingList = browser.SelectList(Find.ById("job_1_shipping_select"));

                            Span subTotalSpan = browser.Span(Find.ById("subby"));
                            string subtotal = subTotalSpan.Text;
                            
                            PriceRecord priceRecord = new PriceRecord() { Color = color, Price = subtotal, RunSize = runsize, TurnAroundTime = turnAroundTime };
                            priceRecord.ShippingList = new List<string>();
                            foreach (string shippingOption in shippingList.AllContents)
                            {
                                priceRecord.ShippingList.Add(shippingOption);
                            }

                            model.Prices.Add(priceRecord);
                        }
                    }
                }
            }
        }
    }



    public class FrontModel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Price { get; set; }
        public List<PriceRecord> prices = new List<PriceRecord>();

        public List<PriceRecord> Prices
        {
            get { return this.prices; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string rowPrototype = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n";
            foreach (PriceRecord priceRecord in Prices)
            {
                string shipping = string.Join("\t", priceRecord.ShippingList);
                sb.AppendFormat(rowPrototype, "Postcards", Title, priceRecord.RunSize, priceRecord.Color,
                    priceRecord.TurnAroundTime, priceRecord.Price,shipping);
            }
            return sb.ToString();
        }
    }

    public class PriceRecord
    {
        public string RunSize { get; set; }
        public string Color { get; set; }
        public string TurnAroundTime { get; set; }
        public string Price { get; set; }
        public List<string> ShippingList { get; set; }
    }
}
