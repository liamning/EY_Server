using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;
using Ionic.Zip;

public class VotingCreditorGeneration
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    public VotingCreditorGeneration()
    {
    }

    #region Get
    public List<VotingCreditorGenerationInfo> Get(string clientID)
    {
        db.Open();
        String query = @"SELECT CreditorMaster.ClientID, 
                     CreditorMaster.CreditorID, 
                     GR1.ChiDesc CreditorType, 
                     CreditorMaster.CreditorName, 
                     PropertySecurityView.AdminExamineConfirm PropertySecurity, 
                     OrdinaryCreditorRightView.AdminExamineConfirm OrdinaryCreditorRight 
                     FROM CreditorMaster 
                     left outer join GeneralMaster GR1 on [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType' 
                     left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 1) PropertySecurityView ON PropertySecurityView.ClientID = CreditorMaster.ClientID AND PropertySecurityView.CreditorID = CreditorMaster.CreditorID 
                     left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 4) OrdinaryCreditorRightView ON OrdinaryCreditorRightView.ClientID = CreditorMaster.ClientID AND OrdinaryCreditorRightView.CreditorID = CreditorMaster.CreditorID 
                     WHERE CreditorMaster.ClientID = @ClientID ORDER BY CreditorMaster.CreditorID";
        List<VotingCreditorGenerationInfo> obj = (List<VotingCreditorGenerationInfo>)db.Query<VotingCreditorGenerationInfo>(query, new { ClientID = clientID });
        db.Close();

        //InitInvitation(clientID);

        return obj;

    }

    public List<VotingCreditorGenerationInfo> Get(string clientID, int EventID, string VoteMethod = "")
    {
        db.Open();
        String query = @"

SELECT CreditorMaster.ClientID, 
CreditorMaster.CreditorID, 
GR1.ChiDesc CreditorType, 
CreditorMaster.CreditorName, 
PropertySecurityView.AdminExamineConfirm PropertySecurity, 
PropertySecurityView2.AdminExamineConfirm PropertyLabour, 
PropertySecurityView3.AdminExamineConfirm PropertyTax, 
OrdinaryCreditorRightView.AdminExamineConfirm OrdinaryCreditorRight 
FROM CreditorMaster 
Join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID  
left outer join GeneralMaster GR1 on [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType' 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 1) PropertySecurityView ON PropertySecurityView.ClientID = CreditorMaster.ClientID AND PropertySecurityView.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 2) PropertySecurityView2 ON PropertySecurityView2.ClientID = CreditorMaster.ClientID AND PropertySecurityView2.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 3) PropertySecurityView3 ON PropertySecurityView3.ClientID = CreditorMaster.ClientID AND PropertySecurityView3.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 4) OrdinaryCreditorRightView ON OrdinaryCreditorRightView.ClientID = CreditorMaster.ClientID AND OrdinaryCreditorRightView.CreditorID = CreditorMaster.CreditorID 
WHERE CreditorMaster.ClientID = @ClientID
and VotingReplyEntry.EventID = @EventID and VotingReplyEntry.Attendance = '1'
and (VoteMethod =@VoteMethod or VotingReplyEntry.VoteMethod = @VoteMethod)
 ORDER BY CreditorMaster.CreditorID

";
        List<VotingCreditorGenerationInfo> obj = (List<VotingCreditorGenerationInfo>)db.Query<VotingCreditorGenerationInfo>(query, new { ClientID = clientID, VoteMethod = VoteMethod, EventID = EventID });
        db.Close();

        InitInvitation(clientID, VoteMethod);

        return obj;

    }

    public List<VotingCreditorGenerationInfo> GetAcknowledgement(string clientID)
    {
        db.Open();
        String query = @"SELECT CreditorMaster.ClientID, 
                     CreditorMaster.CreditorID, 
                     GR1.ChiDesc CreditorType, 
                     CreditorMaster.CreditorName, 
                     PropertySecurityView.AdminExamineConfirm PropertySecurity, 
                     OrdinaryCreditorRightView.AdminExamineConfirm OrdinaryCreditorRight 
                     FROM CreditorMaster 
                     left outer join GeneralMaster GR1 on [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType' 
                     left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 1) PropertySecurityView ON PropertySecurityView.ClientID = CreditorMaster.ClientID AND PropertySecurityView.CreditorID = CreditorMaster.CreditorID 
                     left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 4) OrdinaryCreditorRightView ON OrdinaryCreditorRightView.ClientID = CreditorMaster.ClientID AND OrdinaryCreditorRightView.CreditorID = CreditorMaster.CreditorID 
                     WHERE CreditorMaster.ClientID = @ClientID ORDER BY CreditorMaster.CreditorID";
        List<VotingCreditorGenerationInfo> obj = (List<VotingCreditorGenerationInfo>)db.Query<VotingCreditorGenerationInfo>(query, new { ClientID = clientID });
        db.Close();

        InitAcknowlegement(clientID);

        return obj;

    }


    public List<VotingCreditorGenerationInfo> GetLiveVotingForm(string clientID, int EventID)
    {
        db.Open();
        String query = @"

SELECT CreditorMaster.ClientID, 
CreditorMaster.CreditorID, 
GR1.ChiDesc CreditorType, 
CreditorMaster.CreditorName, 
PropertySecurityView.AdminExamineConfirm PropertySecurity, 
PropertySecurityView2.AdminExamineConfirm PropertyLabour, 
PropertySecurityView3.AdminExamineConfirm PropertyTax, 
OrdinaryCreditorRightView.AdminExamineConfirm OrdinaryCreditorRight 
FROM CreditorMaster 
Join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID  
left outer join GeneralMaster GR1 on [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType' 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 1) PropertySecurityView ON PropertySecurityView.ClientID = CreditorMaster.ClientID AND PropertySecurityView.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 2) PropertySecurityView2 ON PropertySecurityView2.ClientID = CreditorMaster.ClientID AND PropertySecurityView2.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 3) PropertySecurityView3 ON PropertySecurityView3.ClientID = CreditorMaster.ClientID AND PropertySecurityView3.CreditorID = CreditorMaster.CreditorID 
left outer JOIN (SELECT ClientID, CreditorID, AdminExamineConfirm FROM CreditorAuditDetail WHERE RowNo = 4) OrdinaryCreditorRightView ON OrdinaryCreditorRightView.ClientID = CreditorMaster.ClientID AND OrdinaryCreditorRightView.CreditorID = CreditorMaster.CreditorID 
WHERE CreditorMaster.ClientID = @ClientID
and VotingReplyEntry.EventID = @EventID and VotingReplyEntry.Attendance = '1'
and VotingReplyEntry.VoteMethod = '2'
 ORDER BY CreditorMaster.CreditorID

";
        List<VotingCreditorGenerationInfo> obj = (List<VotingCreditorGenerationInfo>)db.Query<VotingCreditorGenerationInfo>(query, new { ClientID = clientID, EventID = EventID });
        db.Close();

        InitLiveVotingForm(clientID);

        return obj;

    }

    #endregion

    public byte[] GenerateVotingForm(string clientID, string EventID)
    {
        //prepare template file
        string templatePath = HttpContext.Current.Server.MapPath("~/Template/VotingForm/VotingForm.docx");
        string tmpRoot = HttpContext.Current.Server.MapPath("~/tmp");
        string tmpClientFolder = tmpRoot + @"\VotingForm\" + clientID;
        string tmpFile = "";
        string input = "";

        if (!Directory.Exists(tmpClientFolder)) Directory.CreateDirectory(tmpClientFolder);


        db.Open();
        string query = @"
                     select * from GeneralMaster
                     where  Category = 'decisionGroup2'
                        ";

        var obj = db.Query(query, new { ClientID = clientID });

        Dictionary<string, string> headerDict = new Dictionary<string, string>();
        int i = 0;
        foreach (var info in obj)
        {
            headerDict.Add("Decision" + (++i), info.ChiDesc);
        }

        query = @"
select 
ClientMaster.CompanyName,
EventDescription, EventDate
from ClientMaster  
join VotingEventSetup on VotingEventSetup.ClientID = ClientMaster.ClientID 
where VotingEventSetup.ClientID = @ClientID and VotingEventSetup.EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        foreach (var info in obj)
        {
            headerDict.Add("EventDescription", info.EventDescription);
            headerDict.Add("CompanyName", info.CompanyName);
            headerDict.Add("EventDate", string.Format("{0:dd/MM/yyyy}", info.EventDate));
        }

        query = @"
select * from VotingQuestion 
where ClientID = @ClientID and EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });
        foreach (var info in obj)
        {
            headerDict.Add("Question" + info.QuestionID, info.Question);
            if(info.AnswerType == "2")
                headerDict.Add("DecisionThree_" + info.QuestionID, headerDict["Decision3"]);
            else
                headerDict.Add("DecisionThree_" + info.QuestionID, " ");
        }


