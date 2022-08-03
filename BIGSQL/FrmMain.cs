using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIGSQL
{
    public partial class FrmMain : Form
    {
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataReader dr;
        string file_path = "";
        bool click_stop = false;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                con = new MySqlConnection("Server=" + txtServer.Text + ";Database=" + txtDatabae.Text + ";user=" + txtUser.Text + ";Pwd=" + txtPassword.Text + ";SslMode=none");
                con.Open();
                con.Close();
                Properties.Settings.Default.server = txtServer.Text;
                Properties.Settings.Default.user = txtUser.Text;
                Properties.Settings.Default.password = txtPassword.Text;
                Properties.Settings.Default.database = txtDatabae.Text;
                Properties.Settings.Default.Save();
                MessageBox.Show("Başarılı");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            txtServer.Text = Properties.Settings.Default.server;
            txtDatabae.Text = Properties.Settings.Default.database;
            txtPassword.Text = Properties.Settings.Default.password;
            txtUser.Text = Properties.Settings.Default.user;

        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.ShowDialog();

            file_path = opf.FileName;


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!File.Exists(file_path))
            {
                MessageBox.Show("SQL dosyası bulunamadı: "  + file_path);
                return;
            }
            Properties.Settings.Default.pos = txtPos.Text;
            Properties.Settings.Default.table = txtTable.Text;
            Properties.Settings.Default.Save();

            click_stop = false;
            con = new MySqlConnection("Server=" + txtServer.Text + ";Database=" + txtDatabae.Text + ";user=" + txtUser.Text + ";Pwd=" + txtPassword.Text + ";SslMode=none");
            cmd = new MySqlCommand();
            cmd.Connection = con;
            con.Open();

            using (FileStream fs = File.Open(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                string sql = "";
                int sira =0;
                int pos = int.Parse(txtPos.Text);
                txtSql.Text = "Başladı\r";

                while ( (line = sr.ReadLine()) != null)
                {
                    sira++;
                    if (sira < pos) continue;
                    if (line.IndexOf("--") == 0) continue;
                    if (line.IndexOf("/*") == 0) continue;
                    txtPos.Text = sira.ToString();
                    sql += line;
                    if (sql.EndsWith(";"))
                    {
                        try
                        {
                       
                            if (sql != "" && sql.IndexOf( txtTable.Text )>=0)
                            {
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                            }

                        }
                        catch (Exception ex)
                        {
                            txtSql.Text += sira.ToString() + " Line: " + ex.Message + "\r";
                        }
                                sql = "";

                    }
   
                    Application.DoEvents();
                    if (click_stop)
                    {
                        click_stop = false;
                        txtSql.Text += "Kesildi\r";
                        break;
                    }
                }
                txtSql.Text += "Bitti\r";
            }
            Properties.Settings.Default.pos = txtPos.Text;
            Properties.Settings.Default.table = txtTable.Text;
            Properties.Settings.Default.Save();

            con.Close();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            click_stop = true;
        }
    }
}
