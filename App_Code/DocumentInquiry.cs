using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;

public class DocumentInquiry
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    
    public DocumentInquiry()
    {
    }

    public List<DocumentInquiryInfo> Get(string clientID, string creditorID, string attName, string attType, string attRemark)
    {
        db.Open();
        String query = @"
SELECT TOP 100
                     [dbo].[Attachment].ID 
                     ,[dbo].[Attachment].AttName 
                     --,[dbo].[Attachment].AttType 
					 ,ChiDesc AttType 
                     ,[dbo].[Attachment].AttPage 
                     ,[dbo].[Attachment].AttRemark 
                     FROM 
                     [dbo].[CreditorAttachment] 
                     JOIN [dbo].[Attachment] ON [dbo].[CreditorAttachment].AttachmentID = [dbo].[Attachment].ID 
					 join GeneralMaster on Category = 'AttType' and Code = AttType

                     WHERE (@ClientID = '' OR [dbo].[CreditorAttachment].ClientID = @ClientID) 
                     AND (@CreditorID = '' OR [dbo].[CreditorAttachment].CreditorID = @CreditorID) 
                     AND (@AttName = '' OR [dbo].[Attachment].AttName LIKE '%' + @AttName + '%') 
                     AND (@AttType = '' OR [dbo].[Attachment].AttType LIKE '%' + @AttType + '%') 
                     AND (@AttRemark = '' OR [dbo].[Attachment].AttRemark LIKE '%' + @AttRemark + '%') 
";
        List<DocumentInquiryInfo> obj = (List<DocumentInquiryInfo>)db.Query<DocumentInquiryInfo>(query, new { ClientID = clientID, CreditorID = creditorID, AttName = attName, AttType = attType, AttRemark = attRemark });
        return obj;
    }
}