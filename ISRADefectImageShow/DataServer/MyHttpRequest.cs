using HtmlAgilityPack;
using ISRADefectImageShow.Services;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Management;
using System.Diagnostics;

namespace ISRADefectImageShow.DataServer
{
    public class MyHttpRequest
    {
        // 默认组合纸病差值，非连续类型纸病，多个纸病合并条件间隔100米以内合并为一个纸病
        private double defectOffsetLength = 300;
        // 连续纸病 x轴差值 小于等于3 时 可以合并，大于3则认为单个纸病
        private double defectOffsetXLength = 3;
        //
        private List<FileInfo> fileInfoList = new List<FileInfo>();
        private string print1Url = System.Configuration.ConfigurationManager.AppSettings["puId2"].ToString();
        private string print2Url = System.Configuration.ConfigurationManager.AppSettings["puId4"].ToString();

        public MyHttpRequest()
        {
            double.TryParse(System.Configuration.ConfigurationManager.AppSettings["defectOffsetLength"].ToString(), out defectOffsetLength);
        }

        //body是要传递的参数,格式"roleId=1&uid=2"
        //post的cotentType填写:"application/x-www-form-urlencoded"
        //soap填写:"text/xml; charset=utf-8"
        public string PostHttp(string url, string body, string contentType)
        {
            string responseContent = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebRequest.ContentType = contentType;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 10 * 60 * 1000;

                byte[] btBodys = Encoding.UTF8.GetBytes(body);
                httpWebRequest.ContentLength = btBodys.Length;
                httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                responseContent = streamReader.ReadToEnd();

                httpWebResponse.Close();
                streamReader.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
            }
            catch (Exception ex) 
            { 
                
            }
            return responseContent;
        }

        public string GetDefectHtml(GetDefectImageAllWebNumRequestData reqData)
        {
            DataOpt dbopt = new DataOpt();
            DataTable dt = dbopt.getDefectImageNameBypfId(reqData.pfId);    //53532_171115_D.bmp
            // 默认页面，没有找到对应html文件时，显示
            string htmlPath = "D:\\ISRA\\HTML" + reqData.rollNum.Substring(0, 5) + "\\" + reqData.rollNum + "\\";
            string imageName = dt.Rows[0]["defectImageName"].ToString();
            if (imageName.IndexOf("_D.bmp") > 0)
            {
                htmlPath = "D:\\ISRA\\HTML" + reqData.rollNum.Substring(0, 5) + "\\" + reqData.rollNum + "\\" + imageName.Substring(0,imageName.IndexOf("_D.bmp")+1)+".html";
            }
            string content = string.Empty;

            //文件读取
            if (File.Exists(htmlPath))
            {
                FileStream fs = new FileStream(htmlPath, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = string.Empty;//直接读取一行
                while ((line = sr.ReadLine()) != null)
                {
                    //不为空，继续读取
                    content += line;
                }

                sr.Close();
                fs.Close();
            }
            else
            {
                                
            }

            //html转译
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);
            HtmlNodeCollection tableList = doc.DocumentNode.SelectNodes("//table");

            // 图片名改名
            tableList[1].SelectNodes("tr")[0].SelectNodes("td")[0].SelectNodes("img")[0].SetAttributeValue("src", imageName);
            tableList[1].SelectNodes("tr")[0].SelectNodes("td")[1].SelectNodes("img")[0].SetAttributeValue("src", imageName.Substring(0, imageName.IndexOf("_D.bmp") + 1) + ".bmp");
            // 模板图片改名
            HtmlNodeCollection liList = tableList[2].SelectNodes("tr")[0].SelectNodes("td")[0].SelectNodes("div")[0].SelectNodes("ul")[0].SelectNodes("li");
            liList[0].SelectNodes("img")[0].SetAttributeValue("src", "IMAGE\\Template\\" + reqData.rollNum.Substring(0, 5) + "\\Camera1.jpeg");
            liList[1].SelectNodes("img")[0].SetAttributeValue("src", "IMAGE\\Template\\" + reqData.rollNum.Substring(0, 5) + "\\Camera2.jpeg");
            liList[2].SelectNodes("img")[0].SetAttributeValue("src", "IMAGE\\Template\\" + reqData.rollNum.Substring(0, 5) + "\\Camera3.jpeg");

            return doc.ToString();
        }

