using System;

/// <summary>
/// Summary description for StaffInfo
/// </summary>
public class StaffInfo
{

    public string StaffNo { get; set; }
    public string StaffName { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public string LastUpdateUser { get; set; }


    public class FieldName
    {
        public const string ID = "ID";
        public const string Name = "Name";
        public const string Type = "Type";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string UpdateDate = "UpdateDate";
        public const string UpdateUser = "UpdateUser";
    }

}