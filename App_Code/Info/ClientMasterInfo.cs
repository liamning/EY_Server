using System;
using System.Collections.Generic;

public class ClientMasterInfo
{
    public string ClientID { get; set; }
    public string CompanyName { get; set; }
    public string CompanyType { get; set; }
    public string RegPlace { get; set; }
    public string RegNo { get; set; }
    public string TaxCodeNo { get; set; }
    public string OrganizationCode { get; set; }
    public string SocialUnifiedCreditCode { get; set; }
    public string ExternalDebtNo { get; set; }
    public string RegCapitalCurrency { get; set; }
    public decimal RegCapital { get; set; }
    public string RegShareCurrency { get; set; }
    public decimal RegShare { get; set; }
    public string NotRegCapitalCurrency { get; set; }
    public decimal NotRegCapital { get; set; }
    public int TotalCreditor { get; set; }
    public string Website { get; set; }
    public string Director { get; set; }
    public string DirectorIdenNo { get; set; }
    public string DirectorPhone { get; set; }
    public string DirectorAddress { get; set; }
    public string GenManager { get; set; }
    public string GenManagerIdenNo { get; set; }
    public string GenManagerPhone { get; set; }
    public string GenManagerAddress { get; set; }
    public string Secretary { get; set; }
    public string SecretaryIdenNo { get; set; }
    public string SecretaryPhone { get; set; }
    public string SecretaryAddress { get; set; }
    public string Supervisor { get; set; }
    public string SupervisorIdenNo { get; set; }
    public string SupervisorPhone { get; set; }
    public string SupervisorAddress { get; set; }
    public string Admin { get; set; }
    public string AdminIdenNo { get; set; }
    public string AdminPhone { get; set; }
    public string AdminAddress { get; set; }
    public string ContactEmail { get; set; }
    public string LegalRepresentative { get; set; }
    public string Relationship { get; set; }
    public string EntrustAgent { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public List<ClientShareholderInfo> ClientShareholderList { get; set; }
    public List<ResponsibleStaffInfo> ResponsibleStaffList { get; set; }
    public List<CurrencyInfo> CurrencyList { get; set; }
    public List<ClientMemberInfo> ClientMemberList { get; set; }
    public List<ClientCourtJudgeInfo> ClientCourtJudgeList { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string CompanyName = "CompanyName";
        public const string CompanyType = "CompanyType";
        public const string RegPlace = "RegPlace";
        public const string RegNo = "RegNo";
        public const string TaxCodeNo = "TaxCodeNo";
        public const string ExternalDebtNo = "ExternalDebtNo";
        public const string RegCapital = "RegCapital";
        public const string RegShare = "RegShare";
        public const string NotRegCapital = "NotRegCapital";
        public const string Director = "Director";
        public const string DirectorIdenNo = "DirectorIdenNo";
        public const string DirectorPhone = "DirectorPhone";
        public const string DirectorAddress = "DirectorAddress";
        public const string GenManager = "GenManager";
        public const string GenManagerIdenNo = "GenManagerIdenNo";
        public const string GenManagerPhone = "GenManagerPhone";
        public const string GenManagerAddress = "GenManagerAddress";
        public const string Secretary = "Secretary";
        public const string SecretaryIdenNo = "SecretaryIdenNo";
        public const string SecretaryPhone = "SecretaryPhone";
        public const string SecretaryAddress = "SecretaryAddress";
        public const string Supervisor = "Supervisor";
        public const string SupervisorIdenNo = "SupervisorIdenNo";
        public const string SupervisorPhone = "SupervisorPhone";
        public const string SupervisorAddress = "SupervisorAddress";
        public const string Admin = "Admin";
        public const string AdminIdenNo = "AdminIdenNo";
        public const string AdminPhone = "AdminPhone";
        public const string AdminAddress = "AdminAddress";
        public const string ContactEmail = "ContactEmail";
        public const string LegalRepresentative = "LegalRepresentative";
        public const string Relationship = "Relationship";
        public const string EntrustAgent = "EntrustAgent";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}