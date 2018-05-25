using System;
public class CurrencyInfo
{
    public string ClientID { get; set; }
    public string CurrencyCode { get; set; }
    public string Currency { get; set; }
    public decimal Rate { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedUser { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string CurrencyCode = "CurrencyCode";
        public const string Currency = "Currency";
        public const string Rate = "Rate";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string LastModifiedDate = "LastModifiedDate";
        public const string LastModifiedUser = "LastModifiedUser";
    }
}