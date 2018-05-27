using System;
public class ResponsibleStaffInfo
{
    public string ClientID { get; set; }
    public string StaffID { get; set; }
    public string StaffName { get; set; }
    public string ResponsibilityType { get; set; }
    public string ResponsibilityTypeDesc { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedUser { get; set; }

    public class FieldName {
        public const string ClientID = "ClientID";
        public const string StaffID = "StaffID";
        public const string StaffName = "StaffName";
        public const string ResponsibilityType = "ResponsibilityType";
        public const string CreateUser = "CreateUser";
        public const string CreateDate = "CreateDate";
        public const string LastModifiedUser = "LastModifiedUser";
        public const string LastModifiedDate = "LastModifiedDate";
    }
}

public class ResponsibleStaffSelectionInfo
{
    public string StaffID { get; set; }
    public string StaffName { get; set; }
    public string ResponsibilityType { get; set; }
}