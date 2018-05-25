<%@ WebHandler Language="C#" Class="LogoutHandler" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class LogoutHandler : IHttpHandler, IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        HttpResponse Response = context.Response;
        System.Web.SessionState.HttpSessionState Session = context.Session;

        Session.Clear();
        Response.Clear();
        Response.Redirect("~/"); 
      
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}

