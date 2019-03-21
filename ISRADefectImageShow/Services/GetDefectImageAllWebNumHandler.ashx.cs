using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.Services
{
    /// <summary>
    /// Summary description for GetDefectImageAllWebNumHandler
    /// </summary>
    public class GetDefectImageAllWebNumHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            // 根据数据库中纸病id，查找html, 读取文件，返回html


            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
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