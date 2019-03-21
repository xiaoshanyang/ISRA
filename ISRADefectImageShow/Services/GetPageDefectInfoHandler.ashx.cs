using ISRADefectImageShow.DataServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.Services
{


    /// <summary>
    /// Summary description for GetPageDefectInfoHandler
    /// </summary>
    public class GetPageDefectInfoHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var requestBody = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            GetDefectInfoListRequestData reqData = JsonConvert.DeserializeObject<GetDefectInfoListRequestData>(requestBody);
            MyHttpRequest myReq = new MyHttpRequest();
            Dictionary<string, object> dictionary = myReq.GetDefectInfoList(reqData);

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