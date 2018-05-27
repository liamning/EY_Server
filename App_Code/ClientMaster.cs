using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;

public class ClientMaster
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;
    public ClientMaster()
    {
    }

    public List<HomeInfo> Search(string userID, string role, Dictionary<string, string> criteria, Dictionary<string, string> fieldsDataType, Dictionary<string, string> criteriaOperator)
    {
        List<string> criteriaList = new List<string>();
        string value = null, op = null;
        foreach (string field in criteria.Keys)
        {
            if (string.IsNullOrEmpty(criteria[field].Trim())) continue;

            op = " = ";
            value = string.Format("N'{0}'", criteria[field]);

            switch (fieldsDataType[field])
            {
                case "number":
                    value = criteria[field];
                    break;
                case "string":
                    switch (criteriaOperator[field])
                    {
                        case "like":
                            op = " like ";
                            value = string.Format("N'%{0}%'", criteria[field]);
                            break;
                    }
                    break;
                case "datetime":
                    value = string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", DateTime.ParseExact(criteria[field], GlobalSetting.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                    break;
            }
            criteriaList.Add(string.Format("ClientMaster.{0} {1} {2}", field, op, value));
        }


        //3 = admin

        db.Open();
        String query = "SELECT distinct [dbo].[ClientMaster].ClientID "
                     + ",[dbo].[ClientMaster].CompanyName "
                     + ",[dbo].[GeneralMaster].EngDesc CompanyType "
                     + ",[dbo].[ClientMaster].RegCapital "
                     + ",[dbo].[ClientMaster].RegShare "
                     + ",[dbo].[ClientMaster].NotRegCapital "
                     + "FROM [dbo].[ClientMaster] "
                     + "left outer join ResponsibleStaffList ON [dbo].[ResponsibleStaffList].ClientID = [dbo].[ClientMaster].ClientID "
                     + "left outer join GeneralMaster ON Category = 'CompanyType' and [dbo].[GeneralMaster].Code = [dbo].[ClientMaster].CompanyType "
                     + "WHERE (StaffID = @UserID or @Role=3 or StaffID=null) "
                     + (criteriaList.Count > 0 ? " and " + string.Join(" and ", criteriaList.ToArray()) : "")
                     ;
        List<HomeInfo> obj = (List<HomeInfo>)db.Query<HomeInfo>(query, new { UserID = userID, Role = role });
        db.Close();
        return obj;
    }

    #region Save
    public ClientMasterInfo Save(ClientMasterInfo clientMasterInfo)
    {
        db.Open(); 
        transaction = db.BeginTransaction();
        try
        {

            if (this.IsExisted(clientMasterInfo.ClientID))
                clientMasterInfo = this.Update(clientMasterInfo);
            else
                clientMasterInfo = this.Insert(clientMasterInfo);

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

        return clientMasterInfo;
        
    }

    public void SaveResponsibleStaffInfo(ResponsibleStaffInfo responsibleStaffInfo, string clientID)
    {
        if (this.IsResponsibleStaffExisted(clientID, responsibleStaffInfo.StaffID))
            this.UpdateResponsibleStaff(responsibleStaffInfo);
        else
            this.InsertResponsibleStaff(responsibleStaffInfo);
    }

    public void SaveCurrencyInfo(CurrencyInfo currencyInfo, string clientID)
    {
        if (this.IsCurrencyExisted(clientID, currencyInfo.CurrencyCode))
            this.UpdateCurrency(currencyInfo);
        else
            this.InsertCurrency(currencyInfo);
    }
    #endregion

    #region Insert
    public ClientMasterInfo Insert(ClientMasterInfo clientMasterInfo)
    { 
            string query = "INSERT INTO [dbo].[ClientMaster] "
                        + " ( [CompanyName] "
        + ",[CompanyType] "
        + ",[RegPlace] "
        + ",[RegNo] "
        + ",[TaxCodeNo] "
        + ",[OrganizationCode] "
        + ",[SocialUnifiedCreditCode] "
        + ",[ExternalDebtNo] "
        + ",[RegCapitalCurrency] "
        + ",[RegCapital] "
        + ",[RegShareCurrency] "
        + ",[RegShare] "
        + ",[NotRegCapitalCurrency] "
        + ",[NotRegCapital] "
        + ",[TotalCreditor] "
        + ",[Website] "
        + ",[Director] "
        + ",[DirectorIdenNo] "
        + ",[DirectorPhone] "
        + ",[DirectorAddress] "
        + ",[GenManager] "
        + ",[GenManagerIdenNo] "
        + ",[GenManagerPhone] "
        + ",[GenManagerAddress] "
        + ",[Secretary] "
        + ",[SecretaryIdenNo] "
        + ",[SecretaryPhone] "
        + ",[SecretaryAddress] "
        + ",[Supervisor] "
        + ",[SupervisorIdenNo] "
        + ",[SupervisorPhone] "
        + ",[SupervisorAddress] "
        + ",[Admin] "
        + ",[AdminIdenNo] "
        + ",[AdminPhone] "
        + ",[AdminAddress] "
        + ",[ContactEmail] "
        + ",[LegalRepresentative] "
        + ",[Relationship] "
        + ",[EntrustAgent] "
        + ",[CreateUser] "
        + ",[CreateDate] "
        + ",[LastModifiedUser] "
        + ",[LastModifiedDate] "
        + ") "
        + "VALUES ( @CompanyName "
        + ",@CompanyType "
        + ",@RegPlace "
        + ",@RegNo "
        + ",@TaxCodeNo "
        + ",@OrganizationCode "
        + ",@SocialUnifiedCreditCode "
        + ",@ExternalDebtNo "
        + ",@RegCapitalCurrency "
        + ",@RegCapital "
        + ",@RegShareCurrency "
        + ",@RegShare "
        + ",@NotRegCapitalCurrency "
        + ",@NotRegCapital "
        + ",@TotalCreditor "
        + ",@Website "
        + ",@Director "
        + ",@DirectorIdenNo "
        + ",@DirectorPhone "
        + ",@DirectorAddress "
        + ",@GenManager "
        + ",@GenManagerIdenNo "
        + ",@GenManagerPhone "
        + ",@GenManagerAddress "
        + ",@Secretary "
        + ",@SecretaryIdenNo "
        + ",@SecretaryPhone "
        + ",@SecretaryAddress "
        + ",@Supervisor "
        + ",@SupervisorIdenNo "
        + ",@SupervisorPhone "
        + ",@SupervisorAddress "
        + ",@Admin "
        + ",@AdminIdenNo "
        + ",@AdminPhone "
        + ",@AdminAddress "
        + ",@ContactEmail "
        + ",@LegalRepresentative "
        + ",@Relationship "
        + ",@EntrustAgent "
        + ",@CreateUser "
        + ",@CreateDate "
        + ",@LastModifiedUser "
        + ",@LastModifiedDate "
        + ");select SCOPE_IDENTITY(); ";

            int id = ((List<int>)db.Query<int>(query, clientMasterInfo, transaction))[0];
            string tmpString = string.Format("00000000{0}", id);
            clientMasterInfo.ClientID = "C" + tmpString.Substring(tmpString.Length - 8);
            if (clientMasterInfo.ClientShareholderList != null)
            {  
                ClientShareholder cs = new ClientShareholder(this.db, transaction); 
                foreach (ClientShareholderInfo clientShareholderInfo in clientMasterInfo.ClientShareholderList)
                {
                    clientShareholderInfo.ClientID = clientMasterInfo.ClientID;
                    clientShareholderInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                    clientShareholderInfo.CreateDate = DateTime.Now;
                    clientShareholderInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    clientShareholderInfo.LastModifiedDate = DateTime.Now;
                    cs.Save(clientShareholderInfo);
                }

            }



            if (clientMasterInfo.ResponsibleStaffList != null)
            {
                foreach (ResponsibleStaffInfo responsibleStaffInfo in clientMasterInfo.ResponsibleStaffList)
                {
                    responsibleStaffInfo.ClientID = clientMasterInfo.ClientID;
                    responsibleStaffInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                    responsibleStaffInfo.CreateDate = DateTime.Now;
                    responsibleStaffInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    responsibleStaffInfo.LastModifiedDate = DateTime.Now;
                    this.InsertResponsibleStaff(responsibleStaffInfo);
                }
            }

            if (clientMasterInfo.CurrencyList != null)
            {
                foreach (CurrencyInfo currencyInfo in clientMasterInfo.CurrencyList)
                {
                    currencyInfo.ClientID = clientMasterInfo.ClientID;
                    currencyInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                    currencyInfo.CreateDate = DateTime.Now;
                    currencyInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    currencyInfo.LastModifiedDate = DateTime.Now;
                    this.InsertCurrency(currencyInfo);
                }
            }

            //Client Member
            if (clientMasterInfo.ClientMemberList != null)
            {
                ClientMember cm = new ClientMember(this.db, transaction);
                foreach (ClientMemberInfo info in clientMasterInfo.ClientMemberList)
                {
                    info.ClientID = clientMasterInfo.ClientID;
                    info.CreateUser = clientMasterInfo.LastModifiedUser;
                    info.CreateDate = DateTime.Now;
                    info.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    info.LastModifiedDate = DateTime.Now;
                    cm.Save(info);
                }
            }
            //Court Judge
            if (clientMasterInfo.ClientCourtJudgeList != null)
            {
                ClientCourtJudge ClientCourtJudge = new ClientCourtJudge(this.db, transaction);
                foreach (ClientCourtJudgeInfo info in clientMasterInfo.ClientCourtJudgeList)
                {
                    info.ClientID = clientMasterInfo.ClientID;
                    info.CreateUser = clientMasterInfo.LastModifiedUser;
                    info.CreateDate = DateTime.Now;
                    info.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    info.LastModifiedDate = DateTime.Now;
                    ClientCourtJudge.Save(info);
                }
            }

             
        return clientMasterInfo;
    }

    public void InsertClientShareholder(ClientShareholderInfo clientShareholderInfo)
    {
        string query = "INSERT INTO [dbo].[ClientShareholder] "
                   + "([ClientID] "
                   + ",[RowNo] "
                   + ",[ShareholderName] "
                   + ",[NoOfShares] "
                   + ",[CreateUser] "
                   + ",[CreateDate] "
                   + ",[LastModifiedUser] "
                   + ",[LastModifiedDate] ) "
                   + "VALUES "
                   + "(@ClientID "
                   + ",@RowNo"
                   + ",@ShareholderName "
                   + ",@NoOfShares "
                   + ",@CreateUser "
                   + ",@CreateDate "
                   + ",@LastModifiedUser "
                   + ",@LastModifiedDate) ";
        db.Execute(query, clientShareholderInfo, transaction);
    }

    public void InsertResponsibleStaff(ResponsibleStaffInfo responsibleStaffInfo)
    {
        string query = "INSERT INTO [dbo].[ResponsibleStaffList] "
                   + "([ClientID] "
                   + ",[StaffID] "
                   + ",[StaffName] "
                   + ",[ResponsibilityType] "
                   + ",[CreateUser] "
                   + ",[CreateDate] "
                   + ",[LastModifiedUser] "
                   + ",[LastModifiedDate] ) "
                   + "VALUES "
                   + "(@ClientID "
                   + ",@StaffID "
                   + ",@StaffName "
                   + ",@ResponsibilityType "
                   + ",@CreateUser "
                   + ",@CreateDate "
                   + ",@LastModifiedUser "
                   + ",@LastModifiedDate) ";
        db.Execute(query, responsibleStaffInfo, transaction);
    }

    public void InsertCurrency(CurrencyInfo currencyInfo)
    {
        string query = "INSERT INTO [dbo].[CurrencyList] "
                   + "([ClientID] "
                   + ",[CurrencyCode] "
                   + ",[Currency] "
                   + ",[Rate] "
                   + ",[CreateUser] "
                   + ",[CreateDate] "
                   + ",[LastModifiedUser] "
                   + ",[LastModifiedDate] ) "
                   + "VALUES "
                   + "(@ClientID "
                   + ",@CurrencyCode "
                   + ",@Currency "
                   + ",@Rate "
                   + ",@CreateUser "
                   + ",@CreateDate "
                   + ",@LastModifiedUser "
                   + ",@LastModifiedDate) ";
        db.Execute(query, currencyInfo, transaction);
    }
    #endregion

    #region Read
    public List<string> GetClientIDList(string clientID, string userID, string role)
    {
        db.Open();
        try
        {
            string query = @"select distinct top 10 ClientMaster.ClientID 
from ClientMaster
left outer join ResponsibleStaffList ON [dbo].[ResponsibleStaffList].ClientID = [dbo].[ClientMaster].ClientID
where (@ClientID = '' or ClientMaster.ClientID like '%' + @ClientID + '%') 
and (StaffID = @UserID or @Role=3 or StaffID=null) 
order by ClientMaster.ClientID";

            var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, UserID = userID, Role = role });
            return obj;
        }
        finally
        {
            db.Close();
        }
         
    }

    public List<ValTxtInfo> GetStaffIDList(string staffID, string clientID)
    {
        db.Open();
        String query = "select top 10 StaffNo \"val\", StaffName \"txt\" from StaffProfile where (@ID = '' or StaffNo like '%' + @ID + '%') order by StaffNo";
        var obj = (List<ValTxtInfo>)db.Query<ValTxtInfo>(query, new { ClientID = clientID, ID = staffID });
        db.Close();
        return obj;
    }

    public List<GeneralCodeDesc> GetStaffIDList()
    {
        db.Open();
        String query = "select top 10 StaffNo \"Code\", StaffName \"Desc\" from StaffProfile order by StaffNo";
        var obj = (List<GeneralCodeDesc>)db.Query<GeneralCodeDesc>(query);
        db.Close();
        return obj;
    }


    public ClientMasterInfo Get(string clientID)
    {
        db.Open();
        String query = "select * from ClientMaster where ClientID = @ClientID";
        var obj = (List<ClientMasterInfo>)db.Query<ClientMasterInfo>(query, new { ClientID = clientID });
        db.Close();

        if (obj.Count > 0)
        {
            obj[0].ClientShareholderList = this.GetClientShareholderList(clientID);
            obj[0].ResponsibleStaffList = this.GetResponsibleStaffList(clientID);
            obj[0].CurrencyList = this.GetCurrencyList(clientID);
            obj[0].ClientMemberList = new ClientMember().GetMemberList(clientID);
            obj[0].ClientCourtJudgeList = new ClientCourtJudge().GetCourtJudgeList(clientID);
            return obj[0];
        }
        else
            return null;
    }

    public List<ClientShareholderInfo> GetClientShareholderList(string clientID)
    {
        db.Open();
        String query = "select * from ClientShareholder where ClientID = @ClientID ";
        List<ClientShareholderInfo> list = (List<ClientShareholderInfo>)db.Query<ClientShareholderInfo>(query, new { ClientID = clientID });
        db.Close();
        return list;
    }

    public List<ResponsibleStaffInfo> GetResponsibleStaffList(string clientID)
    {
        db.Open();
        String query = @"select ResponsibleStaffList.*, StaffProfile.StaffName, GeneralMaster.ChiDesc ResponsibilityTypeDesc
from ResponsibleStaffList 
join StaffProfile on StaffNo = StaffID
join GeneralMaster on Code = ResponsibilityType and Category= 'staffType'
Where ClientID = @ClientID ";
        List<ResponsibleStaffInfo> list = (List<ResponsibleStaffInfo>)db.Query<ResponsibleStaffInfo>(query, new { ClientID = clientID });
        db.Close();
        return list;
    }

    public List<CurrencyInfo> GetCurrencyList(string clientID)
    {
        db.Open();
        String query = "select * from CurrencyList Where ClientID = @ClientID ";
        List<CurrencyInfo> list = (List<CurrencyInfo>)db.Query<CurrencyInfo>(query, new { ClientID = clientID });
        db.Close();
        return list;
    }

    public string GetStaffName(string staffID, string clientID)
    {
        db.Open();
        String query = "select StaffName from StaffProfile where StaffNo = @StaffNo";
        List<string> list = (List<string>)db.Query<string>(query, new { ClientID = clientID, StaffNo = staffID });
        db.Close();

        if (list.Count > 0 && list[0] != null)
        {
            return list[0];
        }
        else
            return null;
    }

    public int? GetMaxRowNoOfClientShareholderList(string clientID)
    {
        String query = "select MAX(RowNo) from ClientShareholder where ClientID = @ClientID";
        List<int?> MaxRowNo = (List<int?>)db.Query<int?>(query, new { ClientID = clientID });
        if (MaxRowNo.Count > 0 && MaxRowNo[0] != null)
            return MaxRowNo[0];
        else
            return 0;
    }
    #endregion

    #region Update

    public void CompareCreditor(CreditorMasterInfo newCreditorMaster)
    {

    }

    public ClientMasterInfo Update(ClientMasterInfo clientMasterInfo)
    { 
            string query =
              "UPDATE [dbo].[ClientMaster] "
            + "SET  "
            + " [CompanyName] = @CompanyName "
        + ", [CompanyType] = @CompanyType "
        + ", [RegPlace] = @RegPlace "
        + ", [RegNo] = @RegNo "
        + ", [TaxCodeNo] = @TaxCodeNo "
        + ", [OrganizationCode] = @OrganizationCode "
        + ", [SocialUnifiedCreditCode] = @SocialUnifiedCreditCode "
        + ", [ExternalDebtNo] = @ExternalDebtNo "
        + ", [RegCapitalCurrency] = @RegCapitalCurrency "
        + ", [RegCapital] = @RegCapital "
        + ", [RegShareCurrency] = @RegShareCurrency "
        + ", [RegShare] = @RegShare "
        + ", [NotRegCapitalCurrency] = @NotRegCapitalCurrency "
        + ", [NotRegCapital] = @NotRegCapital "
        + ", [TotalCreditor] = @TotalCreditor "
        + ", [Website] = @Website "
        + ", [Director] = @Director "
        + ", [DirectorIdenNo] = @DirectorIdenNo "
        + ", [DirectorPhone] = @DirectorPhone "
        + ", [DirectorAddress] = @DirectorAddress "
        + ", [GenManager] = @GenManager "
        + ", [GenManagerIdenNo] = @GenManagerIdenNo "
        + ", [GenManagerPhone] = @GenManagerPhone "
        + ", [GenManagerAddress] = @GenManagerAddress "
        + ", [Secretary] = @Secretary "
        + ", [SecretaryIdenNo] = @SecretaryIdenNo "
        + ", [SecretaryPhone] = @SecretaryPhone "
        + ", [SecretaryAddress] = @SecretaryAddress "
        + ", [Supervisor] = @Supervisor "
        + ", [SupervisorIdenNo] = @SupervisorIdenNo "
        + ", [SupervisorPhone] = @SupervisorPhone "
        + ", [SupervisorAddress] = @SupervisorAddress "
        + ", [Admin] = @Admin "
        + ", [AdminIdenNo] = @AdminIdenNo "
        + ", [AdminPhone] = @AdminPhone "
        + ", [AdminAddress] = @AdminAddress "
        + ", [ContactEmail] = @ContactEmail "
        + ", [LegalRepresentative] = @LegalRepresentative "
        + ", [Relationship] = @Relationship "
        + ", [EntrustAgent] = @EntrustAgent "
        + ", [CreateUser] = @CreateUser "
        + ", [CreateDate] = @CreateDate "
        + ", [LastModifiedUser] = @LastModifiedUser "
        + ", [LastModifiedDate] = @LastModifiedDate " 
            + "WHERE ClientID = @ClientID ";
            db.Execute(query, clientMasterInfo, transaction);

            // Delete record
            List<int> clientShareholderRowNoList = new List<int>();
            List<string> responsibleStaffStaffIDList = new List<string>();
            List<string> currencyCodeList = new List<string>();
            List<int> memberListToBeDeleted = new List<int>();
            List<int> courtJudgeListToBeDeleted = new List<int>();

            foreach (ClientShareholderInfo clientShareholderInfo in clientMasterInfo.ClientShareholderList)
            {
                clientShareholderRowNoList.Add(clientShareholderInfo.RowNo);
            }

            foreach (ResponsibleStaffInfo responsibleStaffInfo in clientMasterInfo.ResponsibleStaffList)
            {
                responsibleStaffStaffIDList.Add(responsibleStaffInfo.StaffID);
            }

            foreach (CurrencyInfo currencyInfo in clientMasterInfo.CurrencyList)
            {
                currencyCodeList.Add(currencyInfo.CurrencyCode);
            }

            foreach (ClientMemberInfo info in clientMasterInfo.ClientMemberList)
            {
                memberListToBeDeleted.Add(info.RowNo);
            }
            foreach (ClientCourtJudgeInfo info in clientMasterInfo.ClientCourtJudgeList)
            {
                courtJudgeListToBeDeleted.Add(info.RowNo);
            }

            this.DeleteResponsibleStaffNotInTheList(responsibleStaffStaffIDList, clientMasterInfo.ClientID);
            this.DeleteCurrencyNotInTheList(currencyCodeList, clientMasterInfo.ClientID);



            ClientShareholder cs = new ClientShareholder(this.db, transaction);
            cs.DeleteClientShareholderNotInTheList(clientShareholderRowNoList, clientMasterInfo.ClientID);
            foreach (ClientShareholderInfo clientShareholderInfo in clientMasterInfo.ClientShareholderList)
            {
                clientShareholderInfo.ClientID = clientMasterInfo.ClientID;
                clientShareholderInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                clientShareholderInfo.CreateDate = DateTime.Now;
                clientShareholderInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                clientShareholderInfo.LastModifiedDate = DateTime.Now;
                cs.Save(clientShareholderInfo);
            }

             
            foreach (ResponsibleStaffInfo responsibleStaffInfo in clientMasterInfo.ResponsibleStaffList)
            {
                responsibleStaffInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                responsibleStaffInfo.CreateDate = DateTime.Now;
                responsibleStaffInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                responsibleStaffInfo.LastModifiedDate = DateTime.Now;
                this.SaveResponsibleStaffInfo(responsibleStaffInfo, clientMasterInfo.ClientID);
            }

            foreach (CurrencyInfo currencyInfo in clientMasterInfo.CurrencyList)
            {
                currencyInfo.CreateUser = clientMasterInfo.LastModifiedUser;
                currencyInfo.CreateDate = DateTime.Now;
                currencyInfo.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                currencyInfo.LastModifiedDate = DateTime.Now;
                this.SaveCurrencyInfo(currencyInfo, clientMasterInfo.ClientID);
            }
             
            ClientMember cm = new ClientMember(this.db, transaction);
            cm.deleteClientMemberTypeNotInList(memberListToBeDeleted, clientMasterInfo.ClientID);
            if (clientMasterInfo.ClientMemberList != null)
            {
                foreach (ClientMemberInfo info in clientMasterInfo.ClientMemberList)
                {
                    info.ClientID = clientMasterInfo.ClientID;
                    info.CreateUser = clientMasterInfo.LastModifiedUser;
                    info.CreateDate = DateTime.Now;
                    info.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    info.LastModifiedDate = DateTime.Now;
                    cm.Save(info);
                }
            }

            //Court Judge
            ClientCourtJudge ClientCourtJudge = new ClientCourtJudge(this.db, transaction);
            ClientCourtJudge.DeleteClientCourtJudgeNotInTheList(courtJudgeListToBeDeleted, clientMasterInfo.ClientID);
            if (clientMasterInfo.ClientCourtJudgeList != null)
            {
                foreach (ClientCourtJudgeInfo info in clientMasterInfo.ClientCourtJudgeList)
                {
                    info.ClientID = clientMasterInfo.ClientID;
                    info.CreateUser = clientMasterInfo.LastModifiedUser;
                    info.CreateDate = DateTime.Now;
                    info.LastModifiedUser = clientMasterInfo.LastModifiedUser;
                    info.LastModifiedDate = DateTime.Now;
                    ClientCourtJudge.Save(info);
                }
            } 
         
        return clientMasterInfo;
    }

    public void UpdateClientShareholder(ClientShareholderInfo clientShareholderInfo)
    {
        string query =
            "UPDATE [dbo].[ClientShareholder] "
            + "SET  "
            + " [ShareholderName] = @ShareholderName "
            + ",[NoOfShares] = @NoOfShares "
            + ",[LastModifiedUser] = @LastModifiedUser "
            + ",[LastModifiedDate] = @LastModifiedDate "
            + "WHERE ClientID = @ClientID and RowNo = @RowNo  ";

        db.Execute(query, clientShareholderInfo, transaction);
    }

    public void UpdateResponsibleStaff(ResponsibleStaffInfo responsibleStaffInfo)
    {
        string query =
            "UPDATE [dbo].[ResponsibleStaffList] "
            + "SET  "
            + "[StaffName] = @StaffName "
            + ",[ResponsibilityType] = @ResponsibilityType "
            + ",[LastModifiedUser] = @LastModifiedUser "
            + ",[LastModifiedDate] = @LastModifiedDate "
            + "WHERE ClientID = @ClientID and StaffID = @StaffID  ";
        db.Execute(query, responsibleStaffInfo, transaction);
    }

    public void UpdateCurrency(CurrencyInfo currencyInfo)
    {
        string query =
            "UPDATE [dbo].[CurrencyList] "
            + "SET  "
            + " [Currency] = @Currency "
            + ",[Rate] = @Rate "
            + ",[LastModifiedUser] = @LastModifiedUser "
            + ",[LastModifiedDate] = @LastModifiedDate "
            + "WHERE ClientID = @ClientID and CurrencyCode = @CurrencyCode  ";
        db.Execute(query, currencyInfo, transaction);
    }
    #endregion

    #region Delete
    public void Delete(string clientID)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            string query = "Delete from ClientMaster where ClientID = @ClientID ";
            db.Execute(query, new { ClientID = clientID }, transaction);

            this.DeleteClientShareholder(clientID);
            this.DeleteResponsibleStaff(clientID);
            this.DeleteCurrency(clientID);
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

    public void DeleteClientShareholder(string clientID)
    {
        string query = "Delete from ClientShareholder where ClientID = @ClientID";
        db.Execute(query, new { ClientID = clientID }, transaction);
    }

    public void DeleteResponsibleStaff(string clientID)
    {
        string query = "Delete from ResponsibleStaffList where ClientID = @ClientID";
        db.Execute(query, new { ClientID = clientID }, transaction);
    }

    public void DeleteCurrency(string clientID)
    {
        string query = "Delete from CurrencyList where ClientID = @ClientID";
        db.Execute(query, new { ClientID = clientID }, transaction);
    }
     
    public void DeleteResponsibleStaffNotInTheList(List<string> staffIDList, string clientID)
    {
        string query = "Delete from ResponsibleStaffList where ClientID = @ClientID and StaffID not in @StaffIDList ";
        db.Execute(query, new { ClientID = clientID, StaffIDList = staffIDList }, transaction);
    }

    public void DeleteCurrencyNotInTheList(List<string> currencyCodeList, string clientID)
    {
        string query = "Delete from CurrencyList where ClientID = @ClientID and CurrencyCode not in @CurrencyCodeList";
        db.Execute(query, new { ClientID = clientID, CurrencyCodeList = currencyCodeList }, transaction);
    }
    #endregion

    #region Check exists
    public bool IsExisted(string clientID)
    { 
        string query = "select count(*) from ClientMaster where ClientID = @ClientID";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID }, this.transaction); 
        return obj[0] > 0;
    }

    public bool IsClientShareholderExisted(string clientID, int rowNo)
    {
        String query = "select count(*) from ClientShareholder where ClientID = @ClientID and RowNo = @RowNo";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, RowNo = rowNo }, transaction);
        return obj[0] > 0;
    }

    public bool IsResponsibleStaffExisted(string clientID, string staffID)
    {
        String query = "select count(*) from ResponsibleStaffList where ClientID = @ClientID and StaffID = @StaffID";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, StaffID = staffID }, transaction);
        return obj[0] > 0;
    }

    public bool IsCurrencyExisted(string clientID, string currencyCode)
    {
        String query = "select count(*) from CurrencyList where ClientID = @ClientID and CurrencyCode = @CurrencyCode";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CurrencyCode = currencyCode }, transaction);
        return obj[0] > 0;
    }
    #endregion
}