using System;
public class AttendanceListInfo
{
    public string ClientID { get; set; }
    public int EventID { get; set; }
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public decimal Amount { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string EventID = "EventID";
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
    }
}