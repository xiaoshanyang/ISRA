using ISRADefectImageShow.DataServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.Services
{

    public class PutDefectInfoToDBRequestData
    {
        public string ppID { get; set; }
        public string eventID { get; set; }
        public string rollNum { get; set; }
        public List<DefectDetailInfoToDB> selectDefectInfoList { get; set; }
        public string userId { get; set; }
        public string pathId { get; set; }       //机台    
        public List<StopRecordList> stopRecordList { get; set; }
        //"0", "0", "0" result+treament_procedure+paper_direction
        // 将工单信息同步带入 ppID,event_Id,userId
    }

    public class StopRecordList
    {
        public string defectId { get; set; }
        public List<string> stopRecordId { get; set; }
    }


    /// <summary>
    /// Summary description for PutDefectInfoToDBHandler
    /// </summary>
    public class PutDefectInfoToDBHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var requestBody = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            PutDefectInfoToDBRequestData reqData = JsonConvert.DeserializeObject<PutDefectInfoToDBRequestData>(requestBody);
            Dictionary<string, object> dictionary = SendDefectInfotoDB(reqData);

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

        public Dictionary<string, object> SendDefectInfotoDB(PutDefectInfoToDBRequestData reqData)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            ArrayList arrayList = new ArrayList();
            DataOpt dbopt = new DataOpt();
            bool isAdd = true;
            // 解析数据
            foreach (DefectDetailInfoToDB tmp in reqData.selectDefectInfoList)
            {
                if (isAdd)
                {
                    // defectId 与 stopRecord 关联
                    string stopRecordId = "0";
                    for (int pk = 0; pk < reqData.stopRecordList.Count; pk++)
                    {
                        if (tmp.defectId == reqData.stopRecordList[pk].defectId && reqData.stopRecordList[pk].stopRecordId.Count>0)
                        {
                            stopRecordId = reqData.stopRecordList[pk].stopRecordId[0].ToString();
                        }
                    }

                    int rowsLine = 0;
                    DataSet ds = dbopt.insertDefectInfo("0", reqData.ppID, reqData.eventID, reqData.rollNum, reqData.userId, reqData.pathId, "0", "0", "0",
                    tmp.defectId, tmp.defectImageName.Substring(tmp.defectImageName.LastIndexOf("/") + 1), tmp.pfCode.Split('_')[0], tmp.pcCode.Split('_')[1], tmp.isTreatment, tmp.startMeter, tmp.endMeter, tmp.remark,
                    tmp.webNum.Substring(0, 1), tmp.webNum.Substring(1, 1), tmp.webNum.Substring(2, 1), tmp.webNum.Substring(3, 1), tmp.webNum.Substring(4, 1),
                    tmp.webNum.Substring(5, 1), tmp.webNum.Substring(6, 1), tmp.webNum.Substring(7, 1), tmp.webNum.Substring(8, 1), tmp.webNum.Substring(9, 1),
                    tmp.dWidth, tmp.dLength, tmp.dArea, stopRecordId);

                    if (ds != null && ds.Tables.Count > 1 && ds.Tables[0].Rows.Count > 0)
                    {
                        Dictionary<string, object> defectInfo = new Dictionary<string, object>();
                        int.TryParse(ds.Tables[0].Rows[0][0].ToString(), out rowsLine);
                        defectInfo.Add("pf_Id", rowsLine);
                        //defectInfo.Add("pf_Code", tmp.pfCode.Split('_')[1]);
                        defectInfo.Add("pf_Code", tmp.pfCode);
                        defectInfo.Add("IsPrintTreatment", tmp.pcCode.Split('_')[0]);
                        defectInfo.Add("BedshaftMeter", tmp.startMeter);
                        defectInfo.Add("WatchSpindleMeter", tmp.endMeter);
                        defectInfo.Add("defectId", tmp.defectId);
                        defectInfo.Add("defectImageName", tmp.defectImageName.Substring(tmp.defectImageName.LastIndexOf("/")+1));
                        defectInfo.Add("Remark", tmp.remark);
                        defectInfo.Add("Img1", tmp.webNum.Substring(0, 1));
                        defectInfo.Add("Img2", tmp.webNum.Substring(1, 1));
                        defectInfo.Add("Img3", tmp.webNum.Substring(2, 1));
                        defectInfo.Add("Img4", tmp.webNum.Substring(3, 1));
                        defectInfo.Add("Img5", tmp.webNum.Substring(4, 1));
                        defectInfo.Add("Img6", tmp.webNum.Substring(5, 1));
                        defectInfo.Add("Img7", tmp.webNum.Substring(6, 1));
                        defectInfo.Add("Img8", tmp.webNum.Substring(7, 1));
                        defectInfo.Add("Img9", tmp.webNum.Substring(8, 1));
                        defectInfo.Add("Img10", tmp.webNum.Substring(9, 1));
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            defectInfo.Add("Event_Reason_Code", ds.Tables[1].Rows[0][0].ToString());
                            defectInfo.Add("Start_Time", ds.Tables[1].Rows[0][1].ToString());
                            defectInfo.Add("End_Time", ds.Tables[1].Rows[0][2].ToString());
                        }
                       

                        arrayList.Add(defectInfo); //ArrayList集合中添加键值
                    }else{
                        isAdd = false;
                    }
                }
                 
            }

            // 返回状态，message，defectid, 更新页面两个表格
            if (!isAdd)
            {
                dictionary.Add("state", 2);
                dictionary.Add("errmessage", "写入数据库失败！");
                return dictionary;
            }
            
            dictionary.Add("state", 1);
            // 组成一条新数据导入数据库表中
            dictionary.Add("Event_Num", reqData.rollNum);
            dictionary.Add("defectListDB", arrayList);


            return dictionary;
            

        }
    }
}
