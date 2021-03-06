﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SearchYahoo
{
    public class SearchYahoo
    {
        static IWebDriver LocalDriver_;
        private string SearchKeyWord_;
        private Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail_;

        public SearchYahoo(string SearchKeyWord, ref Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> ProductDetail)
        {
            this.SearchKeyWord_ = SearchKeyWord;
            this.ProductDetail_ = ProductDetail;
        }

        public void FindPrice()
        {
            IWebDriver driver = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());

            //超級商城
            driver.Navigate().GoToUrl("https://tw.mall.yahoo.com/");

            driver.FindElement(By.XPath("//*[@id=\"srp-ac-bar\"]")).SendKeys(this.SearchKeyWord_);
            driver.FindElement(By.XPath("//*[@id=\"UHSearchProperty\"]")).Click();

            Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> info = new Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>();

            for (; ; )
            {
                List<IWebElement> products = driver.FindElements(By.XPath("//*[@id=\"srp_result_list\"]/div/div/div[1]/a")).ToList();
                foreach (var product in products)
                {
                    ReadProductDetailMail(product.GetAttribute("href"), ref info);
                }

                try
                {
                    if (driver.FindElement(By.XPath("//*[@id=\"srp_sl_result\"]/div[3]/ul")).FindElements(
                            By.XPath(".//li/a")).ToList().Last().Text == "下一頁")
                    {
                        driver.FindElement(By.XPath("//*[@id=\"srp_sl_result\"]/div[3]/ul")).FindElements(
                            By.XPath(".//li/a")).ToList().Last().Click();
                    }
                    else
                    {
                        break;
                    }

                    products.Clear();
                    Thread.Sleep(1000);
                }
                catch (NoSuchElementException e)
                {
                    break;
                }
            }

            
            //購物中心
            driver.Navigate().GoToUrl("https://tw.buy.yahoo.com/");
            driver.FindElement(By.XPath("//*[@id=\"srp-ac-bar\"]")).SendKeys(this.SearchKeyWord_);
            driver.FindElement(By.XPath("//*[@id=\"UHSearchProperty\"]")).Click();
            List<IWebElement> buyproducts = driver.FindElements(By.XPath("//*[@id=\"srp_result_list\"]/div/div/div[1]/a")).ToList();

            for (; ; )
            {
                List<IWebElement> products = driver.FindElements(By.XPath("//*[@id=\"srp_result_list\"]/div/div/div[1]/a")).ToList();
                foreach (var product in buyproducts)
                {
                    ReadProductDetailBuy(product.GetAttribute("href"), ref info);
                }

                try
                {
                    if (driver.FindElement(By.XPath("//*[@id=\"srp_sl_result\"]/div[3]/ul")).FindElements(
                            By.XPath(".//li/a")).ToList().Last().Text == "下一頁")
                    {
                        driver.FindElement(By.XPath("//*[@id=\"srp_sl_result\"]/div[3]/ul")).FindElements(
                            By.XPath(".//li/a")).ToList().Last().Click();
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                catch (NoSuchElementException e)
                {
                    break;
                }
            }


            this.ProductDetail_.Add("Yahoo", info);
            driver.Quit();
            if (LocalDriver_ != null)
            {
                LocalDriver_.Quit();
            }
            return;
        }

        static private bool ReadProductDetailMail(string URL, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            if (LocalDriver_ == null)
            {
                LocalDriver_ = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            }
            LocalDriver_.Navigate().GoToUrl(URL);

            Dictionary<ProductInfo.ProductInfo, string> info = new Dictionary<ProductInfo.ProductInfo, string>();
            try
            {
                info.Add(ProductInfo.ProductInfo.URL, URL);
                info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(By.XPath("//*[@id=\"ypsiif\"]/div/div[1]/div[4]/div[1]/h1/span[1]")).Text);
                info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(By.XPath("//span[@itemprop=\"price\"]")).Text);
            }
            catch (System.Exception ex)
            {
                return false;	
            }
            
            var name = info[ProductInfo.ProductInfo.name];
            int i = 1;
            while (ProductDetail.ContainsKey(name))
            {
                name = string.Format("{0}_{1}", name, i);
                ++i;
            }
            ProductDetail.Add(name, info);
            //LocalDriver.Quit();
            return true;
        }

        static private bool ReadProductDetailBuy(string URL, ref Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>> ProductDetail)
        {
            if (LocalDriver_ == null)
            {
                LocalDriver_ = new RemoteWebDriver(new Uri("http://192.168.1.4:4444/wd/hub"), DesiredCapabilities.Firefox());
            }
            LocalDriver_.Navigate().GoToUrl(URL);

            Dictionary<ProductInfo.ProductInfo, string> info = new Dictionary<ProductInfo.ProductInfo, string>();
            info.Add(ProductInfo.ProductInfo.URL, URL);
            info.Add(ProductInfo.ProductInfo.name, LocalDriver_.FindElement(
                By.XPath("//h1")).Text);
            info.Add(ProductInfo.ProductInfo.price, LocalDriver_.FindElement(
                By.XPath("//div[@class=\"priceinfo\"]/span[2]")).Text);

            var name = info[ProductInfo.ProductInfo.name];
            int i = 1;
            while (ProductDetail.ContainsKey(name))
            {
                name = string.Format("{0}_{1}", info[ProductInfo.ProductInfo.name], i);
                ++i;
            }
            ProductDetail.Add(name, info);
            //LocalDriver.Quit();
            return true;
        }
    }
}
