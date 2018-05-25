using System;
public class VotingReplyListInfo
{
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public string CreditorType { get; set; }
    public string CreditorTypeDesc { get; set; }
    public string ResponsiblePerson { get; set; }
    public decimal? BookAmt { get; set; }
    public decimal? DeclareAmt { get; set; }
    public decimal? AdminExamineNotConfirm { get; set; }
    public decimal? AdminExamineWaitConfirm { get; set; }
    public decimal? AdminExamineConfirm { get; set; }
    public string Attendance { get; set; }
    public string AttendanceDesc { get; set; }
    public string VoteMethod { get; set; }
    public string VoteMethodDesc { get; set; }
    public DateTime? ReplyDate { get; set; }

    public class FieldName {
        public const string CreditorID = "CreditorID";
        public const string CreditorName = "CreditorName";
        public const string CreditorType = "CreditorType";
        public const string ResponsiblePerson = "ResponsiblePerson";
        public const string BookAmt = "BookAmt";
        public const string DeclareAmt = "DeclareAmt";
        public const string AdminExamineNotConfirm = "AdminExamineNotConfirm";
        public const string AdminExamineWaitConfirm = "AdminExamineWaitConfirm";
        public const string AdminExamineConfirm = "AdminExamineConfirm";
        public const string Attendance = "Attendance";
        public const string VoteMethod = "VoteMethod";
        public const string ReplyDate = "ReplyDate";
    }
}