using System;
using System.Web;
public class CreditorAttachmentInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public int RowNo { get; set; }
    public int AttachmentID { get; set; }
    public string AttName { get; set; }
    public string AttType { get; set; }
    public string AttPage { get; set; }
    public string DirectoryPath { get; set; }
    public string FilePath { get; set; }
    public string AttRemark { get; set; }
    public string DataURL { get; set; }
    public string FileExt { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string RowNo = "RowNo";
        public const string AttachmentID = "AttachmentID";
        public const string AttName = "AttName";
        public const string AttType = "AttType";
        public const string AttPage = "AttPage";
        public const string DirectoryPath = "DirectoryPath";
        public const string FilePath = "FilePath";
        public const string AttRemark = "AttRemark";
        public const string DataURL = "DataURL";
        public const string FileExt = "FileExt";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastMofifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}