using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AnimeManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dataPath = "D:\\Users\\Aqlan Najwan\\Documents\\Anime Manager\\";

            DirectoryInfo mainFolder = new DirectoryInfo(dataPath);
            foreach (DirectoryInfo animeFolder in mainFolder.EnumerateDirectories())
            {
                string animeTitle = animeFolder.Name;


            }
        }
    }
}