//        --"2"
//--Desc
//--:
//--"现场投票"

        query = @"



select distinct
CreditorMaster.CreditorID,
CreditorName,
CreditorAuditDetail.CreditorType,
GeneralMaster.ChiDesc CreditTypeDesc,
VotingReplyEntry.VoteMethod
from 
CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID and @EventID = VotingReplyEntry.EventID
join GeneralMaster on Category = 'CreditType' and CreditorAuditDetail.CreditorType = Code

where AdminExamineConfirm > 0 and CreditorMaster.ClientID = @ClientID and VotingReplyEntry.VoteMethod = '2'

"; 

        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        db.Close();

        Dictionary<string, string> replaceDictionary = null;
        Dictionary<string, string> filePathDict = new Dictionary<string, string>();


        byte[] emptyFile = new byte[0];

        string creditorID = "";

        foreach (var info in obj)
        {
            replaceDictionary = new Dictionary<string, string>();
            tmpFile = string.Format(@"{0}\{1}.docx", tmpClientFolder, DateTime.Now.Ticks.ToString());
            File.Copy(templatePath, tmpFile);


            replaceDictionary.Add("CreditorID", info.CreditorID);
            replaceDictionary.Add("CreditorName", info.CreditorName);
            replaceDictionary.Add("CreditTypeDesc", info.CreditTypeDesc);

            foreach (var key in headerDict.Keys)
            {
                replaceDictionary.Add(key, headerDict[key]);
            }
            creditorID = info.CreditorID;
             
            input = string.Format("{0}/{1}/{2}/{3}", clientID, EventID, creditorID, info.CreditorType);
            
            byte[] file = ReportHelper.readRTF(tmpFile, replaceDictionary, input);

            File.WriteAllBytes(string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType), file);
            filePathDict.Add(creditorID + "_" + info.CreditorType, string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType));

        }

        byte[] result = File.ReadAllBytes(ZipImages(filePathDict, clientID, tmpClientFolder));


        return result;
    }
    
    public byte[] GenerateAcknowledgement(string clientID, string EventID)
    {
        //prepare template file
        string path = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.EventAcknowledgementPath, clientID).Replace("\\\\", "\\");

        string tmpRoot = HttpContext.Current.Server.MapPath("~/tmp");
        string tmpClientFolder = tmpRoot + @"\EventAcknowledgementPath\" + clientID;


        string tmpFile = "";
        string input = "";

        if (!Directory.Exists(tmpClientFolder)) Directory.CreateDirectory(tmpClientFolder);

        db.Open();
        String query = "SELECT CreditorMaster.* "
                     + "FROM CreditorMaster "
                    + "WHERE CreditorMaster.ClientID = @ClientID ORDER BY CreditorMaster.CreditorID";
        var obj = db.Query(query, new { ClientID = clientID });
        db.Close();

        Dictionary<string, string> replaceDictionary = null;
        Dictionary<string, string> filePathDict = new Dictionary<string, string>();
        byte[] emptyFile = new byte[0];
        string creditorID = "";

        foreach (var info in obj)
        {
            replaceDictionary = new Dictionary<string, string>();
            tmpFile = string.Format(@"{0}\{1}.docx", tmpClientFolder, DateTime.Now.Ticks.ToString());
            File.Copy(path, tmpFile);

            foreach (var pro in info)
            {
                if (pro.Value != null)
                    replaceDictionary.Add(pro.Key, string.Format("{0}", pro.Value));
                else

                    replaceDictionary.Add(pro.Key, " ");

                if (pro.Key == "CreditorID")
                {
                    creditorID = pro.Value;
                }

            }
            input = string.Format("{0}/{1}/{2}", clientID, EventID, creditorID);
            byte[] file = ReportHelper.readRTF(tmpFile, replaceDictionary, input);

            File.WriteAllBytes(string.Format(@"{0}\{1}.docx", tmpClientFolder, creditorID), file);
            filePathDict.Add(creditorID, string.Format(@"{0}\{1}.docx", tmpClientFolder, creditorID));

        }

        byte[] result = File.ReadAllBytes(ZipImages(filePathDict, clientID, tmpClientFolder));


        return result;
    }

    public byte[] GenerateLiveVotingForm(string clientID, string EventID)
    {
        //prepare template file
        string templatePath = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.LiveVotingFormPath, clientID).Replace("\\\\", "\\");

        string tmpRoot = HttpContext.Current.Server.MapPath("~/tmp");
        string tmpClientFolder = tmpRoot + @"\LiveVotingFormPath\" + clientID;


        string tmpFile = "";
        string input = "";

        if (!Directory.Exists(tmpClientFolder)) Directory.CreateDirectory(tmpClientFolder);


        db.Open();
        string query = @"
                     select * from GeneralMaster
                     where  Category = 'decisionGroup2'
                        ";

        var obj = db.Query(query, new { ClientID = clientID });

        Dictionary<string, string> headerDict = new Dictionary<string, string>();
        int i = 0;
        foreach (var info in obj)
        {
            headerDict.Add("Decision" + (++i), info.ChiDesc);
        }

        query = @"
