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
        private IWebDriver LocalDriver_;
        private string SearchKeyWord_;
        private Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail_;

        public SearchMomo(string SearchKeyWord, ref Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail)
        {
            this.SearchKeyWord_ = SearchKeyWord;
            this.ProductDetail_ = ProductDetail;
        }

        public void FindPrice()
        {
            IWebDriver driver_ = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            driver_.Navigate().GoToUrl("http://www.momoshop.com.tw");


            driver_.FindElement(By.CssSelector("#keyword")).SendKeys(this.SearchKeyWord_);
            driver_.FindElement(By.CssSelector(".inputbtn")).Click();

            Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> info = new Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>();

            try
            {
                if (driver_.FindElements(By.XPath("//*[@id=\"BodyBase\"]/form[1]/div/div[2]/div[5]/ul/li")).ToList().Count == 0)
                {
                    ReadProductDetail(driver_.Url, ref info);
                }
                else
                {
                    var PageCount = driver_.FindElements(By.XPath("//*[@id=\"BodyBase\"]/form[1]/div/div[2]/div[5]/ul/li")).ToList().Count;

                    for (var CurrentPage = 1; CurrentPage <= PageCount;)
                    {
                        List<IWebElement> products = driver_.FindElements(By.XPath("//*[@id=\"chessboard\"]/li/a")).ToList();
                        foreach (var product in products)
                        {
                            ReadProductDetail(product.GetAttribute("href"), ref info);
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

            this.ProductDetail_.Add("momo", info);
            driver_.Quit();
            if (LocalDriver_ != null)
            {
                LocalDriver_.Quit();
            }
            return;
        }

        private bool ReadProductDetail(string URL, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            if (LocalDriver_ == null)
            {
                LocalDriver_ = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            }
            

            LocalDriver_.Navigate().GoToUrl(URL);

            Dictionary<ProductInfo.ProductInfo, string> info = new Dictionary<ProductInfo.ProductInfo, string>();

            info.Add(ProductInfo.ProductInfo.URL, URL);
            info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/div[1]/h1")).Text);
            info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/div[1]/table[1]/tbody/tr[2]/td/span/b")).Text);

            LocalDriver_.FindElement(By.XPath("//*[@id=\"productForm\"]/div[2]/ul/li[2]/b")).Click();
            
            var spec = "";
            while(spec == "")
            {
                spec = LocalDriver_.FindElement(By.CssSelector("#productForm > div.prdwarp.bt770class > div.vendordetailview.specification")).Text;
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
            return true;
        }
    }
}
