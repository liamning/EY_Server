using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.IO;
using Dapper;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class CreditorMaster
{
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString);
    SqlTransaction transaction;

    public CreditorMaster()
    {
    }

    public byte[] GenerateCreditorAmountReport(string clientID)
    {
        HSSFWorkbook hssfwb = new HSSFWorkbook();
        IRow tmpRow;

        string query = @"
SELECT CreditorMaster.[ClientID]
      ,CreditorMaster.[CreditorID] 
      ,CreditorMaster.CreditorName 
      ,ChiDesc [CreditorType]
      ,[CreditorAuditDetail].[Currency]
      ,isnull([BookAmt],0) [BookAmt]
      ,isnull([DeclareAmt],0) [DeclareAmt]
      ,isnull([AdminExamineNotConfirm],0) [AdminExamineNotConfirm]
      ,isnull([AdminExamineWaitConfirm],0) [AdminExamineWaitConfirm]
      ,isnull([AdminExamineConfirm],0)[AdminExamineConfirm]
      ,isnull([LawyerExamineNotConfirm],0)[LawyerExamineNotConfirm]
      ,isnull([LawyerExamineWaitConfirm],0)[LawyerExamineWaitConfirm]
      ,isnull([LawyerExamineConfirm]  ,0)[LawyerExamineConfirm]
  FROM [dbo].[CreditorAuditDetail]
  join CreditorMaster on [CreditorAuditDetail].ClientID = CreditorMaster.ClientID and [CreditorAuditDetail].[CreditorID] = CreditorMaster.[CreditorID]
join GeneralMaster on Category = 'CreditType' and Code = [CreditorAuditDetail].CreditorType
where  [CreditorAuditDetail].ClientID = @ClientID

";

        var result = this.db.Query(query, new { ClientID = clientID });
        ISheet sheetSummary = hssfwb.CreateSheet("債權金額");

        string[] header = new string[] { 
            "客户编号",
"债权人编号",
"债权人姓名/名称",
"债权类别",
"币种",
"企业账面金额",
"申报金额",
"不予确认(管理人)",
"暂缓确认 (管理人)",
"债权确认金额(管理人)",
"不予确认(律师)",
"暂缓确认(律师)",
"债权确认金额(律师)",

        };

        tmpRow = sheetSummary.CreateRow(0);
        for (int i = 0; i < header.Length; i++)
        {
            tmpRow.CreateCell(i).SetCellValue(header[i]);
        }

        int rowNo = 1;
        int cellNo = 0;
        foreach (var obj in result)
        {
            tmpRow = sheetSummary.CreateRow(rowNo);

            cellNo = 0;
            tmpRow.CreateCell(cellNo++).SetCellValue(obj.ClientID);
            tmpRow.CreateCell(cellNo++).SetCellValue(obj.CreditorID);
            tmpRow.CreateCell(cellNo++).SetCellValue(obj.CreditorName);
            tmpRow.CreateCell(cellNo++).SetCellValue(obj.CreditorType);
            tmpRow.CreateCell(cellNo++).SetCellValue(obj.Currency);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.BookAmt);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.DeclareAmt);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.AdminExamineNotConfirm);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.AdminExamineWaitConfirm);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.AdminExamineConfirm);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.LawyerExamineNotConfirm);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.LawyerExamineWaitConfirm);
            tmpRow.CreateCell(cellNo++).SetCellValue((double)obj.LawyerExamineConfirm);

            rowNo++;
        }

        for (int i = 0; i < header.Length; i++)
        {
            sheetSummary.AutoSizeColumn(i);
        }



        //var tmpFolder = HttpContext.Current.Server.MapPath(string.Format("~/tmp/CreditImportLog/{0}", "test"));
        //var fileName = DateTime.Now.Ticks.ToString() + ".xls";
        //var tmpFilePath = tmpFolder + "\\" + fileName;
        //if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);
        //FileStream fileOut = File.Create(tmpFilePath);

        //hssfwb.Write(fileOut);

        MemoryStream outStream = new MemoryStream();
        hssfwb.Write(outStream);

        byte[] bytes = outStream.ToArray();

        //byte[] bytes = bos.toByteArray();



        return bytes;
    }

    public List<CreditorPreviewInfo> Search(string clientID, string userID, Dictionary<string, string> criteria, Dictionary<string, string> fieldsDataType, Dictionary<string, string> criteriaOperator)
    {
        List<string> criteriaList = new List<string>();
        string value = null, op = null, sqlField;
        foreach (string field in criteria.Keys)
        {
            if (field == "ClientID") continue;
            if (string.IsNullOrEmpty(criteria[field].Trim())) continue;

            op = " " + criteriaOperator[field] + " ";
            value = string.Format("N'{0}'", criteria[field]);
            sqlField = field;
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
                    sqlField = field.Substring(0, field.IndexOf("_"));
                    break;
            }
            criteriaList.Add(string.Format("CreditorMaster.{0} {1} {2}", sqlField, op, value));
        }


        //3 = admin
        db.Open();
        string query =