        public Dictionary<string, object> DeletePaperFault(DeletePaperFaultequestData reqData)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            DataOpt dbopt = new DataOpt();
            bool isDelete = true;
            string successedpfId = "start";
            // 解析数据
            foreach (string tmp in reqData.selectedpfId)
            {
                if (isDelete)
                {
                    isDelete = dbopt.deleteDefectInfo(tmp);
                    successedpfId = successedpfId + (isDelete ? (","+tmp) : "");
                }
            }

            //如果successedpfId非空
            if (!successedpfId.Equals("start"))
            {
                List<string> refDefectId = reqData.refDefectId;
                dictionary = GetDefectInfoList(reqData as GetDefectInfoListRequestData, true, refDefectId);
            }
            else
            {
                dictionary.Add("state", 2);
                dictionary.Add("errmessage", "写入数据库失败！");
                return dictionary;
            }


            if (dictionary["state"].ToString() == "2")
            {
                if (!isDelete)
                {
                    dictionary["errmessage"] = "写入数据库失败！" + dictionary["errmessage"];
                }
            }
            else
            {
                if (!isDelete)
                {
                    dictionary["state"] = 2;
                    dictionary.Add("errmessage", "写入数据库失败！");
                }
            }

            return dictionary;
        }

        public Dictionary<string, object> ModifyPaperFault(ModifyPaperFaultRequestData reqData)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            DataOpt dbopt = new DataOpt();
            bool isModify = true;
            // 解析数据
            foreach (ModifyPaperFaultDetail tmp in reqData.selectDefectInfoList)
            {
                if (isModify)
                {
                    // 是否关联停机代码
                    string stopRecordId = "0";
                    for (int pk = 0; pk < reqData.stopRecordList.Count; pk++)
                    {
                        if (tmp.pfId == reqData.stopRecordList[pk].pfId && reqData.stopRecordList[pk].stopRecordId.Count > 0)
                        {
                            stopRecordId = reqData.stopRecordList[pk].stopRecordId[0].ToString();
                        }
                    }

                    DataSet ds = dbopt.insertDefectInfo(tmp.pfId, reqData.ppID, reqData.eventID, reqData.rollNum, reqData.userId, reqData.pathId,"0", "0", "0", 
                        "0", "0", tmp.pfCode.Split('_')[0], tmp.pcCode.Split('_')[1], tmp.isTreatment, tmp.startMeter, tmp.endMeter, tmp.remark,
                        tmp.webNum.Substring(0, 1), tmp.webNum.Substring(1, 1), tmp.webNum.Substring(2, 1), tmp.webNum.Substring(3, 1), tmp.webNum.Substring(4, 1),
                        tmp.webNum.Substring(5, 1), tmp.webNum.Substring(6, 1), tmp.webNum.Substring(7, 1), tmp.webNum.Substring(8, 1), tmp.webNum.Substring(9, 1),
                        "0", "0", "0", stopRecordId);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        isModify = true;
                    }
                    else
                    {
                        isModify = false;
                    }
                }
            }

            // 返回状态，message，defectid, 更新页面两个表格
            if (!isModify)
            {
                dictionary.Add("state", 2);
                dictionary.Add("errmessage", "写入数据库失败！");
                return dictionary;
            }

            dictionary.Add("state", 1);

            return dictionary;
        }

        public Dictionary<string, object> GetDefectInfoList(GetDefectInfoListRequestData reqData, bool isDelete = false, List<string> refDefectId = null)
        {
            // 通过接口获取最新的纸病信息，返回后，本地保存到json文件中
            Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合             
            
            try
            {
                // 如果是当前工单的第一卷，则 取上一个下卷记录的结束时间为当前卷的起始时间
                DataOpt dOpt = new DataOpt();
                //18019T01
                if (reqData.rollNum.Length == 8 && reqData.rollNum.Substring(6,2) == "01")
                {
                    string startTime = dOpt.getRollStartTime(reqData.startTime, reqData.puId);
                    if (!string.IsNullOrEmpty(startTime))
                    {
                        reqData.startTime = startTime;
                    }
                }
                

                MyHttpRequest myRq = new MyHttpRequest();
                List<DefectInfo> dfList = myRq.GetISRADefectList(reqData);
                //1.5 保存dfList到JSON文件，当删除数据库中数据时，动态刷新上方表格，添加

                //2. 读取数据库获取已经处理完成的数据
                
                DataSet dtDefectInfo = dOpt.getDefectInfoByRollNum(reqData.puId, reqData.eventId);
                if (dtDefectInfo == null || dtDefectInfo.Tables.Count != 2)// || dtDefectInfo.Rows.Count == 0
                {
                    dictionary.Clear();
                    dictionary.Add("state", 2);
                    dictionary.Add("errmessage", "获取纸病信息失败");
                    return dictionary;
                }
                dictionary.Add("eventNum", dtDefectInfo.Tables[0].Rows[0][0].ToString());
                dictionary.Add("isBlocked", string.IsNullOrEmpty(dtDefectInfo.Tables[0].Rows[0][1].ToString()) ? "-1" : dtDefectInfo.Tables[0].Rows[0][1].ToString());
                // 存在已经保存入库的纸病记录，与接口获取的原始纸病对比，保留未处理的数据
                for (int i = 0; i < dfList.Count; )
                {
                    bool isSame = false;
                    for (int j = 0; j < dtDefectInfo.Tables[1].Rows.Count; j++)
                    {
                        if (dtDefectInfo.Tables[1].Rows[j]["defectID"].ToString().IndexOf(dfList[i].defectId) >= 0)
                        {
                            dfList.RemoveAt(i);
                            isSame = true;
                            break;
                        }
                    }

                    if (!isSame)
                    {
                        i++;
                    }
                }

                if (isDelete)
                {
                    List<int> refDefectIdInt = new List<int>();
                    for (int k = 0; k < refDefectId.Count; k++) 
                    {
                        refDefectIdInt.Add(Convert.ToInt32(refDefectId[k].ToString()));
                    }
                    refDefectIdInt.Sort();
                    for (int k = 0, i = 0; k < refDefectIdInt.Count; k++)
                    {
                        for (; i < dfList.Count; )
                        {
                            if (refDefectIdInt[k].ToString() != dfList[i].defectId.ToString())
                            {
                                dfList.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                                if (k != refDefectIdInt.Count - 1)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    
                }

                //3、 读取数据库获取纸病代码
                if (!isDelete)
                {
                    DataTable dtDefectCode = dOpt.getDefectCodeList();
                    if (dtDefectCode == null || dtDefectCode.Rows.Count == 0)
                    {
                        dictionary.Clear();
                        dictionary.Add("state", 2);
                        dictionary.Add("errmessage", "获取纸病代码失败");
                        return dictionary;
                    }
                    // 整理纸病代码
                    ArrayList defectCodeList = new ArrayList();
                    foreach (DataRow dataRow in dtDefectCode.Rows)
                    {
                        Dictionary<string, object> defectCode = new Dictionary<string, object>();
                        foreach (DataColumn dataColumn in dtDefectCode.Columns)
                        {
                            defectCode.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                        }
                        defectCodeList.Add(defectCode); //ArrayList集合中添加键值
                    }
                    dictionary.Add("defectCodeList", defectCodeList);
                }
                
                

                //4、 读取处理工序
                if (!isDelete)
                {
                    DataTable dtDefectProcedure = dOpt.getDefectProcedure();
                    if (dtDefectProcedure == null || dtDefectProcedure.Rows.Count == 0)
                    {
                        dictionary.Clear();
                        dictionary.Add("state", 2);
                        dictionary.Add("errmessage", "获取纸病代码失败");
                        return dictionary;
                    }
                    // 整理处理工序
                    ArrayList defectProcedureList = new ArrayList();
                    foreach (DataRow dataRow in dtDefectProcedure.Rows)
                    {
                        Dictionary<string, object> defectProcedure = new Dictionary<string, object>();
                        foreach (DataColumn dataColumn in dtDefectProcedure.Columns)
                        {
                            defectProcedure.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                        }
                        defectProcedureList.Add(defectProcedure); //ArrayList集合中添加键值
                    }
                    dictionary.Add("defectProcedureList", defectProcedureList);
                }
                

                // 组合两个表格数据为JSON, 返回页面     
                if (!isDelete)
                {
                    ArrayList arrayList = new ArrayList();
                    foreach (DataRow dataRow in dtDefectInfo.Tables[1].Rows)
                    {
                        Dictionary<string, object> defectInfo = new Dictionary<string, object>();
                        foreach (DataColumn dataColumn in dtDefectInfo.Tables[1].Columns)
                        {
                            defectInfo.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                        }
                        arrayList.Add(defectInfo); //ArrayList集合中添加键值
                    }
                    dictionary.Add("defectListDB", arrayList);
                }
                
                // 根据dfList中数据，查看图片文件是否存在
                if (!CheckDefectImage(dfList, reqData.puId, reqData.eventId))
                {
                    dictionary.Add("state", 3);
                }
                else
                {
                    dictionary.Add("state", 1);
                }

                dictionary.Add("defectList", dfList);
            }
            catch (Exception ex)
            {
                // 记录日志
                dictionary.Clear();
                dictionary.Add("state", 2);
                dictionary.Add("errmessage", ex.StackTrace.ToString());
            }
            
            return dictionary;
        }

        public List<DefectInfo> GetISRADefectList(GetDefectInfoListRequestData reqData) {
            // 如果是refresh，需要在数据库查询时间
            if (reqData.refresh == "1")
            {
                DataOpt dOpt = new DataOpt();
                DataTable dt = dOpt.getRollTimeByEventId(reqData.eventId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Start_Time"].ToString() == dt.Rows[0]["timeStamp"].ToString())
                    {
                        reqData.endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        reqData.endTime = dt.Rows[0]["timeStamp"].ToString();
                    }
                }   
            }

            // 接口调用获取纸病信息
            string url = "http://"+print1Url+":18096/getDefectInfo/";
            //string url = "http://127.0.0.1:18096/getDefectInfo/";

            // 根据pathId、puId确定访问印刷1、印刷2？    2	印刷1下卷/4	印刷2下卷
            if (reqData.puId == "2")
            {

            }
            else if (reqData.puId == "4")
            {
                url = "http://" + print2Url + ":18096/getDefectInfo/";
            }
            

            string resData = PostHttp(url, JsonConvert.SerializeObject(reqData), "application/json");
            //string resData = "{\"uploadFailDefectId\":\"\",\"defectList\":[{\"defectId\":\"18464\",\"time\":\"2018/8/17 12:42:42\",\"startMeter\":\"150.270935743123\",\"endMeter\":\"150.270935743123\",\"defectType\":\"advanced streak\",\"webNum\":\"6\"},{\"defectId\":\"19624\",\"time\":\"2018/8/17 12:50:52\",\"startMeter\":\"3662.37127868693\",\"endMeter\":\"3662.37127868693\",\"defectType\":\"small defect\",\"webNum\":\"6\"}]}";
            //1. 解析数据
            ResponseDefectInfoList res = JsonConvert.DeserializeObject<ResponseDefectInfoList>(resData);
            if (res == null)
            {
                return new List<DefectInfo>();
            }
            List<DefectInfo> dfList = res.defectList.ToList<DefectInfo>();
            //1.5 解析纸病信息，组合纸病
            return MergeDefect(dfList);
            //1.5 保存dfList到JSON文件，当删除数据库中数据时，动态刷新上方表格，添加

        }

        public List<DefectInfo> MergeDefect(List<DefectInfo> dfList)
        {
            // 遍历纸病
            for (int i = 0; i < dfList.Count; i++)
            {
                // 纸病类型 repeating defect
                bool isRepeatedDefect = "repeating defect".Equals(dfList[i].defectType) ? true : false;
                string webNum = dfList[i].webNum;
                double xLength1 = 0;
                double endMeter1 = 0;
                double.TryParse(dfList[i].endMeter, out endMeter1);
                double.TryParse(dfList[i].xLength, out xLength1);
                for (int j = i + 1; j < dfList.Count; )
                {
                    if (webNum.Equals(dfList[j].webNum))
                    {
                        //查看纸病类型
                        if (isRepeatedDefect)
                        {
                            if ("repeating defect".Equals(dfList[j].defectType))
                            {
                                // 如果x轴方向差值大于 1mm 则认为当前位单个纸病
                                double xLength2 = 0;
                                double.TryParse(dfList[j].xLength, out xLength2);
                                if (Math.Abs(xLength2 - xLength1) > defectOffsetXLength)
                                {
                                    break;
                                }
                                else
                                {
                                    dfList[i].endMeter = dfList[j].startMeter;
                                    dfList.RemoveAt(j);
                                    break;
                                }
                            }
                            else
                            {
                                j++;
                                continue;
                            }

                        }
                        //查看米数差
                        if (!"repeating defect".Equals(dfList[j].defectType))
                        {
                            double endMeter2 = 0;
                            double.TryParse(dfList[j].endMeter, out endMeter2);
                            if (endMeter2 - endMeter1 < this.defectOffsetLength)
                            {
                                //差值小于500m
                                dfList[i].endMeter = dfList[j].endMeter;
                                dfList.RemoveAt(j);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            j++;
                            continue;
                        }
                    }
                    else
                    {
                        j++;
                        continue;
                    }
                }
            }
            return dfList;
        }

        public Dictionary<string, object> GetDefectImage(GetDefectImageByPfIDRequestData reqData)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            //数据库中获取当前pfId对应的defectId
            DataOpt dbopt = new DataOpt();
            DataTable dt = dbopt.getDefectImageNameBypfId(reqData.pfId);    //53532_171115_D.bmp
            if (dt == null || dt.Rows.Count == 0)
            {
                dictionary.Add("state", 2);
                dictionary.Add("errmessage", "获取纸病图片失败");
            }
            else
            {
                dictionary.Add("state", 1);
                // 组合图片路径
                //IMAGE\18012\18012002\30573_051613.bmp
                string basicPath = "IMAGE\\" + dt.Rows[0]["Event_Num"].ToString().Substring(0, 5) + "\\" + dt.Rows[0]["Event_Id"].ToString() + "\\";
                string moudleImage = dt.Rows[0]["defectImageName"].ToString().Replace("_D", "");
                dictionary.Add("sourceImg", basicPath + moudleImage);
                dictionary.Add("defectImg", basicPath + dt.Rows[0]["defectImageName"].ToString());
                dictionary.Add("dLength", dt.Rows[0]["dLength"].ToString());
                dictionary.Add("dWidth", dt.Rows[0]["dWidth"].ToString());
                dictionary.Add("dArea", dt.Rows[0]["dArea"].ToString());
                
            }

            return dictionary;
        }

        public bool CheckDefectImage(List<DefectInfo> dfList, string puId, string eventId)
        {
            bool result = true;

            //根据当前puId 决定IP地址，2,4   2:印刷1 4:印刷2
            string IP = System.Configuration.ConfigurationManager.AppSettings["puId" + puId].ToString();
            //根据time字段组合图片路径
            string defectImagePath = @"\\"+IP+@"\ISRADefectInfo\Image\";
            //string defectHtmlePath = @"\\" + IP + @"\ISRADefectInfo\Html\";
            string curdefectImagePath = string.Empty;
            try
            {
                for (int i = 0; i < dfList.Count; i++)
                {
                    DateTime dt;
                    DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                    dtFormat.ShortDatePattern = "hh:MM:ss";
                    dt = Convert.ToDateTime(dfList[i].time, dtFormat);
                    if (!Directory.Exists(@"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5)))
                    {
                        Directory.CreateDirectory(@"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5));
                        Directory.CreateDirectory(@"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5) + @"\" + eventId + @"\");
                    }
                    else
                    {
                        if (!Directory.Exists(@"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5) + @"\" + eventId + @"\"))
                        {
                            Directory.CreateDirectory(@"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5) + @"\" + eventId + @"\");
                        }
                    }

                    curdefectImagePath = @"D:\ISRA\IMAGE\" + dfList[i].rollNum.Substring(0, 5) + @"\" + eventId + @"\" + dfList[i].defectId + "_" + dt.ToString("HHmmss");
                    //curdefectImagePath = @"Y:\IMAGE\" + dfList[i].rollNum.Substring(0, 5) + @"\" + dfList[i].rollNum + @"\" + dfList[i].defectId + "_" + dt.ToString("HHmmss");
                    // +"_D.bmp";
                    if (!File.Exists(curdefectImagePath + "_D.bmp"))
                    {
                        //拷贝文件
                        //\\192.168.101.163\ISRADefectInfo\Image\2018-09-27
                        string sourceTmpPath = defectImagePath + dt.ToString("yyyy-MM-dd") + @"\" + dfList[i].defectId + "_" + dt.ToString("HHmmss");
                        //string sourceHtmlpath = defectHtmlePath + dt.ToString("yyyy-MM-dd") + @"\" + dfList[i].defectId + "_" + dt.ToString("HHmmss");
                        if (File.Exists(sourceTmpPath + "_D.bmp"))
                        {
                            File.Move(sourceTmpPath + "_D.bmp", curdefectImagePath + "_D.bmp");
                        }
                        if (File.Exists(sourceTmpPath + ".bmp"))
                        {
                            File.Move(sourceTmpPath + ".bmp", curdefectImagePath + ".bmp");
                        }
                        //if (File.Exists(sourceHtmlpath + ".html"))
                        //{
                        //    File.Move(sourceHtmlpath + ".html", curdefectImagePath + ".html");
                        //}
                    }
                }
            }
            catch(Exception ex)
            {
                result = false;
            }
            
            return result;
        }

        public Dictionary<string, object> MoveImageFile(string puId, string startDate)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合 

            //根据当前puId 决定IP地址，2,4   2:印刷1 4:印刷2
            string IP = System.Configuration.ConfigurationManager.AppSettings["puId" + puId].ToString();
            bool status = false;

            //图片路径
            string defectImagePath = @"\\" + IP + @"\ISRADefectInfo\Image\";
            //html路径
            string defectHtmlPath = @"\\" + IP + @"\ISRADefectInfo\Html\";
            //Tagert
            string tagertPath = @"Y:\";
            //string tagertPath = @"D:\ISRA\";
            try
            {
                //验证共享地址能否访问
                //直接用IP也可以
                status = ConnectState(@"\\"+IP+@"\ISRADefectInfo", @"ISRA", "huhu");
                if (!status) {
                    dictionary.Add("state", 2);
                    dictionary.Add("message", "连接共享目录失败失败：" + defectHtmlPath);
                    return dictionary;
                }

                //获取html列表
                //string[] htmlList = Directory.GetFiles(defectHtmlPath);

                DirectoryInfo[] sourcepath = {new DirectoryInfo(defectHtmlPath)};
                GetFilesByDir(sourcepath, startDate);
                //fileInfoList
                for (int i = 0; i < fileInfoList.Count; i++)
                {
                    string curTmp = fileInfoList[i].FullName;
                    //文件名拆分，得到defectId，查看数据库中是否存在，存在则移动
                    string fileName = fileInfoList[i].Name.Replace(".html","");
                    string order = "";
                    string rollNum = "";
                    string eventId = "";
                    GetOrderInfo(fileName.Substring(0, fileName.IndexOf("_")), out order, out rollNum, out eventId);
                    if (!string.IsNullOrEmpty(order) && !string.IsNullOrEmpty(rollNum))
                    {
                        //移动文件
                        string tagert = tagertPath + @"IMAGE\" + order + @"\" + eventId + @"\" + fileName;
                        string tagertHtml = tagertPath + @"HTML\" + order + @"\" + eventId + @"\" + fileName;
                        string curSource = curTmp.Replace(".html", "").Replace("Html", "Image");
                        if (!Directory.Exists(tagertPath + @"IMAGE\" + order + @"\" + eventId))
                        {
                            Directory.CreateDirectory(tagertPath + @"IMAGE\" + order + @"\" + eventId);
                        }
                        if (!Directory.Exists(tagertPath + @"HTML\" + order + @"\" + eventId))
                        {
                            Directory.CreateDirectory(tagertPath + @"HTML\" + order + @"\" + eventId);
                        }
                        if (File.Exists(curTmp))
                        {
                            if (File.Exists(tagertHtml + ".html"))
                            {
                                File.Delete(curTmp);
                            }
                            else
                            {                                
                                File.Move(curTmp, tagertHtml + ".html");
                            }
                        }
                        if (File.Exists(curSource + ".bmp"))
                        {
                            if (File.Exists(tagert + ".bmp"))
                            {
                                File.Delete(curSource + ".bmp");
                            }
                            else
                            {
                                File.Move(curSource + ".bmp", tagert + ".bmp");
                            }
                        }
                        if (File.Exists(curSource + "_D.bmp"))
                        {
                            if (File.Exists(tagert + "_D.bmp"))
                            {
                                File.Delete(curSource + "_D.bmp");
                            }
                            else
                            {
                                File.Move(curSource + "_D.bmp", tagert + "_D.bmp");
                            }
                        }
                    }
                }
                dictionary.Add("state", 1);
            }
            catch (Exception ex)
            {
                dictionary.Add("state", 2);
                dictionary.Add("message", ex.Message);
            }
            return dictionary;
        }

        public void GetOrderInfo(string defectId, out string order, out string rollNum, out string eventId)
        {
            order = "";
            rollNum = "";
            eventId = "";
            //Y:\IMAGE\18123\18123002
            DataOpt dOpt = new DataOpt();
            DataTable dt = dOpt.getRollNumBydefectId(defectId);
            if (dt != null && dt.Rows.Count > 0)
            {
                rollNum = dt.Rows[0]["Event_Num"].ToString();
                order = rollNum.Substring(0, 5);
                eventId = dt.Rows[0]["Event_Id"].ToString();
            }
        }

        public void GetFilesByDir(DirectoryInfo[] directorys, string startDate)
        {
            if (directorys.Length == 0)
                return;
 
            foreach (var dire in directorys) 
            {
                // 如果小于当前时间，则跳过
                if (dire.Name.CompareTo(startDate)<0)
                {

                }
                else
                {
                    //获取当前目录下的所有文件
                    fileInfoList.AddRange(dire.GetFiles());
                    //递归下层目录
                    GetFilesByDir(dire.GetDirectories(), startDate);
                }
                
            }
        }

        public bool ConnectState(string path, string userName, string passWord)
        {
            bool Flag = false;

            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                //net use \\192.168.101.128\ISRADefectInfo /delete
                //net use \\192.168.101.128\ISRADefectInfo huhu /user:ISRA
                string dosLineDelete = "net use " + path + " /delete";
                string dosLine = "net use " + path +" " + passWord + @" /user:" + userName;
                proc.StandardInput.WriteLine(dosLineDelete);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                proc.StandardError.Close();

                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        public Dictionary<string, object> GetStopCodeList(GetStopCodeListBytimeRequestData reqData)
        {
            // 通过接口获取最新的停机列表，返回后，本地保存到json文件中
            Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合       

            // 读取数据库获取 停机记录
            DataOpt dopt = new DataOpt();
            DataTable dt = dopt.getStopRecords(reqData.eventId, reqData.defectTime, reqData.pfId, reqData.newRecord);

            // 组合数据，返回
            if (dt == null)
            {
                // 获取停机记录失败
                dictionary.Add("state", 2);
            }
            else if(dt.Rows.Count == 0)
            {
                // 无停机记录
                dictionary.Add("state", 1);
            }
            else
            {
                // get停机记录
                dictionary.Add("state", 0);
                ArrayList stopRecordList = new ArrayList();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Dictionary<string, object> stopRecord = new Dictionary<string, object>();  //实例化一个参数集合 
                    stopRecord.Add("stoprecordId", dt.Rows[i]["id"].ToString());
                    stopRecord.Add("RollNo", dt.Rows[i]["RollNo"].ToString());
                    stopRecord.Add("Start_Time", dt.Rows[i]["Start_Time"].ToString());
                    stopRecord.Add("End_Time", dt.Rows[i]["End_Time"].ToString());
                    stopRecord.Add("Event_Reason_Code", dt.Rows[i]["Event_Reason_Code"].ToString());
                    stopRecord.Add("Event_Reason_Name", dt.Rows[i]["Event_Reason_Name"].ToString());
                    stopRecord.Add("Remark", dt.Rows[i]["Remark"].ToString());
                    stopRecord.Add("Team", dt.Rows[i]["Team"].ToString());
                    stopRecord.Add("Shift", dt.Rows[i]["Shift"].ToString());
                    stopRecord.Add("User_Desc", dt.Rows[i]["User_Desc"].ToString());
                    stopRecordList.Add(stopRecord);
                }
                dictionary.Add("stopRecordList", stopRecordList);
            }
            return dictionary;
        }

        /// <summary>
        /// 文件夹改名，为eventId
        /// </summary>
        /// <param name="puId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public Dictionary<string, object> MoveImageFile(string puId, int changefilename)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合 

            //1、首先获取路径名
            DirectoryInfo basehtmlpath = new DirectoryInfo("Y:\\HTML");
            DirectoryInfo imagepath = new DirectoryInfo("Y:\\IMAGE");

            DirectoryInfo[] orderDirs = basehtmlpath.GetDirectories();
            foreach (var dire in orderDirs)
            {
                //2、查找eventId
                //3、进行替换
                GetEventIdByFileName(puId, dire.GetDirectories());
            }

            return dictionary;
        }

        public void GetEventIdByFileName(string puId, DirectoryInfo[] eventnum)
        {
            // 将当前工单下所有卷移动
            DataOpt dOpt = new DataOpt();
            
            foreach (var num in eventnum)
            {
                if (num.Name.Length == 8)
                {
                    string eventId = dOpt.geteventId(puId, num.Name);
                    if (eventId != null)
                    {
                        // 移动图片
                        string imgPath = num.FullName.ToString().Replace("HTML", "IMAGE");
                        DirectoryInfo imagepath = new DirectoryInfo(imgPath);
                        // 获取当前
                        //num.MoveTo(num.FullName.Substring(0, num.FullName.LastIndexOf("\\") + 1) + eventId);
                        //imagepath.MoveTo(imagepath.FullName.Substring(0, imagepath.FullName.LastIndexOf("\\") + 1) + eventId);
                    }
                    

                }
                
            }
        }
    }
}