using System;
public class HomeInfo
{
    public string ClientID { get; set; }
    public string CompanyName { get; set; }
    public string CompanyType { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string CompanyName = "CompanyName";
        public const string CompanyType = "CompanyType";
    }
}