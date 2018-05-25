using System;
public class VotingProjectionInfo
{
    public string ClientID { get; set; }
    public string CompanyName { get; set; }
    public int EventID { get; set; }
    public string EventDescription { get; set; }
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public decimal AdminExamineConfirmSum { get; set; }
    public int QuestionID { get; set; }
    public string Question { get; set; }
    public string AnswerType { get; set; }
    public string CreditType { get; set; }
    public string Decision { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CompanyName = "CompanyName";
        public const string EventID = "EventID";
        public const string EventDescription = "EventDescription";
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
        public const string AdminExamineConfirmSum = "AdminExamineConfirm";
        public const string QuestionID = "QuestionID";
        public const string AnswerType = "AnswerType";
        public const string Decision = "Decision";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}