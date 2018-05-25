using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class CreditorAmount
{
	#region Standar Function
    SqlConnection db; 
	SqlTransaction transaction;
	bool isSubTable = false;

    public CreditorAmount()
    {
        this.db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    } 

    public CreditorAmount(SqlConnection db, SqlTransaction transaction)
    { 
		this.isSubTable = true;
		this.db = db;
		this.transaction = transaction;
    }


    public bool IsExisted(CreditorAmountInfo info)
    {
		if(!this.isSubTable)
			db.Open();
		
        String query = "select count(*)  from CreditorAmount "
        + " where ClientID = @ClientID and CreditorID = @CreditorID and RowNo=@RowNo ";
        var obj = (List<int>)db.Query<int>(query, info, this.transaction);
        
		
		if(!this.isSubTable)
			db.Close();
		
        return obj[0] > 0;
    }

    public void Save(CreditorAmountInfo info)
    {
        if(this.IsExisted(info))
            this.Update(info);
        else
            this.Insert(info); 
    }


    public List<CreditorAmountInfo> GetCreditorAmountList(string ClientID, string CreditorID)
    {
        if (!this.isSubTable)
            db.Open();

        string query = "select * from CreditorAmount "
        + " where ClientID = @ClientID  and CreditorID = @CreditorID";

        var obj = (List<CreditorAmountInfo>)db.Query<CreditorAmountInfo>(query, new { ClientID = ClientID, CreditorID = CreditorID }, this.transaction);


        if (!this.isSubTable)
            db.Close();

        return obj;
    }

    public void Delete(string ClientID)
    {
		if(!this.isSubTable)
			db.Open();

        string query = "delete  from CreditorAmount "
        + " where ClientID = @ClientID  and CreditorID = @CreditorID and RowNo=@RowNo";
		
        db.Execute(query, new {  ClientID = ClientID  }, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }
	
    public void Update(CreditorAmountInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = " UPDATE [dbo].[CreditorAmount] SET  "
		+ " [CreditorID] = @CreditorID " 
		+ ", [RowNo] = @RowNo " 
		+ ", [Currency] = @Currency " 
		+ ", [Amount] = @Amount " 
		+ ", [CreateDate] = @CreateDate " 
		+ ", [CreateUser] = @CreateUser " 
		+ ", [LastModifiedDate] = @LastModifiedDate " 
		+ ", [LastModifiedUser] = @LastModifiedUser "
        + " where ClientID = @ClientID  and CreditorID = @CreditorID and RowNo=@RowNo ";

         
        db.Execute(query, info, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }

    public void Insert(CreditorAmountInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = "INSERT INTO [dbo].[CreditorAmount] ( [ClientID] " 
		+ ",[CreditorID] " 
		+ ",[RowNo] " 
		+ ",[Currency] " 
		+ ",[Amount] " 
		+ ",[CreateDate] " 
		+ ",[CreateUser] " 
		+ ",[LastModifiedDate] " 
		+ ",[LastModifiedUser] " 
		+") "
		+ "VALUES ( @ClientID "
		+ ",@CreditorID " 
		+ ",@RowNo " 
		+ ",@Currency " 
		+ ",@Amount " 
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
    
    public void DeleteCreditorAmountNotInTheList(List<int> rowNoList, string clientID, string creditorID)
    {
        string query = "Delete from CreditorAmount where ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo not in @RowNoList ";
        db.Execute(query, new { ClientID = clientID, CreditorID = creditorID, RowNoList = rowNoList }, transaction);
    }

}