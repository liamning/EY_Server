using System;
public class AttendanceInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public string CreditorIDNo { get; set; }

    public string CreditAgent { get; set; }
    public string CreditAgentIDNo { get; set; }

    public int EventID { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string EventID = "EventID";
        public const string CreditorID = "CreditorID";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate"; 
    }
}