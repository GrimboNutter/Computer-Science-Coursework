using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        void loginSuccess(string Username, int AccessLevel)
        {
            MessageBox.Show("Welcome back " + Username);
            Form Form1 = new Form1();
            Form1.Show();
            this.Hide();
        }

        OleDbConnection con = new OleDbConnection();

        private void button2_Click(object sender, EventArgs e)
        {
            bool taken = false;
            //Lookup
            OleDbCommand dataSearch = new OleDbCommand("SELECT Username FROM Users", con);
            con.Open();
            OleDbDataReader reader1 = dataSearch.ExecuteReader();
            if (reader1.HasRows)
            {
                while (reader1.Read())
                {
                    //Check if username is taken
                    if (reader1[0].ToString() == textBox1.Text)
                    {
                        MessageBox.Show("Username already taken.");
                        taken = true;
                    }
                }
            }
            con.Close();
            if (!taken)
            {
                //Register Google Authenticator
                string GoogleAuthCode;
                Random rnd = new Random();
                GoogleAuthCode = rnd.Next(1000000, 9999999).ToString();
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var setupInfo = tfa.GenerateSetupCode("Callum Grimble's Project", textBox1.Text, GoogleAuthCode, 300, 300);

                webBrowser1.Navigate(setupInfo.QrCodeSetupImageUrl);
                label4.Text = "Manual: "+setupInfo.ManualEntryKey;

                //Add User to DB
                OleDbCommand insertData = new OleDbCommand("INSERT INTO Users VALUES (@Username, @Password, @GoogleAuthCode);", con);
                insertData.Parameters.AddWithValue("@Username", textBox1.Text);
                insertData.Parameters.AddWithValue("@Password", textBox2.Text);
                insertData.Parameters.AddWithValue("@GoogleAuthCode", GoogleAuthCode);
                con.Open();
                insertData.ExecuteNonQuery();
                con.Close();
                this.Height = 559;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string password = textBox2.Text;
            bool passwordCorrect = false;
            //Lookup
            OleDbCommand dataSearch = new OleDbCommand("SELECT Password, GoogleAuthCode FROM Users WHERE Username=@SeachID", con);
            dataSearch.Parameters.AddWithValue("@SearchID", textBox1.Text);
            con.Open();
            OleDbDataReader reader1 = dataSearch.ExecuteReader();
            if (reader1.HasRows)
            {
                while (reader1.Read())
                {
                    if (password == reader1[0].ToString())
                    {
                        passwordCorrect = true;
                        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                        bool isCorrectPIN = tfa.ValidateTwoFactorPIN(reader1[1].ToString(), textBox3.Text);
                        if (isCorrectPIN)
                        {
                            loginSuccess(textBox1.Text, 3);
                        } else
                        {
                            MessageBox.Show("Incorrect Google Code!");
                        }
                    }
                }
            }
            con.Close();
            if (!passwordCorrect)
            {
                MessageBox.Show("Incorrect Password");
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            //Connect
            con.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = ./UserDatabas.accdb;";
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                con.Close();
                Application.Exit();
            }
            finally
            {
                this.Text = "Login - Connected to DB";
                con.Close();
            }
        }
    }
}
