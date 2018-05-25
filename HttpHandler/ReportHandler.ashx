<%@ WebHandler Language="C#" Class="ReportHandler" %>

using System;
using System.Web;
using System.IO;
using System.Web.SessionState;

public class ReportHandler : IHttpHandler, IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {
        HttpRequest request = context.Request;
        HttpResponse response = context.Response;
        System.Web.SessionState.HttpSessionState session = context.Session;

        string userID;
        if (session[GlobalSetting.SessionKey.LoginID] != null)
            userID = session[GlobalSetting.SessionKey.LoginID].ToString();



#if DEBUG
            userID = "Administrator";
#endif
          
        try
        {
            string action = request.QueryString["action"].ToString();
            
            byte[] FileData = null;
            string Extension = "";
            string FileName = "";
            
            switch (action)
            {
                //Added by Ning
                case "generateInvitation":
                    VotingCreditorGeneration votingGen = new VotingCreditorGeneration();
                    string clientID = request.QueryString["ClientID"];
                    string EventID = request.QueryString["EventID"];
                    string VoteMethod = request.QueryString["VoteMethod"];

                    FileData = votingGen.GenerateInvitation(clientID, EventID, VoteMethod);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".zip";

                    break;
                //Added by Ning
                case "generateAcknowledgement":
                    votingGen = new VotingCreditorGeneration();
                    clientID = request.QueryString["ClientID"];
                    EventID = request.QueryString["EventID"];

                    FileData = votingGen.GenerateAcknowledgement(clientID, EventID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".zip";

                    break;
                //Added by Ning
                case "generateLiveVotingForm":
                    votingGen = new VotingCreditorGeneration();
                    clientID = request.QueryString["ClientID"];
                    EventID = request.QueryString["EventID"];

                    FileData = votingGen.GenerateLiveVotingForm(clientID, EventID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".zip";

                    break;
                //Added by Ning
                case "generateVotingForm":
                    votingGen = new VotingCreditorGeneration();
                    clientID = request.QueryString["ClientID"];
                    EventID = request.QueryString["EventID"];

                    FileData = votingGen.GenerateVotingForm(clientID, EventID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".zip";

                    break;
                case "generateCreditorAmountReport":
                    CreditorMaster creditorMaster = new CreditorMaster();
                    clientID = request.QueryString["ClientID"];

                    FileData = creditorMaster.GenerateCreditorAmountReport(clientID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".xls";

                    break;
                case "downloadTemplate":
                    string path = request.QueryString["Path"];
                    clientID = request.QueryString["ClientID"]; 
                    VoteMethod = request.QueryString["VoteMethod"]; 
                    string filePath = context.Server.MapPath("~") + string.Format(path, clientID, VoteMethod);

                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = filePath.Substring(filePath.LastIndexOf("."));
                    FileData = File.ReadAllBytes(filePath);

                    break;

                case "exportCreditors":
                    clientID = request.QueryString["ClientID"];
                    FileData = new CreditorMaster().Export(clientID, userID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".xls";

                    break;

                case "exportEmployee":
                    clientID = request.QueryString["ClientID"];
                    FileData = new Employee().Export(clientID, userID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".xls";

                    break;

                case "exportCreditorContact":
                    clientID = request.QueryString["ClientID"];
                    FileData = new CreditorMaster().ExportContact(clientID, userID);
                    FileName = DateTime.Now.Ticks.ToString();
                    Extension = ".xls";

                    break;
                case "downloadLogReport":
                    FileName = request.QueryString["FileName"];
                    string ClientID = request.QueryString["ClientID"];
                    var tmpFolder = HttpContext.Current.Server.MapPath(string.Format("~/tmp/CreditImportLog/{0}", ClientID));
                    var tmpFilePath = tmpFolder + "\\" + FileName; 
                    FileData = File.ReadAllBytes(tmpFilePath);
                    File.Delete(tmpFilePath);

                    break;
                case "downloadDocumentByID":
                    int AttachmentID = Convert.ToInt32(request.QueryString["ID"]);
                    CreditorAttachmentInfo creditorAttachmentInfo = new CreditorMaster().GetAttachment(AttachmentID);
                    if (creditorAttachmentInfo != null)
                    {
                        string ExactFilePath = HttpContext.Current.Server.MapPath(creditorAttachmentInfo.FilePath);
                        FileName = creditorAttachmentInfo.AttName;
                        if (File.Exists(ExactFilePath))
                        {
                            string GenFileName = Path.GetFileName(ExactFilePath);
                            Extension = GenFileName.Substring(GenFileName.LastIndexOf('.'));
                            FileData = File.ReadAllBytes(ExactFilePath);
                        }
                        else
                        {
                            throw new Exception("File Missing.");
                            throw new Exception("Path: " + ExactFilePath);
                        }
                    } 

                    break;
            }


            //response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8080");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=\"" + FileName  + Extension + "\"");
            response.OutputStream.Write(FileData, 0, FileData.Length);

        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Error(e.StackTrace);
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}