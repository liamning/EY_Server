using System;
public class ClientEmployeeMasterInfo
{
    public string ClientID { get; set; }
    public string ClientStaffID { get; set; }
    public decimal ClientStaffYear { get; set; }
    public string ClientStaffName { get; set; }
    public DateTime? ContractDate { get; set; }
    public DateTime? ContractExpDate { get; set; }
    public decimal MonthlyWage { get; set; }
    public decimal AnnualBonus { get; set; }
    public decimal AvgWage { get; set; }
    public decimal NoReleaseWage { get; set; }
    public decimal NoReleaseBonus { get; set; }
    public decimal NoReimbursementAmt { get; set; }
    public decimal Total { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedUser { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string ClientStaffID = "ClientStaffID";
        public const string ClientStaffYear = "ClientStaffYear";
        public const string ClientStaffName = "ClientStaffName";
        public const string ContractDate = "ContractDate";
        public const string ContractExpDate = "ContractExpDate";
        public const string MonthlyWage = "MonthlyWage";
        public const string AnnualBonus = "AnnualBonus";
        public const string AvgWage = "AvgWage";
        public const string NoReleaseWage = "NoReleaseWage";
        public const string NoReleaseBonus = "NoReleaseBonus";
        public const string NoReimbursementAmt = "NoReimbursementAmt";
        public const string Total = "Total";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string LastModifiedDate = "LastModifiedDate";
        public const string LastModifiedUser = "LastModifiedUser";
    }
}