select 
ClientMaster.CompanyName,
EventDescription, EventDate
from ClientMaster  
join VotingEventSetup on VotingEventSetup.ClientID = ClientMaster.ClientID 
where VotingEventSetup.ClientID = @ClientID and VotingEventSetup.EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        foreach (var info in obj)
        {
            headerDict.Add("EventDescription", info.EventDescription);
            headerDict.Add("CompanyName", info.CompanyName);
            headerDict.Add("EventDate", string.Format("{0:dd/MM/yyyy}", info.EventDate));
        }

        query = @"
select * from VotingQuestion 
where ClientID = @ClientID and EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });
        foreach (var info in obj)
        {
            headerDict.Add("Question" + info.QuestionID, info.Question);
            if (info.AnswerType == "2")
                headerDict.Add("DecisionThree_" + info.QuestionID, headerDict["Decision3"]);
            else
                headerDict.Add("DecisionThree_" + info.QuestionID, " ");
        }


        //        --"2"
        //--Desc
        //--:
        //--"现场投票"

        query = @"



select distinct
CreditorMaster.CreditorID,
CreditorName,
CreditorAuditDetail.CreditorType,
GeneralMaster.ChiDesc CreditTypeDesc,
VotingReplyEntry.VoteMethod
from 
CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID and @EventID = VotingReplyEntry.EventID
join GeneralMaster on Category = 'CreditType' and CreditorAuditDetail.CreditorType = Code

