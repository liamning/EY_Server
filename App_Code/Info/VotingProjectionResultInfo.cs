using System;
public class VotingProjectionResultInfo
{
    public string ClientID { get; set; }
    public string CompanyName { get; set; }
    public int EventID { get; set; }
    public string EventDescription { get; set; }
    public int QuestionID { get; set; }
    public string Question { get; set; }
    public int? AgressNo { get; set; }
    public decimal? AgressPct { get; set; }
    public decimal? AgreeAmt { get; set; }
    public decimal? AgreeAmtPct { get; set; }
    public int? DisagressNo { get; set; }
    public decimal? DisagressPct { get; set; }
    public decimal? DisagreeAmt { get; set; }
    public decimal? DisagreeAmtPct { get; set; }
    public int? AbstentionNo { get; set; }
    public decimal? AbstentionPct { get; set; }
    public decimal? AbstentionAmt { get; set; }
    public decimal? AbstentionAmtPct { get; set; }
    public int? TotalNo { get; set; }
    public decimal TotalAmt { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string CompanyName = "CompanyName";
        public const string EventID = "EventID";
        public const string EventDescription = "EventDescription";
        public const string QuestionID = "QuestionID";
        public const string Question = "Question";
        public const string AgressNo = "AgressNo";
        public const string AgressPct = "AgressPct";
        public const string AgreeAmt = "AgreeAmt";
        public const string AgreeAmtPct = "AgreeAmtPct";
        public const string DisagressNo = "DisagressNo";
        public const string DisagressPct = "DisagressPct";
        public const string DisagreeAmt = "DisagreeAmt";
        public const string DisagreeAmtPct = "DisagreeAmtPct";
        public const string TotalNo = "TotalNo";
        public const string TotalAmt = "TotalAmt";
    }
}