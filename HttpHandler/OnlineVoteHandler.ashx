<%@ WebHandler Language="C#" Class="OnlineVoteHandler" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class OnlineVoteHandler : HandlerBase
{
    public OnlineVoteHandler()
    {
        base.NeedAuthorization = false;
    }
    
    protected override void action(string action)
    {
        switch (action)
        {
            case "getOnlineVotingInfo":
                int ID = int.Parse(Request.Form["ID"]);
                string Token = Request.Form["Token"];

                OnlineVotePage OnlineVotePage = new OnlineVotePage();

                string[] keyFields = OnlineVotePage.GetOnlineVotingKeyFields(ID, Token);
                if (keyFields == null)
                {
                    Response.StatusCode = 404;
                    break;
                }

                string ClientID = keyFields[0];
                string CreditorID = keyFields[1];
                string EventIDStr = keyFields[2];
                string CreditType = keyFields[3];

                VotingEventSetupMaster eventSetup = new VotingEventSetupMaster();
                ClientID = keyFields[0];

                //var eventInfo = eventSetup.Get(ClientID, EventID)
                var OnlineVoteQuestionList = OnlineVotePage.Get(ClientID, CreditorID, Int32.Parse(EventIDStr), CreditType);
                
                //var CreditorName = OnlineVotePage.GetCreditorVotingInfo(ClientID, keyFields[1]);
                
                var votingInfoList = OnlineVotePage.GetCreditorVotingInfo(ClientID, CreditorID, CreditType);
                var CreditorName = "";
                var CreditTypeDesc = "";
                if (votingInfoList.Count > 0)
                {
                    CreditorName = votingInfoList[0][0];
                    CreditTypeDesc = votingInfoList[0][2];
                }

                var EventDescription = new VotingProjection().GetDescription(ClientID, Int32.Parse(EventIDStr));
                var IsOutOfTimeRange = OnlineVotePage.IsOutOfTimeRange(ClientID, Int32.Parse(EventIDStr));
                var IsExisted = OnlineVotePage.IsExisted(ClientID, keyFields[1], Int32.Parse(EventIDStr), CreditType);

                Result = "{\"OnlineVoteQuestionList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(OnlineVoteQuestionList) + ", "
                       + "\"ClientID\": \"" + ClientID + "\", "
                       + "\"CreditorID\": \"" + keyFields[1] + "\", "
                       + "\"EventID\": \"" + keyFields[2] + "\", "
                       + "\"CreditorName\": \"" + CreditorName + "\", "
                       + "\"CreditType\": \"" + CreditType + "\", "
                       + "\"CreditTypeDesc\": \"" + CreditTypeDesc + "\", "
                       + "\"EventDescription\": \"" + EventDescription + " \", "
                       + "\"IsOutOfTimeRange\": \"" + IsOutOfTimeRange + "\", "
                       + "\"IsExisted\": \"" + IsExisted + "\"}";
                break;
                
                
            //case "generateOnlineVotingToken":
            //    int EventID = Int32.Parse(Request.Form["EventID"]);
            //    CreditorID = Request.Form["CreditorID"];
            //    ClientID = Request.Form["ClientID"];


            //    Result = "{\"tokenList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new OnlineVotePage().GenerateOnlineVotingToken(ClientID, CreditorID, EventID)) + "}";

            //    break;
            case "submitVote":
                ClientID = Request.Form["ClientID"];
                CreditType = Request.Form["CreditType"];
                CreditorID = Request.Form["CreditorID"];
                string CreditorIDNo = Request.Form["CreditorIDNo"];
                if (!new CreditorMaster().CheckCreditorIDNo(ClientID, CreditorID, CreditorIDNo))
                {
                    Result = "{\"message\": \"身份证号码不正确.\"}";
                    break;
                }
                string onlineVoteQuestionListStr = Request.Form["OnlineVoteQuestionList"];
                var onlineVoteQuestionList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OnlineVotePageInfo>>(onlineVoteQuestionListStr);
                foreach (OnlineVotePageInfo onlineVotePageInfo in onlineVoteQuestionList)
                {
                    onlineVotePageInfo.CreateUser = UserID;
                    onlineVotePageInfo.CreateDate = DateTime.Now;
                    onlineVotePageInfo.LastModifiedUser = UserID;
                    onlineVotePageInfo.LastModifiedDate = DateTime.Now;
                }
                new OnlineVotePage().SubmitVote(ClientID, Request.Form["CreditorID"], Int32.Parse(Request.Form["EventID"]), CreditType, onlineVoteQuestionList);
                Result = "{\"message\": \"投票成功.\"}";
                break;
            default:
                break;
        }
    }


}

