using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.IO;


namespace RSAProjesi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        DialogResult result;
        string path;
        char[] dizi;
        private BigInteger a, b, phi, n=null;
        long p, q, sifreliDeger;
        string sifreliDizi;
        BigInteger[] sifreliDegerDizisi;
        int exp;
        public Dictionary<char, int> TurkceAlfabe;
        private BigInteger AralarindaAsalBul(BigInteger phi, BigInteger b)
        {
            BigInteger i = b - 1;

            while (i < b || i > 1)
            {
                if (phi % i != 0)
                {
                    goto etiket;
                }
                else
                {
                    i--;
                }
            }
        etiket:
            return i;
        }
        private bool AralarindaAsalMi(BigInteger phi, BigInteger b)
        {
            BigInteger i = 2;
            while (i < b)
            {
                if (phi % i == 0 && b % i == 0)//ikisi birden bölünüyorsa
                    return false;
                else
                    i++;
            }
            return true;
        }
        private bool Asalmi(BigInteger sayi)
        {
            if (sayi == 2) return true;
            if (sayi < 2 || sayi % 2 == 0) return false;
            for (long i = 3; i <= (Math.Sqrt(sayi.LongValue())); i += 2)
                if (sayi % i == 0)
                    return false;
            return true;
        }
        public void CreateKeys() 
        {
            Random rnd = new Random();
            while (true)
            {
            etiket1:
                p = rnd.Next(100, 999); //ilk sayı üretimi
                if (Asalmi(p)) //eğer sayı asal ise
                {
                etiket2:
                    q = rnd.Next(100, 999);//ikinci sayı üretimi
                    if (Asalmi(q))                //ilk sayı asalken ikinci sayı da asalsa    
                        break;                    //döngüden çık
                    else
                        goto etiket2;   //ikinci sayı asal değilse yeniden q üret
                }
                else
                    goto etiket1;                //ilk sayı asal değilse yeniden p üret
            }

            n = p * q; // n bulunur
            phi = EulerAlgorithm(p, q);//phi(n) sonucunu döndürür
            b = BigInteger.genPseudoPrime(12, 2, rnd); //rnd.Next(1, phi.IntValue());
            while (!AralarindaAsalMi(phi, b))
            {
                b = AralarindaAsalBul(phi, b);
            }
            //burada int e döndürülmemesi gerekiyor. long kabul eden bulunmalı
            a = b.modInverse(phi);// ModulerTersAl(b, phi);// gizli anahtar b^-1 = a bulunur          
            textBox_RSA_PublicKey.Text = b.ToString();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            textBox_RSA_PublicKey.ReadOnly = true;
            CreateKeys();            
        }
        private long EulerAlgorithm(long p, long q)//Big int döndürmeli
        {
            long euler = (p - 1) * (q - 1);
            return euler;
        }
        public string OpenFile()
        {
            string FilePath = "";
            openFileDialog1.Title = "Please select a text file";
            // openFileDialog1.Filter = "Text files(*.txt)|*.txt";
            openFileDialog1.Filter = " (*.txt)|*.txt";//|(*.docx)|*.docx|(*.doc)|*.doc";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog1.Multiselect = false;
            result = openFileDialog1.ShowDialog();
            path = openFileDialog1.FileName;
            foreach (string str in openFileDialog1.FileNames)
                FilePath = str;
            return FilePath;
        }
        public void TextYaz(RichTextBox cipher, RichTextBox plain, ListBox frekans, ListBox solution)
        {
            if (checkBox1.Checked == false)
            {
                cipher.Text = "";
                frekans.Items.Clear();
                solution.Items.Clear();
                OpenFile();
                if (result == DialogResult.OK)
                {
                    StreamReader FileYKCipherText = new StreamReader(File.OpenRead(path));
                    DataTable dt = new DataTable();
                    string first_row = FileYKCipherText.ReadLine();
                    while (first_row != null && !FileYKCipherText.EndOfStream)
                    {
                        cipher.Text += first_row;
                        first_row = FileYKCipherText.ReadLine();
                    }
                    cipher.Text += " " + first_row;
                    FileYKCipherText.Close();
                }
                else
                    MessageBox.Show("Please select a text file!");

            }
            
            plain.Text = "";
        }
        private void FrekansHesapla(RichTextBox metin)
        {
            dizi = metin.Text.ToCharArray();
        }

        private BigInteger KareAlmaVeCarpma(BigInteger deger, BigInteger b, BigInteger n)
        {
            string binValue = Convert.ToString(b.LongValue(), 2);
            //BigInteger intValue = Convert.ToInt32(binValue, 2);
            int l = binValue.Length;
            BigInteger sonuc = 1;
            //Array.Reverse(binValue.ToArray(), 0, (l - 1));
            for (int i = 0; i < l; i++)
            {
                if (binValue[i] == '1')
                {
                    sonuc = (sonuc * sonuc * deger) % n;
                }
                else
                {
                    sonuc = (sonuc * sonuc) % n;
                }
            }
            return sonuc;
        }
        private long NGraphDonusumu(int[] deger, int grafsayisi, int alfabeBoyutu, BigInteger n) 
        {            
            long sifreliDeger=0;
            int temp = grafsayisi;
            for (int i = 0; i < grafsayisi; i++)
            {
                sifreliDeger += (int)( Math.Pow(alfabeBoyutu, temp - 1))*deger[i];              
                temp--;
            }       
            return sifreliDeger;
        }
        public int KacGrafDonusumuYapilacak(BigInteger n, int alfabeBoyutu) 
        {
            int graphsayisi = 0, graf_deger=1;
            while ( graf_deger-1<n ) 
            {
                graf_deger *= alfabeBoyutu;
                graphsayisi++;
            }
            return graphsayisi-1;
        }
        private string KarakterSifreleme(string ascii, BigInteger b, BigInteger n, BigInteger phi)
        {
            //BigInteger[] asciiDeger= new BigInteger[ascii.Length];
            string sifreliKarakter = "", temp = "";
            //ascii = ascii.Replace(" ", ""); 
            char[] charDizisi = ascii.ToLower().ToCharArray();
            
            int substringindex = 0;
            int graf_sayisi=KacGrafDonusumuYapilacak(n,TurkceAlfabe.Count);
            string donusumyap;
            int[] asciiDeger = new int[graf_sayisi];//bir graf dönüşümü için o kadar harfin değerini tutar
            if (charDizisi.Length%graf_sayisi!=0)
            {                
                for (int i = 0; i < graf_sayisi-(charDizisi.Length%graf_sayisi); i++)
                {
                    temp += " ";//eksik kalan yerler boşluk ile doldurulur
                }
            }
            ascii += temp;
            charDizisi = ascii.ToCharArray();
            sifreliDegerDizisi = new BigInteger[ascii.Length / graf_sayisi];

            for (int i = 0; i < ascii.Length/graf_sayisi; i++)
            {
                donusumyap = ascii.Substring(substringindex,graf_sayisi);//ilk n karakteri alır
                int sayac = 0;
                for (int j = substringindex; j < graf_sayisi + substringindex; j++)
                {
                    var keys = from entry in TurkceAlfabe
                               where entry.Key == charDizisi[j]
                               select entry.Value;
                    foreach (var key in keys)
                        if (TurkceAlfabe.ContainsKey(charDizisi[j]))
                        {
                            asciiDeger[sayac] = key;
                            sayac++;
                        }
                }               
                sifreliDeger = NGraphDonusumu(asciiDeger, graf_sayisi, TurkceAlfabe.Count, n);//Donusum degeri hesaplanır           
                sifreliDegerDizisi[i] = KareAlmaVeCarpma(sifreliDeger, b, n);
                sifreliKarakter += GrafDonusumu(sifreliDegerDizisi[i], n, TurkceAlfabe.Count);//ASCII karakter tablosu için 256
                
                substringindex += graf_sayisi;
            }
            return sifreliKarakter;
        }
        private void button1_Click(object sender, EventArgs e)
        { 
            try
            {
                if (textBox_RSA_PublicKey.Text != "")
                {
                    richTextBox2.Text = "";
                    TextYaz(richTextBox1, richTextBox2, listBox1, listBox2);
                    sifreliDizi = KarakterSifreleme(richTextBox1.Text, b, n, phi);//şifrelenmiş karakterler atanır.
                    richTextBox2.Text = sifreliDizi.ToString();
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (NullReferenceException NRE)
            {
                MessageBox.Show("Please create a public key to encryption");
            }
            finally 
            {
                
            }
           
        }
        private string GrafDonusumu(BigInteger deger, BigInteger n, BigInteger karakterMod) //n şifreleme yapılan mod, karaktermod ise donusum için kullanılacak alfabe
        {
            BigInteger gecici = 1, k;
            string sifreliKarakter = "";
           
           exp = 0; // exp üs derecesini tutuyor, k ise katsayı
            while (n>gecici)
            {
                exp++;
                gecici *= karakterMod;
                //kaç graf dönüşümü yapılacağını tutar
            }
            exp--;
            gecici /= karakterMod;
            //fazladan çarpılan bölünüyor.
            for (int j = 0; j < exp; j++)
            {
                {
                    k = deger / gecici;
                    deger -= k * gecici;
                   

                    var keys = from entry in TurkceAlfabe
                               where entry.Value == k.IntValue()
                               select entry.Key;
                    foreach (var key in keys)
                        sifreliKarakter+= key;

                      //System.Text.ASCIIEncoding.UTF32.GetChars(new byte[] { 2 }); ; //System.Text.Encoding.ASCII.GetChars(new byte[] { 2 });           
                    gecici /= karakterMod;
                }
            }
            //sifreliKarakter += (char)deger.IntValue();
            return sifreliKarakter;
        }
        public string SifreCoz(string sifreliMetin) 
        {
            BigInteger[] plaintext=new BigInteger[sifreliDegerDizisi.Length];
            char[] cipher=sifreliMetin.ToCharArray();  
            char [] CozulenHarfDegeri=new char[exp];
            string cozulenMetin = "";
            for (int i = 0; i < sifreliDegerDizisi.Length; i++)
			{
                plaintext[i]= KareAlmaVeCarpma(sifreliDegerDizisi[i],a, n);//buradan gelen değer sonucunda harflerin çözülmesi için graf dönüşümü yapılır
                BigInteger temp = plaintext[i];
                BigInteger ust, katsayi;
                int grap_sayisi=exp;
                for (int j = 0; j<exp; j++)
                {
                   ust = (long)(Math.Pow(TurkceAlfabe.Count,grap_sayisi-1));
                   katsayi = temp / ust;
                   temp -= ust * katsayi; 
                    
                    var keys = from entry in TurkceAlfabe
                               where entry.Value == katsayi
                               select entry.Key;
                    foreach (var key in keys)
                        CozulenHarfDegeri[j] = key;
                    cozulenMetin += CozulenHarfDegeri[j];
                    grap_sayisi--;
                }
                
			}
           
            return cozulenMetin;
           
        }
        private void button2_Click(object sender, EventArgs e)
        {
           //richTextBox1.Text = "Encrypted Text is: "+SifreCoz(sifreliDizi);
	     try
            {
                if (richTextBox2.Text == "")
                {
                    throw new NullReferenceException();
                }
                else
                {
                    MessageBox.Show("Encrypted Text: " + SifreCoz(sifreliDizi));
                }
            }
            catch (NullReferenceException ex) 
            {
                MessageBox.Show("Please enter a plaintext first and press encrypt button");
            }
            finally 
            {
            	
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TurkceAlfabe = new Dictionary<char, int>();
            char[] trk_alf={'a','b','c','ç','d','e','f','g','ğ','h','ı','i','j','k','l','m','n','o','ö','p','r','s','ş','t','u','ü','v','y','z',' ','.','*','+','-','_','/',',','%','½','&','\\','?','!','\'','#','<','>','|','^','{','}','(',')','[',']','=','0','1','2','3','4','5','6','7','8','9','"','@','q','w','x','~',';',':','é','$','€','æ','₺','ß','£','A','B','C','Ç','D','E','F','G','Ğ','H','I','İ','J','K','L','M','N','O','Ö','P','R','S','Ş','T','U','Ü','V','Y','Z'};
            for (int i = 0; i < trk_alf.Length; i++)
            {
                TurkceAlfabe.Add(trk_alf[i],i);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked==true)
            {
                richTextBox1.ReadOnly = false;
            }
            else
            {
                richTextBox1.ReadOnly = true;
            }
        }
    }
}
