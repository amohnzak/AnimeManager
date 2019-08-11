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
using System.Data.SqlClient;

namespace AnimeManager
{
    public partial class Form1 : Form
    {
        public string dataPath = "D:\\Users\\Aqlan Najwan\\Documents\\Anime Manager\\";
        public string noImagePath = "D:\\Users\\Aqlan Najwan\\Documents\\Anime Manager\\noimage.jpg";
        public string backImagePath = "D:\\Users\\Aqlan Najwan\\Documents\\Anime Manager\\back.png";
        public string strConn = "server=ANMZH-G3\\SQLEXPRESS;database=Anime Manager;Trusted_Connection=True";
        public List<AnimeDetails> gbl_animeList = new List<AnimeDetails>();
        public string gbl_selectedAnime = "";
        public List<string> gbl_episodeList = new List<string>(); // Selected Anime Episode List

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DirectoryInfo mainFolder = new DirectoryInfo(dataPath);
            int currentX = 15;
            int currentY = 15;

            Panel mainScreen = new Panel
            {
                Location = new Point(0, 0),
                AutoSize = true,
                Name = "MainScreen"
            };
            splitContainer1.Panel2.Controls.Add(mainScreen);
            //this.Controls.Add(mainScreen);

            foreach (DirectoryInfo animeFolder in mainFolder.EnumerateDirectories())
            {
                string animeTitle = animeFolder.Name;
                bool iconFound = false;
                AnimeDetails newAnime = new AnimeDetails
                {
                    title = animeTitle
                };

                Panel animeIcon = new Panel
                {
                    Location = new Point(currentX, currentY),
                    Size = new Size(225, 250),
                    Name = "pnl" + animeTitle.Split()[0],
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };
                mainScreen.Controls.Add(animeIcon);
                animeIcon.Click += new EventHandler(GoToDetails);
                animeIcon.MouseEnter += new EventHandler(IconHover);
                animeIcon.MouseLeave += new EventHandler(IconDefault);

                foreach (FileInfo file in animeFolder.EnumerateFiles())
                {
                    if (file.Name.StartsWith("thumbnail"))
                    {
                        PictureBox thumbnail = new PictureBox
                        {
                            Location = new Point(5, 5),
                            ImageLocation = file.FullName,
                            Size = new Size(225, 225)
                        };
                        if (Image.FromFile(file.FullName).Width > thumbnail.Width || Image.FromFile(file.FullName).Height > thumbnail.Height)
                        {
                            thumbnail.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                        else
                        {
                            thumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
                        }
                        animeIcon.Controls.Add(thumbnail);
                        thumbnail.Click += new EventHandler(GoToDetails);
                        thumbnail.MouseEnter += new EventHandler(IconHover);
                        thumbnail.MouseLeave += new EventHandler(IconDefault);
                        iconFound = true;
                        newAnime.imgpath = file.FullName;
                    }
                }

                if (!iconFound)
                {
                    PictureBox thumbnail = new PictureBox
                    {
                        Location = new Point(5, 5),
                        ImageLocation = noImagePath,
                        Size = new Size(225, 225)
                    };
                    if (Image.FromFile(noImagePath).Width > thumbnail.Width || Image.FromFile(noImagePath).Height > thumbnail.Height)
                    {
                        thumbnail.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        thumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    animeIcon.Controls.Add(thumbnail);
                    thumbnail.Click += new EventHandler(GoToDetails);
                    thumbnail.MouseEnter += new EventHandler(IconHover);
                    thumbnail.MouseLeave += new EventHandler(IconDefault);
                    newAnime.imgpath = noImagePath;
                }

                Label title = new Label
                {
                    Location = new Point(5, 230),
                    Text = animeTitle,
                    Width = 225,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                animeIcon.Controls.Add(title);
                title.Click += new EventHandler(GoToDetails);
                title.MouseEnter += new EventHandler(IconHover);
                title.MouseLeave += new EventHandler(IconDefault);

                if (currentX > 1050 - 250*2)
                {
                    currentY += 270;
                    currentX = 15;
                }
                else
                {
                    currentX += 250;
                }

                gbl_animeList.Add(newAnime);
            }
        }

        private void GoToDetails(Object sender, EventArgs e)
        {
            Control c = sender as Control;
            Control mainScreen = splitContainer1.Panel2.Controls["MainScreen"];
            mainScreen.Hide();

            Panel detailScreen = new Panel
            {
                Location = new Point(5,5),
                AutoSize = true,
                Name = "DetailScreen"
            };
            splitContainer1.Panel2.Controls.Add(detailScreen);

            Button btnBack = new Button
            {
                Size = new Size(20, 20),
                Location = new Point(0, 5),
                Image = Image.FromFile(backImagePath),
                Cursor = Cursors.Hand
            };
            toolTip1.SetToolTip(btnBack, "Back");
            detailScreen.Controls.Add(btnBack);
            btnBack.Click += new EventHandler(BackToMain);

            string pnlID = c.Parent.Name.Substring(3);

            if (c.Name.StartsWith("pnl"))
            {
                pnlID = c.Name.Substring(3);
            }

            foreach (AnimeDetails anime in gbl_animeList)
            {
                if (anime.title.StartsWith(pnlID))
                {
                    gbl_selectedAnime = anime.title;
                    // Thumbnail //
                    PictureBox thumbnail = new PictureBox
                    {
                        Location = new Point(25, 5),
                        ImageLocation = anime.imgpath,
                        Size = new Size(225, 225)
                    };
                    if (Image.FromFile(anime.imgpath).Width > thumbnail.Width || Image.FromFile(anime.imgpath).Height > thumbnail.Height)
                    {
                        thumbnail.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        thumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    detailScreen.Controls.Add(thumbnail);

                    // Title //
                    Label title = new Label
                    {
                        Location = new Point(265, 5),
                        Text = anime.title,
                        Font = new Font("Arial", 16, FontStyle.Bold),
                        AutoSize = true
                    };
                    detailScreen.Controls.Add(title);

                    // Description //
                    if (String.IsNullOrEmpty(anime.desc))
                    {
                        GetDesc(anime);
                    }

                    Panel pnlDesc = new Panel
                    {
                        Location = new Point(270, 35),
                        Height = 195,
                        Width = 700,
                        AutoScroll = true,
                        Padding = new Padding(5,5,5,5)
                    };
                    detailScreen.Controls.Add(pnlDesc);

                    Label desc = new Label
                    {
                        Location = new Point(0, 0),
                        Text = anime.desc,
                        Font = new Font("Arial", 10, FontStyle.Regular),
                        MaximumSize = new Size(680, 0),
                        AutoSize = true
                    };
                    pnlDesc.Controls.Add(desc);

                    // Genre Tags //
                    int posX = 25;
                    int posY = 240;
                    foreach (string genre in anime.genres)
                    {
                        Label tag = new Label
                        {
                            Text = genre,
                            Location = new Point(posX, posY),
                            AutoSize = true,
                            BackColor = Color.Orange,
                            Padding = new Padding(3)
                        };
                        detailScreen.Controls.Add(tag);
                        if (posX > 100)
                        {
                            posX = 25;
                            posY += 25;
                        }
                        else
                        {
                            posX += tag.Width + 10;
                        }
                    }

                    // Episode List //
                    ListView lstEpisode = new ListView
                    {
                        Location = new Point(265, 240),
                        Size = new Size(704, 300),
                        Font = new Font("Arial", 11),
                        View = View.Details,
                        FullRowSelect = true,
                        HeaderStyle = ColumnHeaderStyle.None,
                        Activation = ItemActivation.OneClick,
                        ShowItemToolTips = true
                    };
                    lstEpisode.Columns.Add("Episode", 500);
                    lstEpisode.Columns.Add("Date Released", 176, HorizontalAlignment.Right);
                    detailScreen.Controls.Add(lstEpisode);
                    lstEpisode.ItemActivate += new EventHandler(WatchEpisode);

                    GetEpisodeList(anime, lstEpisode);

                    break;
                }
            }
        }

        private void IconHover(Object sender, EventArgs e)
        {
            Control c = sender as Control;

            if (c.GetType().ToString().EndsWith("Panel"))
            {
                Panel animeIcon = c as Panel;
                if (animeIcon.BackColor != Color.DarkGray)
                {
                    animeIcon.BackColor = Color.DarkGray;
                }
            }
            else
            {
                Panel animeIcon = c.Parent as Panel;
                if(animeIcon.BackColor != Color.DarkGray)
                {
                    animeIcon.BackColor = Color.DarkGray;
                }
            }
        }

        private void IconDefault(Object sender, EventArgs e)
        {
            Control c = sender as Control;

            if (c.GetType().ToString().EndsWith("Panel"))
            {
                Panel animeIcon = c as Panel;
                if (animeIcon.BackColor != SystemColors.Control)
                {
                    animeIcon.BackColor = SystemColors.Control;
                }
            }
            else
            {
                Panel animeIcon = c.Parent as Panel;
                if (animeIcon.BackColor != SystemColors.Control)
                {
                    animeIcon.BackColor = SystemColors.Control;
                }
            }
        }

        private void BackToMain(Object sender, EventArgs e)
        {
            splitContainer1.Panel2.Controls.RemoveByKey("DetailScreen");
            Control mainScreen = splitContainer1.Panel2.Controls["MainScreen"];
            mainScreen.Show();
            gbl_selectedAnime = "";
        }

        private void GetDesc(AnimeDetails anime)
        {
            using (SqlConnection sqlConn = new SqlConnection(strConn))
            {
                try
                {
                    sqlConn.Open();
                    string strQuery = "SELECT * FROM [Anime Manager].[dbo].[tbl_ANIME_MAIN] WHERE a_title='" + anime.title + "'";
                    SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
                    SqlDataReader sqlRdr = sqlCmd.ExecuteReader();
                    bool dataFound = false;
                    while (sqlRdr.Read())
                    {
                        dataFound = true;
                        anime.genres = sqlRdr.GetValue(2).ToString().Split(',').ToList();
                        anime.lastwatched = Convert.ToInt32(sqlRdr.GetValue(3));
                        if (sqlRdr.GetValue(1) != null)
                        {
                            anime.desc = sqlRdr.GetValue(1).ToString();
                        }
                        else
                        {
                            anime.desc = "< No Description >";
                        }
                    }
                    if (!dataFound)
                    {
                        anime.desc = "< No Description >";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    sqlConn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GetEpisodeList(AnimeDetails anime, ListView episodeList)
        {
            gbl_episodeList.Clear();
            DirectoryInfo animeFolder = new DirectoryInfo(dataPath + anime.title);
            int episodeIndex = 1;
            foreach (FileInfo file in animeFolder.EnumerateFiles())
            {
                if (file.Extension.ToLower() == ".mkv" || file.Extension.ToLower() == ".mp4")
                {
                    string[] arr = new string[2];
                    arr[0] = "Episode " + episodeIndex;
                    arr[1] = file.LastWriteTime.ToString("yyyy-MM-dd");
                    ListViewItem item = new ListViewItem(arr);
                    if (episodeIndex > anime.lastwatched)
                    {
                        item.SubItems[0].Text += " *NEW*";
                        item.SubItems[0].ForeColor = Color.Red;
                    }
                    item.ToolTipText = "Watch Episode " + episodeIndex;
                    episodeList.Items.Add(item);
                    gbl_episodeList.Add(file.FullName);
                    episodeIndex++;
                }
            }
        }

        private void WatchEpisode(Object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            ListView lv = sender as ListView;

            for (int i = 0; i <= lv.SelectedItems[0].Index; i++)
            {
                if (lv.Items[i].Text.EndsWith("*NEW*"))
                {
                    lv.Items[i].Text = lv.Items[i].Text.Substring(0, lv.Items[i].Text.Length - 6);
                    lv.Items[i].ForeColor = SystemColors.ControlText;
                }
            }
            UpdateLastWatched(lv.SelectedItems[0].Index + 1);

            int episodeNumber = lv.SelectedItems[0].Index;

            string fileName = gbl_episodeList[episodeNumber];
            var process = new System.Diagnostics.Process();

            process.StartInfo = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = fileName };

            process.Start();
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }

        private void UpdateLastWatched(int episodeNumber)
        {
            using (SqlConnection sqlConn = new SqlConnection(strConn))
            {
                try
                {
                    sqlConn.Open();
                    string strQuery = "UPDATE [Anime Manager].[dbo].[tbl_ANIME_MAIN] SET a_last_watched=" + episodeNumber.ToString() + " WHERE a_title='" + gbl_selectedAnime + "'";
                    SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn);
                    sqlCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    sqlConn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public class AnimeDetails
        {
            public string title;
            public string desc;
            public string imgpath;
            public List<string> genres = new List<string>();
            public int lastwatched;
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            if (splitContainer1.SplitterDistance == 44)
            {
                splitContainer1.SplitterDistance = 100;
                toolTip1.SetToolTip(btnMenu, "Collapse Menu");
            }
            else
            {
                splitContainer1.SplitterDistance = 44;
                toolTip1.SetToolTip(btnMenu, "Expand Menu");
            }
        }
    }
}
