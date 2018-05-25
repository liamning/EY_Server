using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;

public class ComboValueMaster
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public ComboValueMaster()
    {
    }

    #region Get
    public List<ComboValueSelectionInfo> GetCategoryList()
    {
        db.Open();
        //string query = "SELECT DISTINCT Category, CategoryDesc FROM [dbo].[GeneralMaster] where IsLocked is null ";
        string query = "SELECT DISTINCT Category, CategoryDesc FROM [dbo].[GeneralMaster]";
        List<ComboValueSelectionInfo> obj = (List<ComboValueSelectionInfo>)db.Query<ComboValueSelectionInfo>(query);
        db.Close();
        return obj;
    }

    public List<ComboValueMasterInfo> Get(string category)
    {
        db.Open();
        String query = "SELECT * FROM [dbo].[GeneralMaster] WHERE [dbo].[GeneralMaster].Category = @Category ORDER BY Seq";
        List<ComboValueMasterInfo> obj = (List<ComboValueMasterInfo>)db.Query<ComboValueMasterInfo>(query, new { Category = category });
        db.Close();
        return obj;
    }
     

    public List<ValTxtInfo> GetValTxt(string category)
    {
        db.Open();
        String query = "SELECT [Code] val, [ChiDesc] txt FROM [dbo].[GeneralMaster] WHERE [dbo].[GeneralMaster].Category = @Category ORDER BY Seq";
        List<ValTxtInfo> obj = (List<ValTxtInfo>)db.Query<ValTxtInfo>(query, new { Category = category });
        db.Close();
        return obj;
    }
    public Dictionary<string, List<ValTxtInfo>> GetValTxt(string[] categories)
    {
        Dictionary<string, List<ValTxtInfo>> dict = new Dictionary<string, List<ValTxtInfo>>();
        foreach (string category in categories)
        {
            dict.Add(category, this.GetValTxt(category));
        }
        return dict;
    }
    #endregion

    #region Save
    public void Save(string category, string categoryDesc, List<ComboValueMasterInfo> comboValueList)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            this.DeleteAllComboValueInCategory(category);

            for (int i = 0; i < comboValueList.Count; i++)
            {
                comboValueList[i].Category = category;
                comboValueList[i].CategoryDesc = categoryDesc;
                comboValueList[i].Seq = i + 1;
                comboValueList[i].Code = (i + 1).ToString();
            }

            foreach (ComboValueMasterInfo comboValue in comboValueList)
            {
                this.Insert(comboValue);
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
    #endregion

    #region Insert
    public void Insert(ComboValueMasterInfo comboValue)
    {
        String query = "INSERT INTO [dbo].[GeneralMaster] "
                     + "(Category"
                     + ",CategoryDesc"
                     + ",Seq "
                     + ",Code "
                     + ",EngDesc "
                     + ",ChiDesc "
                     + ",CreateUser "
                     + ",CreateDate "
                     + ",LastModifiedUser "
                     + ",LastModifiedDate ) "
                     + "VALUES "
                     + "(@Category "
                     + ",@CategoryDesc "
                     + ",@Seq "
                     + ",@Code "
                     + ",@EngDesc "
                     + ",@ChiDesc "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, comboValue, transaction);
    }
    #endregion

    #region Delete
    public void DeleteAllComboValueInCategory(string category)
    {
        String query = "DELETE FROM [dbo].[GeneralMaster] WHERE Category = @Category";
        db.Execute(query, new { Category = category }, transaction);
    }
    #endregion
}