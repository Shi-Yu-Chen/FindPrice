using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SearchMomo;
using SearchPchome;

namespace FindPrice
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO fi);

        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        const uint FLASHW_ALL = 3;
        const uint FLASHW_TIMERNOFG = 12;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        //handle click URL event
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                System.Diagnostics.Process.Start(this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
            //Thread.Sleep(100);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //cleanup
            this.dataGridView1.Rows.Clear();
            List<Thread> threadpool = new List<Thread>();
            
            Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>> PlatformProductInfo = 
                new Dictionary<string, Dictionary<string, Dictionary<ProductInfo.ProductInfo, string>>>();
            if (this.MomocheckBox.Checked)
            {
                SearchMomo.SearchMomo momo = new SearchMomo.SearchMomo(this.textBox1.Text, ref PlatformProductInfo);
                threadpool.Add(new Thread(momo.FindPrice));
                threadpool.Last().Start();
            }

            if (this.PchomecheckBox.Checked)
            {
                SearchPchome.SearchPchome pchome = new SearchPchome.SearchPchome(this.textBox1.Text, ref PlatformProductInfo);
                threadpool.Add(new Thread(pchome.FindPrice));
                threadpool.Last().Start();
            }

            if (this.YahoocheckBox.Checked)
            {
                SearchYahoo.SearchYahoo yahoo = new SearchYahoo.SearchYahoo(this.textBox1.Text, ref PlatformProductInfo);
                threadpool.Add(new Thread(yahoo.FindPrice));
                threadpool.Last().Start();
            }

            //wait for all thread finish
            foreach (var threads in threadpool)
            {
                while (threads.IsAlive)
                {
                    Thread.Sleep(500);
                }
            }

            //output info
            foreach (var Platform in PlatformProductInfo)
            {
                foreach (var info in Platform.Value)
                {
                    this.dataGridView1.Rows.Add(Platform.Key, info.Value[ProductInfo.ProductInfo.name], info.Value[ProductInfo.ProductInfo.price],
                        info.Value.ContainsKey(ProductInfo.ProductInfo.spec)?info.Value[ProductInfo.ProductInfo.spec]:"N/A", info.Value[ProductInfo.ProductInfo.URL]);
                }
            }
            
            this.dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

            //FLASH WINDOWS
            FLASHWINFO FlashWINInfo = new FLASHWINFO();
            FlashWINInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(FlashWINInfo));
            FlashWINInfo.hwnd = Handle;
            FlashWINInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            FlashWINInfo.uCount = uint.MaxValue;
            FlashWINInfo.dwTimeout = 0;
            FlashWindowEx(ref FlashWINInfo);
        }
    }
}
