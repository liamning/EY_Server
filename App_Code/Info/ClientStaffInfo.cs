using System;
public class ClientStaffInfo
{
	public string ClientNo { get; set; }
	public string ID { get; set; }
	public string Name { get; set; }
	public string Type { get; set; }
	public DateTime CreateDate { get; set; }
	public string CreateUser { get; set; }
	public DateTime UpdateDate { get; set; }
	public string UpdateUser { get; set; }
	public class FieldName
	{
		public const string ClientNo = "ClientNo";
		public const string ID = "ID";
		public const string Name = "Name";
		public const string Type = "Type";
		public const string CreateDate = "CreateDate";
		public const string CreateUser = "CreateUser";
		public const string UpdateDate = "UpdateDate";
		public const string UpdateUser = "UpdateUser";
	}
}
