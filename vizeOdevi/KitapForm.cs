using Newtonsoft.Json;
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
using System.Xml.XPath;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace vizeOdevi
{
    public partial class KitapForm : Form
    {
        private List<Book> books = new List<Book>();
        private string jsonFilePath = Path.Combine(Application.StartupPath, "books.json");
        private DateTime kitapCikisTarih;

        string sqliteConnectionString = "Data Source=mydatabase.db;Version=3;";
        SQLiteConnection sqliteConnection = new SQLiteConnection(sqliteConnectionString);
        sqliteConnection.Open();

        string createBooksTableQuery = @"CREATE TABLE IF NOT EXISTS Books (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    KitapAd TEXT,
                                    KitapYazar TEXT,
                                    Yayin TEXT,
                                    KitapCikisTarih TEXT)";
        SQLiteCommand createBooksTableCmd = new SQLiteCommand(createBooksTableQuery, sqliteConnection);
        createBooksTableCmd.ExecuteNonQuery();

        public KitapForm()
        {
            InitializeComponent();
            LoadBooksFromJson();
        }

        private void LoadBooksFromJson()
        {
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                List<Book> books = JsonConvert.DeserializeObject<List<Book>>(json);

                foreach (var book in books)
                {
                    string insertBookQuery = @"INSERT INTO Books (KitapAd, KitapYazar, Yayin, KitapCikisTarih) 
                                         VALUES (@KitapAd, @KitapYazar, @Yayin, @KitapCikisTarih)";
                    SQLiteCommand insertBookCmd = new SQLiteCommand(insertBookQuery, sqliteConnection);
                    insertBookCmd.Parameters.AddWithValue("@KitapAd", book.KitapAd);
                    insertBookCmd.Parameters.AddWithValue("@KitapYazar", book.KitapYazar);
                    insertBookCmd.Parameters.AddWithValue("@Yayin", book.Yayin);
                    insertBookCmd.Parameters.AddWithValue("@KitapCikisTarih", book.KitapCikisTarih.ToString());
                    insertBookCmd.ExecuteNonQuery();
                }
            }
        }

        private void SaveBooksToJson()
        {
            string json = JsonConvert.SerializeObject(books);
            File.WriteAllText(jsonFilePath, json);
        }
        private void btnKitapEkle_Click(object sender, EventArgs e)
        {
            string kitapAd = kitapAdi.Text;
            string kitapYazar = kitapYazari.Text;
            string yayin = yayini.Text;
            DateTime kitapCikisTarih = dateTimePicker1.Value; // DateTimePicker kullanarak tarih değerini alın

            Book newBook = new Book(kitapAd, kitapYazar, yayin, kitapCikisTarih);
            books.Add(newBook);
            SaveBooksToJson();
            AddBookToSQLite(newBook); // SQLite'a ekle
            RefreshDataGridView();
            kitapAdi.Text = "";
            kitapYazari.Text = "";
            yayini.Text = "";
            dateTimePicker1.Value = DateTime.Now; // Tarih seçiciyi varsayılan değere geri dön
            MessageBox.Show("Kitap başarıyla kaydedildi.");
        }

        private void RefreshDataGridView()
        {
            dGVKitapListesi.DataSource = null;
            dGVKitapListesi.DataSource = books;
        }

        private void bntKitapSil_Click(object sender, EventArgs e)
        {
            if (dGVKitapListesi.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dGVKitapListesi.SelectedRows[0];
                Book selectedBook = (Book)selectedRow.DataBoundItem;
                books.Remove(selectedBook);
                SaveBooksToJson();
                RefreshDataGridView();
                MessageBox.Show("Seçilen kitap başarıyla silindi.");
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz kitabı seçin.");
            }
        }
        //-----------------------------
        private Book selectedBook;
        private void btnKitapSec_Click(object sender, DataGridViewCellEventArgs e)
        {
            KitapForm kitapForm = new KitapForm();
            if (kitapForm.ShowDialog() == DialogResult.OK)
            {
                selectedBook = kitapForm.SelectedBook;
            }
        }
        //-----------------------------
    }
    public class Book
    {
        public string KitapAd { get; set; }
        public string KitapYazar { get; set; }
        public string yayin { get; set; }
        public DateTime KitapCikisTarih { get; set; }

        public Book(string kitapAdi, string kitapYazari, string yayini, DateTime kitapCikisTarihi)
        {
            KitapAd = kitapAdi;
            KitapYazar = kitapYazari;
            yayin = yayini;
            KitapCikisTarih = kitapCikisTarihi;
        }
    }
    sqliteConnection.Close();
}
