using System;
using System.Web;
using System.Web.SessionState;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class HandlerBase : IHttpHandler, IRequiresSessionState
{

    protected string UserID = string.Empty;
    protected string Role = string.Empty;
    protected string Result = string.Empty;
    protected bool NeedAuthorization = true;

    protected HttpRequest Request;
    protected HttpResponse Response;
    protected HttpSessionState Session;

    protected IsoDateTimeConverter dateConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" };
    protected IsoDateTimeConverter datetimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };

    protected virtual void action(string action)
    {
    }

    protected virtual string getAction()
    {
        return Request.Form["action"].ToString();
    }

    protected virtual void SetResponse()
    {
        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8080");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        Response.ContentType = "application/json;charset=UTF-8;";
    }

    public void ProcessRequest(HttpContext context)
    {
        Request = context.Request;
        Response = context.Response;
        Session = context.Session;

        this.SetResponse();

        if (Request.Cookies["ASP.NET_SessionId"] != null)
        {
            var value = Request.Cookies["ASP.NET_SessionId"].Value;

            System.Diagnostics.Debug.WriteLine(string.Format("===================={0}====================", value));
        }


        if (Session[GlobalSetting.SessionKey.LoginID] != null)
        {
            UserID = Session[GlobalSetting.SessionKey.LoginID].ToString();
            Role = Session[GlobalSetting.SessionKey.LoginRole].ToString();
        }



//#if DEBUG
//        UserID = "Administrator";
//        Role = "3";
//#endif

        if (NeedAuthorization && string.IsNullOrEmpty(UserID))
        {
            Response.Clear();
            Response.StatusCode = 403;
            Response.End();
        }

        try
        {
            string action = this.getAction();
            this.action(action);
        }
        catch (Exception e)
        {
            Result = "{\"message\":\"" + e.Message.Replace("\r\n", "") + "\"}";
            Log.Error(e.Message);
            Log.Error(e.StackTrace);
        }

        Response.Clear();
        Response.Write(Result);
        Response.End();
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}