where AdminExamineConfirm > 0 and CreditorMaster.ClientID = @ClientID and VotingReplyEntry.VoteMethod = '2'

";

        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        db.Close();

        Dictionary<string, string> replaceDictionary = null;
        Dictionary<string, string> filePathDict = new Dictionary<string, string>();


        byte[] emptyFile = new byte[0];

        string creditorID = "";

        foreach (var info in obj)
        {
            replaceDictionary = new Dictionary<string, string>();
            tmpFile = string.Format(@"{0}\{1}.docx", tmpClientFolder, DateTime.Now.Ticks.ToString());
            File.Copy(templatePath, tmpFile);


            replaceDictionary.Add("CreditorID", info.CreditorID);
            replaceDictionary.Add("CreditorName", info.CreditorName);
            replaceDictionary.Add("CreditTypeDesc", info.CreditTypeDesc);

            foreach (var key in headerDict.Keys)
            {
                replaceDictionary.Add(key, headerDict[key]);
            }
            creditorID = info.CreditorID;

            input = string.Format("{0}/{1}/{2}/{3}", clientID, EventID, creditorID, info.CreditorType);

            byte[] file = ReportHelper.readRTF(tmpFile, replaceDictionary, input);

            File.WriteAllBytes(string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType), file);
            filePathDict.Add(creditorID + "_" + info.CreditorType, string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType));

        }

        byte[] result = File.ReadAllBytes(ZipImages(filePathDict, clientID, tmpClientFolder));


        return result;
    }

    public byte[] GenerateInvitation(string clientID, string EventID, string VoteMethod)
    {
        //prepare template file
        string templatePath = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.InvitationPath, clientID, VoteMethod).Replace("\\\\", "\\");

        string tmpRoot = HttpContext.Current.Server.MapPath("~/tmp");
        string tmpClientFolder = tmpRoot + @"\Invitation\" + clientID;


        string tmpFile = "";
        string input = "";

        if (!Directory.Exists(tmpClientFolder)) Directory.CreateDirectory(tmpClientFolder);
         

        db.Open();
        string query = @"
                     select * from GeneralMaster
                     where  Category = 'decisionGroup2'
                        ";

        var obj = db.Query(query, new { ClientID = clientID });

        Dictionary<string, string> headerDict = new Dictionary<string, string>();
        int i = 0;
        foreach (var info in obj)
        {
            headerDict.Add("Decision" + (++i), info.ChiDesc);
        }

        query = @"
