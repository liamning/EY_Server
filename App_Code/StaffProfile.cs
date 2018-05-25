using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 
using System.Web;
using Dapper;


public class StaffProfile
{

    #region Standar Function
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);


    public List<string> GetStaffNoList(string StaffNo)
    {
        db.Open();
        String query = "select top 10 StaffNo from StaffProfile where (@StaffNo = '' or StaffNo like '%' + @StaffNo + '%') order by StaffNo";
        var obj = (List<string>)db.Query<string>(query, new { StaffNo = StaffNo });
        db.Close();
        return obj;
    }


    public bool IsExisted(StaffProfileInfo info)
    {
        db.Open();
        String query = "select count(*)  from StaffProfile "
        + " where StaffNo = @StaffNo ";
        var obj = (List<int>)db.Query<int>(query, info);
        db.Close();
        return obj[0] > 0;
    }

    public void Save(StaffProfileInfo info)
    {
        if (this.IsExisted(info))
            this.Update(info);
        else
            this.Insert(info);
    }


    public StaffProfileInfo Get(string StaffNo)
    {
        db.Open();

        string query = "select * from StaffProfile "
        + " where StaffNo = @StaffNo ";

        var obj = (List<StaffProfileInfo>)db.Query<StaffProfileInfo>(query, new { StaffNo = StaffNo });
        db.Close();

        if (obj.Count > 0)
            return obj[0];
        else
            return null;
    }

    public void Delete(string StaffNo)
    {
        db.Open();

        string query = "delete  from StaffProfile "
        + " where StaffNo = @StaffNo ";

        db.Execute(query, new { StaffNo = StaffNo });
        db.Close();
    }

    public void Update(StaffProfileInfo info)
    {
        db.Open();

        string query = " UPDATE [dbo].[StaffProfile] SET  "
        + " [StaffName] = @StaffName "
        + ", [Password] = @Password "
        + ", [Role] = @Role "
        + ", [CreateUser] = @CreateUser "
        + ", [CreateDate] = @CreateDate "
        + ", [LastUpdateUser] = @LastUpdateUser "
        + ", [LastUpdateDate] = @LastUpdateDate "
        + " where StaffNo = @StaffNo ";


        db.Execute(query, info);
        db.Close();
    }

    public void Insert(StaffProfileInfo info)
    {
        db.Open();

        string query = "INSERT INTO [dbo].[StaffProfile] ( [StaffNo] "
        + ",[StaffName] "
        + ",[Password] "
        + ",[Role] "
        + ",[CreateUser] "
        + ",[CreateDate] "
        + ",[LastUpdateUser] "
        + ",[LastUpdateDate] "
        + ") "
        + "VALUES ( @StaffNo "
        + ",@StaffName "
        + ",@Password "
        + ",@Role "
        + ",@CreateUser "
        + ",@CreateDate "
        + ",@LastUpdateUser "
        + ",@LastUpdateDate "
        + ") ";


        db.Execute(query, info);
        db.Close();
    }
    #endregion 

     
    public bool ResetPassword(string staffNo, string password, string newPassword)
    {
        db.Open();

        string query = "update StaffProfile "
            + " set password = @NewPassword"
        + " where StaffNo = @StaffNo and  Password = @Password ";

        int result = db.Execute(query, new
        {
            Password = password,
            StaffNo = staffNo,
            NewPassword = newPassword
        });
         
        db.Close();

        return result == 1;
    }

    public StaffInfo Login(string staffNo, string password)
    {
        db.Open();

        string query = "select * from StaffProfile "
        + " where StaffNo = @StaffNo and  Password = @Password ";

        List<StaffInfo> result = (List<StaffInfo>)db.Query<StaffInfo>(query, new
        {
            Password = password,
            StaffNo = staffNo, 
        }); 
        db.Close();

        if (result.Count > 0)
        {
            return result[0];
        }

        return null;
    }

}