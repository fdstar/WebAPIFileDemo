using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebAPIFileDemo.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value" + id;
        }

        // POST api/values
        [HttpPost]
        public async Task<Dictionary<string, string>> Post(int id = 0)
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string root = HttpContext.Current.Server.MapPath("~/App_Data");//指定要将文件存入的服务器物理位置
            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {//接收文件
                    Trace.WriteLine(file.Headers.ContentDisposition.FileName);//获取上传文件实际的文件名
                    Trace.WriteLine("Server file path: " + file.LocalFileName);//获取上传文件在服务上默认的文件名
                }//TODO:这样做直接就将文件存到了指定目录下，暂时不知道如何实现只接收文件数据流但并不保存至服务器的目录下，由开发自行指定如何存储，比如通过服务存到图片服务器
                foreach (var key in provider.FormData.AllKeys)
                {//接收FormData
                    dic.Add(key, provider.FormData[key]);
                }
            }
            catch
            {
                throw;
            }
            return dic;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}