select 
ClientMaster.CompanyName,
EventDescription, EventDate
from ClientMaster  
join VotingEventSetup on VotingEventSetup.ClientID = ClientMaster.ClientID 
where VotingEventSetup.ClientID = @ClientID and VotingEventSetup.EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        foreach (var info in obj)
        {
            headerDict.Add("EventDescription", info.EventDescription);
            headerDict.Add("CompanyName", info.CompanyName);
            headerDict.Add("EventDate", string.Format("{0:dd/MM/yyyy}", info.EventDate));
        }

        query = @"
select * from VotingQuestion 
where ClientID = @ClientID and EventID = @EventID

";
        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });
        foreach (var info in obj)
        {
            headerDict.Add("Question" + info.QuestionID, info.Question);
            if (info.AnswerType == "2")
                headerDict.Add("DecisionThree_" + info.QuestionID, headerDict["Decision3"]);
            else
                headerDict.Add("DecisionThree_" + info.QuestionID, " ");
        }


        //        --"2"
        //--Desc
        //--:
        //--"现场投票"

        query = @"



select distinct
CreditorMaster.CreditorID,
CreditorName,
CreditorAuditDetail.CreditorType,
GeneralMaster.ChiDesc CreditTypeDesc,
VotingReplyEntry.VoteMethod
from 
CreditorMaster 
join CreditorAuditDetail on CreditorMaster.CreditorID = CreditorAuditDetail.CreditorID and CreditorMaster.ClientID = CreditorAuditDetail.ClientID 
join VotingReplyEntry on CreditorMaster.ClientID = VotingReplyEntry.ClientID and CreditorMaster.CreditorID = VotingReplyEntry.CreditorID and @EventID = VotingReplyEntry.EventID
join GeneralMaster on Category = 'CreditType' and CreditorAuditDetail.CreditorType = Code

where AdminExamineConfirm > 0 and CreditorMaster.ClientID = @ClientID and VotingReplyEntry.VoteMethod = '2'

