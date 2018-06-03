using System;
using System.Collections.Generic;

public class CreditorMasterInfo
{
    public int ID { get; set; }
    public string ClientID { get; set; }
    public string Prefix { get; set; }
    public string CreditorID { get; set; }
    public string CreditorType { get; set; }
    public decimal AdminExamineConfirm { get; set; }
    public string CreditTypeDesc { get; set; }
    public string Status { get; set; }
    public string ResponsiblePerson { get; set; }
    public string MainCreditor { get; set; }
    public string CompanyType { get; set; }
    public string CompanyTypeDesc { get; set; }
    public string CreditorName { get; set; }
    public DateTime? ReportDate { get; set; }
    public string CreditorIDNo { get; set; }
    public string LegalRepresentative { get; set; }
    public string LegalRepPhone { get; set; }
    public string CreditAgent { get; set; }
    public string CreditAgentPhone { get; set; }
    public string CreditAgentIDNo { get; set; }
    public string CreditorRelation { get; set; }
    public string CreditorAddress { get; set; }
    public string CreditAgentAddress { get; set; }
    public string BusinessCode { get; set; }
    public string RegPlace { get; set; }
    public string BankName { get; set; }
    public string BankAccount { get; set; }
    public string Currency { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Interest { get; set; }
    public string SecuredClaimItems { get; set; }
    public string SecuredCurrency { get; set; }
    public decimal? SecuredRate { get; set; }
    public decimal? SecuredAmount { get; set; }
    public string Gage { get; set; }
    public DateTime? GageDate { get; set; }
    public decimal? GageValue { get; set; }
    public string JointCreditor { get; set; }
    public decimal? JointCreditorAmt { get; set; }
    public string Party { get; set; }
    public string PartyIDNo { get; set; }
    public string PartyEmail { get; set; }
    public string PartyAddress { get; set; }
    public string PartyZipCode { get; set; }
    public string PartyContact { get; set; }
    public string PartyContactPhone { get; set; }
    public string ThirdCompanyType { get; set; }
    public string ThirdCreditorName { get; set; }
    public DateTime? ThirdReportDate { get; set; }
    public string ThirdCreditorIDNo { get; set; }
    public string ThirdLegalRepresentative { get; set; }
    public string ThirdLegalRepPhone { get; set; }
    public string ThirdCreditAgent { get; set; }
    public string ThirdCreditAgentPhone { get; set; }
    public string ThirdCreditAgentIDNo { get; set; }
    public string ThirdCreditorRelation { get; set; }
    public string ThirdCreditorAddress { get; set; }
    public string ThirdCreditAgentAddress { get; set; }
    public string ThirdBankName { get; set; }
    public string ThirdBankAccount { get; set; }
    public string LegalOpinion { get; set; }
    public EnumImportStatus ImportStatus { get; set; }


    public bool HasSpecialCreditType { get; set; }
    public string SpecialCreditTypeRemarks { get; set; }
    public string WarrantyRemarks { get; set; }


    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedUser { get; set; }

    public List<CreditorAuditDetailInfo> CreditorAuditDetailList { get; set; }
    public List<ExamineRecordInfo> ExamineRecordList { get; set; }
    public List<CreditorAttachmentInfo> CreditorAttachmentList { get; set; }
    public List<ChangeRecordInfo> ChangeRecordList { get; set; }
    public List<CreditorAmountInfo> CreditorAmountList { get; set; }

    public enum EnumImportStatus
    {
        Update,
        InvalidCreditorID,
        New,
        None
    }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string CreditorType = "CreditorType";
        public const string Status = "Status";
        public const string ResponsiblePerson = "ResponsiblePerson";
        public const string MainCreditor = "MainCreditor";
        public const string CompanyType = "CompanyType";
        public const string CreditorName = "CreditorName";
        public const string ReportDate = "ReportDate";
        public const string CreditorIDNo = "CreditorIDNo";
        public const string LegalRepresentative = "LegalRepresentative";
        public const string LegalRepPhone = "LegalRepPhone";
        public const string CreditAgent = "CreditAgent";
        public const string CreditAgentPhone = "CreditAgentPhone";
        public const string CreditAgentIDNo = "CreditAgentIDNo";
        public const string CreditorRelation = "CreditorRelation";
        public const string CreditorAddress = "CreditorAddress";
        public const string CreditAgentAddress = "CreditAgentAddress";
        public const string BusinessCode = "BusinessCode";
        public const string RegPlace = "RegPlace";
        public const string BankName = "BankName";
        public const string BankAccount = "BankAccount";
        public const string Currency = "Currency";
        public const string Rate = "Rate";
        public const string Amount = "Amount";
        public const string Interest = "Interest";
        public const string SecuredClaimItems = "SecuredClaimItems";
        public const string SecuredCurrency = "SecuredCurrency";
        public const string SecuredRate = "SecuredRate";
        public const string SecuredAmount = "SecuredAmount";
        public const string Gage = "Gage";
        public const string GageDate = "GageDate";
        public const string GageValue = "GageValue";
        public const string JointCreditor = "JointCreditor";
        public const string JointCreditorAmt = "JointCreditorAmt";
        public const string Party = "Party";
        public const string PartyIDNo = "PartyIDNo";
        public const string PartyEmail = "PartyEmail";
        public const string PartyAddress = "PartyAddress";
        public const string PartyZipCode = "PartyZipCode";
        public const string PartyContact = "PartyContact";
        public const string PartyContactPhone = "PartyContactPhone";
        public const string ThirdCompanyType = "ThirdCompanyType";
        public const string ThirdCreditorName = "ThirdCreditorName";
        public const string ThirdReportDate = "ThirdReportDate";
        public const string ThirdCreditorIDNo = "ThirdCreditorIDNo";
        public const string ThirdLegalRepresentative = "ThirdLegalRepresentative";
        public const string ThirdLegalRepPhone = "ThirdLegalRepPhone";
        public const string ThirdCreditAgent = "ThirdCreditAgent";
        public const string ThirdCreditAgentPhone = "ThirdCreditAgentPhone";
        public const string ThirdCreditAgentIDNo = "ThirdCreditAgentIDNo";
        public const string ThirdCreditorRelation = "ThirdCreditorRelation";
        public const string ThirdCreditorAddress = "ThirdCreditorAddress";
        public const string ThirdCreditAgentAddress = "ThirdCreditAgentAddress";
        public const string ThirdBankName = "ThirdBankName";
        public const string ThirdBankAccount = "ThirdBankAccount";
        public const string LegalOpinion = "LegalOpinion";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string LastModifiedDate = "LastModifiedDate";
        public const string LastModifiedUser = "LastModifiedUser";
    }
}
