using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;

namespace SearchMomo
{
    public class SearchMomo
    {
        static private IWebDriver driver_;

        static public bool FindPrice(string SearchKeyWord, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            driver_ = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            driver_.Navigate().GoToUrl("http://www.momoshop.com.tw");


            driver_.FindElement(By.CssSelector("#keyword")).SendKeys(SearchKeyWord);
            driver_.FindElement(By.CssSelector(".inputbtn")).Click();

            try
            {
                if (driver_.FindElements(By.XPath("//*[@id=\"BodyBase\"]/form[1]/div/div[2]/div[5]/ul/li")).ToList().Count == 0)
                {
                    ReadProductDetail(driver_.Url, ref ProductDetail);
                }
                else
                {
                    var PageCount = driver_.FindElements(By.XPath("//*[@id=\"BodyBase\"]/form[1]/div/div[2]/div[5]/ul/li")).ToList().Count;

                    for (var CurrentPage = 1; CurrentPage <= PageCount;)
                    {
                        List<IWebElement> products = driver_.FindElements(By.XPath("//*[@id=\"chessboard\"]/li/a")).ToList();
                        foreach (var product in products)
                        {
                            ReadProductDetail(product.GetAttribute("href"), ref ProductDetail);
                        }

                        //Last Page
                        if (CurrentPage == PageCount)
                        {
                            break;
                        }

                        //Next Page
                        ++CurrentPage;
                        driver_.FindElement(By.XPath(string.Format("//*[@id=\"BodyBase\"]/form[1]/div/div[2]/div[5]/ul/li[{0}]/a", CurrentPage))).Click();
                    }
                    
                }
            }
            catch (NoSuchElementException)
            {
            }

            driver_.Quit();
            return true;
        }

        static private bool ReadProductDetail(string URL, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            IWebDriver LocalDriver = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            //IWebDriver LocalDriver = new FirefoxDriver();

            LocalDriver.Navigate().GoToUrl(URL);

            Dictionary<ProductInfo.ProductInfo, string> info = new Dictionary<ProductInfo.ProductInfo, string>();

            info.Add(ProductInfo.ProductInfo.URL, URL);
            info.Add(ProductInfo.ProductInfo.name, LocalDriver.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/div[1]/h1")).Text);
            info.Add(ProductInfo.ProductInfo.price, LocalDriver.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/div[1]/table[1]/tbody/tr[2]/td/span/b")).Text);

            LocalDriver.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/ul/li[2]/b")).Click();
            
            var spec = "";
            while(spec == "")
            {
                spec = LocalDriver.FindElement(By.CssSelector("#productForm > div.prdwarp.bt770class > div.vendordetailview.specification")).Text;
                Thread.Sleep(500);
            }
            List<string> spec_split = spec.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            foreach (var detail in spec_split)
            {
                if (detail.Contains("重量") || detail.Contains("容量") || detail.Contains("規格") || detail.Contains("內容量"))
                {
                    if (detail.Contains(":"))
                    {
                        info.Add(ProductInfo.ProductInfo.spec, detail.Split(':')[1]);
                    }
                    else if (detail.Contains("："))
                    {
                        info.Add(ProductInfo.ProductInfo.spec, detail.Split('：')[1]);
                    }
                    
                    break;
                }
            }

            ProductDetail.Add(info[ProductInfo.ProductInfo.name], info);

            LocalDriver.Quit();
            return true;
        }
    }
}
