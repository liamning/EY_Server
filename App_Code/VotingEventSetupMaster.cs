using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;

public class VotingEventSetupMaster
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public VotingEventSetupMaster()
    {
    }

    #region Get
    public List<string> GetEventIDList(string clientID, string eventID)
    {
        db.Open();
        String query = "select top 10 EventID from VotingEventSetup where ClientID = @ClientID and (@EventID = '' or EventID like '%' + @EventID + '%') order by EventID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, EventID = eventID });
        db.Close();
        return obj;
    }
    public List<string> GetEventIDList(string clientID)
    {
        db.Open();
        String query = "select top 10 EventID from VotingEventSetup where ClientID = @ClientID  order by EventID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID });
        db.Close();
        return obj;
    }

    public int? GetLastEventID(string clientID)
    {
        db.Open();
        String query = "SELECT MAX(EventID) FROM [dbo].[VotingEventSetup] WHERE ClientID = @ClientID ";
        var obj = (List<int?>)db.Query<int?>(query, new { ClientID = clientID });
        if (obj.Count > 0 && obj[0] != null)
        {
            return obj[0];
        }
        else
            return 0;
    }

    public VotingEventSetupInfo Get(string clientID, int eventID, string creditType = "1")
    {
        db.Open();
        String query = "SELECT * FROM VotingEventSetup WHERE ClientID = @ClientID AND EventID = @EventID ";
        var obj = (List<VotingEventSetupInfo>)db.Query<VotingEventSetupInfo>(query, new { ClientID = clientID, EventID = eventID }, transaction);
        if (obj.Count > 0)
        {
            obj[0].VotingQuestionList = this.GetVotingQuestionList(clientID, eventID);
            obj[0].VotingReplyList = this.GetVotingReplyList(clientID, eventID);
            obj[0].VotingResultList = this.GetVotingResultList(clientID, eventID);
            obj[0].VotingResultDetailList = this.GetVotingResultDetailList(clientID, eventID, creditType);
            obj[0].AttendanceSummary = this.GetAttendanceSummary(clientID, eventID);
            obj[0].AttendanceList = this.GetAttendanceList(clientID, eventID);
            return obj[0];
        }
        else
            return null;
    }
     

    public List<AttendanceSummaryInfo> GetAttendanceSummary(string clientID, int eventID)
    {
        string query = @"
select 
(select ChiDesc from GeneralMaster where Category='VoteMethod' and Code='2')
VoteMethodDesc, count(*) ReplyHeadcount, 
isnull(sum(AdminExamineConfirm), 0) ReplyTotalAmount, isnull(sum(ActualAmount), 0) ActualTotalAmount, 
isnull(sum(ActualHeadcount), 0) ActualHeadcount
from
(
select v.ClientID, v.EventID, v.CreditorID, c.AdminExamineConfirm,  
case when a.CreditorID is null
then 0
else c.AdminExamineConfirm
end ActualAmount,
case when a.CreditorID is null
then 0
else 1
end ActualHeadCount
from VotingReplyEntry v

join 
(select sum(AdminExamineConfirm*Rate) AdminExamineConfirm, [CreditorAuditDetail].ClientID, CreditorID 
from [CreditorAuditDetail] 
join CurrencyList on CreditorAuditDetail.ClientID = CurrencyList.ClientID and CurrencyCode = CreditorAuditDetail.Currency
group by [CreditorAuditDetail].ClientID, CreditorID) 
c on v.ClientID = c.ClientID and v.CreditorID = c.CreditorID 

left outer join [Attendance] a on a.ClientID = v.ClientID and a.EventID = v.EventID and a.CreditorID = v.CreditorID 
where v.ClientID = @ClientID and v.EventID = @EventID and v.VoteMethod = '2'  and Attendance = '1'
) tmp
union all
select 
(select ChiDesc from GeneralMaster where Category='VoteMethod' and Code='1')
VoteMethodDesc, count(*) ReplyHeadcount, 
isnull(sum(AdminExamineConfirm), 0) ReplyTotalAmount, isnull(sum(ActualAmount), 0) ActualTotalAmount, 
isnull(sum(ActualHeadcount), 0) ActualHeadcount
from
(
select v.ClientID, v.EventID, v.CreditorID, c.AdminExamineConfirm,  
case when a.CreditorID is null
then 0
else c.AdminExamineConfirm
end ActualAmount,
case when a.CreditorID is null
then 0
else 1
end ActualHeadCount
from VotingReplyEntry v

join 
(select sum(AdminExamineConfirm*Rate) AdminExamineConfirm, [CreditorAuditDetail].ClientID, CreditorID 
from [CreditorAuditDetail] 
join CurrencyList on CreditorAuditDetail.ClientID = CurrencyList.ClientID and CurrencyCode = CreditorAuditDetail.Currency
group by [CreditorAuditDetail].ClientID, CreditorID) 
c on v.ClientID = c.ClientID and v.CreditorID = c.CreditorID 

left outer join [OnlineVotePage] a on a.ClientID = v.ClientID and a.EventID = v.EventID and a.CreditorID = v.CreditorID and a.QuestionID = 1
where v.ClientID = @ClientID and v.EventID = @EventID and v.VoteMethod = '1'  and Attendance = '1'
) tmp

";
        List<AttendanceSummaryInfo> obj = (List<AttendanceSummaryInfo>)db.Query<AttendanceSummaryInfo>(query, new { ClientID = clientID, EventID = eventID });


        return obj;
    }


    public List<AttendanceInfo> GetAttendanceList(string clientID, int eventID)
    {
        string query = @"
select Attendance.*, CreditorMaster.CreditorName
from Attendance
join CreditorMaster on Attendance.CreditorID = CreditorMaster.CreditorID and Attendance.ClientID = CreditorMaster.ClientID
where Attendance.ClientID = @ClientID and Attendance.EventID = @EventID 
";
        List<AttendanceInfo> obj = (List<AttendanceInfo>)db.Query<AttendanceInfo>(query, new { ClientID = clientID, EventID = eventID });


        return obj;
    }


    public List<VotingQuestionInfo> GetVotingQuestionList(string clientID, int eventID)
    {
        String query = "SELECT * FROM VotingQuestion WHERE ClientID = @ClientID AND EventID = @EventID ";
        List<VotingQuestionInfo> obj = (List<VotingQuestionInfo>)db.Query<VotingQuestionInfo>(query, new { ClientID = clientID, EventID = eventID });
        return obj;
    }

    public List<VotingResultDetailInfo> GetVotingResultDetailList(string clientID, int eventID, string creditType)
    {
        String query = @"
                   select ChiDesc decisionDesc, * from 
 (SELECT [dbo].[LiveVotePage].CreditorID,  
                    [dbo].[CreditorMaster].CreditorName,  
                    [dbo].[LiveVotePage].QuestionID , 
                    [dbo].[LiveVotePage].Decision  
                    FROM [dbo].[LiveVotePage]  
                    JOIN [dbo].[CreditorMaster] ON [dbo].[LiveVotePage].ClientID = [dbo].[CreditorMaster].ClientID AND [dbo].[LiveVotePage].CreditorID = [dbo].[CreditorMaster].CreditorID 
                    WHERE [dbo].[LiveVotePage].ClientID = @ClientID AND [dbo].[LiveVotePage].EventID = @EventID AND [dbo].[LiveVotePage].CreditType = @CreditType

                    union all

                    SELECT [dbo].[OnlineVotePage].CreditorID,  
                    [dbo].[CreditorMaster].CreditorName,  
                    [dbo].[OnlineVotePage].QuestionID , 
                    [dbo].[OnlineVotePage].Decision  
                    FROM [dbo].[OnlineVotePage]  
                    JOIN [dbo].[CreditorMaster] ON [dbo].[OnlineVotePage].ClientID = [dbo].[CreditorMaster].ClientID AND [dbo].[OnlineVotePage].CreditorID = [dbo].[CreditorMaster].CreditorID 
                    WHERE [dbo].[OnlineVotePage].ClientID = @ClientID AND [dbo].[OnlineVotePage].EventID = @EventID AND [dbo].[OnlineVotePage].CreditType = @CreditType
) voteDetails
join GeneralMaster on Category='decisionGroup2' and Code = Decision
ORDER BY CreditorID
";
        List<VotingResultDetailInfo> obj = (List<VotingResultDetailInfo>)db.Query<VotingResultDetailInfo>(query, new { ClientID = clientID, EventID = eventID, CreditType = creditType });
        return obj;
    }

    public List<VotingResultInfo> GetVotingResultList(string clientID, int eventID)
    {
//        String query = @"
//                    SELECT  
//                    VotingQuestion.QuestionID,  
//                    VotingQuestion.Question,  
//                    (SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '1' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) AgressNo,  
//                    CAST(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '1' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)) , 0)as decimal(18,4))AgressPct,  
//                    ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) AgreeAmt,  
//                    ISNULL(CAST(ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) AgreeAmtPct,  
//                    (SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '2' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) DisagressNo,  
//                    CAST(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '2' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)),0) as decimal(18,4))DisagressPct,  
//                    ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) DisagreeAmt,  
//                    ISNULL(CAST(ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) DisagreeAmtPct,  
//                    (SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '3' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) AbstentionNo,  
//                    CAST(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.Decision = '3' AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.QuestionID = VotingQuestion.QuestionID AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID) as decimal(18,2)),0) as decimal(18,4))AbstentionPct,  
//                    ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) AbstentionAmt,  
//                    ISNULL(CAST(ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) AbstentionAmtPct,  
//                    (SELECT COUNT(*) FROM OnlineVotePage WHERE OnlineVotePage.QuestionID = VotingQuestion.QuestionID)TotalNo,  
//                    ISNULL(AgressAmtView.AdminExamineConfirmSum, 0)  + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) TotalAmt  
//                    FROM VotingQuestion  
//                    LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, OnlineVotePage.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, OnlineVotePage.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN OnlineVotePage ON OnlineVotePage.Decision = '1' AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = OnlineVotePage.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) a) AgressAmtView on AgressAmtView.QuestionID  = VotingQuestion.QuestionID  
//                    LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, OnlineVotePage.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, OnlineVotePage.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN OnlineVotePage ON OnlineVotePage.Decision = '2' AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = OnlineVotePage.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) b) DisagressAmtView on DisagressAmtView.QuestionID  = VotingQuestion.QuestionID  
//                    LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, OnlineVotePage.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, OnlineVotePage.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN OnlineVotePage ON OnlineVotePage.Decision = '3' AND OnlineVotePage.ClientID = VotingQuestion.ClientID AND OnlineVotePage.EventID = VotingQuestion.EventID AND OnlineVotePage.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = OnlineVotePage.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) c) AbstentionAmtView on AbstentionAmtView.QuestionID  = VotingQuestion.QuestionID  
//                    WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID  
//                    ORDER BY VotingQuestion.QuestionID 
//";
        string query = @"

select 
result.QuestionID, result.Question,
sum(AgreeTotal) AgressNo, sum(DisagreeTotal) DisagressNo, sum(AbandonTotal) AbstentionNo, sum(Total) TotalNo, 
sum(AgreeAmount) AgreeAmt, sum(DisagreeAmount) DisagreeAmt, sum(AbandonAmount) AbstentionAmt, sum(TotalAmount) TotalAmt from 
(select 
VotePage.QuestionID, Question,
case when Decision = '1'
then 1
else 0 end AgreeTotal,
case when Decision = '2'
then 1
else 0 end DisagreeTotal,
case when Decision = '3'
then 1
else 0 end AbandonTotal,
1 Total,
case when Decision = '1'
then Amount
else 0 end AgreeAmount,
case when Decision = '2'
then Amount
else 0 end DisagreeAmount,
case when Decision = '3'
then Amount
else 0 end AbandonAmount,
Amount TotalAmount
from 
(
select * from LiveVotePage 
where LiveVotePage.ClientID = @ClientID and LiveVotePage.EventID = @EventID
union all  
select * from OnlineVotePage
where OnlineVotePage.ClientID = @ClientID and OnlineVotePage.EventID = @EventID
) VotePage 


join 
(select CreditorAuditDetail.ClientID, CreditorID, CreditorType, sum(AdminExamineConfirm*Rate) Amount 
FROM [EYVoting].[dbo].[CreditorAuditDetail] 
join CurrencyList on CreditorAuditDetail.ClientID = CurrencyList.ClientID and CurrencyCode = CreditorAuditDetail.Currency
group by CreditorAuditDetail.ClientID, CreditorID, CreditorType)

as ConfirmAmount on VotePage.ClientID = ConfirmAmount.ClientID and  VotePage.CreditorID = ConfirmAmount.CreditorID and VotePage.CreditType = ConfirmAmount.CreditorType

join VotingQuestion on @ClientID = VotingQuestion.ClientID and @EventID = VotingQuestion.EventID and VotePage.QuestionID = VotingQuestion.QuestionID

) result

group by QuestionID, Question
order by QuestionID

";
        List<VotingResultInfo> obj = (List<VotingResultInfo>)db.Query<VotingResultInfo>(query, new { ClientID = clientID, EventID = eventID });
        return obj;
    }

    public List<VotingReplyListInfo> GetVotingReplyList(string clientID, int eventID)
    {
        String query = @"
select 
GM_VoteMethod.ChiDesc VoteMethodDesc, 
GM_YN.ChiDesc AttendanceDesc, 
GM_CreditorType.ChiDesc CreditorTypeDesc, 
VotingReply.* from 
(SELECT 
VotingReplyEntry.CreditorID  
,CreditorMaster.CreditorName  
,CreditorMaster.CreditorType  
,CreditorMaster.ResponsiblePerson  
,SUM(ISNULL(CreditorAuditDetail.BookAmt, 0))BookAmt  
,SUM(ISNULL(CreditorAuditDetail.DeclareAmt, 0))DeclareAmt  
,SUM(ISNULL(CreditorAuditDetail.AdminExamineNotConfirm, 0))AdminExamineNotConfirm  
,SUM(ISNULL(CreditorAuditDetail.AdminExamineWaitConfirm, 0))AdminExamineWaitConfirm  
,SUM(ISNULL(CreditorAuditDetail.AdminExamineConfirm, 0))AdminExamineConfirm  
,VotingReplyEntry.Attendance  
,VotingReplyEntry.VoteMethod  
,VotingReplyEntry.ReplyDate  
FROM VotingReplyEntry  
JOIN CreditorMaster ON VotingReplyEntry.ClientID = CreditorMaster.ClientID  
AND VotingReplyEntry.CreditorID = CreditorMaster.CreditorID  

JOIN 
(
select 
CreditorAuditDetail.ClientID, CreditorID, CreditorType, CurrencyCode,Rate
,BookAmt*Rate BookAmt  
,DeclareAmt*Rate DeclareAmt  
,AdminExamineNotConfirm*Rate AdminExamineNotConfirm  
,AdminExamineWaitConfirm*Rate AdminExamineWaitConfirm  
,AdminExamineConfirm*Rate AdminExamineConfirm 
 from CreditorAuditDetail 
 join CurrencyList on CreditorAuditDetail.ClientID = CurrencyList.ClientID and CurrencyCode = CreditorAuditDetail.Currency
) 
CreditorAuditDetail ON CreditorAuditDetail.ClientID = VotingReplyEntry.ClientID  

AND CreditorAuditDetail.CreditorID = VotingReplyEntry.CreditorID  
WHERE VotingReplyEntry.ClientID = @ClientID AND VotingReplyEntry.EventID = @EventID  
GROUP BY VotingReplyEntry.ClientID  
,VotingReplyEntry.CreditorID  
,CreditorMaster.CreditorName  
,CreditorMaster.CreditorType  
,CreditorMaster.ResponsiblePerson  
,VotingReplyEntry.Attendance  
,VotingReplyEntry.VoteMethod  
,VotingReplyEntry.ReplyDate 
) VotingReply
left outer JOIN GeneralMaster GM_VoteMethod ON GM_VoteMethod.Category = 'VoteMethod' and GM_VoteMethod.Code = VotingReply.VoteMethod 
left outer JOIN GeneralMaster GM_YN ON GM_YN.Category = 'YesNo' and GM_YN.Code = VotingReply.Attendance    
left outer JOIN GeneralMaster GM_CreditorType ON GM_CreditorType.Category = 'CreditorType' and GM_CreditorType.Code = VotingReply.CreditorType    

";
        List<VotingReplyListInfo> obj = (List<VotingReplyListInfo>)db.Query<VotingReplyListInfo>(query, new { ClientID = clientID, EventID = eventID });
        return obj;
    }
    #endregion

    #region Save
    public void Save(VotingEventSetupInfo votingEventSetupInfo)
    {
        if (this.IsExisted(votingEventSetupInfo.ClientID, votingEventSetupInfo.EventID))
            this.Update(votingEventSetupInfo);
        else
            this.Insert(votingEventSetupInfo);
    }

    #endregion

    #region Insert
    public void Insert(VotingEventSetupInfo votingEventSetupInfo)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            String query = "INSERT INTO VotingEventSetup "
                         + "([ClientID] "
                         + ",[EventID] "
                         + ",[EventDescription] "
                         + ",[EventDate] "
                         + ",[EffectDateFrom] "
                         + ",[EffectDateTo] "
                         + ",[CreateUser] "
                         + ",[CreateDate] "
                         + ",[LastModifiedUser] "
                         + ",[LastModifiedDate] ) "
                         + "VALUES "
                         + "(@ClientID "
                         + ",@EventID "
                         + ",@EventDescription "
                         + ",@EventDate "
                         + ",@EffectDateFrom "
                         + ",@EffectDateTo "
                         + ",@CreateUser "
                         + ",@CreateDate "
                         + ",@LastModifiedUser "
                         + ",@LastModifiedDate ) ";
            db.Execute(query, votingEventSetupInfo, transaction);

            if (votingEventSetupInfo.VotingQuestionList != null)
            {
                for (int i = 0; i < votingEventSetupInfo.VotingQuestionList.Count; i++)
                {
                    VotingQuestionInfo votingQuestionInfo = votingEventSetupInfo.VotingQuestionList[i];
                    votingQuestionInfo.CreateUser = votingEventSetupInfo.LastModifiedUser;
                    votingQuestionInfo.CreateDate = DateTime.Now;
                    votingQuestionInfo.LastModifiedUser = votingEventSetupInfo.LastModifiedUser;
                    votingQuestionInfo.LastModifiedDate = DateTime.Now;
                    votingQuestionInfo.ClientID = votingEventSetupInfo.ClientID;
                    votingQuestionInfo.EventID = votingEventSetupInfo.EventID;
                    votingQuestionInfo.QuestionID = i + 1;
                    this.InsertVotingQuestion(votingQuestionInfo);
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            transaction.Dispose();
            transaction = null;
            db.Close();
        }
    }

    public void InsertVotingQuestion(VotingQuestionInfo votingQuestionInfo)
    {
        String query = "INSERT INTO [dbo].[VotingQuestion] "
                     + "([ClientID] "
                     + ",[EventID] "
                     + ",[QuestionID] "
                     + ",[Question] "
                     + ",[AnswerType] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@EventID "
                     + ",@QuestionID "
                     + ",@Question "
                     + ",@AnswerType "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate) ";
        db.Execute(query, votingQuestionInfo, transaction);
    }
    #endregion

    #region Update
    public void Update(VotingEventSetupInfo votingEventSetupInfo)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            String query = "UPDATE [dbo].[VotingEventSetup] "
                         + "SET "
                         + "[EventDescription] = @EventDescription "
                         + ",[EventDate] = @EventDate "
                         + ",[EffectDateFrom] = @EffectDateFrom "
                         + ",[EffectDateTo] = @EffectDateTo "
                         + ",[LastModifiedUser] = @LastModifiedUser "
                         + ",[LastModifiedDate] = @LastModifiedDate "
                         + "WHERE ClientID = @ClientID AND EventID = @EventID ";
            db.Execute(query, votingEventSetupInfo, transaction);

            this.DeleteVotingQuestion(votingEventSetupInfo.ClientID, votingEventSetupInfo.EventID);

            if (votingEventSetupInfo.VotingQuestionList != null)
            {
                for (int i = 0; i < votingEventSetupInfo.VotingQuestionList.Count; i++)
                {
                    VotingQuestionInfo votingQuestionInfo = votingEventSetupInfo.VotingQuestionList[i];
                    votingQuestionInfo.CreateUser = votingEventSetupInfo.LastModifiedUser;
                    votingQuestionInfo.CreateDate = DateTime.Now;
                    votingQuestionInfo.LastModifiedUser = votingEventSetupInfo.LastModifiedUser;
                    votingQuestionInfo.LastModifiedDate = DateTime.Now;
                    votingQuestionInfo.ClientID = votingEventSetupInfo.ClientID;
                    votingQuestionInfo.EventID = votingEventSetupInfo.EventID;
                    votingQuestionInfo.QuestionID = i + 1;
                    this.InsertVotingQuestion(votingQuestionInfo);
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally {
            transaction.Dispose();
            transaction = null;
            db.Close();
        }
    }

    #endregion

    #region Delete
    public void DeleteVotingQuestion(string clientID, int eventID)
    {
        String query = "DELETE FROM VotingQuestion  WHERE ClientID = @ClientID AND EventID = @EventID ";
        db.Execute(query, new { ClientID = clientID, EventID = eventID }, transaction);
    }
    #endregion

    #region Check exists
    public bool IsExisted(string clientID, int eventID)
    {
        db.Open();
        String query = "SELECT Count(*) FROM [dbo].[VotingEventSetup] WHERE ClientID = @ClientID AND EventID = @EventID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, EventID = eventID }, transaction);
        db.Close();
        return obj[0] > 0;
    }

    public bool IsVotingQuestionExisted(string clientID, int eventID, int questionID)
    {
        String query = "SELECT Count(*) FROM [dbo].[VotingQuestion] WHERE ClientID = @ClientID AND EventID = @EventID AND QuestionID = @QuestionID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, EventID = eventID, QuestionID = questionID }, transaction);
        return obj[0] > 0;
    }
    #endregion
}