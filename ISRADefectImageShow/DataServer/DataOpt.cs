using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.DataServer
{
    public class DataOpt
    {

        private DataBase nmDB = new DataBase();
        public DataOpt()
        {
            
        }

        /// <summary>
        /// 获取当前卷的起始时间
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string getRollStartTime(string curTime, string puId)
        {
            string sql = "select top 1 TimeStamp from [dbo].[Events] where TimeStamp <= '" + curTime + "' and pu_Id = " + puId + " order by TimeStamp desc";
            DataSet ds = new DataSet();
            string startTime = string.Empty;
            try
            {
                nmDB.RunProc(sql, ds);
                startTime = ds.Tables[0].Rows[0][0].ToString();
            }
            catch (Exception ex)
            {

            }
            return startTime;
        }

        /// <summary>
        /// 获取印刷卷当前的时间区间
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DataTable getRollTimeByEventId(string eventId)
        {
            string sql = "select Start_Time, timeStamp from events where Event_Id = " + eventId;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                nmDB.RunProc(sql, ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return dt;
        }

        /// <summary>
        /// 根据卷号+机台号 获取 停机代码信息关联
        /// </summary>
        /// <param name="rollNum"></param>
        /// <returns></returns>
        public DataSet getDefectInfoByRollNum(string puId, string eventId)
        {
            DataSet ds = new DataSet();          
            try
            {
                // 根据eventId获取一下最新的rollNum
                string sql = string.Format(@"select top 100 Event_Num,Blocked_In_Location  
                                            from events e
                                            left join Event_Details ed on ed.Event_Id = e.Event_Id 
                                            where e.Event_Id = {0};", eventId);
                // 根据eventId 获取纸病记录 where e.pu_id = {0} and e.event_num = '{1}' and pfc1.PDCode not in (33102, 33101)
                sql += string.Format(@"select top 1000 pf.pf_Id, pf.Event_Num, isnull(pfc.PDCode+'_'+pfc.PDCNText,pfc1.PDCode+'_'+pfc1.PDCNText) as pf_Code, pf.BedshaftMeter, pf.WatchSpindleMeter, 
                        pf.Remark, pf.Img1, pf.Img2,pf.Img3,pf.Img4,pf.Img5,pf.Img6,pf.Img7,pf.Img8,pf.Img9,pf.Img10, pl.PL_Desc as IsPrintTreatment,
                        r.ID, isnull(er.Event_Reason_Code+'_'+ er.Event_Reason_Name,'') as Event_Reason_Code, CONVERT(char(11),r.Start_Time,120)+CONVERT(char(8),r.Start_Time,114) Start_Time, CONVERT(char(11),r.End_Time,120)+CONVERT(char(8),r.End_Time,114) End_Time, 
                        isnull(pd.defectId,0) as defectId, isnull(pd.defectImageName,'000.bmp') as defectImageName
                        from [dbo].[UDT_PaperFault] pf
                        left join Events as e on e.Event_Id = pf.Event_Id
                        left join UDT_PaperFault_DefectID as pd on pd.pf_id = pf.pf_Id
                        left join UDT_PageDisease_Code as pfc on pfc.PDCode = pf.pf_Code
                        left join UDT_PaperDefect_Code as pfc1 on pfc1.PDCode = pf.pf_Code
                        left join dbo.Prod_Lines as pl on pl.PL_Id = pf.IsPrintTreatment
                        left join UDT_RunRecord_Defect_Relation dr on pf.pf_id = dr.Defect_ID
                        left join UDT_Run_Record r on dr.Stop_record_ID=r.Id
                        left join Event_Reasons er on er.event_reason_Id=r.Code
                        where e.pu_id = {0} and e.Event_Id = {1} and pfc1.PDCode not in (33102, 33101)
                        order by pf.pf_Id desc;", puId, eventId);
                nmDB.RunProc(sql, ds);
            }
            catch (Exception ex)
            {

            }
            return ds;
        }

        /// <summary>
        /// 获取纸病代码
        /// </summary>
        /// <returns></returns>
        public DataTable getDefectCodeList()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                //string sql = @"select PDCode,PDCNText from UDT_PageDisease_Code where ProcessLine = 1 order by PDCode asc";
                string sql = @"select PDCode,PDCNText from UDT_PaperDefect_Code 
                                where ProcessLine in (1,12) and ActiveStatus = 1 and WasteType in (0,1) or PDCode in (34101,34102,34103,33104,33105)
                                order by PDCode asc";
                nmDB.RunProc(sql, ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return dt;
        }

        /// <summary>
        /// 获取纸病处理工序
        /// </summary>
        /// <returns></returns>
        public DataTable getDefectProcedure()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                string sql = @"select PL_Id,PL_Desc from dbo.Prod_Lines where PL_Id in (1,2,3,4)";
                nmDB.RunProc(sql, ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return dt;
        }

        /// <summary>
        /// 插入/更新纸病信息
        /// </summary>
        /// <returns></returns>
        public DataSet insertDefectInfo(string pfId, string ppId, string eventId, string eventNum, string creatorId, string pathId, string result, string procedure, string paper_direction,
            string defectId, string defectImageName, string pfCode, string treatmentNode, string isTreatment, string bedshaftMeter, string watchSpindleMeter, string remark,         
            string img1, string img2, string img3,
            string img4, string img5, string img6, string img7, string img8, string img9, string img10,
            string dWidth, string dLength, string dArea, string stopRecordId)
        {
            DataSet ds = new DataSet();
            try
            {
                //spGUI_InsterPaperFault_AD 存储过程
                //0 @pf_Id    Int ,   --纸病ID 
                //1 @pp_Id    Int ,   --ppID
                //2 @Event_Id int,    --
                //3 @Event_Num   VarChar(50), --纸卷号
                //4 @pf_Code   VarChar(50), --纸病代码
                //5 @BedshaftMeter  VarChar(50), --开始米数
                //6 @WatchSpindleMeter VarChar(50), --结束米数
                //7 @Remark    VarChar(5000), --轴表米数
                //8 @Img1   int, --幅号1
                //9 @Img2   int, --幅号2
                //10    @Img3   int, --幅号3
                //11    @Img4   int, --幅号4
                //12    @Img5   int, --幅号5
                //13    @Img6   int, --幅号6
                //14    @Img7   int, --幅号7
                //15    @IsPrintTreatment int,   --是否复合处理
                //16    @IsTreatment      VarChar(50),
                //17    @CreatorId   Int,   --创建人ID
                //18    @Path_id     Int,   --机台ID
                //19    @result      VarChar(50),
                //20    @treament_procedure int, --处理工序
                //21    @paper_direction int,--纸病方向：0=轴底到轴表；1=轴表到轴底
                //22    @Img8   int, --幅号8
                //23    @Img9   int, --幅号9
                //24    @Img10   int --幅号10
                //25    @defectId int 纸病id
                //26    @defectImageName varchar(50) 纸病图片名
                //string[] sql = new string[1];
                //sql[0] = string.Format(@"exec spGUI_InsterISRAPaperFault_AD {0},{1},{2},'{3}','{4}','{5}','{6}','{7}',{8},{9},{10},{11},{12},{13},{14},{15},'{16}',{17},{18},'{19}',{20},{21},{22},{23},{24},{25},'{26}'",
                //    pfId, ppId, eventId, eventNum, pfCode, bedshaftMeter, watchSpindleMeter, remark,
                //    img1, img2, img3, img4, img5, img6, img7,
                //    treatmentNode, isTreatment, creatorId, pathId, result, procedure, paper_direction, img8, img9, img10, defectId, defectImageName);
                
                SqlParameter[] sqlparams = new SqlParameter[31];

                sqlparams[0] = new SqlParameter("@pf_Id", SqlDbType.Int);
                sqlparams[0].Value = pfId;

                sqlparams[1] = new SqlParameter("@pp_Id", SqlDbType.Int);
                sqlparams[1].Value = ppId;

                sqlparams[2] = new SqlParameter("@Event_Id", SqlDbType.Int);
                sqlparams[2].Value = eventId;

                sqlparams[3] = new SqlParameter("@Event_Num", SqlDbType.Char);
                sqlparams[3].Value = eventNum;

                sqlparams[4] = new SqlParameter("@pf_Code", SqlDbType.Char);
                sqlparams[4].Value = pfCode;

                sqlparams[5] = new SqlParameter("@BedshaftMeter", SqlDbType.Float);
                sqlparams[5].Value = bedshaftMeter;

                sqlparams[6] = new SqlParameter("@WatchSpindleMeter", SqlDbType.Float);
                sqlparams[6].Value = watchSpindleMeter;

                sqlparams[7] = new SqlParameter("@Remark", SqlDbType.VarChar);
                sqlparams[7].Value = remark;

                sqlparams[8] = new SqlParameter("@Img1", SqlDbType.Int);
                sqlparams[8].Value = img1;

                sqlparams[9] = new SqlParameter("@Img2", SqlDbType.Int);
                sqlparams[9].Value = img2;

                sqlparams[10] = new SqlParameter("@Img3", SqlDbType.Int);
                sqlparams[10].Value = img3;

                sqlparams[11] = new SqlParameter("@Img4", SqlDbType.Int);
                sqlparams[11].Value = img4;

                sqlparams[12] = new SqlParameter("@Img5", SqlDbType.Int);
                sqlparams[12].Value = img5;

                sqlparams[13] = new SqlParameter("@Img6", SqlDbType.Int);
                sqlparams[13].Value = img6;

                sqlparams[14] = new SqlParameter("@Img7", SqlDbType.Int);
                sqlparams[14].Value = img7;

                sqlparams[15] = new SqlParameter("@IsPrintTreatment", SqlDbType.Int);
                sqlparams[15].Value = treatmentNode;

                sqlparams[16] = new SqlParameter("@IsTreatment", SqlDbType.Int);
                sqlparams[16].Value = isTreatment;

                sqlparams[17] = new SqlParameter("@CreatorId", SqlDbType.Int);
                sqlparams[17].Value = creatorId;

                sqlparams[18] = new SqlParameter("@Path_id", SqlDbType.Int);
                sqlparams[18].Value = pathId;

                sqlparams[19] = new SqlParameter("@result", SqlDbType.VarChar);
                sqlparams[19].Value = result;

                sqlparams[20] = new SqlParameter("@treament_procedure", SqlDbType.Int);
                sqlparams[20].Value = procedure;

                sqlparams[21] = new SqlParameter("@paper_direction", SqlDbType.Int);
                sqlparams[21].Value = paper_direction;

                sqlparams[22] = new SqlParameter("@Img8", SqlDbType.Int);
                sqlparams[22].Value = img8;

                sqlparams[23] = new SqlParameter("@Img9", SqlDbType.Int);
                sqlparams[23].Value = img9;

                sqlparams[24] = new SqlParameter("@Img10", SqlDbType.Int);
                sqlparams[24].Value = img10;
                
                sqlparams[25] = new SqlParameter("@defectId", SqlDbType.VarChar);
                sqlparams[25].Value = defectId;

                sqlparams[26] = new SqlParameter("@defectImageName", SqlDbType.Char);
                sqlparams[26].Value = defectImageName;

                sqlparams[27] = new SqlParameter("@dWidth", SqlDbType.Float);
                sqlparams[27].Value = string.IsNullOrEmpty(dWidth) ? "0" : dWidth;

                sqlparams[28] = new SqlParameter("@dLength", SqlDbType.Float);
                sqlparams[28].Value = string.IsNullOrEmpty(dLength) ? "0" : dLength;

                sqlparams[29] = new SqlParameter("@dArea", SqlDbType.Float);
                sqlparams[29].Value = string.IsNullOrEmpty(dArea) ? "0" : dArea;

                sqlparams[30] = new SqlParameter("@stopRecordId", SqlDbType.VarChar);
                sqlparams[30].Value = stopRecordId;
                
                
                ds = nmDB.RunProc("spGUI_InsterISRAPaperFault_AD", sqlparams, ds);               
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    
                }
            }
            catch (Exception ex)
            {
                
            }
            return ds;
        }

        /// <summary>
        /// 删除纸病信息
        /// </summary>
        /// <param name="pfId"></param>
        /// <returns></returns>
        public bool deleteDefectInfo( string pfId )
        {
            bool isSuccessed = false;
            try
            {
                DataSet ds = new DataSet();
                //spGUI_Doc_DeleteFault 存储过程
                string sql = string.Format("exec spGUI_Doc_DeleteFault '{0}'", pfId);
                isSuccessed = nmDB.RunProc(sql, ds) == 1 ? true : false;
            }
            catch (Exception ex)
            {

            }
            return isSuccessed;
        }

        /// <summary>
        /// 获取纸病图片信息
        /// </summary>
        /// <param name="pfId"></param>
        /// <returns></returns>
        public DataTable getDefectImageNameBypfId(string pfId)
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = string.Format(@"select ISNULL(pf1.Event_Num,'0') as Event_Num, isnull(pd.defectId,0) as defectId, isnull(pd.defectImageName,'000.bmp') as defectImageName,ISNULL(pf1.Event_Id,'0') as Event_Id,
                                isnull(pd.dWidth,0) as dWidth, isnull(pd.dLength,0) as dLength,
                                isnull(pd.dArea,0) as dArea
                                from [dbo].[UDT_PaperFault] pf
                                left join UDT_PaperFault_DefectID as pd on pd.pf_id = pf.fromPfId
                                left join [dbo].[UDT_PaperFault] as pf1 on pf1.pf_Id = pf.fromPfId
                                where pf.pf_Id = {0}", pfId);
                nmDB.RunProc(sql, ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 获取卷信息
        /// </summary>
        /// <param name="defectId"></param>
        /// <returns></returns>
        public DataTable getRollNumBydefectId(string defectId)
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = string.Format(@"select top 10 pf.pf_Id, pf.Event_Num, pf.Event_Id,
                            isnull(pd.defectId,0) as defectId, isnull(pd.defectImageName,'000.bmp') as defectImageName
                            from [dbo].[UDT_PaperFault] pf
                            left join Events as e on e.Event_Id = pf.Event_Id
                            left join UDT_PaperFault_DefectID as pd on pd.pf_id = pf.pf_Id
                            left join UDT_PageDisease_Code as pfc on pfc.PDCode = pf.pf_Code
                            left join dbo.Prod_Lines as pl on pl.PL_Id = pf.IsPrintTreatment
                            where e.pu_id in (2,4) and defectId = '{0}'
                            order by pf.pf_Id desc", defectId);
                nmDB.RunProc(sql, ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        /// <summary>
        /// 根据下卷号获取停机记录+纸病记录时间
        /// author:wmh.
        /// date:2018-11-07.
        /// </summary>
        /// <param name="event_id"></param>
        /// <returns></returns>
        public DataTable getStopRecords(string event_id, string defect_time, string pfId, bool newRecord)
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = string.Empty;
                if (string.IsNullOrEmpty(pfId) && !newRecord)
                {
                    sql = string.Format(@" select cast(0 as bit) [选择], r.id, e.Event_Num 'RollNo', CONVERT(char(11),r.Start_Time,120)+CONVERT(char(8),r.Start_Time,114) Start_Time, CONVERT(char(11),r.End_Time,120)+CONVERT(char(8),r.End_Time,114) End_Time,
                                         er.Event_Reason_Code, er.Event_Reason_Name, r.Remark, pu.PU_Desc_Local Team, pu2.PU_Desc_Local [Shift], u.User_Desc
                                         from events e
                                         join UDT_Run_Record r on e.PU_Id=r.PU_Id
                                         left join Event_Reasons er on er.event_reason_Id=r.Code
                                         left join Prod_Units pu on pu.pu_id=r.teamID
                                         left join Prod_Units pu2 on pu2.pu_id=r.shiftID
                                         left join Users u on u.user_id=r.creator
                                         where e.Event_Id={0} and r.RunStatus=0 and r.Start_Time <= '{1}' and DateAdd(minute,30,r.End_Time) >= '{1}' --停机时间包含纸病时间
                                         order by r.Start_Time desc", event_id, defect_time);
                    //where e.Event_Id={0} and r.RunStatus=0 and r.Start_Time <= '{1}' and DateAdd(minute,30,r.End_Time)r.End_Time >= '{1}' --停机时间包含纸病时间
                
                }
                else
                {
                    //(DateAdd(minute,30,r.End_Time)r.End_Time>e.Start_Time and r.End_Time<=e.TimeStamp) or   --结束时间在卷中间
                    sql = string.Format(@" select cast(0 as bit) [选择], r.id, e.Event_Num 'RollNo', CONVERT(char(11),r.Start_Time,120)+CONVERT(char(8),r.Start_Time,114) Start_Time, CONVERT(char(11),r.End_Time,120)+CONVERT(char(8),r.End_Time,114) End_Time,
                                         er.Event_Reason_Code, er.Event_Reason_Name, r.Remark, pu.PU_Desc_Local Team, pu2.PU_Desc_Local [Shift], u.User_Desc
                                         from events e
                                         join UDT_Run_Record r on 
                                         ((r.Start_Time>=e.Start_Time and r.End_Time<=e.TimeStamp) or --开始和结束时间在卷之内
										 (r.Start_Time>=e.Start_Time and r.Start_Time<e.TimeStamp) or --开始时间在卷中间
										 (DateAdd(minute,30,r.End_Time)>e.Start_Time and r.End_Time<=e.TimeStamp) or   --结束时间在卷中间
										 (r.Start_Time<e.Start_Time and r.End_Time>e.TimeStamp))  --开始时间和结束时间在卷之外
										 and e.PU_Id=r.PU_Id
                                         left join Event_Reasons er on er.event_reason_Id=r.Code
                                         left join Prod_Units pu on pu.pu_id=r.teamID
                                         left join Prod_Units pu2 on pu2.pu_id=r.shiftID
                                         left join Users u on u.user_id=r.creator
                                         where e.Event_Id={0} and r.RunStatus=0 and r.id not in (select Stop_record_ID from dbo.UDT_RunRecord_Defect_Relation where Event_Id={0})
                                         order by r.Start_Time desc", event_id, pfId);//and r.id not in (select Stop_record_ID from [UDT_RunRecord_Defect_Relation] where Defect_ID<>'{1}' )
                }

                
                nmDB.RunProc(sql, ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 根据eventnum获取eventid
        /// </summary>
        /// <param name="puId"></param>
        /// <param name="eventnum"></param>
        /// <returns></returns>
        public string geteventId(string puId, string eventnum)
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = string.Format("select top 1 * from events where Event_Num = '{0}' and PU_Id = {1} ",eventnum, puId);
                
                nmDB.RunProc(sql, ds);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return null;
                }
                return ds.Tables[0].Rows[0][0].ToString();
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}