";

        obj = db.Query(query, new { ClientID = clientID, EventID = EventID });

        db.Close();

        Dictionary<string, string> replaceDictionary = null;
        Dictionary<string, string> filePathDict = new Dictionary<string, string>();


        byte[] emptyFile = new byte[0];

        string creditorID = "";

        foreach (var info in obj)
        {
            replaceDictionary = new Dictionary<string, string>();
            tmpFile = string.Format(@"{0}\{1}.docx", tmpClientFolder, DateTime.Now.Ticks.ToString());
            File.Copy(templatePath, tmpFile);


            replaceDictionary.Add("CreditorID", info.CreditorID);
            replaceDictionary.Add("CreditorName", info.CreditorName);
            replaceDictionary.Add("CreditTypeDesc", info.CreditTypeDesc);

            foreach (var key in headerDict.Keys)
            {
                replaceDictionary.Add(key, headerDict[key]);
            }
            creditorID = info.CreditorID;

            input = string.Format("{0}/{1}/{2}/{3}", clientID, EventID, creditorID, info.CreditorType);

            byte[] file = ReportHelper.readRTF(tmpFile, replaceDictionary, input);

            File.WriteAllBytes(string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType), file);
            filePathDict.Add(creditorID + "_" + info.CreditorType, string.Format(@"{0}\{1}({2}).docx", tmpClientFolder, creditorID, info.CreditorType));

        }

        byte[] result = File.ReadAllBytes(ZipImages(filePathDict, clientID, tmpClientFolder));


        return result;
    }

    public void InitInvitation(string clientID, string voteMethod)
    {
        //prepare template file
        string path = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.InvitationPath, clientID, voteMethod).Replace("\\\\", "\\");
        string originTemplate = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.OriginInvitationPath, clientID).Replace("\\\\", "\\");

        string directoty = path.Substring(0, path.LastIndexOf("\\") + 1);

        if (!Directory.Exists(directoty))
            Directory.CreateDirectory(directoty);

        if (!File.Exists(path))
            File.Copy(originTemplate, path);

    }

    public void InitAcknowlegement(string clientID)
    {
        //prepare template file
        string path = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.EventAcknowledgementPath, clientID).Replace("\\\\", "\\");
        string originTemplate = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.OriginEventAcknowledgementPath, clientID).Replace("\\\\", "\\");

        string directoty = path.Substring(0, path.LastIndexOf("\\") + 1);

        if (!Directory.Exists(directoty))
            Directory.CreateDirectory(directoty);

        if (!File.Exists(path))
            File.Copy(originTemplate, path);

    }

    public void InitLiveVotingForm(string clientID)
    {
        //prepare template file
        string path = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.LiveVotingFormPath, clientID).Replace("\\\\", "\\");
        string originTemplate = HttpContext.Current.Server.MapPath("~") + string.Format(GlobalSetting.Path.OriginLiveVotingFormPath, clientID).Replace("\\\\", "\\");

        string directoty = path.Substring(0, path.LastIndexOf("\\") + 1);

        if (!Directory.Exists(directoty))
            Directory.CreateDirectory(directoty);

        if (!File.Exists(path))
            File.Copy(originTemplate, path);

    }

    public string ZipImages(Dictionary<string, string> dict, string clientID, string folder)
    {
        string result;
        using (ZipFile zip = new ZipFile())
        {
            foreach (string key in dict.Keys)
            {
                zip.AddFile(dict[key], clientID);
            }
            result = folder + "\\" + clientID + ".zip";
            zip.Save(result);
        }

        return result;
    }



    //debug
    public DataTableResultSet<VotingCreditorGenerationInfo> GetDataTable(List<Dictionary<string, string>> criteria, int start, int draw)
    {
        string clientID = criteria[0]["value"];
        string creditorID = criteria[1]["value"];

        db.Open();
        String query = @"SELECT top 10  
                     CreditorMaster.CreditorID, 
                     CreditorMaster.CreditorName
                     FROM CreditorMaster 
                     join (
                     select 
                     ROW_NUMBER() OVER (ORDER BY CreditorID) AS ROW_NUM,ClientID,CreditorID
                     from CreditorMaster where CreditorMaster.ClientID = @ClientID and CreditorMaster.CreditorID like '%' + @CreditorID + '%'
                     ) x on x.ClientID = CreditorMaster.ClientID and x.CreditorID = CreditorMaster.CreditorID
                     WHERE ROW_NUM > @Start ORDER BY CreditorMaster.CreditorID";
        List<VotingCreditorGenerationInfo> obj = (List<VotingCreditorGenerationInfo>)db.Query<VotingCreditorGenerationInfo>(query, new { ClientID = clientID, CreditorID = creditorID, @Start = start });

        query = @"SELECT count(*)
                     FROM CreditorMaster 
                     WHERE CreditorMaster.ClientID = @ClientID  and CreditorMaster.CreditorID like '%' + @CreditorID + '%' ";
        List<int> countList = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, });
        db.Close();


        DataTableResultSet<VotingCreditorGenerationInfo> result = new DataTableResultSet<VotingCreditorGenerationInfo>();
        result.data = obj;
        result.draw = Convert.ToInt32(draw);
        result.recordsFiltered = countList[0];
        result.recordsTotal = countList[0];


        return result;

    }
}