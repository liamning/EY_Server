using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;

public class LiveVotePage
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public LiveVotePage()
    {
    }

    public void SubmitVote(string clientID, string creditorID, int eventID, string creditType, List<OnlineVotePageInfo> list)
    {
        if(!this.isVoted(clientID, creditorID, eventID, creditType))
        {
            this.Insert(clientID, creditorID, eventID, creditType, list);
        }
    }

    private bool isVoted(string clientID, string creditorID, int eventID, string CreditType)
    {
        db.Open(); 
        try
        {
            string query = @"select count(*) total from LiveVotePage where ClientID = @ClientID and CreditorID = @CreditorID and EventID = @EventID and CreditType = @CreditType";
            List<int> total = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType = CreditType });
            return total[0] > 0;
        }
        catch
        { 
            throw;
        }
        finally
        { 
            db.Close();
        } 
    }

    #region Insert
    public void Insert(string clientID, string creditorID, int eventID, string creditType, List<OnlineVotePageInfo> list)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            foreach (OnlineVotePageInfo onlineVotePageInfo in list)
            {
                onlineVotePageInfo.ClientID = clientID;
                onlineVotePageInfo.CreditorID = creditorID;
                onlineVotePageInfo.EventID = eventID;
                onlineVotePageInfo.CreditType = creditType;
                this.InsertLiveVotePage(onlineVotePageInfo);
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

    public void InsertLiveVotePage(OnlineVotePageInfo onlineVotePageInfo)
    {
        String query = "INSERT INTO [dbo].[LiveVotePage] "
                     + "([ClientID] "
                     + ",[CreditorID] "
                     + ",[EventID] "
                     + ",[CreditType] "
                     + ",[QuestionID] "
                     + ",[Decision] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@CreditorID "
                     + ",@EventID "
                     + ",@CreditType "
                     + ",@QuestionID "
                     + ",@Decision "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, onlineVotePageInfo, transaction);
    }
    #endregion

    #region Get
    public string GetCreditorName(string clientID, string creditorID)
    {
        db.Open();
        String query = "SELECT CreditorName FROM [dbo].[CreditorMaster] WHERE ClientID = @ClientID AND CreditorID = @CreditorID ";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, CreditorID = creditorID });
        db.Close();
        if (obj.Count > 0 && obj[0] != null)
            return obj[0];
        else
            return null;
    }
    public List<OnlineVotePageInfo> GetCreditorVotingDetails(string clientID, string creditorID, string eventID, string creditType)
    {
        db.Open();
        string query = @"
                    SELECT [dbo].[LiveVotePage].Decision  
                    ,[dbo].[VotingQuestion].Question  
                    ,[dbo].[VotingQuestion].AnswerType  
                    FROM [dbo].[LiveVotePage]  
                    JOIN [dbo].[VotingQuestion]  
                    ON [dbo].[LiveVotePage].ClientID = [dbo].[VotingQuestion].ClientID  
                    AND [dbo].[LiveVotePage].EventID = [dbo].[VotingQuestion].EventID  
                    AND [dbo].[LiveVotePage].QuestionID = [dbo].[VotingQuestion].QuestionID  
                    WHERE [dbo].[LiveVotePage].ClientID = @ClientID AND [dbo].[LiveVotePage].CreditorID = @CreditorID AND [dbo].[LiveVotePage].EventID = @EventID and CreditType = @CreditType
";
        List<OnlineVotePageInfo> obj = (List<OnlineVotePageInfo>)db.Query<OnlineVotePageInfo>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType=creditType });
        return obj;
    }

    public List<OnlineVotePageInfo> Get(string clientID, int eventID)
    {
        db.Open();
        string query = "";
        query = "SELECT QuestionID, Question, AnswerType FROM VotingQuestion WHERE ClientID = @ClientID AND EventID = @EventID";
        List<OnlineVotePageInfo> obj = (List<OnlineVotePageInfo>)db.Query<OnlineVotePageInfo>(query, new { ClientID = clientID, EventID = eventID });
        return obj;
    }

    public List<CreditorMasterInfo> SearchUnVoteCreditor(string clientID, int eventID)
    {
        db.Open();
        string query = "";

        query = @"


select distinct
CreditorMaster.CreditorID,
CreditorName,
CreditorAuditDetail.CreditorType,
GeneralMaster.ChiDesc CreditTypeDesc, 
CreditorAuditDetail.AdminExamineConfirm
from 
CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID 
and @EventID = VotingReplyEntry.EventID
join Attendance on Attendance.ClientID =  VotingReplyEntry.ClientID and Attendance.EventID =  VotingReplyEntry.EventID and Attendance.CreditorID =  VotingReplyEntry.CreditorID
join GeneralMaster on Category = 'CreditType' and CreditorAuditDetail.CreditorType = Code

where AdminExamineConfirm > 0 and VotingReplyEntry.VoteMethod = '2' AND CreditorMaster.ClientID = @ClientID
and VotingReplyEntry.Attendance = '1'
and not exists (select 1 from LiveVotePage where ClientID = @ClientID and CreditorMaster.CreditorID = LiveVotePage.CreditorID)
 

";

        List<CreditorMasterInfo> obj = (List<CreditorMasterInfo>)db.Query<CreditorMasterInfo>(query, new { ClientID = clientID, EventID = eventID });

        return obj;
    }

    #endregion

    #region Checking
    public bool IsExisted(string clientID, string creditorID, int eventID)
    {
        String query = "SELECT COUNT(*) FROM LiveVotePage WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND EventID = @EventID ";
        var obj = (List<int?>)db.Query<int?>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID });
        return obj[0] > 0;
    }

    public bool IsOutOfTimeRange(string clientID, int eventID)
    {
        String query = "SELECT EffectDateTo FROM VotingEventSetup WHERE ClientID = @ClientID AND EventID = @EventID ";
        var obj = (List<DateTime>)db.Query<DateTime>(query, new { ClientID = clientID, EventID = eventID });
        return DateTime.Now > obj[0];
    }
    #endregion


}