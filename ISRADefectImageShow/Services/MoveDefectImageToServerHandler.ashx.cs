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
    /// Summary description for MoveDefectImageToServerHandler
    /// </summary>
    public class MoveDefectImageToServerHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var requestBody = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            MoveDefectImageByPUIdRequestData reqData = JsonConvert.DeserializeObject<MoveDefectImageByPUIdRequestData>(requestBody);
            MyHttpRequest myReq = new MyHttpRequest();
            Dictionary<string, object> dictionary = new Dictionary<string,object>();
            if (reqData.changefilename == 1)
            {
                dictionary = myReq.MoveImageFile(reqData.puId, reqData.changefilename);
            }
            else
            {
                dictionary = myReq.MoveImageFile(reqData.puId, reqData.startDate);
            }
            

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