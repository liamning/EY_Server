using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;
using System.Linq;

public class OnlineVotePage
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public OnlineVotePage()
    {
    }



    public void SubmitVote(string clientID, string creditorID, int eventID, string creditType, List<OnlineVotePageInfo> list)
    {
        if (!this.isVoted(clientID, creditorID, eventID, creditType))
        {
            this.Insert(clientID, creditorID, eventID, creditType, list);
        }
    }

    private bool isVoted(string clientID, string creditorID, int eventID, string creditType)
    {
        db.Open();
        try
        {
            string query = @"select count(*) total from OnlineVotePage where ClientID = @ClientID and CreditorID = @CreditorID and EventID = @EventID and CreditType = @CreditType";
            List<int> total = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType = creditType, });
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

    private static Random random = new Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string[] GetOnlineVotingKeyFields(int ID, string token)
    {
        List<string> list = new List<string>();
        db.Open();
        try
        {
            string query = @"
select ClientID, CreditorID, EventID, CreditType from [OnlineVotingToken]
where Token=@Token and ID=@ID
";

            var result = db.Query(query, new { Token = token, ID = ID }).FirstOrDefault();
            if (result != null)
            {
                list.Add(result.ClientID);
                list.Add(result.CreditorID);
                list.Add(result.EventID.ToString());
                list.Add(result.CreditType); 
                return list.ToArray();
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            db.Close();
        }

        return null;
    }
    public string GenerateOnlineVotingToken(string clientID, string creditorID, int eventID, string creditType)
    {
        try
        { 
            var votingInfo = this.GetCreditorVotingInfo(clientID, creditorID);
            List<string> result = new List<string>();

            db.Open();
            string query = @"
                        delete from [dbo].[OnlineVotingToken]
where ClientID = @ClientID and CreditorID = @CreditorID and EventID = @EventID and CreditType = @CreditType
";
            db.Execute(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType = creditType });

            query = @"
                        INSERT INTO [dbo].[OnlineVotingToken]
                        ([ClientID]
                        ,[EventID]
                        ,[CreditorID]
                        ,[CreditType]
                        ,[Token])
                        VALUES
                        (@ClientID
                        ,@EventID
                        ,@CreditorID
                        ,@CreditType
                        ,@Token
                        );select SCOPE_IDENTITY();

";

            foreach (var info in votingInfo)
            {
                if (info[1] != creditType) continue;

                string randomStr = RandomString(20);
                List<int> total = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, CreditType = info[1], EventID = eventID, Token = randomStr });

                return string.Format("{0}&{1}", total[0], randomStr);
            }


            return "";
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
                this.InsertOnlineVotePage(onlineVotePageInfo);
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

    public void InsertOnlineVotePage(OnlineVotePageInfo onlineVotePageInfo)
    {
        String query = "INSERT INTO [dbo].[OnlineVotePage] "
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


    public bool IsAttended(string clientID, string creditorID, int eventID)
    {
        String query = "SELECT COUNT(*) FROM Attendance WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND EventID = @EventID ";
        var obj = (List<int?>)db.Query<int?>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID });
        return obj[0] > 0;
    }
    public string TakeAttendance(AttendanceInfo info)
    {
        VotingReplyEntry votingReplyEntry = new VotingReplyEntry();
        VotingReplyEntryInfo VotingReplyEntryList = votingReplyEntry.Get(info.ClientID, info.EventID, info.CreditorID);
        //if (VotingReplyEntryList == null) return null;

        ////voting method checking
        //if (VotingReplyEntryList != null && VotingReplyEntryList.VoteMethod != "2") return null;


        db.Open();
        try
        {
            if (!this.IsAttended(info.ClientID, info.CreditorID, info.EventID))
            {
                info.CreditorIDNo = VotingReplyEntryList.CreditorIDNo;

                string query = "INSERT INTO [dbo].[Attendance] "
                            + "([ClientID] "
                            + ",[EventID] "
                            + ",[CreditorID] "
                            + ",[CreditorIDNo] "
                            + ",[CreditAgent] "
                            + ",[CreditAgentIDNo] "
                            + ",[CreateDate] "
                            + ",[CreateUser]) "
                            + "VALUES "
                            + "(@ClientID "
                            + ",@EventID "
                            + ",@CreditorID "
                            + ",@CreditorIDNo "
                            + ",@CreditAgent "
                            + ",@CreditAgentIDNo "
                            + ",@CreateDate "
                            + ",@CreateUser) ";
                db.Execute(query, info, transaction);
                return "1";
            }
            else
            {
                return "2";
            }

        }
        finally
        {
            db.Close();
        }
       // return VotingReplyEntryList;
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

    public string GetCreditorName(string clientID, string creditorID, string creditType)
    {
        db.Open();
        String query = @"SELECT CreditorName 

from CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
WHERE AdminExamineConfirm > 0 and CreditorMaster.ClientID = @ClientID AND CreditorMaster.CreditorID = @CreditorID AND CreditorAuditDetail.CreditorType = @CreditType";

        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, CreditorID = creditorID, CreditType = creditType });
        db.Close();
        if (obj.Count > 0 && obj[0] != null)
            return obj[0];
        else
            return null;
    }

    public List<string[]> GetCreditorVotingInfo(string clientID, string creditorID)
    {
        string query = @"

SELECT CreditorName, CreditorAuditDetail.CreditorType, ChiDesc
from CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join GeneralMaster on Category = 'CreditType' and Code = CreditorAuditDetail.CreditorType
WHERE AdminExamineConfirm > 0 
AND CreditorMaster.ClientID = @ClientID 
AND CreditorMaster.CreditorID = @CreditorID 

";

        db.Open();
        try
        {

            var obj = (List<dynamic>)db.Query<dynamic>(query, new { ClientID = clientID, CreditorID = creditorID });
             
            List<string[]> result = new List<string[]>();

            foreach (var item in obj)
            {
                result.Add(new string[]{
                item.CreditorName,
                item.CreditorType,
                item.ChiDesc,
            });
            }
            return result;
        }
        finally
        {
            db.Close();
        }
         
    }

    public List<string[]> GetCreditorVotingInfo(string clientID, string creditorID, string creditType)
    {
        String query = @"

SELECT CreditorName, CreditorAuditDetail.CreditorType, ChiDesc
from CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join GeneralMaster on Category = 'CreditType' and Code = CreditorAuditDetail.CreditorType
WHERE AdminExamineConfirm > 0 
AND CreditorMaster.ClientID = @ClientID 
AND CreditorMaster.CreditorID = @CreditorID 
And CreditorAuditDetail.CreditorType = @CreditType

";


        db.Open();

        try
        {
            var obj = (List<dynamic>)db.Query<dynamic>(query, new { ClientID = clientID, CreditorID = creditorID, CreditType = creditType });

            List<string[]> result = new List<string[]>();

            foreach (var item in obj)
            {
                result.Add(new string[]{
                item.CreditorName,
                item.CreditorType,
                item.ChiDesc,
            });
            }

            return result;

        }
        finally {
            db.Close();
        }
          
    }

    public List<OnlineVotePageInfo> Get(string clientID, string creditorID, int eventID, string creditType)
    {
        string query = "";
        if (this.IsOutOfTimeRange(clientID, eventID)) return null;
        if (this.IsExisted(clientID, creditorID, eventID, creditType))
            query = "SELECT [dbo].[OnlineVotePage].* "
                  + ",[dbo].[VotingQuestion].Question "
                  + ",[dbo].[VotingQuestion].AnswerType "
                  + "FROM [dbo].[OnlineVotePage] "
                  + "JOIN [dbo].[VotingQuestion] "
                  + "ON [dbo].[OnlineVotePage].ClientID = [dbo].[VotingQuestion].ClientID "
                  + "AND [dbo].[OnlineVotePage].EventID = [dbo].[VotingQuestion].EventID "
                  + "AND [dbo].[OnlineVotePage].QuestionID = [dbo].[VotingQuestion].QuestionID "
                  + "WHERE [dbo].[OnlineVotePage].ClientID = @ClientID AND [dbo].[OnlineVotePage].CreditorID = @CreditorID AND [dbo].[OnlineVotePage].EventID = @EventID AND [dbo].[OnlineVotePage].CreditType = @CreditType ";
        else
            query = "SELECT QuestionID, Question, AnswerType FROM VotingQuestion WHERE ClientID = @ClientID AND EventID = @EventID";
         
        db.Open();
        try
        { 
            List<OnlineVotePageInfo> obj = (List<OnlineVotePageInfo>)db.Query<OnlineVotePageInfo>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType = creditType });
            return obj;
        } 
        finally
        {
            db.Close();
        }
         
    }
    #endregion

    #region Checking
    public bool IsExisted(string clientID, string creditorID, int eventID, string CreditType)
    {
        String query = "SELECT COUNT(*) FROM OnlineVotePage WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND EventID = @EventID AND CreditType = @CreditType ";
        var obj = (List<int?>)db.Query<int?>(query, new { ClientID = clientID, CreditorID = creditorID, EventID = eventID, CreditType = CreditType });
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