using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class ClientMember
{
	#region Standar Function
    SqlConnection db; 
	SqlTransaction transaction;
	bool isSubTable = false;
	
    public ClientMember()
    { 
		this.db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString); 
    }
    public ClientMember(SqlConnection db, SqlTransaction transaction)
    { 
		this.isSubTable = true;
		this.db = db;
		this.transaction = transaction;
    }

    public List<string> GetClientIDList(string ClientID)
    { 
        db.Open();
        String query = "select top 10 ClientID from ClientMember where (@ClientID = '' or ClientID like '%' + @ClientID + '%') order by ClientID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = ClientID });
        db.Close();
        return obj;
    }


    public bool IsExisted(ClientMemberInfo info)
    {
		if(!this.isSubTable)
			db.Open();
		
        String query = "select count(*)  from ClientMember "
        + " where ClientID = @ClientID and RowNo = @RowNo ";
        var obj = (List<int>)db.Query<int>(query, info, this.transaction);
        
		
		if(!this.isSubTable)
			db.Close();
		
        return obj[0] > 0;
    }

    public void Save(ClientMemberInfo info)
    {
        if(this.IsExisted(info))
            this.Update(info);
        else
            this.Insert(info); 
    }

	 
    public List<ClientMemberInfo> GetMemberList(string ClientID)
    {
		db.Open();

        string query = "select * from ClientMember " 
		+ " where ClientID = @ClientID ";
		
        var obj = (List<ClientMemberInfo>)db.Query<ClientMemberInfo>(query, new {  ClientID = ClientID  });
        db.Close();

        return obj;
    }

    public void Delete(string ClientID)
    {
		if(!this.isSubTable)
			db.Open();

        string query = "delete  from ClientMember " 
		+ " where ClientID = @ClientID ";
		
        db.Execute(query, new {  ClientID = ClientID  }, this.transaction);
		
		
		if(!this.isSubTable)
			db.Close();
    }
	
    public void Update(ClientMemberInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = " UPDATE [dbo].[ClientMember] SET  "
		+ " [RowNo] = @RowNo " 
		+ ", [MemberType] = @MemberType " 
		+ ", [Name] = @Name " 
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

    public void Insert(ClientMemberInfo info)
    {
	
		if(!this.isSubTable)
			db.Open();

        string query = "INSERT INTO [dbo].[ClientMember] ( [ClientID] " 
		+ ",[RowNo] " 
		+ ",[MemberType] " 
		+ ",[Name] " 
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
		+ ",@MemberType " 
		+ ",@Name " 
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

    
    public void deleteClientMemberTypeNotInList(List<int> codeList, string clientID)
    {
        string query = "Delete from ClientMember where ClientID = @ClientID and RowNo not in @codeList";
        db.Execute(query, new { ClientID = clientID, codeList = codeList }, transaction);
    }

}