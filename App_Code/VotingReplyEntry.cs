using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;

public class VotingReplyEntry
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);

    public VotingReplyEntry()
    {
    }

    #region Save
    public void Save(VotingReplyEntryInfo votingReplyEntryInfo)
    {
        if (this.IsExisted(votingReplyEntryInfo.ClientID, votingReplyEntryInfo.EventID, votingReplyEntryInfo.CreditorID))
            this.Update(votingReplyEntryInfo);
        else
            this.Insert(votingReplyEntryInfo);
    }

    #endregion

    #region Get
    public VotingReplyEntryInfo GetCreditorInfo(string clientID, string creditorID)
    {
        db.Open();
        String query = "SELECT CreditorName, ResponsiblePerson FROM [dbo].[CreditorMaster] WHERE ClientID = @ClientID AND CreditorID = @CreditorID ";
        var obj = (List<VotingReplyEntryInfo>)db.Query<VotingReplyEntryInfo>(query, new { ClientID = clientID, CreditorID = creditorID });
        db.Close();
        if (obj.Count > 0 && obj[0] != null)
            return obj[0];
        else
            return null;
    }

    public VotingReplyEntryInfo Get(string clientID, int eventID, string creditorID)
    {
        db.Open();
        try
        {
            string query = @"
select VotingReplyEntry.*, 
CreditorMaster.CreditorName, 
CreditorMaster.CreditorIDNo,
VotingEventSetup.EventID, 
VotingEventSetup.EventDescription 
from VotingReplyEntry  
join CreditorMaster on VotingReplyEntry.ClientID = CreditorMaster.ClientID and VotingReplyEntry.CreditorID = CreditorMaster.CreditorID
left outer join VotingEventSetup on VotingEventSetup.ClientID = VotingReplyEntry.ClientID and VotingEventSetup.EventID = VotingReplyEntry.EventID
WHERE VotingReplyEntry.ClientID = @ClientID AND VotingReplyEntry.EventID = @EventID AND VotingReplyEntry.CreditorID = @CreditorID

";
            var obj = (List<VotingReplyEntryInfo>)db.Query<VotingReplyEntryInfo>(query, new { ClientID = clientID, EventID = eventID, CreditorID = creditorID });
            if (obj.Count > 0)
            {
                return obj[0];
            }
            else
            {
                query = @"

select top 1
VotingEventSetup.EventID, 
VotingEventSetup.EventDescription
from VotingEventSetup  
WHERE VotingEventSetup.ClientID = @ClientID AND VotingEventSetup.EventID = @EventID 
";
                var objEventSetup = db.Query(query, new { ClientID = clientID, EventID = eventID});
                foreach (var info in objEventSetup)
                {
                    VotingReplyEntryInfo backInfo = new VotingReplyEntryInfo();
                    backInfo.ClientID = clientID;
                    backInfo.EventID = info.EventID;
                    backInfo.CreditorID = creditorID;
                    backInfo.EventDescription = info.EventDescription;
                    return backInfo;
                }

            }
        }
        finally
        {
            db.Close();
        }
        return null;
    }

    public List<CreditorAuditDetailInfo> GetCreditorAuditDetailList(string clientID, string creditorID)
    {
        String query = @"
SELECT 
CreditorAuditDetail.* ,
ChiDesc CreditTypeDesc
FROM 
CreditorAuditDetail 
join GeneralMaster on Category='CreditType' and Code = CreditorType
WHERE ClientID = @ClientID AND CreditorID = @CreditorID ";
        List<CreditorAuditDetailInfo> CreditorAuditDetailList = (List<CreditorAuditDetailInfo>)db.Query<CreditorAuditDetailInfo>(query, new { ClientID = clientID, CreditorID = creditorID });
        return CreditorAuditDetailList;
    }

    #endregion

    #region Insert
    public void Insert(VotingReplyEntryInfo votingReplyEntryInfo)
    {
        db.Open();
        String query = "INSERT INTO [dbo].[VotingReplyEntry] "
                     + "([ClientID] "
                     + ",[EventID] "
                     + ",[CreditorID] "
                     + ",[Attendance] "
                     + ",[VoteMethod] "
                     + ",[ReplyDate] "
                     + ",[EMSStatus] "
                     + ",[EMSTrackingNo] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@EventID "
                     + ",@CreditorID "
                     + ",@Attendance "
                     + ",@VoteMethod "
                     + ",@ReplyDate "
                     + ",@EMSStatus "
                     + ",@EMSTrackingNo "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, votingReplyEntryInfo);
        db.Close();
    }
    #endregion

    #region Update
    public void Update(VotingReplyEntryInfo votingReplyEntryInfo)
    {
        db.Open();
        String query = "UPDATE [dbo].[VotingReplyEntry] "
                     + "SET "
                     + "[Attendance] = @Attendance "
                     + ",[VoteMethod] = @VoteMethod "
                     + ",[ReplyDate] = @ReplyDate "
                     + ",[EMSStatus] = @EMSStatus "
                     + ",[EMSTrackingNo] = @EMSTrackingNo "
                     + ",[LastModifiedUser] = @LastModifiedUser "
                     + ",[LastModifiedDate] = @LastModifiedDate "
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND EventID = @EventID ";
        db.Execute(query, votingReplyEntryInfo);
        db.Close();
    }
    #endregion

    #region Check exists
    public bool IsExisted(string clientID, int eventID, string creditorID)
    {
        db.Open();
        String query = "SELECT Count(*) FROM VotingReplyEntry WHERE ClientID = @ClientID AND EventID = @EventID AND CreditorID = @CreditorID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, EventID = eventID, CreditorID = creditorID });
        db.Close();
        return obj[0] > 0;
    }
    #endregion
}