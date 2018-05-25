using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;


using System.Threading.Tasks;
using ZXing;
using System.Drawing;

/// <summary>
/// Summary description for ReportHelpler
/// </summary>
public class SampleReportHelper
{

    public static byte[] readRTF(string templatePath, Dictionary<string,string> replaceDictionary,  string qrCodeInput)
    {
        SearchAndReplace(templatePath, replaceDictionary);
         
        byte[] FileData = File.ReadAllBytes(templatePath);
        File.Delete(templatePath);

        return FileData;
    }

    public static void SearchAndReplace(string document, Dictionary<string, string> replaceDictionary)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
        {
            string docText = null;
            using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
            {
                docText = sr.ReadToEnd();
            }

            Regex regexText;

            foreach (string key in replaceDictionary.Keys) 
            {
                regexText = new Regex(key);
                docText = regexText.Replace(docText, replaceDictionary[key]);
            }
            

            using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
            {
                sw.Write(docText);
            }

             
        }
    }
        
}