@"SELECT CreditorID, CreditorName ,GR1.ChiDesc CreditorType,GR2.ChiDesc Status, GR3.ChiDesc MainCreditor 
FROM [dbo].[CreditorMaster] 
left outer join GeneralMaster GR1 on  [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType'
left outer join GeneralMaster GR2 on  [CreditorMaster].Status = GR2.Code and GR2.Category = 'Status'
left outer join GeneralMaster GR3 on  [CreditorMaster].MainCreditor = GR3.Code and GR3.Category = 'YesNo'
WHERE ClientID = @ClientID  
AND ((exists (select 1 from ResponsibleStaffList where ClientID = @ClientID and StaffID = @UserID and ResponsibilityType in (1,3))) or ResponsiblePerson = @UserID or ResponsiblePerson is null) "
+ (criteriaList.Count > 0 ? " and " + string.Join(" and ", criteriaList.ToArray()) : "")
+ "ORDER BY CreditorID";

        List<CreditorPreviewInfo> obj = (List<CreditorPreviewInfo>)db.Query<CreditorPreviewInfo>(query, new { ClientID = clientID, UserID = userID });
        db.Close();
        return obj;
    }

    public CreditorMasterInfo Create()
    {
        var creditType = new ComboValueMaster().Get("CreditType");
        CreditorMasterInfo tmpCreditorInfo = new CreditorMasterInfo();


        //public List<CreditorAuditDetailInfo> CreditorAuditDetailList { get; set; }
        //public List<ExamineRecordInfo> ExamineRecordList { get; set; }
        //public List<CreditorAttachmentInfo> CreditorAttachmentList { get; set; }
        //public List<ChangeRecordInfo> ChangeRecordList { get; set; }

        tmpCreditorInfo.ExamineRecordList = new List<ExamineRecordInfo>();
        tmpCreditorInfo.CreditorAttachmentList = new List<CreditorAttachmentInfo>();
        tmpCreditorInfo.ChangeRecordList = new List<ChangeRecordInfo>();

        tmpCreditorInfo.CreditorAuditDetailList = new List<CreditorAuditDetailInfo>();
        //foreach (var creditTypeObj in creditType)
        //{
        //    tmpCreditorInfo.CreditorAuditDetailList.Add(new CreditorAuditDetailInfo()
        //    {
        //        ClientID = tmpCreditorInfo.ClientID,
        //        CreditorID = tmpCreditorInfo.CreditorID,
        //        CreditorType = creditTypeObj.ChiDesc,
        //        RowNo = creditTypeObj.Seq,
        //    });
        //}



        return tmpCreditorInfo;

    }

    public bool CheckCreditorIDNo(string clientID, string creditorID, string creditorIDNo)
    {
        db.Open();

        string query = @"
                        select count(*) total FROM [dbo].[CreditorMaster] 
                        WHERE ClientID = @ClientID and CreditorID = @CreditorID and CreditorIDNo = @CreditorIDNo
";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, CreditorIDNo = creditorIDNo });
        db.Close();
        return obj[0] > 0;
    }

    public CreditorMasterInfo Compare(CreditorMasterInfo creditorMasterInfo)
    {
        CreditorMasterInfo existingCreditor = this.GetCreditorForExport(creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID);
        //CreditorAmountInfo existingCreditorAmount;
        //CreditorAmountInfo NewCreditorAmount;

        CreditorAuditDetailInfo existingCreditorAuditDetailInfo;
        CreditorAuditDetailInfo NewCreditorAuditDetailInfo;


        bool isUpdate = false;

        //start complare
        if (existingCreditor != null)
        {
            if (creditorMasterInfo.CreditorName != existingCreditor.CreditorName)
                isUpdate = true;

            //for (int i = 0; i < creditorMasterInfo.CreditorAmountList.Count; i++)
            //{
            //    existingCreditorAmount = existingCreditor.CreditorAmountList[i];
            //    NewCreditorAmount = creditorMasterInfo.CreditorAmountList[i];

            //    if (existingCreditorAmount.Currency != NewCreditorAmount.Currency
            //        || (decimal?)existingCreditorAmount.Amount != (decimal?)NewCreditorAmount.Amount)
            //    {
            //        isUpdate = true;
            //        break;
            //    }
            //}

            for (int i = 0; i < creditorMasterInfo.CreditorAuditDetailList.Count; i++)
            {
                existingCreditorAuditDetailInfo = existingCreditor.CreditorAuditDetailList[i];
                NewCreditorAuditDetailInfo = creditorMasterInfo.CreditorAuditDetailList[i];

                if (
                    existingCreditorAuditDetailInfo.CreditorType != NewCreditorAuditDetailInfo.CreditorType
                    || (decimal?)existingCreditorAuditDetailInfo.BookAmt != (decimal?)NewCreditorAuditDetailInfo.BookAmt
                    || (decimal?)existingCreditorAuditDetailInfo.DeclareAmt != (decimal?)NewCreditorAuditDetailInfo.DeclareAmt
                    || (decimal?)existingCreditorAuditDetailInfo.AdminExamineConfirm != (decimal?)NewCreditorAuditDetailInfo.AdminExamineConfirm
                    || (decimal?)existingCreditorAuditDetailInfo.AdminExamineWaitConfirm != (decimal?)NewCreditorAuditDetailInfo.AdminExamineWaitConfirm
                    || (decimal?)existingCreditorAuditDetailInfo.AdminExamineNotConfirm != (decimal?)NewCreditorAuditDetailInfo.AdminExamineNotConfirm
                    || (decimal?)existingCreditorAuditDetailInfo.LawyerExamineConfirm != (decimal?)NewCreditorAuditDetailInfo.LawyerExamineConfirm
                    || (decimal?)existingCreditorAuditDetailInfo.LawyerExamineNotConfirm != (decimal?)NewCreditorAuditDetailInfo.LawyerExamineNotConfirm
                    || (decimal?)existingCreditorAuditDetailInfo.LawyerExamineWaitConfirm != (decimal?)NewCreditorAuditDetailInfo.LawyerExamineWaitConfirm

                    )
                {
                    isUpdate = true;
                    break;
                }
            }

            if (isUpdate)
                creditorMasterInfo.ImportStatus = CreditorMasterInfo.EnumImportStatus.Update;

        }
        else
        {
            creditorMasterInfo.ImportStatus = CreditorMasterInfo.EnumImportStatus.InvalidCreditorID;
        }

        return creditorMasterInfo;
    }



    #region Save

    public CreditorMasterInfo Save(CreditorMasterInfo creditorMasterInfo)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {

            if (this.IsExisted(creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID))
            {
                creditorMasterInfo = this.Update(creditorMasterInfo);
            }

            else
                creditorMasterInfo = this.Insert(creditorMasterInfo);


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
        return creditorMasterInfo;
    }

    public void Save(Dictionary<string,CreditorMasterInfo> creditorDict)
    {
        db.Open();
        transaction = db.BeginTransaction();
        try
        {
            CreditorMasterInfo creditorMasterInfo;
            foreach (var key in creditorDict.Keys)
            {

                creditorMasterInfo = creditorDict[key];
                creditorMasterInfo.ImportStatus = CreditorMasterInfo.EnumImportStatus.None;

                if (!string.IsNullOrEmpty(creditorMasterInfo.CreditorID))
                    this.Compare(creditorMasterInfo);
                else
                    creditorMasterInfo.ImportStatus = CreditorMasterInfo.EnumImportStatus.New;

                if (creditorMasterInfo.ImportStatus == CreditorMasterInfo.EnumImportStatus.InvalidCreditorID) continue;

                creditorMasterInfo = creditorDict[key];
                if (this.IsExisted(creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID))
                {
                    creditorMasterInfo = this.Update(creditorMasterInfo);
                }

                else
                    creditorMasterInfo = this.Insert(creditorMasterInfo);

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


    public List<string> SaveContact(List<CreditorMasterInfo> creditorList)
    {
        db.Open();
        transaction = db.BeginTransaction();
        List<string> invalidCreditorIDList = new List<string>();
        try
        {
            
            foreach (CreditorMasterInfo creditorMasterInfo in creditorList)
            { 
                if (this.IsExisted(creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID))
                {
                    this.UpdateContact(creditorMasterInfo);
                }
                else
                {
                    invalidCreditorIDList.Add(creditorMasterInfo.CreditorID);
                }
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
        return invalidCreditorIDList;
    }


    public string Import(ImportInfo info)
    {
        HSSFWorkbook hssfwb;
        Byte[] bytes = Convert.FromBase64String(info.DataURL);

        using (Stream file = new MemoryStream(bytes))
        {
            hssfwb = new HSSFWorkbook(file);
        }

        var creditType = new ComboValueMaster().Get("CreditType");

        Dictionary<string, string> fieldDescMapping = new Dictionary<string, string>();
        fieldDescMapping.Add("LotNo", "编号");
        fieldDescMapping.Add("CreditorID", "债权人编号");
        fieldDescMapping.Add("CreditorName", "债权人名称");
        fieldDescMapping.Add("Currency", "币种");
        fieldDescMapping.Add("CreditType", "债权性质");
        fieldDescMapping.Add("BookAmt", "清册金额（法院版）");
        fieldDescMapping.Add("DeclareAmt", "申报金额");
        fieldDescMapping.Add("AdminExamineConfirm", "债权确认金额");
        fieldDescMapping.Add("AdminExamineWaitConfirm", "暂缓确认金额");
        fieldDescMapping.Add("AdminExamineNotConfirm", "不予确认金额");
        fieldDescMapping.Add("LawyerExamineConfirm", "律师确认债权表金额");
        fieldDescMapping.Add("LawyerExamineWaitConfirm", "律师暂缓确认金额");
        fieldDescMapping.Add("LawyerExamineNotConfirm", "律师不予确认金额");

         
        Dictionary<string, int> fieldColumnNoMapping = new Dictionary<string, int>();

        ISheet sheet = hssfwb.GetSheet("Master");
        IRow tmpRow = null;
        ICell tmpCell = null;
        Dictionary<string, CreditorMasterInfo> creditorDict = new Dictionary<string, CreditorMasterInfo>();
        Dictionary<string, List<string>> creditorRowNoDict = new Dictionary<string, List<string>>();
        CreditorMasterInfo tmpCreditorInfo = null;
        string tmpCreditorID = null, tmpCreditorName = null;
        string tmpCurrency = "RMB";
        decimal declareAmt = 0;
        int headerRow = 0;
         
        int row = 0;
        for (; row <= sheet.LastRowNum; row++)
        {
            tmpRow = sheet.GetRow(row);
            //null is when the row only contains empty cells 
            if (tmpRow == null) continue;
            if (tmpRow.GetCell(0) == null) continue;
            if (tmpRow.GetCell(0).CellType == CellType.String)
            {
                headerRow = row;
                for (int col = 0; col < tmpRow.LastCellNum; col++)
                {
                    tmpCell = tmpRow.GetCell(col);
                    if (tmpCell != null)
                        foreach (var field in fieldDescMapping.Keys)
                        {
                            if (tmpCell.StringCellValue == fieldDescMapping[field])
                            {
                                fieldColumnNoMapping.Add(field, col);
                            }
                        }
                }
                break;
            } 
        }
            for (; row <= sheet.LastRowNum; row++)
            {
                tmpRow = sheet.GetRow(row);
                //null is when the row only contains empty cells 
                if (tmpRow == null) continue;
                if (tmpRow.GetCell(0) == null) continue;
                if (tmpRow.GetCell(0).CellType != CellType.Numeric) continue;
                try
                {
                    tmpCreditorID = tmpRow.GetCell(fieldColumnNoMapping["CreditorID"]).StringCellValue;
                    tmpCreditorName = tmpRow.GetCell(fieldColumnNoMapping["CreditorName"]).StringCellValue;

                    if (creditorRowNoDict.ContainsKey(tmpCreditorName))
                    {
                        creditorRowNoDict[tmpCreditorName].Add(string.Format("{0}", row));
                        tmpCreditorInfo = creditorDict[creditorRowNoDict[tmpCreditorName][0]];
                    }
                    else
                    {
                        tmpCreditorInfo = new CreditorMasterInfo();
                        creditorRowNoDict.Add(tmpCreditorName, new List<string>());
                        creditorRowNoDict[tmpCreditorName].Add(string.Format("{0}", row));
                        creditorDict.Add(creditorRowNoDict[tmpCreditorName][0], tmpCreditorInfo);

                        //init the credit type
                        tmpCreditorInfo.CreditorAuditDetailList = new List<CreditorAuditDetailInfo>();
                        foreach (var creditTypeObj in creditType)
                        {
                            tmpCreditorInfo.CreditorAuditDetailList.Add(new CreditorAuditDetailInfo()
                            {
                                ClientID = tmpCreditorInfo.ClientID,
                                CreditorID = tmpCreditorInfo.CreditorID,
                                CreditorType = creditTypeObj.Code,
                                CreditorTypeCode = creditTypeObj.Code,
                                CreditTypeDesc = creditTypeObj.ChiDesc,
                                RowNo = creditTypeObj.Seq,
                                Currency = tmpCurrency,

                                BookAmt = 0,
                                DeclareAmt = 0,
                                AdminExamineConfirm = 0,
                                AdminExamineWaitConfirm = 0,
                                AdminExamineNotConfirm = 0,
                                LawyerExamineConfirm = 0,
                                LawyerExamineNotConfirm = 0,
                                LawyerExamineWaitConfirm = 0,

                                CreateDate = info.CreateDate,
                                CreateUser = info.CreateUser,
                                LastModifiedDate = info.LastModifiedDate,
                                LastModifiedUser = info.LastModifiedUser
                            });
                        }

                        tmpCreditorInfo.CreditorAmountList = new List<CreditorAmountInfo>();
                        tmpCreditorInfo.CreditorAmountList.Add(new CreditorAmountInfo()
                        {
                            ClientID = tmpCreditorInfo.ClientID,
                            CreditorID = tmpCreditorInfo.CreditorID,
                            RowNo = 1,
                            //Amount = declareAmt,
                            Currency = tmpCurrency,
                            CreateDate = info.CreateDate,
                            CreateUser = info.CreateUser,
                            LastModifiedDate = info.LastModifiedDate,
                            LastModifiedUser = info.LastModifiedUser
                        });

                        tmpCreditorInfo.ClientID = info.ClientID;
                        tmpCreditorInfo.CreditorID = tmpCreditorID;
                        tmpCreditorInfo.CreditorName = tmpCreditorName;
                        tmpCreditorInfo.Status = "1";

                        tmpCreditorInfo.CreateDate = info.CreateDate;
                        tmpCreditorInfo.CreateUser = info.CreateUser;
                        tmpCreditorInfo.LastModifiedDate = info.LastModifiedDate;
                        tmpCreditorInfo.LastModifiedUser = info.LastModifiedUser;
                    }

                    declareAmt = 0;
                    foreach (var creditTypeObj in tmpCreditorInfo.CreditorAuditDetailList)
                    {
                        //match the credit type by the description for sample "普通债权" = "普通债权" and if no credit type is matched, default the credit type to "普通债权(Code: 4)"
                        if (creditTypeObj.CreditorTypeCode != "4" && creditTypeObj.CreditTypeDesc != tmpRow.GetCell(fieldColumnNoMapping["CreditType"]).StringCellValue) continue;

                        if (fieldColumnNoMapping.ContainsKey("Currency") && tmpRow.GetCell(fieldColumnNoMapping["Currency"]) != null && tmpRow.GetCell(fieldColumnNoMapping["Currency"]).CellType == CellType.String)
                            creditTypeObj.Currency = tmpRow.GetCell(fieldColumnNoMapping["Currency"]).StringCellValue;
                             
                        creditTypeObj.BookAmt += tmpRow.GetCell(fieldColumnNoMapping["BookAmt"]) != null && tmpRow.GetCell(fieldColumnNoMapping["BookAmt"]).CellType == CellType.Numeric
                            ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["BookAmt"]).NumericCellValue : 0;

                        creditTypeObj.DeclareAmt += tmpRow.GetCell(fieldColumnNoMapping["DeclareAmt"]) != null
                            && tmpRow.GetCell(fieldColumnNoMapping["DeclareAmt"]).CellType == CellType.Numeric ?
                            (decimal)tmpRow.GetCell(fieldColumnNoMapping["DeclareAmt"]).NumericCellValue : 0;

                        declareAmt = creditTypeObj.DeclareAmt ?? 0;

                        creditTypeObj.AdminExamineConfirm += tmpRow.GetCell(fieldColumnNoMapping["AdminExamineConfirm"]) != null
                            && tmpRow.GetCell(fieldColumnNoMapping["AdminExamineConfirm"]).CellType == CellType.Numeric
                            ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["AdminExamineConfirm"]).NumericCellValue : 0;

                        creditTypeObj.AdminExamineWaitConfirm += tmpRow.GetCell(fieldColumnNoMapping["AdminExamineWaitConfirm"]) != null
                            && tmpRow.GetCell(fieldColumnNoMapping["AdminExamineWaitConfirm"]).CellType == CellType.Numeric
                            ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["AdminExamineWaitConfirm"]).NumericCellValue : 0;

                        creditTypeObj.AdminExamineNotConfirm += tmpRow.GetCell(fieldColumnNoMapping["AdminExamineNotConfirm"]) != null
                            && tmpRow.GetCell(fieldColumnNoMapping["AdminExamineNotConfirm"]).CellType == CellType.Numeric
                            ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["AdminExamineNotConfirm"]).NumericCellValue : 0;

                        if (fieldColumnNoMapping.ContainsKey("LawyerExamineConfirm"))
                            creditTypeObj.LawyerExamineConfirm += tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineConfirm"]) != null
                                && tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineConfirm"]).CellType == CellType.Numeric
                                ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineConfirm"]).NumericCellValue : 0;

                        if (fieldColumnNoMapping.ContainsKey("LawyerExamineNotConfirm"))
                            creditTypeObj.LawyerExamineNotConfirm = tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineNotConfirm"]) != null
                                && tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineNotConfirm"]).CellType == CellType.Numeric
                                ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineNotConfirm"]).NumericCellValue : 0;

                        if (fieldColumnNoMapping.ContainsKey("LawyerExamineWaitConfirm"))
                            creditTypeObj.LawyerExamineWaitConfirm = tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineWaitConfirm"]) != null
                                && tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineWaitConfirm"]).CellType == CellType.Numeric
                                ? (decimal)tmpRow.GetCell(fieldColumnNoMapping["LawyerExamineWaitConfirm"]).NumericCellValue : 0;

                        break;
                    }

                    //summarize the total credit amount
                    tmpCreditorInfo.CreditorAmountList[0].Amount += declareAmt;

                }
                catch
                {
                    throw;
                }
            }

        //update creditor  
        this.Save(creditorDict);


        //update the excel file and add report log
        int rowNo = headerRow;
        List<string> creditIDList = new List<string>();

        //keep the first sheet only
        int NumberOfSheets = hssfwb.NumberOfSheets;
        int sheetIndex = hssfwb.GetSheetIndex(sheet);
        for (int i = 1; i < NumberOfSheets - sheetIndex; i++)
        {
            hssfwb.RemoveSheetAt(sheetIndex + 1);
        }


        ISheet UpdatedSheet = hssfwb.CreateSheet("Creditor Updated");
        ISheet invalidIDSheet = hssfwb.CreateSheet("Invalid Creditor ID");

        int newRowNo1 = -1;
        int newRowNo2 = -1;

        CopyRow(hssfwb, sheet, UpdatedSheet, rowNo, (++newRowNo1));
        CopyRow(hssfwb, sheet, invalidIDSheet, rowNo, (++newRowNo2));

        CreditorMasterInfo tmpCreditor;
        foreach (string key in creditorDict.Keys)
        {
            tmpCreditor = creditorDict[key];
            foreach (string rowString in creditorRowNoDict[tmpCreditor.CreditorName])
            { 
                rowNo = Convert.ToInt16(rowString);

                tmpRow = sheet.GetRow(rowNo);

                creditIDList.Add(tmpCreditor.CreditorID);

                switch (tmpCreditor.ImportStatus)
                {
                    case CreditorMasterInfo.EnumImportStatus.Update:
                        //list the update the creditor to 'Creditor Updated' sheet
                        CopyRow(hssfwb, sheet, UpdatedSheet, rowNo, (++newRowNo1));
                        break;
                    case CreditorMasterInfo.EnumImportStatus.InvalidCreditorID:
                        //list the update the creditor to 'Creditor Updated' sheet
                        CopyRow(hssfwb, sheet, invalidIDSheet, rowNo, (++newRowNo2));
                        break;
                    case CreditorMasterInfo.EnumImportStatus.New:
                        //update the creditor ID to excel file
                        tmpRow.GetCell(1).SetCellValue(tmpCreditor.CreditorID);
                        break;

                }
            } 
        }

        //remove creditorID
        List<string> removedIDList = this.GetRemoveData(info.ClientID,creditIDList);
        ISheet sheet_IDRemoved = hssfwb.CreateSheet("Creditor Removed in Excel");
        for (int i = 0; i < removedIDList.Count; i++)
        {
            tmpRow = sheet_IDRemoved.CreateRow(i);
            tmpRow.CreateCell(0).SetCellValue(removedIDList[i]);
        }

        //GetSummary
        var summary = this.GetSummary(info.ClientID);
        ISheet sheetSummary = hssfwb.CreateSheet("Summary");

        string[] header = new string[] { 
            "债权人数",
            "企业账面金额",
            "申报金额",
            "管理人不予确认",
            "管理人暂缓确认",
            "管理人债权确认金额",
            "律师不予确认",
            "律师暂缓确认",
            "律师债权确认金额 ",
        };

        tmpRow = sheetSummary.CreateRow(0);
        for (int i = 0; i < header.Length; i++)
        {
            tmpRow.CreateCell(i).SetCellValue(header[i]);
        }

        tmpRow = sheetSummary.CreateRow(1);
        for (int i = 0; i < summary.Count; i++)
        {
            tmpRow.CreateCell(i).SetCellValue((double)summary[i]);
        }

        var tmpFolder = HttpContext.Current.Server.MapPath(string.Format("~/tmp/CreditImportLog/{0}", info.ClientID));
        var fileName = DateTime.Now.Ticks.ToString() + ".xls";
        var tmpFilePath = tmpFolder + "\\" + fileName;
        if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);
        FileStream fileOut = File.Create(tmpFilePath);

        hssfwb.Write(fileOut);
        fileOut.Flush();
        fileOut.Close();

        return fileName;

    }


    public List<string> ImportContact(ImportInfo info)
    {
        HSSFWorkbook hssfwb;
        Byte[] bytes = Convert.FromBase64String(info.DataURL);

        using (Stream file = new MemoryStream(bytes))
        {
            hssfwb = new HSSFWorkbook(file);
        }

        var CreditorType = new ComboValueMaster().Get("CreditorType");
         
          
        ISheet sheet = hssfwb.GetSheetAt(0);
        IRow tmpRow = null; 
        ICell tmpCell = null;
        List<CreditorMasterInfo> creditorList = new List<CreditorMasterInfo>();
        CreditorMasterInfo tmpCreditorInfo = null;
        //string tmpCreditorID = null, tmpCreditorName = null;
         

        int row = 1;
        int col;
        for (; row <= sheet.LastRowNum; row++)
        {
            tmpRow = sheet.GetRow(row);
            //null is when the row only contains empty cells 
            if (tmpRow == null) continue;
            if (tmpRow.GetCell(0) == null) continue;
            //if (tmpRow.GetCell(0).CellType != CellType.Numeric) continue;
            
            try
            {

                col = 0;
                tmpCreditorInfo = new CreditorMasterInfo();

                tmpCreditorInfo.ClientID = info.ClientID;
                tmpCreditorInfo.CreditorID = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditorName = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CompanyTypeDesc = tmpRow.GetCell(col++).StringCellValue;

                foreach(var codeDesc in CreditorType){
                    if (codeDesc.ChiDesc == tmpCreditorInfo.CompanyTypeDesc)
                    {
                        tmpCreditorInfo.CompanyType = codeDesc.Code;
                        break;
                    }
                }

                //ReportDate
                tmpCell = tmpRow.GetCell(col++);
                if (tmpCell.CellType == CellType.Numeric)
                { 
                    tmpCreditorInfo.ReportDate = tmpCell.DateCellValue;
                }
                
                tmpCreditorInfo.CreditorIDNo = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.LegalRepresentative = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.LegalRepPhone = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditAgent = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditAgentPhone = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditAgentIDNo = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditorAddress = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.CreditAgentAddress = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.BusinessCode = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.RegPlace = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.BankName = tmpRow.GetCell(col++).StringCellValue;
                tmpCreditorInfo.BankAccount = tmpRow.GetCell(col++).StringCellValue;

                creditorList.Add(tmpCreditorInfo);
                  
            }
            catch
            {
                throw;
            }
        }

        //update creditor  
        List<string> invalidCreditorIDList = this.SaveContact(creditorList);
         
        //var tmpFolder = HttpContext.Current.Server.MapPath(string.Format("~/tmp/CreditImportLog/{0}", info.ClientID));
        //var fileName = DateTime.Now.Ticks.ToString() + ".xls";
        //var tmpFilePath = tmpFolder + "\\" + fileName;
        //if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);
        //FileStream fileOut = File.Create(tmpFilePath);

        //hssfwb.Write(fileOut);
        //fileOut.Flush();
        //fileOut.Close();

        return invalidCreditorIDList;

    }

    public byte[] Export(string clientID, string creditorID)
    {
        //get creditor data db.Open();
        List<CreditorExportInfo> data = null;
        String query = @"
select CreditorAuditDetail.*, CreditorMaster.CreditorName, GeneralMaster.ChiDesc CreditTypeDesc
from CreditorAuditDetail
join CreditorMaster on CreditorAuditDetail.CreditorID = CreditorMaster.CreditorID
join GeneralMaster on code = CreditorAuditDetail.CreditorType and Category='CreditType'
where CreditorAuditDetail.ClientID = @ClientID 
AND ((exists (select 1 from ResponsibleStaffList where ClientID = @ClientID and StaffID = @CreditorID and ResponsibilityType in (1,3))) or ResponsiblePerson = @CreditorID or ResponsiblePerson is null) 
and (BookAmt > 0
or DeclareAmt > 0
or AdminExamineNotConfirm > 0
or AdminExamineWaitConfirm > 0
or AdminExamineConfirm > 0
or LawyerExamineNotConfirm > 0
or LawyerExamineWaitConfirm > 0
or LawyerExamineConfirm  > 0)
order by CreditorMaster.CreditorName 
";
        try
        {
            data = (List<CreditorExportInfo>)db.Query<CreditorExportInfo>(query, new { ClientID = clientID, CreditorID = creditorID });
        }
        finally
        {
            db.Close();
        }


        string templatePath = HttpContext.Current.Server.MapPath("~/Template/DataImport/CreditorImportTemplate.xls");
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
        if (data != null)
        {
            foreach (CreditorExportInfo info in data)
            {
                tmpRow = sheet.CreateRow(++currentRow);
                currentColumn = 0;
                tmpRow.CreateCell(currentColumn++).SetCellValue(currentRow);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorID);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorName);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.Currency);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditTypeDesc);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.BookAmt);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.DeclareAmt);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.AdminExamineConfirm);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.AdminExamineNotConfirm);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.AdminExamineWaitConfirm);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.LawyerExamineConfirm);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.LawyerExamineNotConfirm);
                tmpRow.CreateCell(currentColumn++).SetCellValue((double)info.LawyerExamineWaitConfirm);
            }
        }

        MemoryStream bos = new MemoryStream();
        hssfwb.Write(bos);
        byte[] bytes = bos.ToArray();
        bos.Close();

        return bytes;
    }


    public byte[] ExportContact(string clientID, string creditorID)
    {
        //get creditor data db.Open();
        List<CreditorMasterInfo> data = null;
        String query = @"
select CreditorMaster.*, GeneralMaster.ChiDesc CompanyTypeDesc
from CreditorMaster
left outer Join GeneralMaster on CreditorMaster.CompanyType = GeneralMaster.Code and GeneralMaster.Category = 'CreditorType'
where CreditorMaster.ClientID = @ClientID
order by CreditorMaster.CreditorName 
";
        try
        {
            data = (List<CreditorMasterInfo>)db.Query<CreditorMasterInfo>(query, new { ClientID = clientID, CreditorID = creditorID });  
        }
        finally
        {
            db.Close();
        }


        string templatePath = HttpContext.Current.Server.MapPath("~/Template/DataImport/CreditorContactImportTemplate.xls"); 
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
            foreach (CreditorMasterInfo info in data)
            {
                tmpRow = sheet.CreateRow(++currentRow);
                currentColumn = 0;
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorID);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorName);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CompanyTypeDesc);
                if (info.ReportDate != null)
                {
                    tmpCell = tmpRow.CreateCell(currentColumn++);
                    tmpCell.SetCellValue(info.ReportDate.Value);
                    //tmpCell.CellStyle = styles["cell"];
                    tmpCell.CellStyle.DataFormat = dataFormatCustom.GetFormat("yyyy-MM-dd");
                }
                else
                { 
                    tmpRow.CreateCell(currentColumn++).SetCellValue("");
                }
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorIDNo);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.LegalRepresentative);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.LegalRepPhone);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditAgent);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditAgentPhone);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditAgentIDNo);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditorAddress);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.CreditAgentAddress);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.BusinessCode);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.RegPlace);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.BankName);
                tmpRow.CreateCell(currentColumn++).SetCellValue(info.BankAccount);
                
            }
        }

        MemoryStream bos = new MemoryStream(); 
        hssfwb.Write(bos); 
        byte[] bytes = bos.ToArray();
        bos.Close();

        return bytes; 
    }

    public List<string> GetRemoveData(string clientID, List<string> creditorIDList)
    {
        string query = "select CreditorID from CreditorMaster where ClientID = @ClientID and CreditorID not in @CreditorIDList";
        List<string> idList = (List<string>)this.db.Query<string>(query, new { ClientID = clientID, CreditorIDList = creditorIDList });

        return idList;
    }

    public List<decimal> GetSummary(string clientID)
    {
        List<decimal> result = new List<decimal>();
        string query = @"select * from

(select 
count(*) CreditorTotal 
from CreditorMaster
where ClientID = @ClientID) tableTotal

join

(select 

isnull(sum(BookAmt),0) BookAmt, 
isnull(sum(DeclareAmt),0) DeclareAmt, 
isnull(sum(AdminExamineNotConfirm),0) AdminExamineNotConfirm, 
isnull(sum(AdminExamineWaitConfirm),0) AdminExamineWaitConfirm, 
isnull(sum(AdminExamineConfirm),0) AdminExamineConfirm,
isnull(sum(LawyerExamineNotConfirm),0) LawyerExamineNotConfirm, 
isnull(sum(LawyerExamineWaitConfirm),0) LawyerExamineWaitConfirm, 
isnull(sum(LawyerExamineConfirm),0) LawyerExamineConfirm

from CreditorAuditDetail
where ClientID = @ClientID) tableSum on 1=1
";
        var tmp = this.db.Query(query, new { ClientID = clientID });
        foreach (var obj in tmp)
        {
            result.Add(obj.CreditorTotal);
            result.Add(obj.BookAmt);
            result.Add(obj.DeclareAmt);

            result.Add(obj.AdminExamineNotConfirm);
            result.Add(obj.AdminExamineWaitConfirm);
            result.Add(obj.AdminExamineConfirm);

            result.Add(obj.LawyerExamineNotConfirm);
            result.Add(obj.LawyerExamineWaitConfirm);
            result.Add(obj.LawyerExamineConfirm);
        }

        return result;
    }

    private static void CopyRow(HSSFWorkbook workbook, ISheet destWorksheet, ISheet resultSheet, int sourceRowNum, int destinationRowNum)
    {
        // Get the source / new row
        IRow newRow = resultSheet.GetRow(destinationRowNum);
        IRow sourceRow = destWorksheet.GetRow(sourceRowNum);

        // If the row exist in destination, push down all rows by 1 else create a new row
        if (newRow != null)
        {
            resultSheet.ShiftRows(destinationRowNum, resultSheet.LastRowNum, 1);
        }
        else
        {
            newRow = resultSheet.CreateRow(destinationRowNum);
        }

        // Loop through source columns to add to new row
        for (int i = 0; i < sourceRow.LastCellNum; i++)
        {
            // Grab a copy of the old/new cell
            ICell oldCell = sourceRow.GetCell(i);
            ICell newCell = newRow.CreateCell(i);

            // If the old cell is null jump to next cell
            if (oldCell == null)
            {
                newCell = null;
                continue;
            }

            //Copy style from old cell and apply to new cell
            //ICellStyle newCellStyle = workbook.CreateCellStyle();
            //newCellStyle.CloneStyleFrom(oldCell.CellStyle);
            newCell.CellStyle = oldCell.CellStyle;

            //// If there is a cell comment, copy
            //if (oldCell.getCellComment() != null) {
            //    newCell.setCellComment(oldCell.getCellComment());
            //}

            // If there is a cell hyperlink, copy
            //if (oldCell.getHyperlink() != null) {
            //    newCell.setHyperlink(oldCell.getHyperlink());
            //}

            // Set the cell data type
            newCell.SetCellType(oldCell.CellType);

            // Set the cell data value
            switch (oldCell.CellType)
            {
                case CellType.Blank:
                    newCell.SetCellValue(oldCell.StringCellValue);
                    break;
                case CellType.Boolean:
                    newCell.SetCellValue(oldCell.BooleanCellValue);
                    break;
                case CellType.Error:
                    newCell.SetCellErrorValue(oldCell.ErrorCellValue);
                    break;
                case CellType.Formula:
                    newCell.SetCellFormula(oldCell.CellFormula);
                    break;
                case CellType.Numeric:
                    newCell.SetCellValue(oldCell.NumericCellValue);
                    break;
                case CellType.String:
                    newCell.SetCellValue(oldCell.RichStringCellValue);
                    break;
            }
        }
    }

    public void SaveCreditorAuditDetail(CreditorAuditDetailInfo creditorAuditDetailInfo)
    {
        if (this.IsCreditorAuditDetailExisted(creditorAuditDetailInfo.ClientID, creditorAuditDetailInfo.CreditorID, creditorAuditDetailInfo.CreditorType, creditorAuditDetailInfo.Currency))
            this.UpdateCreditorAuditDetail(creditorAuditDetailInfo);
        else
            this.InsertCreditorAuditDetail(creditorAuditDetailInfo);
    }

    public void SaveExamineRecord(ExamineRecordInfo examineRecordInfo)
    {
        if (this.IsExamineRecordExisted(examineRecordInfo.ClientID, examineRecordInfo.CreditorID, examineRecordInfo.RowNo))
            this.UpdateExamineRecord(examineRecordInfo);
        else
            this.InsertExamineRecord(examineRecordInfo);
    }

    public void SaveCreditorAttachment(CreditorAttachmentInfo creditorAttachmentInfo)
    {
        if (this.IsCreditorAttachmentExisted(creditorAttachmentInfo.ClientID, creditorAttachmentInfo.CreditorID, creditorAttachmentInfo.RowNo))
            this.UpdateCreditorAttachment(creditorAttachmentInfo);
        else
            this.InsertCreditorAttachment(creditorAttachmentInfo);
    }
    #endregion

    #region Get
    public List<string> GetCreditorIDList(string clientID, string creditorID, string userID)
    {
        db.Open();

        string query = @"
select top 10 CreditorID FROM [dbo].[CreditorMaster] 
WHERE ClientID = @ClientID and (@CreditorID = '' or CreditorID like '%' + @CreditorID + '%')  
AND (@UserID = '' 
or (exists (select 1 from ResponsibleStaffList where ClientID = @ClientID and StaffID = @UserID and ResponsibilityType in (1,3))) 
or ResponsiblePerson is null
or ResponsiblePerson = @UserID) 
ORDER BY CreditorID
";
        var obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, CreditorID = creditorID, UserID = userID });
        db.Close();
        return obj;
    }

    public int GetLastInsertAttachmentID()
    {
        String query = "SELECT Max(ID) FROM [dbo].[Attachment] ";
        return ((List<int>)db.Query<int>(query, null, transaction))[0];
    }

    public string GetStaffName(string staffID)
    {
        String query = "select StaffName from StaffProfile where StaffNo = @StaffNo";
        List<string> list = (List<string>)db.Query<string>(query, new { StaffNo = staffID }, transaction);
        if (list.Count > 0 && list[0] != null)
        {
            return list[0];
        }
        else
            return null;
    }

    public List<ResponsibleStaffSelectionInfo> GetResponsiblePersonList(string clientID)
    {
        db.Open();
        String query = @"SELECT [dbo].[ResponsibleStaffList].StaffID, [dbo].[StaffProfile].StaffName, [dbo].[ResponsibleStaffList].ResponsibilityType 
        FROM [dbo].[ResponsibleStaffList]
        join [dbo].[StaffProfile] on [dbo].[StaffProfile].StaffNo = [dbo].[ResponsibleStaffList].StaffID
        WHERE [dbo].[ResponsibleStaffList].ClientID = @ClientID ";

        List<ResponsibleStaffSelectionInfo> obj = (List<ResponsibleStaffSelectionInfo>)db.Query<ResponsibleStaffSelectionInfo>(query, new { ClientID = clientID });
        db.Close();
        return obj;
    }
    public string GetResponsibleRole(string clientID, string staffID)
    {
        db.Open();
        try
        {
            string query = @"SELECT [dbo].[ResponsibleStaffList].ResponsibilityType 
                            FROM [dbo].[ResponsibleStaffList]
                            join [dbo].[StaffProfile] on [dbo].[StaffProfile].StaffNo = [dbo].[ResponsibleStaffList].StaffID
                            WHERE [dbo].[ResponsibleStaffList].ClientID = @ClientID and StaffID=@StaffID ";

            List<string> obj = (List<string>)db.Query<string>(query, new { ClientID = clientID, StaffID = staffID });
            if (obj.Count > 0)
                return obj[0];
            else
                return "-1";
        }
        finally
        {
            db.Close();
        }

        return null;
    }

    public int? GetMaxRowNoOfExamineRecordList(string clientID, string creditorID)
    {
        db.Open();
        try
        {
            string query = "select MAX(RowNo) from ExamineRecord where ClientID = @ClientID AND CreditorID = @CreditorID";
            List<int?> MaxRowNo = (List<int?>)db.Query<int?>(query, new { ClientID = clientID, CreditorID = creditorID });

            if (MaxRowNo.Count > 0 && MaxRowNo[0] != null)
                return MaxRowNo[0];
        }
        finally
        {
            db.Close();
        }

        return 0;
    }

    public int? GetMaxRowNoOfCreditorAttachmentList(string clientID, string creditorID)
    {
        db.Open();
        try
        {
            string query = "SELECT MAX(RowNo) FROM CreditorAttachment WHERE ClientID = @ClientID AND CreditorID = @CreditorID ";
            List<int?> MaxRowNo = (List<int?>)db.Query<int?>(query, new { ClientID = clientID, CreditorID = creditorID });
            if (MaxRowNo.Count > 0 && MaxRowNo[0] != null)
                return MaxRowNo[0];

        }
        finally
        {
            db.Close();
        }

        return 0;
    }

    public CreditorMasterInfo Get(string clientID, string creditorID, string userID)
    { 
        db.Open();
        String query = @"select * from CreditorMaster 
where ClientID = @ClientID and CreditorID = @CreditorID
AND (
(exists (select 1 from ResponsibleStaffList where ClientID = @ClientID and StaffID = @UserID and ResponsibilityType in (1,3))) 
or ResponsiblePerson = @UserID or ResponsiblePerson is null)
";
        try
        {
            var obj = (List<CreditorMasterInfo>)db.Query<CreditorMasterInfo>(query, new { ClientID = clientID, CreditorID = creditorID, UserID = userID }, this.transaction);

            if (obj.Count > 0)
            {
                obj[0].CreditorAuditDetailList = this.GetCreditorAuditDetailList(clientID, creditorID);
                obj[0].ExamineRecordList = this.GetExamineRecordList(clientID, creditorID);
                obj[0].CreditorAttachmentList = this.GetCreditorAttachmentList(clientID, creditorID);
                obj[0].ChangeRecordList = this.GetChangeRecordList(clientID, creditorID);
                obj[0].CreditorAmountList = new CreditorAmount().GetCreditorAmountList(clientID, creditorID);
                return obj[0];
            }
        }
        finally
        { 
            db.Close();
        }

        return null;
    }


    private CreditorMasterInfo GetCreditorForExport(string clientID, string creditorID)
    {
        String query = "select * from CreditorMaster where ClientID = @ClientID and CreditorID = @CreditorID ";
        var obj = (List<CreditorMasterInfo>)db.Query<CreditorMasterInfo>(query, new { ClientID = clientID, CreditorID = creditorID }, this.transaction);

        if (obj.Count > 0)
        {
            obj[0].CreditorAuditDetailList = this.GetCreditorAuditDetailList(clientID, creditorID);
            obj[0].CreditorAmountList = new CreditorAmount(this.db, this.transaction).GetCreditorAmountList(clientID, creditorID);
            return obj[0];
        }

        return null;
    } 

    public List<CreditorPreviewInfo> GetCreditorList(string clientID, string userID)
    {
        db.Open();
        string query =
@"SELECT CreditorID, CreditorName ,GR1.ChiDesc CreditorType,GR2.ChiDesc Status, GR3.ChiDesc MainCreditor 
FROM [dbo].[CreditorMaster] 
left outer join GeneralMaster GR1 on  [CreditorMaster].CreditorType = GR1.Code and GR1.Category = 'CreditType'
left outer join GeneralMaster GR2 on  [CreditorMaster].Status = GR2.Code and GR2.Category = 'Status'
left outer join GeneralMaster GR3 on  [CreditorMaster].MainCreditor = GR3.Code and GR3.Category = 'YesNo'
WHERE ClientID = @ClientID  
AND ((exists (select 1 from ResponsibleStaffList where ClientID = @ClientID and StaffID = @UserID and ResponsibilityType in (1,3))) or ResponsiblePerson = @UserID or ResponsiblePerson is null) 
ORDER BY CreditorID";

        List<CreditorPreviewInfo> obj = (List<CreditorPreviewInfo>)db.Query<CreditorPreviewInfo>(query, new { ClientID = clientID, UserID = userID });
        db.Close();
        return obj;
    }

    public List<CreditorAuditDetailInfo> GetCreditorAuditDetailList(string clientID, string creditorID)
    {
        String query = @"select CreditorAuditDetail.*, GeneralMaster.ChiDesc CreditTypeDesc  from CreditorAuditDetail 
        join GeneralMaster on Category='CreditType' and Code = CreditorAuditDetail.CreditorType
        where ClientID = @ClientID and CreditorID = @CreditorID order by RowNo ";
        List<CreditorAuditDetailInfo> list = (List<CreditorAuditDetailInfo>)db.Query<CreditorAuditDetailInfo>(query, new { ClientID = clientID, CreditorID = creditorID }, this.transaction);
        return list;
    }

    public List<ExamineRecordInfo> GetExamineRecordList(string clientID, string creditorID)
    {
        String query = "select * from ExamineRecord where ClientID = @ClientID and CreditorID = @CreditorID ORDER BY ExamineDate";
        List<ExamineRecordInfo> list = (List<ExamineRecordInfo>)db.Query<ExamineRecordInfo>(query, new { ClientID = clientID, CreditorID = creditorID }, this.transaction);
        foreach (ExamineRecordInfo examineRecordInfo in list)
        {
            examineRecordInfo.StaffName = this.GetStaffName(examineRecordInfo.StaffID);
        }
        return list;
    }

    public List<CreditorAttachmentInfo> GetCreditorAttachmentList(string clientID, string creditorID)
    {
        String query = "SELECT "
                     + "[dbo].[CreditorAttachment].ClientID "
                     + ",[dbo].[CreditorAttachment].CreditorID "
                     + ",[dbo].[CreditorAttachment].RowNo "
                     + ",[dbo].[CreditorAttachment].AttachmentID "
                     + ",[dbo].[Attachment].AttName "
                     + ",[dbo].[Attachment].AttType "
                     + ",[dbo].[Attachment].AttPage "
                     + ",[dbo].[Attachment].AttRemark "
                     + ",[dbo].[Attachment].CreateUser "
                     + ",[dbo].[Attachment].CreateDate "
                     + ",[dbo].[Attachment].LastModifiedUser "
                     + ",[dbo].[Attachment].LastModifiedDate "
                     + "FROM CreditorAttachment "
                     + "JOIN [dbo].[Attachment] ON [AttachmentID] = [Attachment].ID "
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID ORDER BY RowNo ASC";
        List<CreditorAttachmentInfo> CreditorAttachmentList = (List<CreditorAttachmentInfo>)db.Query<CreditorAttachmentInfo>(query, new { ClientID = clientID, CreditorID = creditorID }, transaction);
        //String query2 = "SELECT * FROM Attachment WHERE ID IN @IDList ";
        //List<CreditorAttachmentInfo> obj = (List<CreditorAttachmentInfo>)db.Query<CreditorAttachmentInfo>(query2, new { IDList = AttachmentIDList }, transaction);
        return CreditorAttachmentList;
    }

    public List<ChangeRecordInfo> GetChangeRecordList(string clientID, string creditorID)
    {
        String query = @"SELECT ChiDesc CreditTypeDesc, ChangeRecord.* FROM ChangeRecord 
join GeneralMaster on Category = 'CreditType' and Code = CreditorType
WHERE ClientID = @ClientID AND CreditorID = @CreditorID order by UpdateDate Desc ";
        List<ChangeRecordInfo> list = (List<ChangeRecordInfo>)db.Query<ChangeRecordInfo>(query, new { ClientID = clientID, CreditorID = creditorID }, this.transaction);
        return list;
    }

    public CreditorAttachmentInfo GetAttachment(int attachmentID)
    {
        db.Open();
        String query = "SELECT FilePath, AttName FROM [dbo].[Attachment] WHERE ID = @ID ";
        List<CreditorAttachmentInfo> obj = (List<CreditorAttachmentInfo>)db.Query<CreditorAttachmentInfo>(query, new { ID = attachmentID });
        if (obj.Count > 0)
        {
            return obj[0];
        }
        else
            return null;
    }
    #endregion

    #region Insert
    public CreditorMasterInfo Insert(CreditorMasterInfo creditorMasterInfo)
    {
        string queryMax = @"select max(ID) + 1 from CreditorMaster where ClientID = @ClientID";

        int id = ((List<int>)db.Query<int>(queryMax, new { ClientID = creditorMasterInfo .ClientID }, transaction))[0];
        creditorMasterInfo.ID = id;

        String query = "INSERT INTO [dbo].[CreditorMaster] "
                     + "(ID, [ClientID] "
                     + ",[Prefix] "
                     + ",[CreditorType] "
                     + ",[Status] "
                     + ",[ResponsiblePerson] "
                     + ",[MainCreditor] "
                     + ",[CompanyType] "
                     + ",[CreditorName] "
                     + ",[ReportDate] "
                     + ",[CreditorIDNo] "
                     + ",[LegalRepresentative] "
                     + ",[LegalRepPhone] "
                     + ",[CreditAgent] "
                     + ",[CreditAgentPhone] "
                     + ",[CreditAgentIDNo] "
                     + ",[CreditorRelation] "
                     + ",[CreditorAddress] "
                     + ",[CreditAgentAddress] "
                     + ",[BusinessCode] "
                     + ",[RegPlace] "
                     + ",[BankName] "
                     + ",[BankAccount] "
                     + ",[Currency] "
                     + ",[Rate] "
                     + ",[Amount] "
                     + ",[Interest] "
                     + ",[SecuredClaimItems] "
                     + ",[SecuredCurrency] "
                     + ",[SecuredRate] "
                     + ",[SecuredAmount] "
                     + ",[Gage] "
                     + ",[GageDate] "
                     + ",[GageValue] "
                     + ",[JointCreditor] "
                     + ",[JointCreditorAmt] "
                     + ",[Party] "
                     + ",[PartyIDNo] "
                     + ",[PartyEmail] "
                     + ",[PartyAddress] "
                     + ",[PartyZipCode] "
                     + ",[PartyContact] "
                     + ",[PartyContactPhone] "
                     + ",[ThirdCompanyType] "
                     + ",[ThirdCreditorName] "
                     + ",[ThirdReportDate] "
                     + ",[ThirdCreditorIDNo] "
                     + ",[ThirdLegalRepresentative] "
                     + ",[ThirdLegalRepPhone] "
                     + ",[ThirdCreditAgent] "
                     + ",[ThirdCreditAgentPhone] "
                     + ",[ThirdCreditAgentIDNo] "
                     + ",[ThirdCreditorRelation] "
                     + ",[ThirdCreditorAddress] "
                     + ",[ThirdCreditAgentAddress] "
                     + ",[ThirdBankName] "
                     + ",[ThirdBankAccount] "
                     + ",[LegalOpinion] "
                    + ",[HasSpecialCreditType] "
                    + ",[SpecialCreditTypeRemarks] "
                    + ",[WarrantyRemarks] " 
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ID, @ClientID "
                     + ",@Prefix "
                     + ",@CreditorType "
                     + ",@Status "
                     + ",@ResponsiblePerson "
                     + ",@MainCreditor "
                     + ",@CompanyType "
                     + ",@CreditorName "
                     + ",@ReportDate "
                     + ",@CreditorIDNo "
                     + ",@LegalRepresentative "
                     + ",@LegalRepPhone "
                     + ",@CreditAgent "
                     + ",@CreditAgentPhone "
                     + ",@CreditAgentIDNo "
                     + ",@CreditorRelation "
                     + ",@CreditorAddress "
                     + ",@CreditAgentAddress "
                     + ",@BusinessCode "
                     + ",@RegPlace "
                     + ",@BankName "
                     + ",@BankAccount "
                     + ",@Currency "
                     + ",@Rate "
                     + ",@Amount "
                     + ",@Interest "
                     + ",@SecuredClaimItems "
                     + ",@SecuredCurrency "
                     + ",@SecuredRate "
                     + ",@SecuredAmount "
                     + ",@Gage "
                     + ",@GageDate "
                     + ",@GageValue "
                     + ",@JointCreditor "
                     + ",@JointCreditorAmt "
                     + ",@Party "
                     + ",@PartyIDNo "
                     + ",@PartyEmail "
                     + ",@PartyAddress "
                     + ",@PartyZipCode "
                     + ",@PartyContact "
                     + ",@PartyContactPhone "
                     + ",@ThirdCompanyType "
                     + ",@ThirdCreditorName "
                     + ",@ThirdReportDate "
                     + ",@ThirdCreditorIDNo "
                     + ",@ThirdLegalRepresentative "
                     + ",@ThirdLegalRepPhone "
                     + ",@ThirdCreditAgent "
                     + ",@ThirdCreditAgentPhone "
                     + ",@ThirdCreditAgentIDNo "
                     + ",@ThirdCreditorRelation "
                     + ",@ThirdCreditorAddress "
                     + ",@ThirdCreditAgentAddress "
                     + ",@ThirdBankName "
                     + ",@ThirdBankAccount "
                     + ",@LegalOpinion "
                    + ",@HasSpecialCreditType "
                    + ",@SpecialCreditTypeRemarks "
                    + ",@WarrantyRemarks " 
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate); ";

        creditorMasterInfo.Prefix = creditorMasterInfo.Prefix ?? "D";
        db.Query<int>(query, creditorMasterInfo, transaction);

        string tmpString = string.Format("00000000{0}", creditorMasterInfo.ID);
        creditorMasterInfo.CreditorID = creditorMasterInfo.Prefix + tmpString.Substring(tmpString.Length - 8);

        if (creditorMasterInfo.CreditorAuditDetailList != null)
        {
            foreach (CreditorAuditDetailInfo creditorAuditDetailInfo in creditorMasterInfo.CreditorAuditDetailList)
            {
                creditorAuditDetailInfo.ClientID = creditorMasterInfo.ClientID;
                creditorAuditDetailInfo.CreditorID = creditorMasterInfo.CreditorID;
                creditorAuditDetailInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                creditorAuditDetailInfo.CreateDate = DateTime.Now;
                creditorAuditDetailInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                creditorAuditDetailInfo.LastModifiedDate = DateTime.Now;
                this.InsertCreditorAuditDetail(creditorAuditDetailInfo);
            }
        }

        if (creditorMasterInfo.ExamineRecordList != null)
        {
            foreach (ExamineRecordInfo examineRecordInfo in creditorMasterInfo.ExamineRecordList)
            {
                examineRecordInfo.ClientID = creditorMasterInfo.ClientID;
                examineRecordInfo.CreditorID = creditorMasterInfo.CreditorID;
                examineRecordInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.CreateDate = DateTime.Now;
                examineRecordInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.LastModifiedDate = DateTime.Now;
                examineRecordInfo.StaffID = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.StaffName = this.GetStaffName(creditorMasterInfo.LastModifiedUser);
                examineRecordInfo.ExamineDate = DateTime.Now;
                this.InsertExamineRecord(examineRecordInfo);
            }
        }

        if (creditorMasterInfo.CreditorAttachmentList != null)
        {
            foreach (CreditorAttachmentInfo creditorAttachmentInfo in creditorMasterInfo.CreditorAttachmentList)
            {
                creditorAttachmentInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                creditorAttachmentInfo.CreateDate = DateTime.Now;
                creditorAttachmentInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                creditorAttachmentInfo.LastModifiedDate = DateTime.Now;
                creditorAttachmentInfo.ClientID = creditorMasterInfo.ClientID;
                creditorAttachmentInfo.CreditorID = creditorMasterInfo.CreditorID;
                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Guid.NewGuid() + creditorAttachmentInfo.FileExt;
                creditorAttachmentInfo.FilePath = "~/Attachment/CreditorAttachment/" + creditorMasterInfo.ClientID + "_" + creditorMasterInfo.CreditorID + "/" + fileName;
                creditorAttachmentInfo.DirectoryPath = "~/Attachment/CreditorAttachment/" + creditorMasterInfo.ClientID + "_" + creditorMasterInfo.CreditorID + "/";
                this.InsertCreditorAttachment(creditorAttachmentInfo);
            }

            foreach (CreditorAttachmentInfo creditorAttachmentInfo in creditorMasterInfo.CreditorAttachmentList)
            {
                this.SaveFile(creditorAttachmentInfo.DirectoryPath, creditorAttachmentInfo.FilePath, creditorAttachmentInfo.DataURL);
            }
        }

        if (creditorMasterInfo.CreditorAmountList != null)
        {
            CreditorAmount CreditorAmount = new CreditorAmount(this.db, transaction);
            foreach (CreditorAmountInfo info in creditorMasterInfo.CreditorAmountList)
            {
                info.ClientID = creditorMasterInfo.ClientID;
                info.CreditorID = creditorMasterInfo.CreditorID;
                info.CreateUser = creditorMasterInfo.LastModifiedUser;
                info.CreateDate = DateTime.Now;
                info.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                info.LastModifiedDate = DateTime.Now;

                CreditorAmount.Save(info);
            }

        }


        return creditorMasterInfo;
    }

    public void InsertCreditorAuditDetail(CreditorAuditDetailInfo creditorAuditDetailInfo)
    {
        String query = "INSERT INTO [dbo].[CreditorAuditDetail] "
                         + "([ClientID] "
                         + ",[CreditorID] "
                         + ",[RowNo] "
                         + ",[CreditorTypeCode] "
                         + ",[CreditorType] "
                         + ",[Currency] "
                         + ",[BookAmt] "
                         + ",[DeclareAmt] "
                         + ",[AdminExamineNotConfirm] "
                         + ",[AdminExamineWaitConfirm] "
                         + ",[AdminExamineConfirm] "
                         + ",[LawyerExamineNotConfirm] "
                         + ",[LawyerExamineWaitConfirm] "
                         + ",[LawyerExamineConfirm] "
                         + ",[MatchOpinion] "
                         + ",[CreateUser] "
                         + ",[CreateDate] "
                         + ",[LastModifiedUser] "
                         + ",[LastModifiedDate] ) "
                         + "VALUES "
                         + "(@ClientID "
                         + ",@CreditorID "
                         + ",@RowNo "
                         + ",@CreditorTypeCode "
                         + ",@CreditorType "
                         + ",@Currency "
                         + ",@BookAmt "
                         + ",@DeclareAmt "
                         + ",@AdminExamineNotConfirm "
                         + ",@AdminExamineWaitConfirm "
                         + ",@AdminExamineConfirm "
                         + ",@LawyerExamineNotConfirm "
                         + ",@LawyerExamineWaitConfirm "
                         + ",@LawyerExamineConfirm "
                         + ",@MatchOpinion "
                         + ",@CreateUser "
                         + ",@CreateDate "
                         + ",@LastModifiedUser "
                         + ",@LastModifiedDate) ";
        db.Execute(query, creditorAuditDetailInfo, transaction);
    }

    public void InsertExamineRecord(ExamineRecordInfo examineRecordInfo)
    {
        String query = "INSERT INTO [dbo].[ExamineRecord] "
                     + "([ClientID] "
                     + ",[CreditorID] "
                     + ",[RowNo] "
                     + ",[StaffID] "
                     + ",[ExamineDate] "
                     + ",[ExamineType] "
                     + ",[ExamineContent] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@CreditorID "
                     + ",@RowNo "
                     + ",@StaffID "
                     + ",@ExamineDate "
                     + ",@ExamineType "
                     + ",@ExamineContent "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate ) ";
        db.Execute(query, examineRecordInfo, transaction);
    }

    public void InsertCreditorAttachment(CreditorAttachmentInfo creditorAttachmentInfo)
    {
        String query = "INSERT INTO [dbo].[Attachment] "
                     + "([AttName] "
                     + ",[AttType] "
                     + ",[AttPage] "
                     + ",[FilePath] "
                     + ",[AttRemark] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@AttName "
                     + ",@AttType "
                     + ",@AttPage "
                     + ",@FilePath "
                     + ",@AttRemark "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate) ";
        db.Execute(query, creditorAttachmentInfo, transaction);

        creditorAttachmentInfo.AttachmentID = this.GetLastInsertAttachmentID();

        String query2 = "INSERT INTO [dbo].[CreditorAttachment] "
                     + "([ClientID] "
                     + ",[CreditorID] "
                     + ",[RowNo] "
                     + ",[AttachmentID] "
                     + ",[CreateUser] "
                     + ",[CreateDate] "
                     + ",[LastModifiedUser] "
                     + ",[LastModifiedDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@CreditorID "
                     + ",@RowNo "
                     + ",@AttachmentID "
                     + ",@CreateUser "
                     + ",@CreateDate "
                     + ",@LastModifiedUser "
                     + ",@LastModifiedDate) ";
        db.Execute(query2, creditorAttachmentInfo, transaction);
    }

    public void InsertChangeRecord(ChangeRecordInfo changeRecordInfo)
    {
        String query = "INSERT INTO [dbo].[ChangeRecord] "
                     + "([ClientID] "
                     + ",[CreditorID] "
                     + ",[StaffID] "
                     + ",[StaffName] "
                     + ",[CreditorType] "
                     + ",[Currency] "
                     + ",[Area] "
                     + ",[ValueFrom] "
                     + ",[ValueTo] "
                     + ",[UpdateDate] ) "
                     + "VALUES "
                     + "(@ClientID "
                     + ",@CreditorID "
                     + ",@StaffID "
                     + ",@StaffName "
                     + ",@CreditorType "
                     + ",@Currency "
                     + ",@Area "
                     + ",@ValueFrom "
                     + ",@ValueTo "
                     + ",@UpdateDate) ";
        db.Execute(query, changeRecordInfo, transaction);
    }
    #endregion

    #region Update
    public CreditorMasterInfo Update(CreditorMasterInfo creditorMasterInfo)
    {

        String query = "UPDATE [dbo].[CreditorMaster] "
                     + "SET "
                     + " [CreditorType] = @CreditorType "
                     + ",[Status] = @Status "
                     + ",[ResponsiblePerson] = @ResponsiblePerson"
                     + ",[MainCreditor] = @MainCreditor "
                     + ",[CompanyType] = @CompanyType "
                     + ",[CreditorName] = @CreditorName "
                     + ",[ReportDate] = @ReportDate "
                     + ",[CreditorIDNo] = @CreditorIDNo "
                     + ",[LegalRepresentative] = @LegalRepresentative "
                     + ",[LegalRepPhone] = @LegalRepPhone "
                     + ",[CreditAgent] = @CreditAgent "
                     + ",[CreditAgentPhone] = @CreditAgentPhone "
                     + ",[CreditAgentIDNo] = @CreditAgentIDNo "
                     + ",[CreditorRelation] = @CreditorRelation "
                     + ",[CreditorAddress] = @CreditorAddress "
                     + ",[CreditAgentAddress] = @CreditAgentAddress "
                     + ",[BusinessCode] = @BusinessCode "
                     + ",[RegPlace] = @RegPlace "
                     + ",[BankName] = @BankName "
                     + ",[BankAccount] = @BankAccount "
                     + ",[Currency] = @Currency "
                     + ",[Rate] = @Rate "
                     + ",[Amount] = @Amount "
                     + ",[Interest] = @Interest "
                     + ",[SecuredClaimItems] = @SecuredClaimItems "
                     + ",[SecuredCurrency] = @SecuredCurrency "
                     + ",[SecuredRate] = @SecuredRate "
                     + ",[SecuredAmount] = @SecuredAmount "
                     + ",[Gage] = @Gage "
                     + ",[GageDate] = @GageDate "
                     + ",[GageValue] = @GageValue "
                     + ",[JointCreditor] = @JointCreditor "
                     + ",[JointCreditorAmt] = @JointCreditorAmt "
                     + ",[Party] = @Party "
                     + ",[PartyIDNo] = @PartyIDNo "
                     + ",[PartyEmail] = @PartyEmail"
                     + ",[PartyAddress] = @PartyAddress "
                     + ",[PartyZipCode] = @PartyZipCode "
                     + ",[PartyContact] = @PartyContact "
                     + ",[PartyContactPhone] = @PartyContactPhone "
                     + ",[ThirdCompanyType] = @ThirdCompanyType "
                     + ",[ThirdCreditorName] = @ThirdCreditorName "
                     + ",[ThirdReportDate] = @ThirdReportDate "
                     + ",[ThirdCreditorIDNo] = @ThirdCreditorIDNo "
                     + ",[ThirdLegalRepresentative] = @ThirdLegalRepresentative "
                     + ",[ThirdLegalRepPhone] = @ThirdLegalRepPhone "
                     + ",[ThirdCreditAgent] = @ThirdCreditAgent "
                     + ",[ThirdCreditAgentPhone] = @ThirdCreditAgentPhone "
                     + ",[ThirdCreditAgentIDNo] = @ThirdCreditAgentIDNo "
                     + ",[ThirdCreditorRelation] = @ThirdCreditorRelation "
                     + ",[ThirdCreditorAddress] = @ThirdCreditorAddress "
                     + ",[ThirdCreditAgentAddress] = @ThirdCreditAgentAddress "
                     + ",[ThirdBankName] = @ThirdBankName "
                     + ",[ThirdBankAccount] = @ThirdBankAccount "
                     + ",[LegalOpinion] = @LegalOpinion "

                    + ", [HasSpecialCreditType] = @HasSpecialCreditType "
                    + ", [SpecialCreditTypeRemarks] = @SpecialCreditTypeRemarks "
                    + ", [WarrantyRemarks] = @WarrantyRemarks " 

                     + ",[LastModifiedUser] = @LastModifiedUser "
                     + ",[LastModifiedDate] = @LastModifiedDate "
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID";
        db.Execute(query, creditorMasterInfo, transaction);

        List<int> CreditorAttachmentInfo = new List<int>();
        List<int> CreditorAmountToBeDeleted = new List<int>();


        //examine record 
        List<int> ExamineRecordRowNoList = new List<int>();
        if (creditorMasterInfo.ExamineRecordList == null) creditorMasterInfo.ExamineRecordList = new List<ExamineRecordInfo>();
        foreach (ExamineRecordInfo examineRecordInfo in creditorMasterInfo.ExamineRecordList)
        {
            ExamineRecordRowNoList.Add(examineRecordInfo.RowNo);
        }
        this.DeleteExamineRecordNotInTheList(ExamineRecordRowNoList, creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID);

        if (creditorMasterInfo.CreditorAttachmentList == null) creditorMasterInfo.CreditorAttachmentList = new List<CreditorAttachmentInfo>();
        foreach (CreditorAttachmentInfo creditorAttachmentInfo in creditorMasterInfo.CreditorAttachmentList)
        {
            CreditorAttachmentInfo.Add(creditorAttachmentInfo.RowNo);
        }
        List<CreditorAttachmentInfo> ToBeDeleteCreditorAttachmentList = this.DeleteCreditorAttachmentNotInTheList(CreditorAttachmentInfo, creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID);


        //audit amount
        List<string> AuditDetailRowNoList = new List<string>();
        if (creditorMasterInfo.CreditorAuditDetailList != null)
            foreach (CreditorAuditDetailInfo creditorAuditDetailInfo in creditorMasterInfo.CreditorAuditDetailList)
            {
                creditorAuditDetailInfo.ClientID = creditorMasterInfo.ClientID;
                creditorAuditDetailInfo.CreditorID = creditorMasterInfo.CreditorID;
                creditorAuditDetailInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                creditorAuditDetailInfo.CreateDate = DateTime.Now;
                creditorAuditDetailInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                creditorAuditDetailInfo.LastModifiedDate = DateTime.Now;
                this.SaveCreditorAuditDetail(creditorAuditDetailInfo);

                AuditDetailRowNoList.Add(creditorAuditDetailInfo.CreditorType + creditorAuditDetailInfo.Currency);
            }
        this.DeleteAuditAmountNotInTheList(AuditDetailRowNoList, creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID);


        if (creditorMasterInfo.ExamineRecordList != null)
            foreach (ExamineRecordInfo examineRecordInfo in creditorMasterInfo.ExamineRecordList)
            {
                examineRecordInfo.ClientID = creditorMasterInfo.ClientID;
                examineRecordInfo.CreditorID = creditorMasterInfo.CreditorID;
                examineRecordInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.CreateDate = DateTime.Now;
                examineRecordInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.LastModifiedDate = DateTime.Now;
                examineRecordInfo.StaffID = creditorMasterInfo.LastModifiedUser;
                examineRecordInfo.StaffName = this.GetStaffName(creditorMasterInfo.LastModifiedUser);
                examineRecordInfo.ExamineDate = DateTime.Now;
                this.SaveExamineRecord(examineRecordInfo);
            }

        List<CreditorAttachmentInfo> AttachmentsToBeSavedList = new List<CreditorAttachmentInfo>();


        if (creditorMasterInfo.CreditorAttachmentList != null)
            foreach (CreditorAttachmentInfo creditorAttachmentInfo in creditorMasterInfo.CreditorAttachmentList)
            {
                creditorAttachmentInfo.ClientID = creditorMasterInfo.ClientID;
                creditorAttachmentInfo.CreditorID = creditorMasterInfo.CreditorID;
                creditorAttachmentInfo.CreateUser = creditorMasterInfo.LastModifiedUser;
                creditorAttachmentInfo.CreateDate = DateTime.Now;
                creditorAttachmentInfo.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                creditorAttachmentInfo.LastModifiedDate = DateTime.Now;
                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Guid.NewGuid() + creditorAttachmentInfo.FileExt;
                creditorAttachmentInfo.FilePath = "~/Attachment/CreditorAttachment/" + creditorMasterInfo.ClientID + "_" + creditorMasterInfo.CreditorID + "/" + fileName;
                creditorAttachmentInfo.DirectoryPath = "~/Attachment/CreditorAttachment/" + creditorMasterInfo.ClientID + "_" + creditorMasterInfo.CreditorID + "/";
                if (creditorAttachmentInfo.AttachmentID == 0)
                    AttachmentsToBeSavedList.Add(creditorAttachmentInfo);
                this.SaveCreditorAttachment(creditorAttachmentInfo);
            }

        if (ToBeDeleteCreditorAttachmentList != null)
        {
            foreach (CreditorAttachmentInfo creditorAttachmentInfo in ToBeDeleteCreditorAttachmentList)
            {
                this.DeleteFile(creditorAttachmentInfo.FilePath);
            }
        }

        foreach (CreditorAttachmentInfo creditorAttachmentInfo in AttachmentsToBeSavedList)
        {
            this.SaveFile(creditorAttachmentInfo.DirectoryPath, creditorAttachmentInfo.FilePath, creditorAttachmentInfo.DataURL);
        }


        CreditorAmount CreditorAmount = new CreditorAmount(this.db, transaction);
        if (creditorMasterInfo.CreditorAmountList != null)
        {
            foreach (CreditorAmountInfo info in creditorMasterInfo.CreditorAmountList)
            {
                CreditorAmountToBeDeleted.Add(info.RowNo);

                info.ClientID = creditorMasterInfo.ClientID;
                info.CreditorID = creditorMasterInfo.CreditorID;
                info.CreateUser = creditorMasterInfo.LastModifiedUser;
                info.CreateDate = DateTime.Now;
                info.LastModifiedUser = creditorMasterInfo.LastModifiedUser;
                info.LastModifiedDate = DateTime.Now;

                CreditorAmount.Save(info);
            }

            CreditorAmount.DeleteCreditorAmountNotInTheList(CreditorAmountToBeDeleted, creditorMasterInfo.ClientID, creditorMasterInfo.CreditorID);

        }


        return creditorMasterInfo;
    }


    public CreditorMasterInfo UpdateContact(CreditorMasterInfo creditorMasterInfo)
    {

        string query = "UPDATE [dbo].[CreditorMaster] "
                     + "SET "    
                     + " [CompanyType] = @CompanyType "
                     + ",[CreditorName] = @CreditorName "
                     + ",[ReportDate] = @ReportDate "
                     + ",[CreditorIDNo] = @CreditorIDNo "
                     + ",[LegalRepresentative] = @LegalRepresentative "
                     + ",[LegalRepPhone] = @LegalRepPhone "
                     + ",[CreditAgent] = @CreditAgent "
                     + ",[CreditAgentPhone] = @CreditAgentPhone "
                     + ",[CreditAgentIDNo] = @CreditAgentIDNo " 
                     + ",[CreditorAddress] = @CreditorAddress "
                     + ",[CreditAgentAddress] = @CreditAgentAddress "
                     + ",[BusinessCode] = @BusinessCode "
                     + ",[RegPlace] = @RegPlace "
                     + ",[BankName] = @BankName "
                     + ",[BankAccount] = @BankAccount " 
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID";
        db.Execute(query, creditorMasterInfo, transaction);
        
        return creditorMasterInfo;
    }


    public void UpdateCreditorAuditDetail(CreditorAuditDetailInfo creditorAuditDetailInfo)
    {
        this.UpdateChangeRecordLog(creditorAuditDetailInfo);
        String query = "UPDATE [dbo].[CreditorAuditDetail] "
                     + "SET "
                     + "[RowNo] = @RowNo "
                     + ",[BookAmt] = @BookAmt "
                     + ",[DeclareAmt] = @DeclareAmt "
                     + ",[AdminExamineNotConfirm] = @AdminExamineNotConfirm "
                     + ",[AdminExamineWaitConfirm] = @AdminExamineWaitConfirm "
                     + ",[AdminExamineConfirm] = @AdminExamineConfirm "
                     + ",[LawyerExamineNotConfirm] = @LawyerExamineNotConfirm "
                     + ",[LawyerExamineWaitConfirm] = @LawyerExamineWaitConfirm "
                     + ",[LawyerExamineConfirm] = @LawyerExamineConfirm "
                     + ",[MatchOpinion] = @MatchOpinion "
                     + ",[LastModifiedUser] = @LastModifiedUser "
                     + ",[LastModifiedDate] = @LastModifiedDate "
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND CreditorType = @CreditorType AND Currency = @Currency";
        db.Execute(query, creditorAuditDetailInfo, transaction);
    }

    public void UpdateExamineRecord(ExamineRecordInfo examineRecordInfo)
    {
        String query = "UPDATE [dbo].[ExamineRecord] "
                     + "SET "
                     + "[ExamineType] = @ExamineType "
                     + ",[ExamineContent] = @ExamineContent "
                     + "WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo = @RowNo";
        db.Execute(query, examineRecordInfo, transaction);
    }

    public void UpdateCreditorAttachment(CreditorAttachmentInfo attachmentInfo)
    {
        String query = "UPDATE [dbo].[Attachment] "
                     + "SET "
                     + "[AttName] = @AttName "
                     + ",[AttType] = @AttType "
                     + ",[AttPage] = @AttPage "
                     + ",[AttRemark] = @AttRemark "
                     + "WHERE ID = @AttachmentID";
        db.Execute(query, attachmentInfo, transaction);
    }

    public void UpdateChangeRecordLog(CreditorAuditDetailInfo creditorAuditDetailInfo)
    {
        String query = "SELECT * FROM [dbo].[CreditorAuditDetail] WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND CreditorType = @CreditorType AND Currency = @Currency";
        var obj = (List<CreditorAuditDetailInfo>)db.Query<CreditorAuditDetailInfo>(query, creditorAuditDetailInfo, transaction);
        List<ChangeRecordInfo> ChangeRecordList = new List<ChangeRecordInfo>();
        if (obj.Count > 0)
        {
            string CreditorType = creditorAuditDetailInfo.CreditorType;
            foreach (PropertyInfo prop in typeof(CreditorAuditDetailInfo).GetProperties())
            {
                switch (prop.Name)
                {
                    case CreditorAuditDetailInfo.FieldName.BookAmt:
                    case CreditorAuditDetailInfo.FieldName.DeclareAmt:
                    case CreditorAuditDetailInfo.FieldName.AdminExamineNotConfirm:
                    case CreditorAuditDetailInfo.FieldName.AdminExamineWaitConfirm:
                    case CreditorAuditDetailInfo.FieldName.AdminExamineConfirm:
                    case CreditorAuditDetailInfo.FieldName.LawyerExamineNotConfirm:
                    case CreditorAuditDetailInfo.FieldName.LawyerExamineWaitConfirm:
                    case CreditorAuditDetailInfo.FieldName.LawyerExamineConfirm:
                        decimal originalValue = prop.GetValue(obj[0], null) == null ? 0 : Decimal.Parse(prop.GetValue(obj[0], null).ToString());
                        decimal newValue = prop.GetValue(creditorAuditDetailInfo, null) == null ? 0 : Decimal.Parse(prop.GetValue(creditorAuditDetailInfo, null).ToString());
                        if (originalValue != newValue)
                        {
                            ChangeRecordInfo changeRecordInfo = new ChangeRecordInfo();
                            changeRecordInfo.ClientID = creditorAuditDetailInfo.ClientID;
                            changeRecordInfo.CreditorID = creditorAuditDetailInfo.CreditorID;
                            changeRecordInfo.StaffID = creditorAuditDetailInfo.LastModifiedUser;
                            changeRecordInfo.Currency = creditorAuditDetailInfo.Currency;
                            changeRecordInfo.StaffName = this.GetStaffName(creditorAuditDetailInfo.LastModifiedUser);
                            changeRecordInfo.CreditorType = CreditorType;
                            FieldInfo fieldInfo = typeof(CreditorAuditDetailInfo.AreaName).GetField(prop.Name);
                            changeRecordInfo.Area = fieldInfo.GetValue(null).ToString();
                            changeRecordInfo.ValueFrom = originalValue.ToString();
                            changeRecordInfo.ValueTo = newValue.ToString();
                            changeRecordInfo.UpdateDate = DateTime.Now;
                            ChangeRecordList.Add(changeRecordInfo);
                        }

                        break;
                }
            }
        }
        if (ChangeRecordList.Count > 0)
        {
            foreach (ChangeRecordInfo changeRecordInfo in ChangeRecordList)
            {
                this.InsertChangeRecord(changeRecordInfo);
            }
        }
    }
    #endregion

    #region Delete
    public void Delete(string clientID, string creditorID)
    {
        this.db.Open();
        this.transaction = this.db.BeginTransaction();

        try
        {
            CreditorMasterInfo info = new CreditorMasterInfo();
            info.ClientID = clientID;
            info.CreditorID = creditorID;

            this.Update(info);

            string query = "Delete from CreditorMaster where ClientID = @ClientID AND CreditorID = @CreditorID ";
            db.Execute(query, new { ClientID = clientID, CreditorID = creditorID }, transaction);

            this.transaction.Commit();
        }
        catch
        {
            this.transaction.Rollback();
            throw;
        }
        finally
        {
            this.db.Close();
        }

    }
    public void DeleteAuditAmountNotInTheList(List<string> creditTypeCurrency, string clientID, string creditorID)
    {
        string query = "Delete from CreditorAuditDetail where ClientID = @ClientID AND CreditorID = @CreditorID AND (CreditorType + Currency) not in @creditTypeCurrency ";
        db.Execute(query, new { ClientID = clientID, CreditorID = creditorID, creditTypeCurrency = creditTypeCurrency }, transaction);
    }


    public void DeleteExamineRecordNotInTheList(List<int> rowNoList, string clientID, string creditorID)
    {
        string query = "Delete from ExamineRecord where ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo not in @RowNoList ";
        db.Execute(query, new { ClientID = clientID, CreditorID = creditorID, RowNoList = rowNoList }, transaction);
    }

    public List<CreditorAttachmentInfo> DeleteCreditorAttachmentNotInTheList(List<int> rowNoList, string clientID, string creditorID)
    {
        List<CreditorAttachmentInfo> CreditorAttachmentList = new List<CreditorAttachmentInfo>();
        String query = "SELECT AttachmentID FROM CreditorAttachment WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo NOT IN @RowNoList ";
        String deletequery = "DELETE FROM [dbo].[CreditorAttachment] WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo NOT IN @RowNoList ";

        List<int> attachmentIDList = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, RowNoList = rowNoList }, transaction);
        db.Execute(deletequery, new { ClientID = clientID, CreditorID = creditorID, RowNoList = rowNoList }, transaction);

        if (attachmentIDList.Count > 0)
        {
            String query2 = "SELECT * FROM [dbo].[Attachment] WHERE ID IN @AttachmentIDList ";
            String deletequery2 = "DELETE FROM [dbo].[Attachment] WHERE ID IN @AttachmentIDList ";
            CreditorAttachmentList = (List<CreditorAttachmentInfo>)db.Query<CreditorAttachmentInfo>(query2, new { AttachmentIDList = attachmentIDList }, transaction);
            db.Execute(deletequery2, new { AttachmentIDList = attachmentIDList }, transaction);
            return CreditorAttachmentList;
        }
        else
            return null;
    }
    #endregion

    #region Check exists
    public bool IsExisted(string clientID, string creditorID)
    {
        String query = "select count(*) FROM [dbo].[CreditorMaster] WHERE ClientID = @ClientID and CreditorID = @CreditorID ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID }, this.transaction);

        return obj[0] > 0;
    }

    public bool IsCreditorAuditDetailExisted(string clientID, string creditorID, string creditType, string currency)
    {
        String query = "select count(*) FROM [dbo].[CreditorAuditDetail] WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND CreditorType = @CreditType AND Currency = @Currency ";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, CreditType = creditType, Currency = currency }, transaction);
        return obj[0] > 0;
    }

    public bool IsExamineRecordExisted(string clientID, string creditorID, int rowNo)
    {
        String query = "select count(*) FROM [dbo].[ExamineRecord] WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo = @RowNo";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, RowNo = rowNo }, transaction);
        return obj[0] > 0;
    }

    public bool IsCreditorAttachmentExisted(string clientID, string creditorID, int rowNo)
    {
        String query = "SELECT Count(*) FROM [dbo].[CreditorAttachment] WHERE ClientID = @ClientID AND CreditorID = @CreditorID AND RowNo = @RowNo";
        var obj = (List<int>)db.Query<int>(query, new { ClientID = clientID, CreditorID = creditorID, RowNo = rowNo }, transaction);
        return obj[0] > 0;
    }
    #endregion

    #region File
    public void SaveFile(string dirPath, string filePath, string dataUrl)
    {
        string exactDirPath = HttpContext.Current.Server.MapPath(dirPath);
        string exactFilePath = HttpContext.Current.Server.MapPath(filePath);
        if (!Directory.Exists(exactDirPath))
            Directory.CreateDirectory(exactDirPath);
        File.WriteAllBytes(exactFilePath, Convert.FromBase64String(dataUrl));
    }

    public void DeleteFile(string filePath)
    {
        string exactFilePath = HttpContext.Current.Server.MapPath(filePath);
        if (File.Exists(exactFilePath))
            File.Delete(exactFilePath);
    }
    #endregion
}