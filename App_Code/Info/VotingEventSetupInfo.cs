using System;
using System.Collections.Generic;
public class VotingEventSetupInfo
{
    public string ClientID { get; set; }
    public int EventID { get; set; }
    public string EventDescription { get; set; }
    public DateTime? EventDate { get; set; }
    public DateTime? EffectDateFrom { get; set; }
    public DateTime? EffectDateTo { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public List<VotingQuestionInfo> VotingQuestionList { get; set; }
    public AttendanceInfo AttendanceInfo { get; set; }
    public List<VotingResultInfo> VotingResultList { get; set; }
    public List<VotingReplyListInfo> VotingReplyList { get; set; }
    public List<AttendanceSummaryInfo> AttendanceSummary { get; set; }
    public List<AttendanceInfo> AttendanceList { get; set; }
    public List<VotingResultDetailInfo> VotingResultDetailList { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string EventID = "EventID";
        public const string EventDescription = "EventDescription";
        public const string EventDate = "EventDate";
        public const string EffectDateFrom = "EffectDateFrom";
        public const string EffectDateTo = "EffectDateTo";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}