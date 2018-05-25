using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class AccessControl
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);

    public AccessControlInfo HasRight(string FunctionName, int Role)
    {
        db.Open();

        string query = "select * from AccessControl "
        + " where FunctionName = @FunctionName  and Role = @Role ";

        var obj = (List<AccessControlInfo>)db.Query<AccessControlInfo>(query, new { FunctionName = FunctionName, Role = Role });
        db.Close();

        if (obj.Count > 0)
            return obj[0];
        else
            return null;
    }

    public AccessControlInfo Get(string FunctionName, int Role)
    {
        db.Open();

        string query = "select * from AccessControl "
        + " where FunctionName = @FunctionName  and Role = @Role ";

        var obj = (List<AccessControlInfo>)db.Query<AccessControlInfo>(query, new { FunctionName = FunctionName, Role = Role });
        db.Close();

        if (obj.Count > 0)
            return obj[0];
        else
            return null;
    }

    public void Delete(string FunctionName , int Role)
    {
		db.Open();

        string query = "delete  from AccessControl " 
		+ " where FunctionName = @FunctionName  and Role = @Role ";
		
        db.Execute(query, new {  FunctionName = FunctionName , Role = Role  });
        db.Close();
    }
	
    public void Update(AccessControlInfo info)
    {
        db.Open();

        string query = " UPDATE [dbo].[AccessControl] SET  "
		+ " [ID] = @ID " 
		+ ", [Enable] = @Enable " 
		+ " where FunctionName = @FunctionName  and Role = @Role ";

         
        db.Execute(query, info);
        db.Close();
    }

    public void Insert(AccessControlInfo info)
    {
        db.Open();

        string query = "INSERT INTO [dbo].[AccessControl] ( [ID] " 
		+ ",[FunctionName] " 
		+ ",[Role] " 
		+ ",[Enable] " 
		+") "
		+ "VALUES ( [ID] "
		+ ",@FunctionName " 
		+ ",@Role " 
		+ ",@Enable " 
		+") ";


        db.Execute(query, info);
        db.Close();
    }

}