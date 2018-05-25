using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;

/// <summary>
/// Summary description for Sample
/// </summary>
public class Sample
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;
	public Sample()
	{
		//
		// TODO: Add constructor logic here
		//
    }

    public List<string> GetClientNoList(string ClientNo)
    { 
        db.Open();
        String query = "select top 10 ClientNo from Sample where (@ClientNo = '' or ClientNo like '%' + @ClientNo + '%') order by ClientNo";
        var obj = (List<string>)db.Query<string>(query, new { ClientNo = ClientNo });
        db.Close();
        return obj;
    }

    public SampleInfo Get(string ClientNo)
    {
        db.Open();
        String query = "select * from Sample where ClientNo=@ClientNo";
        var obj = (List<SampleInfo>)db.Query<SampleInfo>(query, new { ClientNo = ClientNo }); 
        db.Close();


        if (obj.Count > 0){ 
            obj[0].StaffList = this.getStaffList(ClientNo);

            return obj[0];
        }
            
        else
            return null;
    }

    public bool IsExisted(string ClientNo)
    {
        db.Open();
        String query = "select count(*) from Sample where ClientNo=@ClientNo";
        var obj = (List<int>)db.Query<int>(query, new { ClientNo = ClientNo });
        db.Close();
        return obj[0] > 0;
    }

    public void Save(SampleInfo sampleInfo)
    {
        if(this.IsExisted(sampleInfo.ClientNo))
            this.Update(sampleInfo);
        else
            this.Insert(sampleInfo); 
    }

    public void Update(SampleInfo sampleInfo)
    {
        ////db.Open();
        ////transaction = db.BeginTransaction();
        ////try
        ////{ 
        ////    string query =
        ////      "UPDATE [dbo].[Sample] "
        ////    + "SET  "
        ////    + "[CompanyName] = @CompanyName "
        ////    + ",[Manager] = @Manager "
        ////    + ",[ResponsiblePerson] = @ResponsiblePerson "
        ////    + ",[ContactNo] = @ContactNo "
        ////    + ",[Member] = @Member "
        ////    + ",[Address] = @Address "
        ////    + ",[IsOnlineVoting] = @IsOnlineVoting "
        ////    + ",[Email] = @Email "
        ////    + ",[RelationShip] = @RelationShip "
        ////    + ",[Asset] = @Asset "
        ////    + ",[Liability] = @Liability "
        ////    + ",[CreateDate] = @CreateDate "
        ////    + ",[UpdateDate] = @UpdateDate "
        ////    + ",[UpdateUser] = @UpdateUser "
        ////    + "WHERE ClientNo = @ClientNo ";

        ////    db.Execute(query, sampleInfo, transaction);

        ////    List<string> staffIDList = new List<string>();
        ////    foreach (StaffInfo info in sampleInfo.StaffList)
        ////    {
        ////        staffIDList.Add(info.ID);
        ////    }
        ////    this.DeleteStaffNotInTheList(staffIDList, sampleInfo.ClientNo);

        ////    foreach (StaffInfo staff in sampleInfo.StaffList)
        ////    {
        ////        if (!this.StaffIsExisted(sampleInfo.ClientNo, staff.ID))
        ////        {
        ////            staff.CreateDate = DateTime.Now;
        ////            staff.ClientNo = sampleInfo.ClientNo;
        ////            this.InsertStaff(staff);
        ////        }
        ////        else
        ////        {
        ////            staff.UpdateDate = DateTime.Now;
        ////            staff.ClientNo = sampleInfo.ClientNo;
        ////            this.UpdateStaff(staff);
        ////        }
                 
        ////    }
                

        ////    transaction.Commit();
        //}
        //catch
        //{
        //    transaction.Rollback();
        //    throw;
        //}
        //finally
        //{
        //    transaction.Dispose();
        //    transaction = null;
        //    db.Close();
        //}

          
    }

    public void Insert(SampleInfo sampleInfo)
    {
        db.Open();
        string query = "INSERT INTO [dbo].[Sample] "
                    + "([ClientNo] "
                    + ",[CompanyName] "
                    + ",[Manager] "
                    + ",[ResponsiblePerson] "
                    + ",[ContactNo] "
                    + ",[Member] "
                    + ",[Address] "
                    + ",[IsOnlineVoting] "
                    + ",[Email] "
                    + ",[RelationShip] "
                    + ",[Asset] "
                    + ",[liability] "
                    + ",[CreateDate] "
                    + ",[CreateUser] "
                    + ",[UpdateDate] "
                    + ",[UpdateUser]) "
                    + "VALUES "
                    + "(@ClientNo "
                    + ",@CompanyName "
                    + ",@Manager "
                    + ",@ResponsiblePerson "
                    + ",@ContactNo "
                    + ",@Member "
                    + ",@Address "
                    + ",@IsOnlineVoting "
                    + ",@Email "
                    + ",@RelationShip "
                    + ",@Asset "
                    + ",@liability "
                    + ",@CreateDate "
                    + ",@CreateUser "
                    + ",@UpdateDate "
                    + ",@UpdateUser) ";

        db.Execute(query, sampleInfo);
         
        db.Close();
    }

    public void Delete(List<string> clientNoList)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            String query = "Delete from Sample where ClientNo = @ClientNo ";
            foreach (string clientNo in clientNoList)
            {
                db.Execute(query, new { ClientNo = clientNo }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            transaction.Dispose();
            transaction = null;
            db.Close();
        }
    }

    #region client staff

    public bool StaffIsExisted(string ClientNo, string staffID)
    { 
        String query = "select count(*) from ClientStaff where ClientNo=@ClientNo and ID=@ID";
        var obj = (List<int>)db.Query<int>(query, new { ClientNo = ClientNo, ID = staffID }, transaction); 
        return obj[0] > 0;
    }

    public void UpdateStaff(StaffInfo staffInfo)
    {
       // db.Open();
        string query =
            "UPDATE [dbo].[ClientStaff] "
            + "SET  "  
            + " [Name] = @Name "
            + ",[Type] = @Type "
            + ",[UpdateDate] = @UpdateDate "
            + ",[UpdateUser] = @UpdateUser "
            + "WHERE ClientNo = @ClientNo and [ID] = @ID  ";

        db.Execute(query, staffInfo, transaction);
        //db.Close();
    }

    public void InsertStaff(StaffInfo staffInfo)
    {
       // db.Open();
        string query = "INSERT INTO [dbo].[ClientStaff] "
                    + "([ClientNo] "
                    + ",[ID] "
                    + ",[Name] "
                    + ",[Type] "
                    + ",[CreateDate] "
                    + ",[CreateUser] ) "
                    + "VALUES "
                    + "(@ClientNo "
                    + ",@ID "
                    + ",@Name "
                    + ",@Type "
                    + ",@CreateDate "
                    + ",@CreateUser ) ";

        db.Execute(query, staffInfo, transaction);

       // db.Close();
    }

    //delete the staff not in the list
    public void DeleteStaffNotInTheList(List<string> staffNoList, string parentNo)
    {
        String query = "Delete from ClientStaff where ClientNo = @ClientNo and ID not in @Ids ";
        db.Execute(query, new { ClientNo = parentNo, Ids = staffNoList }, transaction);
    }

    public List<StaffInfo> getStaffList(string clientNo)
    {
        db.Open();
        String query = "select * from ClientStaff where ClientNo = @ClientNo ";
        List<StaffInfo> list = (List<StaffInfo>)db.Query<StaffInfo>(query, new { ClientNo = clientNo });
        db.Close();
        return list; 
    }

    #endregion

}