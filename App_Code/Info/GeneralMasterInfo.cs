using System;
public class GeneralMasterInfo
{
    public int ID { get; set; }
    public string Category { get; set; }
    public int Seq { get; set; }
    public string Code { get; set; }
    public string EngDesc { get; set; }
    public string ChiDesc { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public class FieldName {
        public const string ID = "ID";
        public const string Category = "Category";
        public const string Seq = "Seq";
        public const string Code = "Code";
        public const string EngDesc = "EngDesc";
        public const string ChiDesc = "ChiDesc";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}