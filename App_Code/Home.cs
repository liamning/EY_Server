using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;

public class Home
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);

    public Home()
    {
    }

    #region Get
    public List<HomeInfo> Get(string userID, string role)
    {
        //3 = admin

        db.Open();
        String query = "SELECT distinct [dbo].[ClientMaster].ClientID "
                     + ",[dbo].[ClientMaster].CompanyName "
                     + ",[dbo].[ClientMaster].CompanyType "
                     + ",[dbo].[ClientMaster].RegCapital "
                     + ",[dbo].[ClientMaster].RegShare "
                     + ",[dbo].[ClientMaster].NotRegCapital "
                     + "FROM ResponsibleStaffList "
                     + "JOIN [dbo].[ClientMaster] ON [dbo].[ResponsibleStaffList].ClientID = [dbo].[ClientMaster].ClientID "
                     + "WHERE (StaffID = @UserID or @Role=3) ";
        List<HomeInfo> obj = (List<HomeInfo>)db.Query<HomeInfo>(query, new { UserID = userID, Role = role });
        db.Close();
        return obj;
    }
    #endregion
}