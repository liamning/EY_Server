using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;


public class VotingProjectionResult
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    public VotingProjectionResult()
    {
    }

    #region Get

    public List<VotingProjectionResultInfo> Get(string clientID, int eventID)
    {
        //db.Open();
        //String query = "SELECT "
        //             + "VotingQuestion.QuestionID, "
        //             + "VotingQuestion.Question, "
        //             + "(SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '1' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) AgressNo, "
        //             + "CAST(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '1' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)) , 0)as decimal(18,4))AgressPct, "
        //             + "ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) AgreeAmt, "
        //             + "ISNULL(CAST(ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) AgreeAmtPct, "
        //             + "(SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '2' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) DisagressNo, "
        //             + "CAST(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '2' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)),0) as decimal(18,4))DisagressPct, "
        //             + "ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) DisagreeAmt, "
        //             + "ISNULL(CAST(ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) DisagreeAmtPct, "
        //             + "(SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '3' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) AbstentionNo, "
        //             + "CAST(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.Decision = '3' AND VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)) / nullif(CAST((SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.QuestionID = VotingQuestion.QuestionID AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID) as decimal(18,2)),0) as decimal(18,4))AbstentionPct, "
        //             + "ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) AbstentionAmt, "
        //             + "ISNULL(CAST(ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) / NULLIF((ISNULL(AgressAmtView.AdminExamineConfirmSum, 0) + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0)), 0) as decimal(18,4)), 0) AbstentionAmtPct, "
        //             + "(SELECT COUNT(*) FROM VotingProjection WHERE VotingProjection.QuestionID = VotingQuestion.QuestionID)TotalNo, "
        //             + "ISNULL(AgressAmtView.AdminExamineConfirmSum, 0)  + ISNULL(DisagressAmtView.AdminExamineConfirmSum, 0) + ISNULL(AbstentionAmtView.AdminExamineConfirmSum, 0) TotalAmt "
        //             + "FROM VotingQuestion "
        //             + "LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, VotingProjection.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, VotingProjection.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN VotingProjection ON VotingProjection.Decision = '1' AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID AND VotingProjection.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = VotingProjection.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) a) AgressAmtView on AgressAmtView.QuestionID  = VotingQuestion.QuestionID "
        //             + "LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, VotingProjection.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, VotingProjection.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN VotingProjection ON VotingProjection.Decision = '2' AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID AND VotingProjection.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = VotingProjection.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) b) DisagressAmtView on DisagressAmtView.QuestionID  = VotingQuestion.QuestionID "
        //             + "LEFT JOIN(SELECT DISTINCT ClientID, EventID, QuestionID, Question, Decision, AdminExamineConfirmSum FROM(SELECT VotingQuestion.ClientID, VotingQuestion.EventID, VotingProjection.CreditorID, VotingQuestion.QuestionID, VotingQuestion.Question, VotingProjection.Decision, SUM(AdminExamineConfirmSum) over (partition by VotingQuestion.QuestionID) AdminExamineConfirmSum FROM VotingQuestion JOIN VotingProjection ON VotingProjection.Decision = '3' AND VotingProjection.ClientID = VotingQuestion.ClientID AND VotingProjection.EventID = VotingQuestion.EventID AND VotingProjection.QuestionID = VotingQuestion.QuestionID JOIN(SELECT CreditorAuditDetail.ClientID, CreditorAuditDetail.CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail group by ClientID, CreditorID) ExamineConfirm on ExamineConfirm.ClientID = VotingQuestion.ClientID AND ExamineConfirm.CreditorID = VotingProjection.CreditorID WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID) c) AbstentionAmtView on AbstentionAmtView.QuestionID  = VotingQuestion.QuestionID "
        //             + "WHERE VotingQuestion.ClientID = @ClientID AND VotingQuestion.EventID = @EventID "
        //             + "ORDER BY VotingQuestion.QuestionID ";

        //var obj = (List<VotingProjectionResultInfo>)db.Query<VotingProjectionResultInfo>(query, new { ClientID = clientID, EventID = eventID });
        //foreach (VotingProjectionResultInfo info in obj)
        //{
        //    info.AgressPct = Math.Round((decimal)info.AgressPct, 3, MidpointRounding.AwayFromZero);
        //    info.AgreeAmtPct = Math.Round((decimal)info.AgreeAmtPct, 3, MidpointRounding.AwayFromZero);
        //    info.DisagressPct = Math.Round((decimal)info.DisagressPct, 3, MidpointRounding.AwayFromZero);
        //    info.DisagreeAmtPct = Math.Round((decimal)info.DisagreeAmtPct, 3, MidpointRounding.AwayFromZero);
        //    info.AbstentionPct = Math.Round((decimal)info.AbstentionPct, 3, MidpointRounding.AwayFromZero);
        //    info.AbstentionAmtPct = Math.Round((decimal)info.AbstentionAmtPct, 3, MidpointRounding.AwayFromZero);
        //}
        //db.Close();
        //return obj;

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
select * from VotingProjection 
where VotingProjection.ClientID = @ClientID and VotingProjection.EventID = @EventID 
) VotePage 
 
join (select ClientID, CreditorID, CreditorType, sum(AdminExamineConfirm) Amount FROM [EYVoting].[dbo].[CreditorAuditDetail] group by ClientID, CreditorID, CreditorType) 
as ConfirmAmount on VotePage.ClientID = ConfirmAmount.ClientID and  VotePage.CreditorID = ConfirmAmount.CreditorID and VotePage.CreditType = ConfirmAmount.CreditorType

join VotingQuestion on @ClientID = VotingQuestion.ClientID and @EventID = VotingQuestion.EventID and VotePage.QuestionID = VotingQuestion.QuestionID

) result

group by QuestionID, Question
order by QuestionID
";
        List<VotingProjectionResultInfo> obj = (List<VotingProjectionResultInfo>)db.Query<VotingProjectionResultInfo>(query, new { ClientID = clientID, EventID = eventID });

        return obj;


    }
    #endregion
}