using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Login
/// </summary>
public class Template
{
    public void UpdateTemplate(TemplateInfo info)
    {
        string filePath = HttpContext.Current.Server.MapPath("~") + string.Format(info.Path, info.ClientID, info.VoteMethod);
        string directory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        byte[] bytes = Convert.FromBase64String(info.DataURL);
        File.WriteAllBytes(filePath, bytes); 
    }
}