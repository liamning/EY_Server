using System;
public class DocumentInquiryInfo
{
    public int ID { get; set; }
    public string AttName { get; set; }
    public string AttType { get; set; }
    public string AttPage { get; set; }
    public string AttRemark { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string ID = "ID";
        public const string AttName = "AttName";
        public const string AttType = "AttType";
        public const string AttPage = "AttPage";
        public const string AttRemark = "AttRemark";
    }
}