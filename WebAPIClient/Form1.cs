using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebAPIClient
{
    public partial class FormTest : Form
    {
        public FormTest()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (this.openFileDialog1.FileNames != null && this.openFileDialog1.FileNames.Length > 0)
                {
                    foreach (var file in this.openFileDialog1.FileNames)
                    {
                        var idx = this.gv_File.Rows.Add();//将选择的文件路径添加到相应的DataGridView中
                        this.gv_File.Rows[idx].Cells[0].Value = file;
                    }
                }
            }
        }

        private void btRequest_Click(object sender, EventArgs e)
        {
            this.txtResponse.Text = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/" + this.cmbResponseContentType.Text.ToLower()));//设定要响应的数据格式
                using (var content = new MultipartFormDataContent())//表明是通过multipart/form-data的方式上传数据
                {
                    var formDatas = this.GetFormDataByteArrayContent(this.GetNameValueCollection(this.gv_FormData));//获取键值集合对应的ByteArrayContent集合
                    var files = this.GetFileByteArrayContent(this.GetHashSet(this.gv_File));//获取文件集合对应的ByteArrayContent集合
                    Action<List<ByteArrayContent>> act = (dataContents) =>
                    {//声明一个委托，该委托的作用就是将ByteArrayContent集合加入到MultipartFormDataContent中
                        foreach (var byteArrayContent in dataContents)
                        {
                            content.Add(byteArrayContent);
                        }
                    };
                    act(formDatas);//执行act
                    act(files);//执行act
                    try
                    {
                        var result = client.PostAsync(this.txtUrl.Text, content).Result;//post请求
                        this.txtResponse.Text = result.Content.ReadAsStringAsync().Result;//将响应结果显示在文本框内
                    }
                    catch (Exception ex)
                    {
                        this.txtResponse.Text = ex.ToString();//将异常信息显示在文本框内
                    }
                }
            }
        }
        /// <summary>
        /// 获取文件集合对应的ByteArrayContent集合
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private List<ByteArrayContent> GetFileByteArrayContent(HashSet<string> files)
        {
            List<ByteArrayContent> list = new List<ByteArrayContent>();
            foreach (var file in files)
            {
                var fileContent = new ByteArrayContent(File.ReadAllBytes(file));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(file)
                };
                list.Add(fileContent);
            }
            return list;
        }
        /// <summary>
        /// 获取键值集合对应的ByteArrayContent集合
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private List<ByteArrayContent> GetFormDataByteArrayContent(NameValueCollection collection)
        {
            List<ByteArrayContent> list = new List<ByteArrayContent>();
            foreach (var key in collection.AllKeys)
            {
                var dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(collection[key]));
                dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    Name = key
                };
                list.Add(dataContent);
            }
            return list;
        }
        /// <summary>
        /// 从DataGridView中获取键值对集合
        /// </summary>
        /// <param name="gv"></param>
        /// <returns></returns>
        private NameValueCollection GetNameValueCollection(DataGridView gv)
        {
            NameValueCollection collection = new NameValueCollection();
            var rows = gv.Rows;
            foreach (DataGridViewRow row in rows)
            {
                try
                {
                    if (row.Cells[0].Value != null)
                    {
                        collection.Add(row.Cells[0].Value.ToString(),
                            row.Cells[1].Value == null ? string.Empty : row.Cells[1].Value.ToString());
                    }
                }
                catch { }//忽略异常，不检测是否存在重复的键值
            }
            return collection;
        }
        /// <summary>
        /// 从DataGridView中获取选择的文件集合
        /// </summary>
        /// <param name="gv"></param>
        /// <returns></returns>
        private HashSet<string> GetHashSet(DataGridView gv)
        {
            HashSet<string> hash = new HashSet<string>();
            var rows = gv.Rows;
            foreach (DataGridViewRow row in rows)
            {
                if (row.Cells[0].Value != null)
                {
                    hash.Add(row.Cells[0].Value.ToString());
                }
            }
            return hash;
        }
    }
}
