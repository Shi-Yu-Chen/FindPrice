//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SearchPchome
{
    public class SearchPchome
    {
        private IWebDriver LocalDriver_;
        private string SearchKeyWord_;
        private Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail_;

        public SearchPchome(string SearchKeyWord, ref Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail)
        {
            this.SearchKeyWord_ = SearchKeyWord;
            this.ProductDetail_ = ProductDetail;
        }

        public void FindPrice()
        {
            IWebDriver driver = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            driver.Navigate().GoToUrl("http://www.pcstore.com.tw/");
            Thread.Sleep(1000);

            //#id_search_word
            driver.FindElement(By.CssSelector("#id_search_word")).SendKeys(this.SearchKeyWord_);
            driver.FindElement(By.XPath("//*[@id=\"top-search\"]/img")).Click();

            Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> info = new Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>();

            for (; ; )
            {
                List<IWebElement> ProductList = driver.FindElements(By.XPath("//*[@id=\"keyad-pro-right3\"]/div[1]/a")).ToList();
                foreach (var product in ProductList)
                {
                    ReadProductDetail(product.GetAttribute("href"), ref info);
                }

                try
                {
                    driver.FindElement(By.XPath("//*[@id=\"container\"]/div[10]/table[1]/tbody/tr/td[5]/a")).Click();
                    Thread.Sleep(1000);
                }
                catch (System.Exception)
                {
                    break;	
                }
            }

            this.ProductDetail_.Add("PcHome", info);
            if (LocalDriver_ != null)
            {
                LocalDriver_.Quit();
            }
            driver.Quit();
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

            if (IsElementPresent(LocalDriver_, By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/h1")))
            {
                //PCHOME product form 1
                info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/h1")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/table/tbody/tr[3]/td/span[1]")).Text);
            }
            else if (IsElementPresent(LocalDriver_, By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/div[1]")))
            {
                //PCHOME product form 2
                info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/div[1]")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/ul/li/span")).Text);
                //*[@id="inside_content_c2c"]/div[2]/ul/li[2]/span
            }
            else if (IsElementPresent(LocalDriver_, By.XPath("//*[@id=\"main\"]/div[3]/div[1]")))
            {
                //PCHOME product form 3
                info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"main\"]/div[3]/div[1]")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(
                    By.XPath("//*[@id=\"main\"]/div[3]/ul/li[2]/span/b")).Text);
            }
            
            //Pchome 不處理規格
            var name = info[ProductInfo.ProductInfo.name];
            int i = 1;
            while (ProductDetail.ContainsKey(name))
            {
                name = string.Format("{0}_{1}", name, i);
                ++i;
            }
            ProductDetail.Add(name, info);
            return true;
        }

        private bool IsElementPresent(IWebDriver driver, By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
