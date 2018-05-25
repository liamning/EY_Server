using System;
public class VotingResultDetailInfo
{
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public int QuestionID { get; set; }
    public string Decision { get; set; }
    public string decisionDesc { get; set; }

    public class FieldName {
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
        public const string QuestionID = "QuestionID";
        public const string Decision = "Decision";
    }
}