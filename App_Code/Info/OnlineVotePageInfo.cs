﻿using System;
public class OnlineVotePageInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public int EventID { get; set; }
    public string CreditType { get; set; }
    public int QuestionID { get; set; }
    public string Question { get; set; }
    public string AnswerType { get; set; }
    public string Decision { get; set; }
    public string CreateUser { get; set; }
    public DateTime? CreateDate { get; set; }
    public string LastModifiedUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string CreditorID = "CreditorID";
        public const string EventID = "EventID";
        public const string QuestionID = "QuestionID";
        public const string Question = "Question";
        public const string AnswerType = "AnswerType";
        public const string Decision = "Decision";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}