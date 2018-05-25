using System;
public class CreditorAuditDetailInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public int RowNo { get; set; }
    public string CreditorTypeCode { get; set; }
    public string CreditorType { get; set; }
    public string Currency { get; set; }
    public string CreditTypeDesc { get; set; }
    public decimal? BookAmt { get; set; }
    public decimal? DeclareAmt { get; set; }
    public decimal? AdminExamineNotConfirm { get; set; }
    public decimal? AdminExamineWaitConfirm { get; set; }
    public decimal? AdminExamineConfirm { get; set; }
    public decimal? LawyerExamineNotConfirm { get; set; }
    public decimal? LawyerExamineWaitConfirm { get; set; }
    public decimal? LawyerExamineConfirm { get; set; }
    public string MatchOpinion { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    
    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string RowNo = "RowNo";
        public const string CreditorType = "CreditorType";
        public const string CreditorTypeCode = "CreditorTypeCode";
        public const string CreditTypeDesc = "CreditTypeDesc";
        public const string Currency = "Currency";
        public const string BookAmt = "BookAmt";
        public const string DeclareAmt = "DeclareAmt";
        public const string AdminExamineNotConfirm = "AdminExamineNotConfirm";
        public const string AdminExamineWaitConfirm = "AdminExamineWaitConfirm";
        public const string AdminExamineConfirm = "AdminExamineConfirm";
        public const string LawyerExamineNotConfirm = "LawyerExamineNotConfirm";
        public const string LawyerExamineWaitConfirm = "LawyerExamineWaitConfirm";
        public const string LawyerExamineConfirm = "LawyerExamineConfirm";
        public const string MatchOpinion = "MatchOpinion";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string LastModifiedDate = "LastModifiedDate";
        public const string LastModifiedUser = "LastModifiedUser";
    }

    public class AreaName
    {
        public const string BookAmt = "企业账面金额";
        public const string DeclareAmt = "申报金额";
        public const string Currency = "币种";
        public const string AdminExamineNotConfirm = "管理人不予确认金额";
        public const string AdminExamineWaitConfirm = "管理人暂缓确认金额";
        public const string AdminExamineConfirm = "管理人债权确认金额";
        public const string LawyerExamineNotConfirm = "律师不予确认金额";
        public const string LawyerExamineWaitConfirm = "律师暂缓确认金额";
        public const string LawyerExamineConfirm = "律师债权确认金额";
    }
}