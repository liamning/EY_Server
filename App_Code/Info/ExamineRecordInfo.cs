using System;
public class ExamineRecordInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public int RowNo { get; set; }
    public string StaffID { get; set; }
    public DateTime ExamineDate { get; set; }
    public string ExamineType { get; set; }
    public string ExamineContent { get; set; }
    public string StaffName { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedUser { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string RowNo = "RowNo";
        public const string StaffID = "StaffID";
        public const string ExamineDate = "ExamineDate";
        public const string ExamineType = "ExamineType";
        public const string ExamineContent = "ExamineContent";
        public const string StaffName = "StaffName";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string LastModifiedDate = "LastModifiedDate";
        public const string LastModifiedUser = "LastModifiedUser";
    }
}