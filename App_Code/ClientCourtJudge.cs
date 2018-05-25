using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class ClientCourtJudge
{
	#region Standar Function
    SqlConnection db; 
	SqlTransaction transaction;
	bool isSubTable = false;
	
    public ClientCourtJudge()
    { 
		this.db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString); 
    }
    public ClientCourtJudge(SqlConnection db, SqlTransaction transaction)
    { 
		this.isSubTable = true;
		this.db = db;
		this.transaction = transaction;
    }

    public List<string> GetClientIDList(string ClientID)
    { 
        db.Open();
        String query = "select top 10 ClientID from ClientCourtJudge where (@ClientID = '' or ClientID like '%' + @ClientID + '%') order by ClientID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = ClientID });
        db.Close();
        return obj;
    }


    public bool IsExisted(ClientCourtJudgeInfo info)
    {
		if(!this.isSubTable)
			db.Open();
		
        String query = "select count(*)  from ClientCourtJudge "
        + " where ClientID = @ClientID and RowNo = @RowNo";
        var obj = (List<int>)db.Query<int>(query, info, this.transaction);
        
		
		if(!this.isSubTable)
			db.Close();
		
        return obj[0] > 0;
    }

    public void Save(ClientCourtJudgeInfo info)
    {
        if(this.IsExisted(info))
            this.Update(info);
        else
            this.Insert(info); 
    }


    public List<ClientCourtJudgeInfo> GetCourtJudgeList(string ClientID)
    {
		db.Open();

        string query = "select * from ClientCourtJudge " 
		+ " where ClientID = @ClientID ";
		
        var obj = (List<ClientCourtJudgeInfo>)db.Query<ClientCourtJudgeInfo>(query, new {  ClientID = ClientID  });
        db.Close();

        return obj;
    }

    public void Delete(string ClientID)
    {
		if(!this.isSubTable)
			db.Open();

        string query = "delete  from ClientCourtJudge " 
		+ " where ClientID = @ClientID ";
		
        db.Execute(query, new {  ClientID = ClientID  }, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }
	
    public void Update(ClientCourtJudgeInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = " UPDATE [dbo].[ClientCourtJudge] SET  "
		+ " [RowNo] = @RowNo " 
		+ ", [Name] = @Name " 
		+ ", [Court] = @Court " 
		+ ", [Title] = @Title " 
		+ ", [CreateDate] = @CreateDate " 
		+ ", [CreateUser] = @CreateUser " 
		+ ", [LastModifiedDate] = @LastModifiedDate " 
		+ ", [LastModifiedUser] = @LastModifiedUser "
        + " where ClientID = @ClientID and RowNo= @RowNo";

         
        db.Execute(query, info, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }

    public void Insert(ClientCourtJudgeInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = "INSERT INTO [dbo].[ClientCourtJudge] ( [ClientID] " 
		+ ",[RowNo] " 
		+ ",[Name] " 
		+ ",[Court] " 
		+ ",[Title] " 
		+ ",[CreateDate] " 
		+ ",[CreateUser] " 
		+ ",[LastModifiedDate] " 
		+ ",[LastModifiedUser] " 
		+") "
		+ "VALUES ( @ClientID "
		+ ",@RowNo " 
		+ ",@Name " 
		+ ",@Court " 
		+ ",@Title " 
		+ ",@CreateDate " 
		+ ",@CreateUser " 
		+ ",@LastModifiedDate " 
		+ ",@LastModifiedUser " 
		+") ";


        db.Execute(query, info, this.transaction);
		
		if(!this.isSubTable)
			db.Close();
    }
	#endregion 


    
    public void DeleteClientCourtJudgeNotInTheList(List<int> rowNoList, string clientID)
    {
        string query = "Delete from ClientCourtJudge where ClientID = @ClientID and RowNo not in @RowNoList ";
        db.Execute(query, new { ClientID = clientID, RowNoList = rowNoList }, transaction);
    }

}