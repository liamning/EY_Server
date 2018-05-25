using System;
using System.Collections.Generic;

public class TemplateInfo
{
    public string ClientID { get; set; }
    public string VoteMethod { get; set; }
    public string Path { get; set; }
    public string DataURL { get; set; }
    public string FileExt { get; set; }

    public class FieldName
    {
        public const string ClientID = "ClientID";
        public const string Path = "Path";
        public const string DataURL = "DataURL";
        public const string FileExt = "FileExt"; 
    }
}
 