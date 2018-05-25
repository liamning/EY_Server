using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Dapper;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.SS.UserModel;

public class Employee
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    //SqlTransaction transaction;

    public Employee()
    {
    }


    public void Import(ImportInfo info)
    {
        HSSFWorkbook hssfwb;
        Byte[] bytes = Convert.FromBase64String(info.DataURL);

        using (Stream file = new MemoryStream(bytes))
        {
            hssfwb = new HSSFWorkbook(file);
        }
         

        ISheet sheet = hssfwb.GetSheetAt(0);
        IRow tmpRow = null;
        ICell tmpCell = null;
        List<ClientEmployeeMasterInfo> employeeList = new List<ClientEmployeeMasterInfo>();
        ClientEmployeeMasterInfo tmpEmloyeeInfo = null; 
        int col;
        for (int row = 1; row <= sheet.LastRowNum; row++)
        {
            tmpRow = sheet.GetRow(row);

            if (tmpRow != null) //null is when the row only contains empty cells 
            {
                if (tmpRow.GetCell(0) != null)
                {
                    try
                    {

                        col = 0;

                        tmpEmloyeeInfo = new ClientEmployeeMasterInfo(); 

                        tmpEmloyeeInfo.ClientID = info.ClientID;
                        tmpEmloyeeInfo.ClientStaffID = tmpRow.GetCell(col++).StringCellValue;
                        tmpEmloyeeInfo.ClientStaffName = tmpRow.GetCell(col++).StringCellValue;

                        tmpCell = tmpRow.GetCell(col++);
                        if (tmpCell.CellType == CellType.Numeric)
                            tmpEmloyeeInfo.ClientStaffYear = (decimal)tmpCell.NumericCellValue;

                        tmpCell = tmpRow.GetCell(col++);
                        if (tmpCell.CellType == CellType.Numeric)
                            tmpEmloyeeInfo.ContractDate = tmpCell.DateCellValue;

                        tmpCell = tmpRow.GetCell(col++);
                        if (tmpCell.CellType == CellType.Numeric)
                            tmpEmloyeeInfo.ContractExpDate = tmpCell.DateCellValue;


                        tmpEmloyeeInfo.MonthlyWage = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.AnnualBonus = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.AvgWage = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.NoReleaseWage = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.NoReleaseBonus = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.NoReimbursementAmt = (decimal)tmpRow.GetCell(col++).NumericCellValue;
                        tmpEmloyeeInfo.Total = (decimal)tmpRow.GetCell(col++).NumericCellValue;


                        tmpEmloyeeInfo.CreateDate = info.CreateDate;
                        tmpEmloyeeInfo.CreateUser = info.CreateUser;
                        tmpEmloyeeInfo.LastModifiedDate = info.LastModifiedDate;
                        tmpEmloyeeInfo.LastModifiedUser = info.LastModifiedUser;

                        employeeList.Add(tmpEmloyeeInfo);

                    }
                    catch
                    {

                    }

                }
            }
        }

        foreach (var tmp in employeeList)
        {
            this.Save(tmp);
        }
    }


    public byte[] Export(string clientID, string employeeID)
    {
        //get creditor data db.Open();
        List<ClientEmployeeMasterInfo> data = null;
        String query = @"
select * from ClientEmployeeMaster
where ClientID = @ClientID

";
        try
        {
            data = (List<ClientEmployeeMasterInfo>)db.Query<ClientEmployeeMasterInfo>(query, new { ClientID = clientID, CreditorID = employeeID });
        }
        finally
        {
            db.Close();
        }


        string templatePath = HttpContext.Current.Server.MapPath("~/Template/DataImport/EmployeeImportTemplate.xls");
        byte[] FileData = File.ReadAllBytes(templatePath);

        HSSFWorkbook hssfwb;

        using (Stream file = new MemoryStream(FileData))
        {
            hssfwb = new HSSFWorkbook(file);
        }

        ISheet sheet = hssfwb.GetSheetAt(0);

        IRow tmpRow = null;
        int currentRow = 0;
        int currentColumn = 0;

        IDataFormat dataFormatCustom = hssfwb.CreateDataFormat();
        ICell tmpCell = null;
        if (data != null)
        {
            foreach (ClientEmployeeMasterInfo info in data)
            {
                tmpRow = sheet.CreateRow(++currentRow);
                currentColumn = 0;
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.ClientStaffID);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.ClientStaffName);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.ClientStaffYear);

                if (info.ContractDate != null)
                {
                    tmpCell = tmpRow.CreateCell(currentColumn++);
                    tmpCell.SetCellValue(info.ContractDate.Value); 
                    tmpCell.CellStyle.DataFormat = dataFormatCustom.GetFormat("yyyy-MM-dd");
                }
                else
                {
                    tmpRow.CreateCell(currentColumn++).SetCellValue("");
                }

                if (info.ContractExpDate != null)
                {
                    tmpCell = tmpRow.CreateCell(currentColumn++);
                    tmpCell.SetCellValue(info.ContractExpDate.Value); 
                    tmpCell.CellStyle.DataFormat = dataFormatCustom.GetFormat("yyyy-MM-dd");
                }
                else
                {
                    tmpRow.CreateCell(currentColumn++).SetCellValue("");
                }

                //tmpRow.CreateCell(currentColumn++).SetCellValue(info.ContractDate);
                //tmpRow.CreateCell(currentColumn++).SetCellValue(info.ContractExpDate);
                 
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.MonthlyWage);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.AnnualBonus);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.AvgWage);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.NoReleaseWage);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.NoReleaseBonus);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.NoReimbursementAmt);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.Total);
            }
        }

        MemoryStream bos = new MemoryStream();
        hssfwb.Write(bos);
        byte[] bytes = bos.ToArray();
        bos.Close();

        return bytes;
    }



    #region Save
    public bool Save(ClientEmployeeMasterInfo employeeMasterInfo)
    {
        bool IsEmployeeExisted = this.IsEmployeeExisted(employeeMasterInfo.ClientID, employeeMasterInfo.ClientStaffID);
        if (IsEmployeeExisted)
            this.Update(employeeMasterInfo);
        else
            this.Insert(employeeMasterInfo);
        return IsEmployeeExisted;
    }
    #endregion

    #region Insert
    public void Insert(ClientEmployeeMasterInfo employeeMasterInfo)
    {
        db.Open();
        string query = "INSERT INTO [dbo].[ClientEmployeeMaster] "
                    + "([ClientID] "
                    + ",[ClientStaffID] "
                    + ",[ClientStaffYear] "
                    + ",[ClientStaffName] "
                    + ",[ContractDate] "
                    + ",[ContractExpDate] "
                    + ",[MonthlyWage] "
                    + ",[AnnualBonus] "
                    + ",[AvgWage] "
                    + ",[NoReleaseWage] "
                    + ",[NoReleaseBonus] "
                    + ",[NoReimbursementAmt] "
                    + ",[Total] "
                    + ",[CreateDate] "
                    + ",[CreateUser] "
                    + ",[LastModifiedDate] "
                    + ",[LastModifiedUser]) "
                    + "VALUES "
                    + "(@ClientID "
                    + ",@ClientStaffID "
                    + ",@ClientStaffYear "
                    + ",@ClientStaffName "
                    + ",@ContractDate "
                    + ",@ContractExpDate "
                    + ",@MonthlyWage "
                    + ",@AnnualBonus "
                    + ",@AvgWage "
                    + ",@NoReleaseWage "
                    + ",@NoReleaseBonus "
                    + ",@NoReimbursementAmt "
                    + ",@Total"
                    + ",@CreateDate "
                    + ",@CreateUser "
                    + ",@LastModifiedDate "
                    + ",@LastModifiedUser) ";

        db.Execute(query, employeeMasterInfo);
        db.Close();
    }
    #endregion

    #region Update
    public void Update(ClientEmployeeMasterInfo employeeMasterInfo)
    {
        db.Open();
        String query =
              "UPDATE [dbo].[ClientEmployeeMaster] "
            + "SET  "
            + " [ClientStaffYear] = @ClientStaffYear "
            + ",[ClientStaffName] = @ClientStaffName "
            + ",[ContractDate] = @ContractDate "
            + ",[ContractExpDate] = @ContractExpDate "
            + ",[MonthlyWage] = @MonthlyWage "
            + ",[AnnualBonus] = @AnnualBonus "
            + ",[AvgWage] = @AvgWage "
            + ",[NoReleaseWage] = @NoReleaseWage "
            + ",[NoReleaseBonus] = @NoReleaseBonus "
            + ",[NoReimbursementAmt] = @NoReimbursementAmt "
            + ",[Total] = @Total "
            + ",[LastModifiedDate] = @LastModifiedDate "
            + ",[LastModifiedUser] = @LastModifiedUser "
            + "WHERE ClientID = @ClientID and ClientStaffID = @ClientStaffID";
        db.Execute(query, employeeMasterInfo);
        db.Close();
    }
    #endregion

    #region Get
    public List<string> GetClientStaffIDList(string clientID, string clientStaffID)
    {
        db.Open();
        String query = "select top 10 ClientStaffID from ClientEmployeeMaster where ClientID = @ClientID and (@ClientStaffID = '' or ClientStaffID like '%' + @ClientStaffID + '%') order by ClientStaffID";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, ClientStaffID = clientStaffID });
        db.Close();
        return obj;
    }

    public List<ClientEmployeeMasterInfo> Get(string clientID)
    {
        db.Open();
        String query = "SELECT * from ClientEmployeeMaster WHERE ClientID = @ClientID ORDER BY ClientStaffID";
        List<ClientEmployeeMasterInfo> obj = (List<ClientEmployeeMasterInfo>)db.Query<ClientEmployeeMasterInfo>(query, new { ClientID = clientID });
        db.Close();
        return obj;
    }
    #endregion

    #region Delete
    public void Delete(string clientID, string clientStaffID)
    {
        db.Open(); ;
        String query = "Delete From ClientEmployeeMaster where ClientID = @ClientID and ClientStaffID = @ClientStaffID ";
        db.Execute(query, new { ClientID = clientID, ClientStaffID = clientStaffID });
        db.Close();
    }
    #endregion

    #region Check exists
    public bool IsEmployeeExisted(string clientID, string clientStaffID)
    {
        db.Open();
        String query = "select count(*) from ClientEmployeeMaster where ClientID = @ClientID and ClientStaffID = @ClientStaffID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, ClientStaffID = clientStaffID });
        db.Close();
        return obj[0] > 0;
    }
    #endregion
}