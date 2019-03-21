using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.DataServer
{
    public class JsonData
    {
        
    }

    public class MoveDefectImageByPUIdRequestData
    {
        public string puId { get; set; }
        public string pathId { get; set; }
        public string startDate { get; set; }
        public int changefilename { get; set; }
    }

    public class GetDefectInfoListRequestData
    {
        public string orderId { get; set; }
        public string rollNum { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string webNum { get; set; }
        public string puId { get; set; }
        public string refresh { get; set; }
        public string eventId { get; set; }
    }

    public class GetDefectImageByPfIDRequestData
    {
        public string pfId { get; set; }
    }

    public class DeletePaperFaultequestData : GetDefectInfoListRequestData
    {
        public List<string> selectedpfId { get; set; }
        public List<string> refDefectId { get; set; }
    }

    public class ModifyPaperFaultRequestData
    {
        public List<ModifyPaperFaultDetail> selectDefectInfoList { get; set; }
        public string ppID { get; set; }
        public string eventID { get; set; }
        public string rollNum { get; set; }
        public string userId { get; set; }
        public string pathId { get; set; }       //机台
        public List<pfIdStopRecordList> stopRecordList { get; set; }
    }

    public class ModifyPaperFaultDetail
    {
        public string pfId { get; set; }
        public string webNum { get; set; }
        public string pfCode { get; set; }
        public string pcCode { get; set; }
        public string isTreatment { get; set; }
        public string startMeter { get; set; }
        public string endMeter { get; set; }
        public string remark { get; set; }
        
    }

    public class pfIdStopRecordList
    {
        public string pfId { get; set; }
        public List<string> stopRecordId { get; set; }
    }

    public class GetDefectImageAllWebNumRequestData
    {
        public string rollNum { get; set; }
        public string pfId { get; set; }
    }

    public class ResultJsonData_DefectInfo
    {

    }

    public class ResponseDefectInfoList
    {
        public string uploadFailDefectId { get; set; }
        public List<DefectInfo> defectList { get; set; }
    }

    public class DefectInfo
    {
        public string defectId { get; set; }
        public string rollNum { get; set; }
        public string time { get; set; }
        public string startMeter { get; set; }
        public string endMeter { get; set; }
        public string webNum { get; set; }
        public string defectType { get; set; }
        public string xLength { get; set; }
        public string dWidth { get; set; }
        public string dLength { get; set; }
        public string dArea { get; set; }
    }

    public class DefectDetailInfoFromDB
    {
        public string rollNum { get; set; }
        public string startMeter { get; set; }
        public string endMeter { get; set; }
        public string createTime { get; set; }
        public string defectCode { get; set; }
        public string defectCodeDecrible { get; set; }
        public string deal_Process { get; set; }
        public string webNum { get; set; }
        public string remark { get; set; }
    }

    public class DefectDetailInfoToDB
    {
        public string defectId { get; set; }
        public string defectImageName { get; set; }
        public string startMeter { get; set; }
        public string endMeter { get; set; }
        public string pfCode { get; set; }
        public string pcCode { get; set; }
        public string isTreatment { get; set; }
        public string webNum { get; set; }
        public string remark { get; set; }
        public string dWidth { get; set; }
        public string dLength { get; set; }
        public string dArea { get; set; }
    }
    /// <summary>
    /// 停机列表
    /// </summary>
    public class GetStopCodeListBytimeRequestData
    {
        public string defectTime { get; set; }
        public string eventId { get; set; }
        public string pfId { get; set; }
        public bool newRecord { get; set; }
    }
}