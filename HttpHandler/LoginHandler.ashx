<%@ WebHandler Language="C#" Class="LoginHandler" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class LoginHandler : HandlerBase
{
    public LoginHandler()
    {
        base.NeedAuthorization = false;
    }
    
    protected override void action(string action)
    {
        string password;
        switch (action)
        {
            case "login":
                UserID = Request["StaffNo"];
                password = Request["Password"];
                StaffInfo staffInfo = new StaffProfile().Login(UserID, password);
                if (staffInfo != null)
                {
                    Session[GlobalSetting.SessionKey.LoginID] = staffInfo.StaffNo;
                    Session[GlobalSetting.SessionKey.LoginRole] = staffInfo.Role;
                    Result = "{\"result\":\"1\", \"role\":" + staffInfo.Role + "}";
                }
                else
                {
                    Result = "{\"result\":\"0\"}";
                }
                break;

            case "logout":
                Session.Clear();
                break;

            case "getLoginID":
                Result = "{\"loginID\":\"" + UserID + "\"}";
                break;
            case "getComboValue":
                string[] categories = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(Request.Form["Categories"]);
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ComboValueMaster().GetValTxt(categories));
                break;

            default:
                break;

        }
    }


}

