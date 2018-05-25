using System;
public class VotingFormGenerationInfo
{
    public string ClientID { get; set; }
    public int EventID { get; set; }
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public string CreditorType { get; set; }
    public decimal PropertySecurity { get; set; }
    public decimal OrdinaryCreditorRight { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string EventID = "EventID";
        public const string VoteMethod = "VoteMethod";
        public const string DocumentType = "DocumentType";
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
        public const string CreditorType = "CreditorType";
        public const string PropertySecurity = "PropertySecurity";
        public const string OrdinaryCreditorRight = "OrdinaryCreditorRight";
    }
}