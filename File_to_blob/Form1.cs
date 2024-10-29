using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Drawing;

namespace File_to_blob
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileName = @openFileDialog1.FileName;   //открываем выбранный файл
            var fileIn = File.ReadAllBytes(fileName);   //заносим в переменную выбранный файл
            string s = Convert.ToBase64String(fileIn);  //конвертируем в base64
            

            //Отображаем окно выбора файла
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // переменная для удаления файла
                FileInfo fileInf = new FileInfo(saveFileDialog1.FileName);
                //если файл уже существовал, то удаляем его
                fileInf.Delete();

                //переменная для записи данных в файл
                StreamWriter fileWrite = new StreamWriter(fileInf.Create());

                FileInfo fileInf2 = new FileInfo(openFileDialog1.FileName);
                //запись данных в файл
                //fileWrite.WriteLine("Тестовая запись в файл");
                //fileWrite.WriteLine(openFileDialog1.FileName);
                //fileWrite.WriteLine(s);   // заносим конвертированную
                //fileWrite.WriteLine(lenbase64);

                //СОЗДАЮ СКРИПТ
                var offset = 0;
                var i = 1;
                var step =0;
                var lenbase64 = s.Length;
                if (lenbase64 >= 2000) step = 2000; else step = lenbase64;
                var param = s.Substring(offset, step);
                var param2 = s.Substring(offset, lenbase64);

              /*  fileWrite.WriteLine(fileInf2.Name);
                fileWrite.WriteLine("Длинна: " + lenbase64);
                fileWrite.WriteLine("Для проверки: " + s);
                */fileWrite.WriteLine("declare");
                while (lenbase64 > offset)
                {
                    if((lenbase64-offset) < step) step = lenbase64 - offset;
                    param = s.Substring(offset, step);
                    fileWrite.WriteLine(string.Format("v_blob_base{1} varchar2({2}) := '{0}';", param, i, step));
                    offset += step;
                    i++;

                }
                fileWrite.WriteLine("v_blob_data   blob;");
                fileWrite.WriteLine();
                fileWrite.WriteLine("function from_base64(t in varchar2) return varchar2 is");
                fileWrite.WriteLine("begin");
                fileWrite.WriteLine(" return utl_encode.base64_decode(utl_raw.cast_to_raw(t));");
                fileWrite.WriteLine("end from_base64;") ;
                fileWrite.WriteLine();
                fileWrite.WriteLine("procedure writeBlobChunk(p_data in varchar2) is");
                fileWrite.WriteLine("chnk raw(32767) := HEXTORAW(from_base64(p_data));");
                fileWrite.WriteLine("begin");
                fileWrite.WriteLine(" dbms_lob.writeappend(v_blob_data, utl_raw.length(chnk), chnk);");
                fileWrite.WriteLine("end;");
                fileWrite.WriteLine();
                fileWrite.WriteLine("begin");
                fileWrite.WriteLine(string.Format("insert into lob_table(bdy, name_nm)  values(empty_blob(), '{0}')  returning bdy into v_blob_data;", fileInf2.Name));
                fileWrite.WriteLine();

                for (int j=1; j < i; j++)
                { 
                    fileWrite.WriteLine(string.Format("writeBlobChunk(v_blob_base{0});",j)); 
                }
                fileWrite.WriteLine("commit;");
                fileWrite.WriteLine("end;");
                fileWrite.WriteLine("/");

                //закрываем файл
                fileWrite.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
            {

            //Отображаем окно выбора файла
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName != null)
            { 
                //Создаем переменную для чтения из файла
                StreamReader Fileread = new StreamReader(openFileDialog1.FileName);

                listBox1.Items.Add(openFileDialog1.FileName);
                Fileread.Close();
            }
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
