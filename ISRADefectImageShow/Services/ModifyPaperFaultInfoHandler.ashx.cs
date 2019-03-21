using ISRADefectImageShow.DataServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.Services
{
    /// <summary>
    /// Summary description for ModifyPaperFaultInfoHandler
    /// </summary>
    public class ModifyPaperFaultInfoHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var requestBody = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            ModifyPaperFaultRequestData reqData = JsonConvert.DeserializeObject<ModifyPaperFaultRequestData>(requestBody);
            MyHttpRequest myReq = new MyHttpRequest();
            
            Dictionary<string, object> dictionary = myReq.ModifyPaperFault(reqData);

            context.Response.ContentType = "application/json";
            context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(dictionary));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}