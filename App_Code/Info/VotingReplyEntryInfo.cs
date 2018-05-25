using System;
using System.Collections.Generic;
public class VotingReplyEntryInfo
{
    public string ClientID { get; set; }
    public int EventID { get; set; }
    public string EventDescription { get; set; }
    public string CreditorID { get; set; }
    public string CreditorIDNo { get; set; }
    public string CreditorName { get; set; }
    public string CreditorType { get; set; }
    public string Attendance { get; set; }
    public string VoteMethod { get; set; }
    public DateTime? ReplyDate { get; set; }
    public string ResponsiblePerson { get; set; }


    public string EMSStatus { get; set; }
    public string EMSTrackingNo { get; set; }


    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public List<CreditorAuditDetailInfo> CreditorAuditDetailList { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string EventID = "EventID";
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
        public const string CreditorType = "CreditorType";
        public const string Attendance = "Attendance";
        public const string VoteMethod = "VoteMethod";
        public const string ReplyDate = "ReplyDate";
        public const string ResponsiblePerson = "ResponsiblePerson";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}