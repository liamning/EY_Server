using System;
public class CreditorAmountInfo
{
	public string ClientID { get; set; }
	public string CreditorID { get; set; }
	public int RowNo { get; set; }
	public string Currency { get; set; }
	public decimal Amount { get; set; }
	public DateTime? CreateDate { get; set; }
	public string CreateUser { get; set; }
	public DateTime? LastModifiedDate { get; set; }
	public string LastModifiedUser { get; set; }
	public class FieldName
	{
		public const string ClientID = "ClientID";
		public const string CreditorID = "CreditorID";
		public const string RowNo = "RowNo";
		public const string Currency = "Currency";
		public const string Amount = "Amount";
		public const string CreateDate = "CreateDate";
		public const string CreateUser = "CreateUser";
		public const string LastModifiedDate = "LastModifiedDate";
		public const string LastModifiedUser = "LastModifiedUser";
	}
}
