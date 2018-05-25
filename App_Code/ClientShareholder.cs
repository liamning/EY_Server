using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class ClientShareholder
{
	#region Standar Function
    SqlConnection db; 
	SqlTransaction transaction;
	bool isSubTable = false;
	
    public ClientShareholder()
    { 
		this.db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString); 
    }
    public ClientShareholder(SqlConnection db, SqlTransaction transaction)
    { 
		this.isSubTable = true;
		this.db = db;
		this.transaction = transaction;
    }

    public List<string> GetClientIDList(string ClientID)
    { 
        db.Open();
        String query = "select top 10 ClientID from ClientShareholder where (@ClientID = '' or ClientID like '%' + @ClientID + '%') order by ClientID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = ClientID });
        db.Close();
        return obj;
    }


    public bool IsExisted(ClientShareholderInfo info)
    {
		if(!this.isSubTable)
			db.Open();
		
        String query = "select count(*)  from ClientShareholder "
        + " where ClientID = @ClientID and RowNo = @RowNo";
        var obj = (List<int>)db.Query<int>(query, info, this.transaction);
        
		
		if(!this.isSubTable)
			db.Close();
		
        return obj[0] > 0;
    }

    public void Save(ClientShareholderInfo info)
    {
        if(this.IsExisted(info))
            this.Update(info);
        else
            this.Insert(info); 
    }

	 
    public ClientShareholderInfo Get(string ClientID)
    {
		db.Open();

        string query = "select * from ClientShareholder " 
		+ " where ClientID = @ClientID ";
		
        var obj = (List<ClientShareholderInfo>)db.Query<ClientShareholderInfo>(query, new {  ClientID = ClientID  });
        db.Close();
		
        if (obj.Count > 0)
            return obj[0];
        else
            return null;
    }

    public void Delete(string ClientID)
    {
		if(!this.isSubTable)
			db.Open();

        string query = "delete  from ClientShareholder " 
		+ " where ClientID = @ClientID ";
		
        db.Execute(query, new {  ClientID = ClientID  }, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }
	
    public void Update(ClientShareholderInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = " UPDATE [dbo].[ClientShareholder] SET  "
		+ " [RowNo] = @RowNo " 
		+ ", [Name] = @Name " 
		+ ", [NoOfShares] = @NoOfShares " 
		+ ", [Nationality] = @Nationality " 
		+ ", [IdentityNo] = @IdentityNo " 
		+ ", [ContactNo] = @ContactNo " 
		+ ", [Address] = @Address " 
		+ ", [CreateDate] = @CreateDate " 
		+ ", [CreateUser] = @CreateUser " 
		+ ", [LastModifiedDate] = @LastModifiedDate " 
		+ ", [LastModifiedUser] = @LastModifiedUser "
        + " where ClientID = @ClientID and RowNo = @RowNo ";

         
        db.Execute(query, info, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }

    public void Insert(ClientShareholderInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = "INSERT INTO [dbo].[ClientShareholder] ( [ClientID] " 
		+ ",[RowNo] " 
		+ ",[Name] " 
		+ ",[NoOfShares] " 
		+ ",[Nationality] " 
		+ ",[IdentityNo] " 
		+ ",[ContactNo] " 
		+ ",[Address] " 
		+ ",[CreateDate] " 
		+ ",[CreateUser] " 
		+ ",[LastModifiedDate] " 
		+ ",[LastModifiedUser] " 
		+") "
		+ "VALUES ( @ClientID "
		+ ",@RowNo " 
		+ ",@Name " 
		+ ",@NoOfShares " 
		+ ",@Nationality " 
		+ ",@IdentityNo " 
		+ ",@ContactNo " 
		+ ",@Address " 
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

    
    public void DeleteClientShareholderNotInTheList(List<int> rowNoList, string clientID)
    {
        string query = "Delete from ClientShareholder where ClientID = @ClientID and RowNo not in @RowNoList ";
        db.Execute(query, new { ClientID = clientID, RowNoList = rowNoList }, transaction);
    }


}