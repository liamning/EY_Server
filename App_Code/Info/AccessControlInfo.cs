using System;
public class AccessControlInfo
{
	public int ID { get; set; }
	public string FunctionName { get; set; }
	public int Role { get; set; }
	public bool Enable { get; set; }
	public class FieldName
	{
		public const string ID = "ID";
		public const string FunctionName = "FunctionName";
		public const string Role = "Role";
		public const string Enable = "Enable";
	}
}
