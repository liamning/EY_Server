using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SampleInfo
/// </summary>
public class SampleInfo
{
	public SampleInfo()
	{
		//
		// TODO: Add constructor logic here
		//
        this.StaffList = new List<StaffInfo>();
	}

    public string ClientNo { get; set; }
    public string CompanyName { get; set; }
    public string Manager { get; set; }
    public string ResponsiblePerson { get; set; }
    public string ContactNo { get; set; }
    public string Member { get; set; }
    public string Address { get; set; }
    public string IsOnlineVoting { get; set; }
    public string Email { get; set; }
    public string RelationShip { get; set; }
    public decimal Asset { get; set; }
    public decimal Liability { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateUser { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string UpdateUser { get; set; }

    public List<StaffInfo> StaffList { get; set; }


    public class FieldName
    {
        public const string ClientNo = "ClientNo";
        public const string CompanyName = "CompanyName";
        public const string Manager = "Manager";
        public const string ResponsiblePerson = "ResponsiblePerson";
        public const string ContactNo = "ContactNo";
        public const string Member = "Member";
        public const string Address = "Address";
        public const string IsOnlineVoting = "IsOnlineVoting";
        public const string Email = "Email";
        public const string RelationShip = "RelationShip";
        public const string Asset = "Asset";
        public const string Liability = "Liability";
        public const string CreateDate = "CreateDate";
        public const string CreateUser = "CreateUser";
        public const string UpdateDate = "UpdateDate";
        public const string UpdateUser = "UpdateUser";
    }

}
