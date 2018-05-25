using System;
using System.Collections.Generic;

public class CreditorExportInfo
{
    public string ClientID { get; set; }
    public string CreditorID { get; set; }
    public string CreditorName { get; set; }
    public string Currency { get; set; }
    public string CreditTypeDesc { get; set; } 
    public decimal BookAmt { get; set; }
    public decimal DeclareAmt { get; set; }
    public decimal AdminExamineNotConfirm { get; set; }
    public decimal AdminExamineWaitConfirm { get; set; }
    public decimal AdminExamineConfirm { get; set; }
    public decimal LawyerExamineNotConfirm { get; set; }
    public decimal LawyerExamineWaitConfirm { get; set; }
    public decimal LawyerExamineConfirm { get; set; } 

}
 