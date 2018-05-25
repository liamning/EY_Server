using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for GeneralCodeDesc
/// </summary>
public class GeneralCodeDesc
{
	public GeneralCodeDesc()
	{
		//
		// TODO: Add constructor logic here
		//
	}
    public string Code { get; set; }
    public string Desc { get; set; }

    public class FieldName
    {
        public const string Code = "Code";
        public const string Desc = "Desc";
    }
}