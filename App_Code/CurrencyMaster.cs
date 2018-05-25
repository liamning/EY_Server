using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;

public class CurrencyMaster
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public CurrencyMaster()
    {
    }

    #region Get
    public List<string> GetCurrencyCodeList(string currencyCode)
    {
        db.Open();
        String query = "select top 10 CurrencyCode from CurrencyMaster where (@CurrencyCode = '' or CurrencyCode like '%' + @CurrencyCode + '%') order by CurrencyCode";
        var obj = (List<string>)db.Query<string>(query, new { CurrencyCode = currencyCode });
        db.Close();
        return obj;
    }
    public List<GeneralCodeDesc> GetCurrencyCodeDescList()
    {
        db.Open();
        String query = "select top 10 CurrencyCode Code, CurrencyCode [Desc] from CurrencyMaster order by CurrencyCode";
        var obj = (List<GeneralCodeDesc>)db.Query<GeneralCodeDesc>(query);
        db.Close();
        return obj;
    }
    public List<string> GetCurrencyCodeListByClient(string currencyCode, string clientID)
    {
        db.Open();
        String query = "select top 10 CurrencyCode from CurrencyList where ClientID = @ClientID and (@CurrencyCode = '' or CurrencyCode like '%' + @CurrencyCode + '%') order by CurrencyCode";
        var obj = (List<string>)db.Query<string>(query, new { CurrencyCode = currencyCode, ClientID = clientID });
        db.Close();
        return obj;
    }

    public Dictionary<string, object> GetCurrencyDetail(string currencyCode)
    {
        Dictionary<string, object> returnDict = new Dictionary<string, object>();
        db.Open();
        String query = "select * from CurrencyMaster where CurrencyCode = @CurrencyCode";
        List<CurrencyMasterInfo> list = (List<CurrencyMasterInfo>)db.Query<CurrencyMasterInfo>(query, new { CurrencyCode = currencyCode });
        db.Close();

        if (list.Count > 0 && list[0] != null)
        {
            returnDict["Currency"] = list[0].Currency;
            returnDict["Rate"] = list[0].Rate;
            return returnDict;
        }
        else
            return null;
    }

    public Dictionary<string, object> GetClientCurrency(string clientID, string currencyCode)
    {
        Dictionary<string, object> returnDict = new Dictionary<string, object>();
        db.Open();
        String query = "select * from CurrencyList where ClientID = @ClientID and CurrencyCode = @CurrencyCode";
        List<CurrencyMasterInfo> list = (List<CurrencyMasterInfo>)db.Query<CurrencyMasterInfo>(query, new { ClientID = clientID, CurrencyCode = currencyCode });
        db.Close();

        if (list.Count > 0 && list[0] != null)
        {
            returnDict["Currency"] = list[0].Currency;
            returnDict["Rate"] = list[0].Rate;
            return returnDict;
        }
        else
            return null;
    }

    

    public List<CurrencyMasterInfo> GetCurrencyList()
    {
        db.Open();
        String query = "SELECT * FROM [dbo].[CurrencyMaster]";
        List<CurrencyMasterInfo> obj = (List<CurrencyMasterInfo>)db.Query<CurrencyMasterInfo>(query);
        return obj;
    }
    #endregion

    #region Save
    public void Save(List<CurrencyMasterInfo> currencyList)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            List<string> currencyCodeList = new List<string>();
            foreach (CurrencyMasterInfo currency in currencyList)
            {
                currencyCodeList.Add(currency.CurrencyCode);
            }

            this.Delete(currencyCodeList);

            foreach (CurrencyMasterInfo currency in currencyList)
            {
                if (this.IsExisted(currency))
                    this.Update(currency);
                else
                    this.Insert(currency);
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
    public void Insert(CurrencyMasterInfo currency)
    {
        String query = "INSERT INTO [dbo].[CurrencyMaster] "
                     + "([CurrencyCode] "
                     + ",[Currency] "
                     + ",[Rate] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@CurrencyCode "
                     + ",@Currency "
                     + ",@Rate "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, currency, transaction);
    }
    #endregion

    #region Update
    public void Update(CurrencyMasterInfo currency)
    {
        String query = "UPDATE [dbo].[CurrencyMaster] "
                     + "SET "
                     + "[Currency] = @Currency "
                     + ",[Rate] = @Rate "
                     + ",[LastModifiedUser] = @LastModifiedUser "
                     + ",[LastModifiedDate] = @LastModifiedDate "
                     + "WHERE CurrencyCode = @CurrencyCode ";
        db.Execute(query, currency, transaction);
    }
    #endregion

    #region Delete
    public void Delete(List<string> currencyCodeList)
    {
        String query = "DELETE FROM CurrencyMaster WHERE CurrencyCode NOT IN @CurrencyCodeList ";
        db.Execute(query, new { CurrencyCodeList = currencyCodeList }, transaction);
    }
    #endregion

    #region Check Exists
    public bool IsExisted(CurrencyMasterInfo currency)
    {
        String query = "SELECT Count(*) FROM CurrencyMaster WHERE CurrencyCode = @CurrencyCode ";
        var obj = (List<int>)db.Query<int>(query, new { CurrencyCode = currency.CurrencyCode}, transaction);
        return obj[0] > 0;
    }
    #endregion
}