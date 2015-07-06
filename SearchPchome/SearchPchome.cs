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
        static public bool FindPrice(string SearchKeyWord, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            IWebDriver driver = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            driver.Navigate().GoToUrl("http://www.pcstore.com.tw/");
            Thread.Sleep(1000);

            //#id_search_word
            driver.FindElement(By.CssSelector("#id_search_word")).SendKeys(SearchKeyWord);
            driver.FindElement(By.XPath("//*[@id=\"top-search\"]/img")).Click();

            for (; ; )
            {
                List<IWebElement> ProductList = driver.FindElements(By.XPath("//*[@id=\"keyad-pro-right3\"]/div[1]/a")).ToList();
                foreach (var product in ProductList)
                {
                    ReadProductDetail(product.GetAttribute("href"), ref ProductDetail);
                }

                try
                {
                    driver.FindElement(By.XPath("//*[@id=\"container\"]/div[10]/table[1]/tbody/tr/td[5]/a")).Click();
                    Thread.Sleep(1000);
                }
                catch (System.Exception ex)
                {
                    break;	
                }
            }

            driver.Quit();
            return true;
        }

        static private bool ReadProductDetail(string URL, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            IWebDriver LocalDriver = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            LocalDriver.Navigate().GoToUrl(URL);

            Dictionary<ProductInfo.ProductInfo, string> info = new Dictionary<ProductInfo.ProductInfo, string>();
            info.Add(ProductInfo.ProductInfo.URL, URL);

            if (IsElementPresent(LocalDriver, By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/h1")))
            {
                //PCHOME product form 1
                info.Add(ProductInfo.ProductInfo.name, LocalDriver.FindElement(
                    By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/h1")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver.FindElement(
                    By.XPath("//*[@id=\"inside_content\"]/table/tbody/tr/td[2]/table[1]/tbody/tr/td[3]/table/tbody/tr[3]/td/span[1]")).Text);
            }
            else if (IsElementPresent(LocalDriver, By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/div[1]")))
            {
                //PCHOME product form 2
                info.Add(ProductInfo.ProductInfo.name, LocalDriver.FindElement(
                    By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/div[1]")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver.FindElement(
                    By.XPath("//*[@id=\"inside_content_c2c\"]/div[2]/ul/li/span")).Text);
                //*[@id="inside_content_c2c"]/div[2]/ul/li[2]/span
            }
            else if (IsElementPresent(LocalDriver, By.XPath("//*[@id=\"main\"]/div[3]/div[1]")))
            {
                //PCHOME product form 3
                info.Add(ProductInfo.ProductInfo.name, LocalDriver.FindElement(
                    By.XPath("//*[@id=\"main\"]/div[3]/div[1]")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver.FindElement(
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
            LocalDriver.Quit();
            return true;
        }

        static private bool IsElementPresent(IWebDriver driver, By by)
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
