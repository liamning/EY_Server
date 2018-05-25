using System;
public class ChangeRecordInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public string StaffID { get; set; }
    public string StaffName { get; set; }
    public string CreditorType { get; set; }
    public string CreditTypeDesc { get; set; }
    public string Currency { get; set; }
    public string Area { get; set; }
    public string ValueFrom { get; set; }
    public string ValueTo { get; set; }
    public DateTime? UpdateDate { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string StaffID = "StaffID";
        public const string StaffName = "StaffName";
        public const string CreditorType = "CreditorType";
        public const string Area = "Area";
        public const string ValueFrom = "ValueFrom";
        public const string ValueTo = "ValueTo";
        public const string UpdateDate = "UpdateDate";
    }
}