using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;

public class VotingProjection
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public VotingProjection()
    {
    }

    #region Save
    public void Save(List<VotingProjectionInfo> votingProjectionList)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            foreach (VotingProjectionInfo votingProjectionInfo in votingProjectionList)
            {
                this.SaveVotingProjectionInfo(votingProjectionInfo);
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

    public void SaveVotingProjectionInfo(VotingProjectionInfo votingProjectionInfo)
    {
        if (this.IsExisted(votingProjectionInfo.ClientID, votingProjectionInfo.EventID, votingProjectionInfo.CreditorID, votingProjectionInfo.CreditType, votingProjectionInfo.QuestionID))
            this.Update(votingProjectionInfo);
        else
            this.Insert(votingProjectionInfo);

    }
    #endregion

    #region Get
    public string GetCompanyName(string clientID)
    {
        db.Open();
        String query = "SELECT CompanyName FROM ClientMaster WHERE ClientID = @ClientID ";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID });
        db.Close();
        if (obj.Count > 0 && obj[0] != null)
            return obj[0];
        else
            return null;
    }

    public string GetDescription(string clientID, int eventID)
    {
        db.Open();
        String query = "SELECT EventDescription FROM VotingEventSetup WHERE ClientID = @ClientID AND EventID = @EventID ";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, EventID = eventID });
        db.Close();
        if (obj.Count > 0 && obj[0] != null)
            return obj[0];
        else
            return null;
    }

    public List<VotingProjectionInfo> Get(string clientID, int eventID, string creditType)
    {
        db.Open();
        String query = @"SELECT
                      [dbo].[VotingReplyEntry].ClientID, 
                      [dbo].[VotingReplyEntry].EventID, 
                      [dbo].[VotingReplyEntry].CreditorID, 
                      [dbo].[CreditorMaster].CreditorName, 
                      ExamineConfirm.AdminExamineConfirmSum, 
                      [dbo].[VotingQuestion].QuestionID, 
                      [dbo].[VotingQuestion].Question, 
                      [dbo].[VotingQuestion].AnswerType, 
                      [dbo].[VotingProjection].CreditType, 
                      [dbo].[VotingProjection].Decision 
                      FROM [dbo].[VotingReplyEntry] 
                      JOIN [dbo].[CreditorMaster] ON [dbo].[VotingReplyEntry].ClientID = [dbo].[CreditorMaster].ClientID AND [dbo].[VotingReplyEntry].CreditorID = [dbo].[CreditorMaster].CreditorID 
                      JOIN (SELECT ClientID, CreditorID, SUM(AdminExamineConfirm) AdminExamineConfirmSum FROM CreditorAuditDetail where CreditorType=@CreditType GROUP BY ClientID, CreditorID) ExamineConfirm ON ExamineConfirm.ClientID = [dbo].[VotingReplyEntry].ClientID AND ExamineConfirm.CreditorID = [dbo].[VotingReplyEntry].CreditorID 
                      JOIN [dbo].[VotingQuestion] ON [dbo].[VotingReplyEntry].ClientID = [dbo].[VotingQuestion].ClientID AND [dbo].[VotingReplyEntry].EventID = [dbo].[VotingQuestion].EventID 
                      LEFT OUTER JOIN [dbo].[VotingProjection] ON [dbo].[VotingReplyEntry].ClientID = [dbo].[VotingProjection].ClientID
AND [dbo].[VotingReplyEntry].EventID = [dbo].[VotingProjection].EventID 
AND [dbo].[VotingReplyEntry].CreditorID = [dbo].[VotingProjection].CreditorID 
AND [dbo].[VotingQuestion].QuestionID = [dbo].[VotingProjection].QuestionID 
AND [dbo].[VotingProjection].CreditType = @CreditType
                      WHERE [dbo].[VotingReplyEntry].Attendance = '1'
AND [dbo].[VotingReplyEntry].ClientID = @ClientID 
AND [dbo].[VotingReplyEntry].EventID = @EventID  
AND AdminExamineConfirmSum > 0
ORDER BY [dbo].[VotingReplyEntry].CreditorID
";
        var obj = (List<VotingProjectionInfo>)db.Query<VotingProjectionInfo>(query, new { ClientID = clientID, EventID = eventID, CreditType = creditType });
        db.Close();
        return obj;
    }
    #endregion

    #region Insert
    public void Insert(VotingProjectionInfo votingProjectionInfo)
    {
        String query = "INSERT INTO [dbo].[VotingProjection] "
                     + "([ClientID] "
                     + ",[EventID] "
                     + ",[CreditorID] "
                     + ",[QuestionID] "
                     + ",[Decision] "
                     + ",[CreditType] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@EventID "
                     + ",@CreditorID "
                     + ",@QuestionID "
                     + ",@Decision "
                     + ",@CreditType "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, votingProjectionInfo, transaction);
    }
    #endregion

    #region Update
    public void Update(VotingProjectionInfo votingProjectionInfo)
    {
        String query = "UPDATE [dbo].[VotingProjection] "
                     + "SET " 
                     + " [Decision] = @Decision "
                     + ",[LastModifiedUser] = @LastModifiedUser "
                     + ",[LastModifiedDate] = @LastModifiedDate "
                     + "WHERE ClientID = @ClientID AND EventID = @EventID AND CreditorID = @CreditorID AND CreditType = @CreditType AND QuestionID = @QuestionID ";
        db.Execute(query, votingProjectionInfo, transaction);
    }
    #endregion

    #region Check Exists
    public bool IsExisted(string clientID, int eventID, string creditorID, string CreditType, int questionID)
    {
        String query = @"SELECT COUNT(*) FROM [dbo].[VotingProjection] 
        WHERE ClientID = @ClientID 
AND EventID = @EventID 
AND CreditorID = @CreditorID 
AND CreditType = @CreditType 
AND QuestionID = @QuestionID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, EventID = eventID, CreditorID = creditorID, CreditType = CreditType, QuestionID = questionID }, transaction);
        return obj[0] > 0;
    }
    #endregion
}