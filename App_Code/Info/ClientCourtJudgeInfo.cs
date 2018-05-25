using System;
public class ClientCourtJudgeInfo
{
	public string ClientID { get; set; }
	public int RowNo { get; set; }
	public string Name { get; set; }
	public string Court { get; set; }
	public string Title { get; set; }
	public DateTime? CreateDate { get; set; }
	public string CreateUser { get; set; }
	public DateTime? LastModifiedDate { get; set; }
	public string LastModifiedUser { get; set; }
	public class FieldName
	{
		public const string ClientID = "ClientID";
		public const string RowNo = "RowNo";
		public const string Name = "Name";
		public const string Court = "Court";
		public const string Title = "Title";
		public const string CreateDate = "CreateDate";
		public const string CreateUser = "CreateUser";
		public const string LastModifiedDate = "LastModifiedDate";
		public const string LastModifiedUser = "LastModifiedUser";
	}
}
