using System;
public class ClientMemberInfo
{
	public string ClientID { get; set; }
	public int RowNo { get; set; }
	public string MemberType { get; set; }
	public string Name { get; set; }
	public string Nationality { get; set; }
	public string IdentityNo { get; set; }
	public string ContactNo { get; set; }
	public string Address { get; set; }
	public DateTime? CreateDate { get; set; }
	public string CreateUser { get; set; }
	public DateTime? LastModifiedDate { get; set; }
	public string LastModifiedUser { get; set; }
	public class FieldName
	{
		public const string ClientID = "ClientID";
		public const string RowNo = "RowNo";
		public const string MemberType = "MemberType";
		public const string Name = "Name";
		public const string Nationality = "Nationality";
		public const string IdentityNo = "IdentityNo";
		public const string ContactNo = "ContactNo";
		public const string Address = "Address";
		public const string CreateDate = "CreateDate";
		public const string CreateUser = "CreateUser";
		public const string LastModifiedDate = "LastModifiedDate";
		public const string LastModifiedUser = "LastModifiedUser";
	}
}
