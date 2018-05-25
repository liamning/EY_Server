using System;
using System.Collections.Generic;
using System.IO;
using System.Web; 
using System.Web.SessionState;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Configuration;


/// <summary>
/// Summary description for GlobalSetting
/// </summary>
public class GlobalSetting
{
    //public const string DateFormatS = "d/M/yyyy";
    //public const string DateFormatJS = "yyyy-MM-ddTHH:mm:ss";
    public const string DateFormat = "dd/MM/yyyy";
    public const string DateTimeFormat = "dd/MM/yyyy HH:mm:ss";

    public struct SessionKey
    {
        public const string LoginID = "LOGINID";
        public const string LoginRole = "LoginRole";
        public const string LoginRoleList = "LoginRoleList";
 

        public const string LoggedOut = "LoggedOut";
    }

    public const string Culture = "en-US";
    public const string LoginPage = "/#/login";
    public const string AjaxHandler = "HttpHandler/AjaxHandler.ashx";
    public const string LoginHandler = "HttpHandler/LoginHandler.ashx";
    public const string OnlineVoteHandler = "HttpHandler/OnlineVoteHandler.ashx";
    public const string OnlineVotePage = "OnlineVotePage";
    public const string Navigation = "Navigation";

    public const string NoRecordFound = "没有纪录";

    public struct ShiftSource
    {
        public const string RosterTempate = "RT";
        public const string ShiftAdjustment = "AD";
    }

    public class Path
    {
        public static readonly string InvitationPath = System.Configuration.ConfigurationManager.AppSettings["InvitationPath"];
        public static readonly string EventAcknowledgementPath = System.Configuration.ConfigurationManager.AppSettings["EventAcknowledgementPath"];
        public static readonly string OriginInvitationPath = System.Configuration.ConfigurationManager.AppSettings["OriginInvitationPath"];
        public static readonly string OriginEventAcknowledgementPath = System.Configuration.ConfigurationManager.AppSettings["OriginEventAcknowledgementPath"];


        public static readonly string LiveVotingFormPath = System.Configuration.ConfigurationManager.AppSettings["LiveVotingFormPath"]; 
        public static readonly string OriginLiveVotingFormPath = System.Configuration.ConfigurationManager.AppSettings["OriginLiveVotingFormPath"];

    }

    public static string GetHashPassword(string password)
    {
        int salt = 0;

        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] digest = md5.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        string base64digest = Convert.ToBase64String(digest, 0, digest.Length);
        return base64digest.Substring(0, base64digest.Length - 2);
    }

    //public static string ConvertNullToEmptyString(string strInput)
    //{
    //    return strInput == null ? "" : strInput;
    //} 

    //public static void sendMail(EmailInfo info)
    //{
    //    MailAddress fromAddress = new MailAddress(GlobalSetting.SystemMailAccount, GlobalSetting.SystemMailDisplayName);
    //    MailAddress toAddress = new MailAddress(info.ReceiverEmail, info.ReceiverName);

    //    SmtpClient smtp = new SmtpClient(GlobalSetting.SMTPServer, GlobalSetting.SMTPPortNo);
    //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
    //    smtp.UseDefaultCredentials = false;
    //    smtp.Credentials = new NetworkCredential(fromAddress.Address, GlobalSetting.SystemMailPassword);
    //    smtp.EnableSsl = GlobalSetting.SMTPEnableSsl == "Y";

    //    MailMessage message = new MailMessage(fromAddress, toAddress);
    //    message.Subject = info.Subject;
    //    message.Body = info.Body;
    //    message.IsBodyHtml = true;

    //    using (message)
    //    {
    //        smtp.Send(message);
    //    }
    //}
     
}