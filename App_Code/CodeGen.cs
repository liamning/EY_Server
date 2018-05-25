using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.IO;
using Dapper;
using System.Text;


/// <summary>
/// Summary description for CodeGen
/// </summary>
/// 
public class CodeGen
{ 
    SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServerConnString"].ConnectionString); 

    static Dictionary<string, string> dict;
    public string typeMapping(TablaDetails tablaDetails)
    {
        if (dict == null)
        {
            dict = new Dictionary<string, string>();
            dict.Add("56_", "int");
            dict.Add("56_True", "int");
            dict.Add("61_", "DateTime");
            dict.Add("104_", "bool");
            dict.Add("104_True", "bool");
            dict.Add("106_", "decimal");
            dict.Add("167_", "string");
            dict.Add("175_", "string");
            dict.Add("231_", "string");
            dict.Add("61_True", "DateTime?");
            dict.Add("106_True", "decimal");
            dict.Add("167_True", "string");
            dict.Add("175_True", "string");
            dict.Add("231_True", "string");
        }

        return dict[tablaDetails.Type + "_" + tablaDetails.NULLABLE];
    }

    public void genDataInfo()
    {
        db.Open();
        string sql = "SELECT "
                    + "CASE  "
                    + "WHEN D.COLUMN_NAME IS NOT NULL THEN 1 "
                    + "ELSE 0  "
                    + "END  AS ISPRIMARY, "
                    + "CASE  "
                    + "WHEN C.IS_NULLABLE = 0 THEN '' "
                    + "ELSE 'True'  "
                    + "END  AS NULLABLE, " 
                    + "C.system_type_id TYPE, "
                    + "C.NAME  AS 'COLUMNNAME',T.NAME AS 'TABLENAME' "
                    + "FROM SYS.COLUMNS C "
                    + "JOIN SYS.TABLES T ON C.OBJECT_ID = T.OBJECT_ID "
                    + "LEFT OUTER JOIN  "
                    + "(SELECT K.TABLE_NAME, "
                    + "K.COLUMN_NAME, "
                    + "K.CONSTRAINT_NAME "
                    + "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C "
                    + "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K "
                    + "ON C.TABLE_NAME = K.TABLE_NAME "
                    + "AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG "
                    + "AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA "
                    + "AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME "
                    + "WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY') AS D ON T.NAME	 = D.TABLE_NAME AND C.NAME = D.COLUMN_NAME "
                    + "ORDER BY "
                    + "TABLENAME, "
                    + "COLUMN_ID, "
                    + "COLUMNNAME; ";

        List<TablaDetails> tableList = (List<TablaDetails>)db.Query<TablaDetails>(sql);



        //table field
        string lastTableName = "", currentTableName = "";
        Dictionary<string, StringBuilder> fileContent = new Dictionary<string, StringBuilder>();
        StringBuilder sb = null;
        foreach (TablaDetails tableDetails in tableList)
        {
            currentTableName = tableDetails.TableName;
            if (lastTableName != currentTableName)
            { 
                sb = new StringBuilder();
                fileContent.Add(currentTableName, sb);
                lastTableName = currentTableName;


                sb.AppendLine("using System;");
                sb.AppendLine(string.Format("public class {0}Info", currentTableName));
                sb.AppendLine("{");
            }
            sb.Append("\t");
            sb.AppendLine(string.Format("public {0} {1} {2}", typeMapping(tableDetails), tableDetails.ColumnName, "{ get; set; }"));
        }


        //field name
        sb = null;
        Dictionary<string, StringBuilder> fileContent2 = new Dictionary<string, StringBuilder>();
        foreach (TablaDetails tableDetails in tableList)
        {
            currentTableName = tableDetails.TableName;
            if (lastTableName != currentTableName)
            {
                if (sb != null)
                {
                    sb.Append("\t");
                    sb.AppendLine("}");
                    sb.AppendLine("}");
                }
                sb = new StringBuilder();
                fileContent2.Add(currentTableName, sb);
                lastTableName = currentTableName;


                sb.Append("\t");
                sb.AppendLine("public class FieldName");
                sb.Append("\t");
                sb.AppendLine("{");
            }
            sb.Append("\t\t");
            sb.AppendLine(string.Format("public const string {0} = \"{0}\";", tableDetails.ColumnName));
        }
        sb.Append("\t");
        sb.AppendLine("}");
        sb.AppendLine("}");


        StringBuilder sbSelect = null;
        List<string> sbSelect2 = null;
        List<string> sbSelect3 = null;
        StringBuilder sbUpdate = null;
        StringBuilder sbUpdate2 = null;
        StringBuilder sbInsert = null;
        StringBuilder sbInsert2 = null;
        Dictionary<string, StringBuilder> queryDict = new Dictionary<string, StringBuilder>();
        Dictionary<string, List<string>> queryDict2 = new Dictionary<string, List<string>>();
        int updateCount = 0;
        int updateConditionCount = 0;
        foreach (TablaDetails tableDetails in tableList)
        {
            currentTableName = tableDetails.TableName;
            if (lastTableName != currentTableName)
            {
                if (sbInsert != null)
                {
                    //select
                    if (updateConditionCount > 0)
                    {
                        sbSelect.Append(sbUpdate2);
                        sbSelect.Append("\"");
                    }
                    sbSelect.Append(";");

                    //insert
                    sbInsert.Append("\t\t");
                    sbInsert.AppendLine("+\") \"");
                    sbInsert2.Append("\t\t");
                    sbInsert2.AppendLine("+\") \";");
                    sbInsert.Append(sbInsert2);


                    //update
                    if (updateConditionCount > 0)
                    {
                        sbUpdate.Append(sbUpdate2);
                        sbUpdate.Append("\"");
                    }
                    sbUpdate.Append(";");
                    
                }


                updateCount = 0; updateConditionCount = 0;
                sbSelect = new StringBuilder();
                sbSelect2 = new List<string>();
                sbSelect3 = new List<string>();
                sbUpdate = new StringBuilder();
                sbUpdate2 = new StringBuilder();
                sbInsert = new StringBuilder();
                sbInsert2 = new StringBuilder();
                queryDict.Add(currentTableName + "Select", sbSelect);
                queryDict2.Add(currentTableName + "Select2", sbSelect2);
                queryDict2.Add(currentTableName + "Select3", sbSelect3); 

                queryDict.Add(currentTableName + "Update", sbUpdate);
                queryDict.Add(currentTableName + "Update2", sbUpdate2);
                queryDict.Add(currentTableName + "Insert", sbInsert);
                queryDict.Add(currentTableName + "Insert2", sbInsert2);

                lastTableName = currentTableName;
                 
                //select
                sbSelect.AppendLine(string.Format(" from {0} \" ", currentTableName, tableDetails.ColumnName));
                //update
                sbUpdate.AppendLine(string.Format("string query = \" UPDATE [dbo].[{0}] SET  \"", currentTableName));

                //insert
                sbInsert.AppendLine(string.Format("string query = \"INSERT INTO [dbo].[{0}] ( [{1}] \" ", currentTableName, tableDetails.ColumnName));
                sbInsert2.Append("\t\t");
                sbInsert2.AppendLine(string.Format("+ \"VALUES ( @{0} \"", tableDetails.ColumnName));

            }
            else
            { 
                //insert
                sbInsert.Append("\t\t");
                sbInsert.AppendLine(string.Format("+ \",[{0}] \" ", tableDetails.ColumnName));
                sbInsert2.Append("\t\t");
                sbInsert2.AppendLine(string.Format("+ \",@{0} \" ", tableDetails.ColumnName));
            }

            if (tableDetails.IsPrimary == "0")
            { 
                //update
                sbUpdate.Append("\t\t");
                if (updateCount == 0)
                    sbUpdate.AppendLine(string.Format("+ \" [{0}] = @{0} \" ", tableDetails.ColumnName));
                else
                    sbUpdate.AppendLine(string.Format("+ \", [{0}] = @{0} \" ", tableDetails.ColumnName));

                updateCount++;
            }
            else
            {
                if (updateConditionCount == 0)
                { 
                    queryDict.Add(currentTableName+"Key1", new StringBuilder(tableDetails.ColumnName));
                    sbUpdate2.Append("\t\t");
                    sbUpdate2.Append(string.Format("+ \" where {0} = @{0} ", tableDetails.ColumnName)); 
                }
                    
                else
                    sbUpdate2.Append(string.Format(" and {0} = @{0} ", tableDetails.ColumnName));
                updateConditionCount++;


                //select
                sbSelect2.Add(string.Format(" {0} = {0} ", tableDetails.ColumnName));
                sbSelect3.Add(string.Format("{0} {1}", typeMapping(tableDetails), tableDetails.ColumnName));
            }

        }


        if (sbInsert != null)
        {
            //select
            if (updateConditionCount > 0)
            {
                sbSelect.Append(sbUpdate2);
                sbSelect.Append("\"");
            }
            sbSelect.Append(";");

            //insert
            sbInsert.Append("\t\t");
            sbInsert.AppendLine("+\") \"");
            sbInsert2.Append("\t\t");
            sbInsert2.AppendLine("+\") \";");
            sbInsert.Append(sbInsert2);


            //update
            if (updateConditionCount > 0)
            {
                sbUpdate.Append(sbUpdate2);
                sbUpdate.Append("\"");
            }
            sbUpdate.Append(";");
        }

        FileStream tmpFile = null;
        StreamWriter writer = null;
        foreach (string key in fileContent.Keys)
        {
            tmpFile = File.Create(@"C:\Users\Ning\Documents\JKTeam\EY\Voting_System\CodeGen\Info\" + key + "Info.cs");
            writer = new StreamWriter(tmpFile);
            writer.Write(fileContent[key].ToString());
            writer.Write(fileContent2[key].ToString());
            writer.Close();
            tmpFile.Close(); 

        }

        string templateString = null;
        foreach (string key in fileContent.Keys)
        {
            templateString = File.ReadAllText(@"C:\Users\Ning\Documents\JKTeam\EY\Voting_System\CodeGen\Template\Template.cs.tpl");
            templateString = templateString.Replace("{ClassName}", key);
            templateString = templateString.Replace("{SelectQuery}", queryDict[key + "Select"].ToString());
            templateString = templateString.Replace("{Key1}", queryDict[key + "Key1"].ToString());
            templateString = templateString.Replace("{SelectCriteria2}", string.Join(",", queryDict2[key + "Select2"]));
            templateString = templateString.Replace("{SelectCriteria1}", string.Join(" , ", queryDict2[key + "Select3"]));
            templateString = templateString.Replace("{UpdateQuery}", queryDict[key + "Update"].ToString());
            templateString = templateString.Replace("{InsertQuery}", queryDict[key + "Insert"].ToString());
            File.WriteAllText(@"C:\Users\Ning\Documents\JKTeam\EY\Voting_System\CodeGen\" + key + ".cs", templateString);

        }
    }


    public class TablaDetails
    {
        public string Type { get; set; }
        public string IsPrimary { get; set; }
        public string NULLABLE { get; set; }
        public string ColumnName { get; set; }
        public string TableName { get; set; }
    }
}