using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FourOverTest
{
    [TestClass]
    public class MungeData
    {
        [TestMethod]
        public void Munge()
        {
            List<string> newRows = new List<string>();
            foreach (string line in File.ReadLines(@"C:\Projects\NER\FourOver\FourOver\bin\4OverFlyersAndBrochures.txt"))
            {
                string[] bits = line.Split('\t');
                List<string> newRow = new List<string>(bits);
                int insertPosition = 5;
                int pricePosition = 5;
                int count = 0;
                foreach (string bit in bits)
                {
                    if (count <= 5 || bit.StartsWith("UPS"))
                    {
                        count++;
                        continue;
                    }
                    if (bit.Contains("$"))
                    {
                        int dollarPos = bit.IndexOf('$');
                        int spacePos = bit.IndexOf(' ', dollarPos);
                        string additionalPrice = bit.Substring(dollarPos+1, spacePos-dollarPos);
                        decimal additionalPriceDecimal = Convert.ToDecimal(additionalPrice);
                        string price = bits[pricePosition];
                        decimal priceDecimal = Convert.ToDecimal(price.Substring(1, price.Length-2));
                        decimal newPriceDecimal = priceDecimal + additionalPriceDecimal;
                        string newPrice = "$" + newPriceDecimal;


                        string foldType = bit.Substring(0, bit.IndexOf('(')-1);

                        newRow[pricePosition] = newPrice;
                        newRow.Insert(insertPosition, foldType);
                        newRows.Add(string.Join("\t", newRow));
                        newRow = new List<string>(bits);
                        count++;
                    }
                }
            }
            foreach (string row in newRows)
            {
                File.AppendAllText("blah.txt",row+"\r\n");
            }
        }
    }
}
