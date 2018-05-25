using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
//using System.DirectoryServices;
using System.Web.Hosting;
using System.Web.SessionState;

/// <summary>
/// Summary description for AccessControlModule
/// </summary>
public class AccessControlModule : IHttpModule, IRequiresSessionState
{

    public void Dispose()
    {

    }

    public void Init(HttpApplication context)
    {
        context.AcquireRequestState += new EventHandler(context_AcquireRequestState);
    }


    void context_AcquireRequestState(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;
        HttpResponse response = app.Context.Response;

        string path = app.Context.Request.RawUrl.ToLower();
        if (path.Contains(GlobalSetting.LoginHandler.ToLower())) return;
        if (path.Contains(GlobalSetting.LoginPage.ToLower())) return;
        if (path.Contains(GlobalSetting.OnlineVotePage.ToLower())) return;
        if (path.Contains(GlobalSetting.OnlineVoteHandler.ToLower())) return;
        if (path.Contains(GlobalSetting.Navigation.ToLower())) return;
        if (path == "/") return;


        //access control
        if (app.Session[GlobalSetting.SessionKey.LoginID] != null)
        {
            //string functionName = app.Context.Request.Path;

            //if (functionName.ToUpper().Contains("HTTPHANDLER") || AccessControl.HasAccessRight(functionName, app.Session))
            //{
            //    return;
            //}
            return;
        }



        //Invalid request
        response.Clear();
        response.StatusCode = 403;
        response.End();

    }


}
