<%@ WebHandler Language="C#" Class="AjaxHandler" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class AjaxHandler : HandlerBase
{

    public AjaxHandler()
    {
    }

    protected override void action(string action)
    {
        string ClientID, staffNo, tmpObjString, password, newPassword;
        bool success;

        switch (action)
        {
            case "SearchUnVoteCreditor":


                ClientID = Request.Form["ClientID"];

                int currentEventID = Convert.ToInt16(Request.Form["EventID"]);

                Result = "{\"UnVoteCreditorList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new LiveVotePage().SearchUnVoteCreditor(
                    ClientID, currentEventID)) + "}";


                break;

            case "getLiveVotingInfo":
                ClientID = Request.Form["ClientID"];

                Result = "{\"LiveVoteQuestionList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new LiveVotePage().Get(ClientID, Int32.Parse(Request.Form["EventID"]))) + ", "
                         + "\"EventDescription\": \"" + new VotingProjection().GetDescription(ClientID, Int32.Parse(Request.Form["EventID"])) + "\"}";
                break;
            case "submitVote":
                ClientID = Request.Form["ClientID"];
                string liveVoteQuestionListStr = Request.Form["liveVoteQuestionList"];
                var liveVoteQuestionList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OnlineVotePageInfo>>(liveVoteQuestionListStr);
                foreach (OnlineVotePageInfo liveVotePageInfo in liveVoteQuestionList)
                {
                    liveVotePageInfo.CreateUser = UserID;
                    liveVotePageInfo.CreateDate = DateTime.Now;
                    liveVotePageInfo.LastModifiedUser = UserID;
                    liveVotePageInfo.LastModifiedDate = DateTime.Now;
                }
                new LiveVotePage().SubmitVote(ClientID, Request.Form["CreditorID"], Int32.Parse(Request.Form["EventID"]), Request.Form["CreditType"], liveVoteQuestionList);
                Result = "{\"message\": \"投票成功.\", \"status\": \"1\"}";
                break;

            case "takeAttendance":
                string attendanceInfoStr = Request.Form["AttendanceInfo"];
                var attendanceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AttendanceInfo>(attendanceInfoStr);
                attendanceInfo.CreateUser = UserID;
                attendanceInfo.CreateDate = DateTime.Now;
                string statusCode = new OnlineVotePage().TakeAttendance(attendanceInfo);
                //1 success, //2 duplicated
                Result = "{\"status\":\"" + statusCode + "\"}";
                break;
            case "getSimpleVotingReplyEntry":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingReplyEntry().Get(ClientID, Int32.Parse(Request.Form["EventID"]), Request.Form["CreditorID"]), datetimeConverter);
                break;

            case "saveStaff":
                tmpObjString = Request.Form["StaffInfo"];
                var tmpStaffProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<StaffProfileInfo>(tmpObjString, datetimeConverter);
                tmpStaffProfile.CreateUser = UserID;
                new StaffProfile().Save(tmpStaffProfile);
                Result = "{\"message\":\"档案已被储存.\"}";
                break;
            case "saveClient":
                string clientString = Request.Form["ClientInfo"];
                var clientInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientMasterInfo>(clientString, datetimeConverter);
                clientInfo.CreateUser = UserID;
                clientInfo.CreateDate = DateTime.Now;
                clientInfo.LastModifiedUser = UserID;
                clientInfo.LastModifiedDate = DateTime.Now;
                clientInfo = new ClientMaster().Save(clientInfo);
                Result = "{\"message\": \"档案已被储存.\", \"ClientID\":\"" + clientInfo.ClientID + "\"}";
                break;
            case "saveEmployee":
                string employeeString = Request.Form["EmployeeInfo"];
                var employeeInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientEmployeeMasterInfo>(employeeString, datetimeConverter);
                employeeInfo.CreateUser = UserID;
                employeeInfo.CreateDate = DateTime.Now;
                employeeInfo.LastModifiedUser = UserID;
                employeeInfo.LastModifiedDate = DateTime.Now;
                bool isEmployeeExisted = new Employee().Save(employeeInfo);
                Result = "{\"message\":\"档案已被储存.\", \"isEmployeeExisted\": " + isEmployeeExisted.ToString().ToLower() + "}";
                break;
            case "saveCreditor":
                string creditorString = Request.Form["CreditorInfo"];
                var creditorInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditorMasterInfo>(creditorString, datetimeConverter);
                creditorInfo.CreateUser = UserID;
                creditorInfo.CreateDate = DateTime.Now;
                creditorInfo.LastModifiedUser = UserID;
                creditorInfo.LastModifiedDate = DateTime.Now;
                var creditorObj = new CreditorMaster();
                creditorInfo = creditorObj.Save(creditorInfo);

                creditorInfo = creditorObj.Get(creditorInfo.ClientID, creditorInfo.CreditorID, UserID);

                Result = "{\"message\": \"档案已被储存.\", \"ChangeRecordList\": " +
                    Newtonsoft.Json.JsonConvert.SerializeObject(creditorInfo.ChangeRecordList, datetimeConverter)
                    + ",\"ExamineRecordList\": " +
                    Newtonsoft.Json.JsonConvert.SerializeObject(creditorInfo.ExamineRecordList, datetimeConverter)
                    + " }";
                break;
            case "importCreditor":
                creditorString = Request.Form["CreditorInfo"];
                var fileImportData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportInfo>(creditorString);
                fileImportData.CreateUser = UserID;
                fileImportData.LastModifiedUser = UserID;
                fileImportData.CreateDate = DateTime.Now;
                fileImportData.LastModifiedDate = DateTime.Now;

                string filePath = new CreditorMaster().Import(fileImportData);
                Result = "{\"message\": \"已导入债权人.\", \"fileName\": \"" + HttpContext.Current.Server.HtmlEncode(filePath) + "\"}";
                break;
            case "importCreditorContact":
                creditorString = Request.Form["CreditorInfo"];
                fileImportData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportInfo>(creditorString);
                fileImportData.CreateUser = UserID;
                fileImportData.LastModifiedUser = UserID;
                fileImportData.CreateDate = DateTime.Now;
                fileImportData.LastModifiedDate = DateTime.Now;

                new CreditorMaster().ImportContact(fileImportData);
                //Result = "{\"message\": \"已导入债权人联系资料.\", \"fileName\": \"" + HttpContext.Current.Server.HtmlEncode(filePath) + "\"}";
                Result = "{\"message\": \"已导入债权人联系资料.\"}";
                break;
            case "importEmployee":
                var EmployeeInfo = Request.Form["EmployeeInfo"];
                fileImportData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportInfo>(EmployeeInfo);
                fileImportData.CreateUser = UserID;
                fileImportData.LastModifiedUser = UserID;
                fileImportData.CreateDate = DateTime.Now;
                fileImportData.LastModifiedDate = DateTime.Now;

                new Employee().Import(fileImportData);
                Result = "{\"message\": \"已导入员工.\"}";
                break;
            case "updateTemplate":
                string TemplateInfoStr = Request.Form["TemplateInfo"];
                TemplateInfo templateInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplateInfo>(TemplateInfoStr);
                new Template().UpdateTemplate(templateInfo);
                Result = "{\"result\": true ,\"message\": \"档案已被储存.\" }";
                break;
            case "saveVotingEventSetup":
                string votingEventSetupInfoString = Request.Form["VotingEventSetupInfo"];
                var votingEventSetupInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VotingEventSetupInfo>(votingEventSetupInfoString, datetimeConverter);
                votingEventSetupInfo.CreateUser = UserID;
                votingEventSetupInfo.CreateDate = DateTime.Now;
                votingEventSetupInfo.LastModifiedUser = UserID;
                votingEventSetupInfo.LastModifiedDate = DateTime.Now;

                var votingEventSetupMaster = new VotingEventSetupMaster();
                votingEventSetupMaster.Save(votingEventSetupInfo);


                Result = "{\"message\": \"档案已被储存.\", \"info\": " +
                     Newtonsoft.Json.JsonConvert.SerializeObject(votingEventSetupMaster.Get(votingEventSetupInfo.ClientID, votingEventSetupInfo.EventID), datetimeConverter)
                    + "}";
                break;
            case "saveVotingReplyEntry":
                string votingReplyEntryInfoString = Request.Form["VotingReplyEntryInfo"];
                var votingReplyEntryInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VotingReplyEntryInfo>(votingReplyEntryInfoString, datetimeConverter);
                votingReplyEntryInfo.CreateUser = UserID;
                votingReplyEntryInfo.CreateDate = DateTime.Now;
                votingReplyEntryInfo.LastModifiedUser = UserID;
                votingReplyEntryInfo.LastModifiedDate = DateTime.Now;
                new VotingReplyEntry().Save(votingReplyEntryInfo);
                Result = "{\"message\": \"档案已被储存.\"}";
                break;
            case "saveVotingProjection":
                string creditType = Request.Form["creditType"];
                string votingProjectionString = Request.Form["VotingProjectionList"];
                var votingProjectionList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VotingProjectionInfo>>(votingProjectionString);
                foreach (VotingProjectionInfo votingProjectionInfo in votingProjectionList)
                {
                    votingProjectionInfo.CreditType = creditType;
                    votingProjectionInfo.CreateUser = UserID;
                    votingProjectionInfo.CreateDate = DateTime.Now;
                    votingProjectionInfo.LastModifiedUser = UserID;
                    votingProjectionInfo.LastModifiedDate = DateTime.Now;
                }
                new VotingProjection().Save(votingProjectionList);
                Result = "{\"message\": \"档案已被储存.\"}";
                break;
            case "saveCurrency":
                string currencyListStr = Request.Form["currencyList"];
                List<CurrencyMasterInfo> currencyList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CurrencyMasterInfo>>(currencyListStr);
                foreach (CurrencyMasterInfo currency in currencyList)
                {
                    currency.CreateUser = UserID;
                    currency.CreateDate = DateTime.Now;
                    currency.LastModifiedUser = UserID;
                    currency.LastModifiedDate = DateTime.Now;
                }
                new CurrencyMaster().Save(currencyList);
                Result = "{\"message\": \"档案已被储存.\"}";
                break;
            case "saveComboValue":
                string comboValueListStr = Request.Form["ComboValueList"];
                var comboValueList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComboValueMasterInfo>>(comboValueListStr);
                foreach (ComboValueMasterInfo comboValue in comboValueList)
                {
                    comboValue.CreateUser = UserID;
                    comboValue.CreateDate = DateTime.Now;
                    comboValue.LastModifiedUser = UserID;
                    comboValue.LastModifiedDate = DateTime.Now;
                }
                new ComboValueMaster().Save(Request.Form["Category"], Request.Form["CategoryDesc"], comboValueList);
                Result = "{\"message\": \"档案已被储存.\"}";
                break;
            // Get Selection List
            case "getStaffIDList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ClientMaster().GetStaffIDList(Request.Form["StaffID"], Request.Form["ClientID"]));
                break;
            case "getStaffNoList":
                staffNo = Request.Form["StaffNo"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new StaffProfile().GetStaffNoList(staffNo));
                break;
            case "getClientIDList":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ClientMaster().GetClientIDList(ClientID, UserID, Role));
                break;
            case "GetClientStaffIDList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new Employee().GetClientStaffIDList(Request.Form["ClientID"], Request.Form["ClientStaffID"]));
                break;
            case "getCreditorIDList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CreditorMaster().GetCreditorIDList(Request.Form["ClientID"], Request.Form["CreditorID"], ""));
                break;
            case "getCreditorList":
                var creditorMaster = new CreditorMaster();
                var comboValueMaster = new ComboValueMaster();
                Result = "{\"CreditorList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(creditorMaster.GetCreditorList(Request.Form["ClientID"], UserID)) + ", "
                       + "\"CreditType\": " + Newtonsoft.Json.JsonConvert.SerializeObject(comboValueMaster.Get("CreditType")) + ", "
                       + "\"Status\": " + Newtonsoft.Json.JsonConvert.SerializeObject(comboValueMaster.Get("Status")) + ", "
                       + "\"LoginID\": \"" + UserID + "\", "
                       + "\"Role\": " + creditorMaster.GetResponsibleRole(Request.Form["ClientID"], UserID) + ", "
                       + "\"ResponsiblePersonList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(creditorMaster.GetResponsiblePersonList(Request.Form["ClientID"])) + "}";
                break;
            case "getCurrencyCodeList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyMaster().GetCurrencyCodeList(Request.Form["CurrencyCode"]));
                break;
            case "getCurrencyCodeListByClient":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyMaster().GetCurrencyCodeListByClient(Request.Form["CurrencyCode"], Request.Form["ClientID"]));
                break;
            case "getLastEventID":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetLastEventID(ClientID));
                break;
            case "getEventIDList":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID, Request.Form["EventID"]));
                break;
            case "getEventDetailsList":
                //ClientID = Request.Form["ClientID"];
                //Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID, Request.Form["EventID"]));


                ClientID = Request.Form["ClientID"];
                string EventID = Request.Form["EventID"];
                var votingEventObj = new VotingEventSetupMaster();
                var eventIDList = votingEventObj.GetEventIDList(ClientID, "");

                creditorMaster = new CreditorMaster();

                if (string.IsNullOrEmpty(EventID) && eventIDList.Count > 0) EventID = eventIDList[eventIDList.Count - 1];

                if (!string.IsNullOrEmpty(EventID))
                    Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(eventIDList)
                           + ",\"VotingEventSetupMaster\": " + Newtonsoft.Json.JsonConvert.SerializeObject(votingEventObj.Get(ClientID, Convert.ToInt32(EventID)), datetimeConverter)
                           + ",\"ResponsibilityType\": " + creditorMaster.GetResponsibleRole(ClientID, UserID)
                           + "}";
                else
                    Result = "{\"EventIDList\": [] "
                         + ",\"ResponsibilityType\": " + creditorMaster.GetResponsibleRole(ClientID, UserID)
                         + ",\"VotingEventSetupMaster\": {} }";
                break;


            case "getEventIDAndCreditorIDList":
                ClientID = Request.Form["ClientID"];
                creditorMaster = new CreditorMaster();

                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID, Request.Form["EventID"]))
                       + ",\"CreditorIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new CreditorMaster().GetCreditorIDList(Request.Form["ClientID"], Request.Form["CreditorID"], ""))
                       + ",\"ResponsibilityType\": " + Newtonsoft.Json.JsonConvert.SerializeObject(creditorMaster.GetResponsibleRole(ClientID, UserID)) + "}";
                break;
            case "getEventIDAndCompanyName":
                ClientID = Request.Form["ClientID"];
                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID, Request.Form["EventID"])) + ", "
                       + "\"CompanyName\": \"" + new VotingProjection().GetCompanyName(ClientID) + "\"}";
                break;
            case "getHomeClientList":

                Result = "{\"ClientList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new Home().Get(UserID, Role)) + ", "
                       + "\"Role\": " + Role + "}";
                break;
            case "searchClient":
                var Criteria = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["Criteria"]);
                var FieldsDataType = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["FieldsDataType"]);
                var FieldsOperator = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["FieldsOperator"]);

                Result = "{\"ClientList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new ClientMaster().Search(UserID, Role, Criteria, FieldsDataType, FieldsOperator)) + ", "
                       + "\"Role\": " + Role + "}";
                break;
            case "searchCreditor":
                Criteria = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["Criteria"]);
                FieldsDataType = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["FieldsDataType"]);
                FieldsOperator = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Request.Form["FieldsOperator"]);

                Result = "{\"ClientList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new CreditorMaster().Search(Request.Form["ClientID"], UserID, Criteria, FieldsDataType, FieldsOperator)) + ", "
                       + "\"Role\": " + Role + "}";
                break;
            case "getCurrencyList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyMaster().GetCurrencyList());
                break;
            case "getComboValuelCategoryList":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ComboValueMaster().GetCategoryList());
                break;
            case "getComboValue":
                string[] categories = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(Request.Form["Categories"]);
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ComboValueMaster().GetValTxt(categories));
                break;
            // Get Detail
            case "getCurrency":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyMaster().GetCurrencyDetail(Request.Form["CurrencyCode"]));
                break;
            case "getClientCurrency":
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyMaster().GetClientCurrency(Request.Form["ClientID"], Request.Form["CurrencyCode"]));
                break;
            case "getStaffName":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new ClientMaster().GetStaffName(Request.Form["StaffID"], ClientID));
                break;
            case "getCreditorName":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new OnlineVotePage().GetCreditorName(ClientID, Request.Form["CreditorID"]));
                break;
            case "getCreditorVotingDetails":
                ClientID = Request.Form["ClientID"];
                string CreditorID = Request.Form["CreditorID"];
                EventID = Request.Form["EventID"];
                string CreditorName = string.Empty;
                var creditorVotingInfo = new OnlineVotePage().GetCreditorVotingInfo(ClientID, CreditorID);
                if (creditorVotingInfo.Count > 0)
                {
                    CreditorName = creditorVotingInfo[0][0];
                }

                Result = "{\"message\":\"已投票\", \"VotingDetails\":" + Newtonsoft.Json.JsonConvert.SerializeObject(new LiveVotePage().GetCreditorVotingDetails(ClientID, CreditorID, EventID, Request.Form["CreditType"]))
                        + ",\"CreditorName\":\"" + CreditorName + "\" "
                        + ",\"CreditorVotingInfo\":" + Newtonsoft.Json.JsonConvert.SerializeObject(creditorVotingInfo) + "}";
                break;
            // Get            
            case "getSample":
                ClientID = Request.Form["ClientNo"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new Sample().Get(ClientID), datetimeConverter);
                break;
            case "getClientInfo":
                ClientID = Request.Form["ClientID"];
                var clientMaster = new ClientMaster();
                creditorMaster = new CreditorMaster();
                Result = "{\"ClientMasterInfo\":" + Newtonsoft.Json.JsonConvert.SerializeObject(clientMaster.Get(ClientID))
                        + ",\"Role\":" + Role
                        + ",\"ClientShareholderListMaxNo\":" + clientMaster.GetMaxRowNoOfClientShareholderList(ClientID)
                        + ",\"ResponsibilityType\": " + creditorMaster.GetResponsibleRole(ClientID, UserID) + "}";
                break;
            case "getResponsibilityType":
                ClientID = Request.Form["ClientID"];
                creditorMaster = new CreditorMaster();
                Result = "{\"ResponsibilityType\": " + creditorMaster.GetResponsibleRole(ClientID, UserID) + "}";
                break;
            case "getCreditorInfo":
                ClientID = Request.Form["ClientID"];
                CreditorID = Request.Form["CreditorID"];
                creditorMaster = new CreditorMaster();
                var tmpCreditorInfo = creditorMaster.Get(ClientID, CreditorID, UserID);
                if (string.IsNullOrEmpty(CreditorID))
                {
                    tmpCreditorInfo = creditorMaster.Create();
                    tmpCreditorInfo.ClientID = ClientID;
                    //tmpCreditorInfo.CreditorID = CreditorID;
                }
                Result = "{\"CreditorInfo\":" + Newtonsoft.Json.JsonConvert.SerializeObject(tmpCreditorInfo, datetimeConverter) + ","
                        + "\"ExamineRecordMaxRowNo\":" + creditorMaster.GetMaxRowNoOfExamineRecordList(ClientID, CreditorID) + ","
                        + "\"CreditorAttachmentMaxRowNo\":" + creditorMaster.GetMaxRowNoOfCreditorAttachmentList(ClientID, CreditorID) + ","
                       + "\"LoginID\": \"" + UserID + "\", "
                       + "\"Role\": " + creditorMaster.GetResponsibleRole(ClientID, UserID) + ", "
                       + "\"ResponsiblePersonList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(creditorMaster.GetResponsiblePersonList(ClientID)) + "}";
                break;
            case "getResponsiblePersonList":
                ClientID = Request.Form["ClientID"];
                creditorMaster = new CreditorMaster();
                Result = "{ \"ResponsiblePersonList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(creditorMaster.GetResponsiblePersonList(ClientID)) + "}";
                break;
            case "getEmployeeInfo":
                ClientID = Request.Form["ClientID"];

                Result = "{\"EmployeeList\":" + Newtonsoft.Json.JsonConvert.SerializeObject(new Employee().Get(ClientID), datetimeConverter) + ","
                        + "\"ResponsibilityType\": " + new CreditorMaster().GetResponsibleRole(ClientID, UserID) + "}";

                break;
            case "getVotingEventSetupInfo":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().Get(ClientID, Int32.Parse(Request.Form["EventID"])), datetimeConverter);
                break;
            case "GetVotingResultDetailList":
                ClientID = Request.Form["ClientID"];
                creditType = Request.Form["CreditType"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetVotingResultDetailList(ClientID, Int32.Parse(Request.Form["EventID"]), creditType), datetimeConverter);
                break;
            case "getVotingQuestionList":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetVotingQuestionList(ClientID, Int32.Parse(Request.Form["EventID"])));
                break;
            case "GetVotingResultList":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetVotingResultList(ClientID, Int32.Parse(Request.Form["EventID"])), datetimeConverter);
                break;
            case "getStaff":
                staffNo = Request.Form["staffNo"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new StaffProfile().Get(staffNo), datetimeConverter);
                break;
            case "getVotingReplyEntryInfo":
                ClientID = Request.Form["ClientID"];
                Result = "{\"VotingReplyEntryInfo\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingReplyEntry().Get(ClientID, Int32.Parse(Request.Form["EventID"]), Request.Form["CreditorID"]), datetimeConverter) + ", "
                       + "\"CreditorAuditDetailList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingReplyEntry().GetCreditorAuditDetailList(ClientID, Request.Form["CreditorID"])) + ", "
                       + "\"CreditorInfo\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingReplyEntry().GetCreditorInfo(ClientID, Request.Form["CreditorID"])) + "}";
                break;
            case "getVotingProjectionInfo":
                creditType = Request.Form["CreditType"];
                if (string.IsNullOrEmpty(creditType))
                    creditType = new GeneralMaster().GetGeneralMasterList(new string[] { "CreditType" })["CreditType"][0].Code;
                ClientID = Request.Form["ClientID"];
                Result = "{\"VotingProjectionList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingProjection().Get(ClientID, Int32.Parse(Request.Form["EventID"]), creditType)) + ", "
                       + "\"EventDescription\": \"" + new VotingProjection().GetDescription(ClientID, Int32.Parse(Request.Form["EventID"])) + "\"}";
                break;
            case "getVotingProjectionResult":
                ClientID = Request.Form["ClientID"];
                Result = "{\"VotingProjectionResultList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingProjectionResult().Get(ClientID, Int32.Parse(Request.Form["EventID"]))) + ", "
                       + "\"EventDescription\": \"" + new VotingProjection().GetDescription(ClientID, Int32.Parse(Request.Form["EventID"])) + "\"}";
                break;
            case "getEventIDListAndVotingCreditorGenerationList":
                ClientID = Request.Form["ClientID"];
                int EventIDint = 0;
                eventIDList = new VotingEventSetupMaster().GetEventIDList(ClientID);
                if (!string.IsNullOrEmpty(Request.Form["EventID"]))
                    EventIDint = Convert.ToInt32(Request.Form["EventID"]);
                else if (eventIDList.Count > 0)
                {
                    EventIDint = Convert.ToInt32(eventIDList[0]);
                }

                var VoteMethod = Request.Form["VoteMethod"];
                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(eventIDList) + ", "
                       + "\"VotingCreditorGenerationList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingCreditorGeneration().Get(ClientID, EventIDint, VoteMethod)) + "}";
                break;
            case "getEventIDListAndVotingAcknowledgementGenerationList":
                ClientID = Request.Form["ClientID"];
                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID)) + ", "
                       + "\"VotingCreditorGenerationList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingCreditorGeneration().GetAcknowledgement(ClientID)) + "}";
                break;
            case "getEventIDListAndLiveVotingFormList":
                ClientID = Request.Form["ClientID"];
                EventIDint = Convert.ToInt32(Request.Form["EventID"]);
                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID)) + ", "
                       + "\"VotingCreditorGenerationList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingCreditorGeneration().GetLiveVotingForm(ClientID, EventIDint)) + "}";
                break;

            case "getEventIDListAndVotingCreditorGenerationList2":
                int draw = Convert.ToInt32(Request.Form["draw"]);
                int start = Convert.ToInt32(Request.Form["start"]);
                List<Dictionary<string, string>> criteria = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(Request.Form["Criteria"]);
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new VotingCreditorGeneration().GetDataTable(criteria, start, draw));
                break;
            case "getEventIDListAndVotingFormGenerationList":
                ClientID = Request.Form["ClientID"];
                Result = "{\"EventIDList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingEventSetupMaster().GetEventIDList(ClientID, Request.Form["EventID"])) + ", "
                       + "\"VotingFormGenerationList\": " + Newtonsoft.Json.JsonConvert.SerializeObject(new VotingCreditorGeneration().Get(ClientID)) + "}";
                break;
            case "getDocumentInquiryList":
                ClientID = Request.Form["ClientID"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new DocumentInquiry().Get(ClientID, Request.Form["CreditorID"], Request.Form["AttName"], Request.Form["AttType"], Request.Form["AttRemark"]));
                break;
            // Delete
            case "deleteSample":
                ClientID = Request.Form["ClientNo"];
                List<string> clientNoList = new List<string>();
                clientNoList.Add(ClientID);
                new Sample().Delete(clientNoList);
                Result = "{\"message\":\"Done.\"}";
                break;
            case "deleteClient":
                ClientID = Request.Form["ClientID"];
                new ClientMaster().Delete(ClientID);
                Result = "{\"message\": \"档案已被删除.\"}";
                break;
            case "deleteCreditor":
                ClientID = Request.Form["ClientID"];
                CreditorID = Request.Form["CreditorID"];
                new CreditorMaster().Delete(ClientID, CreditorID);
                Result = "{\"message\": \"档案已被删除.\"}";
                break;
            case "deleteStaff":
                staffNo = Request.Form["staffNo"];
                new StaffProfile().Delete(staffNo);
                Result = "{\"message\":\"档案已被删除.\"}";
                break;
            case "deleteEmployee":
                new Employee().Delete(Request.Form["ClientID"], Request.Form["ClientStaffID"]);
                Result = "{\"message\": \"档案已被删除.\"}";
                break;

            // Others
            case "resetPassword":
                password = Request.Form["Password"];
                newPassword = Request.Form["NewPassword"];
                success = new StaffProfile().ResetPassword(UserID, password, newPassword);

                if (success)
                    Result = "{\"message\":\"密码已被更改.\", \"result\":\"1\"}";
                else
                    Result = "{\"message\":\"密码更改失败.\", \"result\":\"0\"}";
                break;


            case "getGeneralMasterList":
                string[] masterNames = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(Request["categories"]);
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new GeneralMaster().GetGeneralMasterList(masterNames));
                break;
            case "refreshList":
                string table = Request["Table"];
                string input = Request["Input"];
                Result = Newtonsoft.Json.JsonConvert.SerializeObject(new GeneralMaster().RefreshTableList(table, input), new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = GlobalSetting.DateTimeFormat });
                break;
            default:
                break;
        }
    }


}

