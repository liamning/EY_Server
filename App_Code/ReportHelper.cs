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
using OpenXmlHelpers.Word;

/// <summary>
/// Summary description for ReportHelpler
/// </summary>
public class ReportHelper
{

    public static byte[] readRTF(string templatePath, Dictionary<string,string> replaceDictionary,  string qrCodeInput)
    {

        SearchAndReplace(templatePath, replaceDictionary);


        if (!string.IsNullOrEmpty(qrCodeInput))
        {
            string image = GenQRCode(qrCodeInput);
            InsertAPicture(templatePath, image);
            File.Delete(image);
        }

        byte[] FileData = File.ReadAllBytes(templatePath);
        File.Delete(templatePath);

        return FileData;
    }

    public static void SearchAndReplace(string document, Dictionary<string, string> replaceDictionary)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
        {
            //string docText = null;
            //using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
            //{
            //    docText = sr.ReadToEnd();
            //}

           // Regex regexText;

            foreach (string key in replaceDictionary.Keys) 
            {
                //regexText = new Regex(key);
                //docText = regexText.Replace(docText, replaceDictionary[key]);


                wordDoc.GetMergeFields(key).ReplaceWithText(replaceDictionary[key]);

                //document.MainDocumentPart.Document.Save();
            }
            

            //using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
            //{
            //    sw.Write(docText);
            //}
             


            //repeatElement(wordDoc);
        }
    }

    //private static void repeatElement(WordprocessingDocument wordDoc)
    //{ 
    //    var firstChild = wordDoc.MainDocumentPart.Document.Body.FirstChild;
         
    //    //clone and insert the element
    //    var the3rdChild = firstChild.ChildElements[3].CloneNode(true); 
    //    firstChild.InsertAt(the3rdChild, 2);
         
    //}

    public static void InsertAPicture(string document, string fileName)
    {
        using (WordprocessingDocument wordprocessingDocument =
            WordprocessingDocument.Open(document, true))
        {
            MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart));
        }
    }

    private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId)
    {
        // Define the reference of the image.
        var element =
             new DocumentFormat.OpenXml.Wordprocessing.Drawing(
                 new DW.Anchor(
                     new DW.SimplePosition()
                     {
                         X = 0,
                         Y = 0
                     },
                     new DW.HorizontalPosition()
                     {
                         HorizontalAlignment = new HorizontalAlignment("right"),
                         RelativeFrom = HorizontalRelativePositionValues.Page,
                         PositionOffset = new PositionOffset("6286000"),
                     },
                     new DW.VerticalPosition()
                     {
                         VerticalAlignment = new VerticalAlignment("top"),
                         RelativeFrom = VerticalRelativePositionValues.Page,
                         PositionOffset = new PositionOffset("228600"),
                     },
                     new DW.Extent() { Cx = 990000L, Cy = 990000L },
                     new DW.EffectExtent()
                     {
                         LeftEdge = 0L,
                         TopEdge = 0L,
                         RightEdge = 0L,
                         BottomEdge = 0L
                     },
                     new DW.WrapNone(), 
                     new DW.DocProperties()
                     {
                         Id = (UInt32Value)1U,
                         Name = "Picture 1"
                     },
                     new DW.NonVisualGraphicFrameDrawingProperties(
                         new A.GraphicFrameLocks() { NoChangeAspect = true }),
                     new A.Graphic(
                         new A.GraphicData(
                             new PIC.Picture(
                                 new PIC.NonVisualPictureProperties(
                                     new PIC.NonVisualDrawingProperties()
                                     {
                                         Id = (UInt32Value)0U,
                                         Name = "New Bitmap Image.jpg"
                                     },
                                     new PIC.NonVisualPictureDrawingProperties()),
                                 new PIC.BlipFill(
                                     new A.Blip(
                                         new A.BlipExtensionList(
                                             new A.BlipExtension()
                                             {
                                                 Uri =
                                                   "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                             })
                                     )
                                     {
                                         Embed = relationshipId,
                                         CompressionState =
                                         A.BlipCompressionValues.Print
                                     },
                                     new A.Stretch(
                                         new A.FillRectangle())),
                                 new PIC.ShapeProperties(
                                     new A.Transform2D(
                                         new A.Offset() { X = 0L, Y = 0L },
                                         new A.Extents() { Cx = 990000L, Cy = 990000L }),
                                     new A.PresetGeometry(
                                         new A.AdjustValueList()
                                     ) { Preset = A.ShapeTypeValues.Rectangle }))
                         ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                 )
                 {
                     DistanceFromTop = (UInt32Value)0U,
                     DistanceFromBottom = (UInt32Value)0U,
                     DistanceFromLeft = (UInt32Value)0U,
                     DistanceFromRight = (UInt32Value)0U,
                     EditId = "50D07946",
                     SimplePos = false,
                     RelativeHeight = (UInt32Value)0U,
                     BehindDoc = true,
                     Locked = false,
                     LayoutInCell = true,
                     AllowOverlap = true
                 });

        // Append the reference to body, the element should be in a Run.
        //wordDoc.MainDocumentPart.Document.Body.InsertAt(
       // wordDoc.MainDocumentPart.Document.Body.ChildElements[0].InsertAt(, 0);


        // Add a paragraph with some text.
        Paragraph para = (Paragraph)wordDoc.MainDocumentPart.Document.Body.ChildElements[0];
        Run run = para.AppendChild(new Run());
        run.AppendChild(element);
         
        //OpenXmlElementList oldchild = wordDoc.MainDocumentPart.Document.Body.FirstChild.ChildElements;

        //element
       // oldchild[2].ChildElements[2].InsertAt(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(element)), 0);
    }
    
    public static string GenQRCode(string input)
    {
        IBarcodeWriter writer = new BarcodeWriter()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                Height = 200,
                Width = 200,
                Margin = 1
            }
        };

        Bitmap result = writer.Write(input);
        string path = string.Format(HttpContext.Current.Server.MapPath("~/tmp/{0}.png"), Guid.NewGuid().ToString("N"));
        result.Save(path);

        return path; 
    }

}