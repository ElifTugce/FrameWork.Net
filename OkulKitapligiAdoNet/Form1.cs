using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OkulKitapligiAdoNet
{
    public partial class FormYazarlar : Form
    {
        public FormYazarlar()
        {
            InitializeComponent();
        }

        //Global Alan
        // SQLCONNECTION Nesnesi: Sql Veritabanına bağlantı kurmak için kullanacağımız classtır. System.Data.Client namespace'i içerisinde yer alır.

        SqlConnection baglanti = new SqlConnection();
        string SQLBaglantiCumlesi = @"Server=DESKTOP-HNE43R2;Database=OKULKITAPLIGI;Trusted_Connection=True;";

        private void FormYazarlar_Load(object sender, EventArgs e)
        {
            baglanti.ConnectionString = SQLBaglantiCumlesi;
            dataGridViewYazarlar.MultiSelect = false; //çoklu seçimi kapatmış olduk.
            // tıkladığımız yerin tüm satırı seçilmiş olsun istiyoruz.
            dataGridViewYazarlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect; //fare ile datagrid üzerinde ir hücreye tıklandığında bulunduğu satırı tamamen seçecek 
            

            dataGridViewYazarlar.ContextMenuStrip = contextMenuStrip1;



            // grid kısmına bilgileri getireceğiz.
            TumYazarlariGetir();
        }

        private void TumYazarlariGetir()
        {

            try
            {


                //SQLCOMMAND nesnesi: Sorgularımızı ve prosedürlerimize ait komutları alan nesnedir.

                SqlCommand komut = new SqlCommand();
                komut.Connection = baglanti;
                komut.CommandType = CommandType.Text; // komuta gireceğimiz şeyin text olacağını tanımladık.
                string sorgu = "Select*From Yazarlar order by YazarId desc"; // neyi sorgulayacaksak onu string bir biçimde tanımlayıp çift tırnak içine yazdık.
                komut.CommandText = sorgu; // sorgu komutunu getirdik yani çalıştırdık.
                BaglantiyiAc();

                //DATASQLADAPTER nesnesi,sorgu çalıştığında oluşan verilerin aktarılması işlemini yapmaktadır.
                //Adoptore hangi komut işleyeceğine ctor'da karar verebiliriz. Ya da daha sonradan bu görevi verebiliriz.

                // 1.Yöntem
                SqlDataAdapter adaptor = new SqlDataAdapter(komut);

                // 2.Yöntem
                //SqlDataAdapter adaptor = new SqlDataAdapter();
                //adaptor.SelectCommand = komut; 

                DataTable sanalTablo = new DataTable(); // bir sanal tablo oluşturuyoruz.
                adaptor.Fill(sanalTablo); // içindeki verilerin hepsini sanal tabloya doldurmasını istiyoruz.

                dataGridViewYazarlar.DataSource = sanalTablo;
                dataGridViewYazarlar.Columns["SilindiMi"].Visible = false; // silindimi tablosu görünmesin
                dataGridViewYazarlar.Columns["YazarAdSoyad"].Width = 329;
                dataGridViewYazarlar.Columns["KayitTarihi"].Width = 270;



                BaglantiyiKapat();

            }
            catch (Exception ex)
            {

                MessageBox.Show($"Beklenmedik bir hata oluştu... {ex.Message}", "HATA" , MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            switch (btnEkle.Text)
            {

                case "Ekle":
                    try
                    {
                        if (string.IsNullOrEmpty(txtYazar.Text))
                        {
                            MessageBox.Show("Lütfen yazar bilgisini giriniz!", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            //ekleme yapacağız.

                            string insertCumlesi = $"insert into Yazarlar (KayitTarihi, YazarAdSoyad,SilindiMi) values ('{TarihiDuzenle(DateTime.Now)}','{txtYazar.Text.Trim()}',0)";

                            // uzun ve karmaşık tek tırnaklarda hatalı yazabiliyoruz.
                            // 20 tane kolon olsa kesin karışır.Bu nedenle string format yapısını bize sunan $'i kullanarak yazdık
                            //string insertCumlesi = "insert into Yazarlar (KayitTarihi, YazarAdSoyad,SilindiMi) values ('"+ DateTime.Now +"','"+txtYazar.Text+"',0)";

                            // Tarihi buradan göndermek istemezsek Getdate yazarak sql içinde alabiliriz.
                            //string insertCumlesi = $"insert into Yazarlar (KayitTarihi, YazarAdSoyad,SilindiMi) values (getdate(),'{txtYazar.Text.Trim()}',0)";


                            SqlCommand insertkomut = new SqlCommand(insertCumlesi, baglanti);
                            //baglantı açılacak metot çağıralım
                            BaglantiyiAc();
                            int sonucum = insertkomut.ExecuteNonQuery();
                            if (sonucum > 0) //effected rows var
                            {
                                MessageBox.Show("Yeni yazar sisteme eklendi.");
                                TumYazarlariGetir();

                            }
                            else
                            {
                                MessageBox.Show("Bir hata oluştu. Yeni yazar eklenemedi!");
                            }

                            // bağlantı kapanacak metot çağıralım

                            BaglantiyiKapat();
                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Ekleme işleminde beklenmedik hata oldu!" + ex.Message);
                    }
                    Temizle();
                    break;


                case "Güncelle":
                    try
                    {
                        if (!string.IsNullOrEmpty(txtYazar.Text))
                        {
                            using (baglanti)
                            {
                                DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                                int YazarId = Convert.ToInt32(satir.Cells["YazarId"].Value);

                                //1.Yol
                                string updateSorgucumlesi = $"Update Yazarlar Set YazarAdSoyad = '{txtYazar.Text.Trim()}' where YazarId ={YazarId}";
                                SqlCommand updateCommand = new SqlCommand(updateSorgucumlesi, baglanti);


                                BaglantiyiAc();


                                int sonuc = updateCommand.ExecuteNonQuery();
                                if (sonuc > 0)
                                {
                                    MessageBox.Show($"Yazar Güncellendi");
                                    TumYazarlariGetir();
                                }
                                else
                                {
                                    MessageBox.Show("Yazar güncellenmedi!");
                                }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Yazar adı yazmadan güncelleştirme yapamam");
                        }



                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Güncelleme işleminde beklenmedik hata oldu!" + ex.Message);
                    }

                    Temizle();
                    break;


                default:
                    break;
            }
        }

        private void Temizle()
        {
            btnEkle.Text = "Ekle";
            txtYazar.Clear();
        }

        private void BaglantiyiKapat()
        {
            try
           
            {
                if (baglanti.State != ConnectionState.Closed)
                {
                    baglanti.Close();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("Bağlantıyı kapatırken bir hata oldu!" + ex.Message);

            }
        }

        private void BaglantiyiAc()
        {
            try
            {
                //bağlantı açık değilse açalım...
                if (baglanti.State != ConnectionState.Open)
                {
                    baglanti.ConnectionString = SQLBaglantiCumlesi;
                    baglanti.Open();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("Bağlantı açılırken bi hata oluştu!" + ex.Message);
            }
        }

        private string TarihiDuzenle(DateTime tarih)
        {
            string tarihString = string.Empty;
            if (tarih != null)
            {

                //Uzun yazımı
                //2021-12-13 10:30
                tarihString = tarih.Year + "-" + tarih.Month + "-" + tarih.Day + " " + tarih.Hour + ":" + tarih.Minute + ":" + tarih.Second;
            }

            return tarihString;
        }

        private void guncelleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            if (dataGridViewYazarlar.SelectedRows.Count>0) // Yani bir şey seçildiyse...
            {
                
                DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                string yazarAdSoyad = Convert.ToString(satir.Cells["YazarAdSoyad"].Value);
                btnEkle.Text = "Güncelle";
                txtYazar.Text = yazarAdSoyad;

                //Kısa olsun isterseniz
                //txtYazar.Text = Convert.ToString(satir.Cells["YazarAdSoyad"].Value);

            }
            else
            {
                MessageBox.Show("Güncelleme işlemi için tablodan bir yazar seçmeniz gerekiyor!","UYARI", MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            Temizle();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow secilenSatir = dataGridViewYazarlar.SelectedRows[0];
            int yazarId = (int)secilenSatir.Cells["YazarId"].Value;
            string yazar = Convert.ToString(secilenSatir.Cells["YazarAdSoyad"].Value);
            // Yazarın kitapları varsa kitaplar tablosunda YazarId foreign key vardır.
            //Bu durumda silme işlemi yapılmamalıdır.
            // Önce bir select sorgusu ile kitaplar tablosunda o yazara ait kayıt var mı diye bakmalıyız.
            //Varsa silmesine izin vermeyeceğiz.
            //Yoksa, silmek ister misin diye son bir kez sorup evet cevabını alırsak sileceğiz.

            SqlCommand komut = new SqlCommand($"select*from Kitaplar where YazarId = {yazarId}", baglanti); // Sql command nesnesine çalıştıracağı osrguyu ve hangi bağlantı üzerinde çalışacağını constructar'da verdik
            komut.Connection = baglanti;
            SqlDataAdapter adaptor = new SqlDataAdapter(komut); // adaptöre işleyeceği komutu adaptörün constructor kısmında verdik.
            DataTable sanalTablo = new DataTable();
            BaglantiyiAc();
            adaptor.Fill(sanalTablo);

            if (sanalTablo.Rows.Count>0)
            {
                MessageBox.Show($"{yazar} adlı yazarın Kitaplar tablosunda verileri bulunmaktadır. Bu yazarı silmek için öncelikle sistemdeki kitapları silmeniz gerekmektedir. Lütfen Kitap İşlemleri sayfasına gidiniz...");
            }
            else
            {
                //Kitabı yok demektir. Foreign Key patlaması olmayacaktır. Yani silebiliriz.
                DialogResult cevap = MessageBox.Show($"{yazar} adlı yazarı silmek istediğinize emin misiniz?", "ONAY",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
                if (cevap==DialogResult.Yes)
                {
                    //Silecek
                    //1. Yöntem
                    //komut.CommandText=$"Delete from Yazarlar where YazarId={yazarid}";
                    //yzrid diyerek bir parametre oluşturmuş olduk.
                    komut.CommandText = $"Delete from Yazarlar where YazarId=@yzrid";
                    komut.Parameters.Clear();
                    //AddWithValue metodu @yzrid yerine yazarId değerini sqlcommand nesnesinin commandText'inde bulunan sorgu cümlesine entegre eder.
                    komut.Parameters.AddWithValue("@yzrid", yazarId);
                    BaglantiyiAc();
                    int sonuc = komut.ExecuteNonQuery();
                    if (sonuc>0)
                    {
                        MessageBox.Show("Silindi");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("Hata: Silinemedi...");
                    }
                    BaglantiyiKapat();
                }

            }


        }

        private void silPasifeCekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (baglanti)
                {
                    DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                    int YazarId = Convert.ToInt32(satir.Cells["YazarId"].Value);

                    //1.Yol
                    string updateSorgucumlesi = $"Update Yazarlar Set YazarAdSoyad = '{txtYazar.Text.Trim()}' where YazarId ={YazarId}";
                    SqlCommand updateCommand = new SqlCommand(updateSorgucumlesi, baglanti);


                    BaglantiyiAc();


                    int sonuc = updateCommand.ExecuteNonQuery();
                    if (sonuc > 0)
                    {
                        MessageBox.Show($"Yazar Güncellendi");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("Yazar güncellenmedi!");
                    }

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("Pasife çek silme işleminde hata: " + ex.Message);            }
           
        }

        private void silBaskaBirYontemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Bu yöntem yukarıdakiler gibi kullanışlı değildir.
            try
            {

                DataGridViewRow secilenSatir = dataGridViewYazarlar.SelectedRows[0];
                int yazarId = (int)secilenSatir.Cells["YazarId"].Value;
                string yazar = Convert.ToString(secilenSatir.Cells["YazarAdSoyad"].Value);
                SqlCommand komut = new SqlCommand();
                komut.Connection = baglanti;

                DialogResult cevap = MessageBox.Show($"{yazar} adlı yazarı silmek istediğinize emin misiniz?", "ONAY", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (cevap == DialogResult.Yes)
                {
                    //silecek
                    //komut.CommandText = $"Delete from Yazarlar where YazarId={yazarId}";
                    //@yzrid diyerek bir parametre oluşturmuş olduk.
                    komut.CommandText = $"Delete from Yazarlar where YazarId=@yzrid";
                    komut.Parameters.Clear();
                    //AddWithValue metodu @yzrid yerine yazarId değerini sqlcommand nesnesinin commendText'inde bulunan sorgu cümlesine entegre eder.
                    komut.Parameters.AddWithValue("@yzrid", yazarId);

                    BaglantiyiAc();
                    int sonuc = komut.ExecuteNonQuery();
                    if (sonuc > 0)
                    {
                        MessageBox.Show("Silindi");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("HATA:Silinemedi!");
                    }
                    BaglantiyiKapat();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("HATA: " + ex.Message);
            }
        }
    }
}
