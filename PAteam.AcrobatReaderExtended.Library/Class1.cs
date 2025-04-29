using Direct.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using log4net;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Reflection;

namespace Direct.PDFExtended.Library
{
    [DirectSealed]
    [DirectDom("PDF Functions")]
    [ParameterType(false)]
    public static class PDFFunctions
    {
        private static readonly ILog _log = LogManager.GetLogger("LibraryObjects");
        private static readonly int nMajorFileVersion = (int)char.GetNumericValue(FileVersionInfo.GetVersionInfo("itextsharp.dll").FileVersion[0]);

        [DirectDom("Extract PDF Pages")]
        [DirectDomMethod("Extract PDF Pages from {starting page} to {end page} out of {Input File Full Path} into seperate PDF {Output File Full Path}")]
        [MethodDescription("Extracts specified PDF pages in a file files into one PDF file")]
        public static bool ExtractPages(int startpage, int endpage, string sourcePDFpath, string outputPDFpath)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Start Extracting pdf: " + sourcePDFpath + " starting page " + startpage + " untill page " + endpage + " and saving to " + outputPDFpath);
                }

                PdfReader reader = null;
                Document sourceDocument = null;
                PdfCopy pdfCopyProvider = null;
                PdfImportedPage importedPage = null;

                reader = new PdfReader(sourcePDFpath);
                sourceDocument = new Document(reader.GetPageSizeWithRotation(startpage));
                pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPDFpath, System.IO.FileMode.Create));

                sourceDocument.Open();

                for (int i = startpage; i <= endpage; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    importedPage.ResetRGBColorFill();
                    pdfCopyProvider.AddPage(importedPage);
                }
                sourceDocument.Close();
                reader.Close();


                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Completed Extracting pdf");
                }
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Extract PDF Files Exception", e);
                return false;
            }
        }

        [DirectDom("Split PDF File")]
        [DirectDomMethod("Split PDF Pages of {Input File Full Path} into seperate PDFs {Output Directory Full Path}")]
        [MethodDescription("Splits specified PDF into seperate files for each page")]
        public static bool SplitPages(string sourcePDFpath, string outputDirectory)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Split pdf: " + sourcePDFpath + " and saving to " + outputDirectory);
                }
                FileInfo file = new FileInfo(sourcePDFpath);
                string name = file.Name.Substring(0, file.Name.LastIndexOf("."));

                PdfReader reader = new PdfReader(sourcePDFpath);

                for (int pagenumber = 1; pagenumber <= reader.NumberOfPages; pagenumber++)
                {
                    string filename = name + "(" + pagenumber.ToString() + ").pdf";
                    string outputPDFpath = outputDirectory + filename;
                    bool result = ExtractPages(pagenumber, pagenumber, sourcePDFpath, outputPDFpath);
                }

                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Completed splitting pdf");
                }
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Split PDF File Exception", e);
                return false;
            }
        }

        [DirectDom("Insert blank pages")]
        [DirectDomMethod("Adds a blank page after every page of {Input File Full Path} and save to {Output File Full Path}")]
        [MethodDescription("Adds blank pages after every page.")]
        public static bool InsertBlankPages(string sourcePDFpath, string outputPDFpath)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Insert blank pages to  pdf: " + sourcePDFpath + " and saving to " + outputPDFpath);
                }

                PdfReader reader = new PdfReader(sourcePDFpath);
                PdfStamper stamper = new PdfStamper(reader, new FileStream(outputPDFpath, FileMode.Create));
                int total = reader.NumberOfPages;

                for (int pageNumber = total; pageNumber > 0; pageNumber--)
                {
                    stamper.InsertPage(pageNumber, PageSize.A4);
                }
                stamper.Close();
                reader.Close();

                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Completed adding blank pages");
                }
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Insert blank pages Exception", e);
                return false;
            }
        }

        [DirectDom("Insert blank pages from to")]
        [DirectDomMethod("Adds a blank page after every page of {Input File Full Path} and save to {Output File Full Path} Starting at {start page} ending at {end page}")]
        [MethodDescription("Adds blank pages after every page.")]
        public static bool InsertBlankPagesFromTo(string sourcePDFpath, string outputPDFpath, int startpage, int endpage)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Insert blank pages to from page number " + startpage + " to page " + endpage + " for pdf: " + sourcePDFpath + " and saving to " + outputPDFpath);
                }

                PdfReader reader = new PdfReader(sourcePDFpath);
                PdfStamper stamper = new PdfStamper(reader, new FileStream(outputPDFpath, FileMode.Create));

                for (int pageNumber = endpage; pageNumber > startpage; pageNumber--)
                {
                    stamper.InsertPage(pageNumber, PageSize.A4);
                }

                stamper.Close();
                reader.Close();

                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Completed adding blank pages for range");
                }
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Insert blank pages from/to Exception", e);
                return false;
            }
        }

        [DirectDom("Merge PDF Files")]
        [DirectDomMethod("Merge PDF Files {Input Files Full Paths} into one PDF {Output File Full Path} and Add Page Numbering {Add Page Numbering}")]
        [MethodDescription("Merges specified PDF files into one PDF file adding page numbering if needed")]
        public static bool MergePdfFiles(DirectCollection<string> inputFilePaths, string outputFilePath, bool enablePageNumbers)
        {
            bool returnFlag = false;
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("Direct.PDFExtended.Library - MergePDFFiles - Output file path [{0}], Enable Page Numbers [{1}]", outputFilePath, enablePageNumbers);
            }

            if (!ValidateInput(string.Empty, outputFilePath, nameof(MergePdfFiles), false))
            {
                return returnFlag;
            }

            foreach (string inputFilePath in inputFilePaths)
            {
                if (_log.IsInfoEnabled)
                {
                    _log.InfoFormat("Direct.PDFExtended.Library - MergePDFFiles - Input file path [{0}]", inputFilePath);
                }

                if (!ValidateInput(string.Empty, inputFilePath, nameof(MergePdfFiles), true))
                {
                    return returnFlag;
                }
            }

            int fieldNameExtender = 0;
            PdfSmartCopy pdfSmartCopy = null;
            Document document = null;
            List<PdfReader> pdfReaderList = new List<PdfReader>();

            try
            {
                foreach (string inputFilePath in inputFilePaths)
                {
                    PdfReader pdfReader = new PdfReader(RenameFields(inputFilePath, ++fieldNameExtender));
                    pdfReaderList.Add(pdfReader);
                }

                int num = 1;

                document = new Document(PageSize.A4, 0.0f, 0.0f, 0.0f, 0.0f);
                pdfSmartCopy = new PdfSmartCopy(document, new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite));
                pdfSmartCopy.SetFullCompression();
                pdfSmartCopy.CompressionLevel = PdfStream.BEST_COMPRESSION;
                document.Open();
                foreach (PdfReader reader in pdfReaderList)
                {
                    for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; ++pageNumber)
                    {
                        PdfImportedPage importedPage = pdfSmartCopy.GetImportedPage(reader, pageNumber);
                        if (enablePageNumbers)
                        {
                            PdfCopy.PageStamp pageStamp = pdfSmartCopy.CreatePageStamp(importedPage);
                            PdfContentByte overContent = pageStamp.GetOverContent();
                            iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle(520f, 6f, 570f, 18f);
                            rectangle.BackgroundColor = iTextSharp.text.Color.WHITE;
                            overContent.Rectangle(rectangle);
                            ColumnText.ShowTextAligned(
                                overContent,
                                2,
                                new Phrase(
                                    new Chunk(string.Format("{0}", num++),
                                    FontFactory.GetFont("Helvetica", 7f, 0, iTextSharp.text.Color.BLACK))
                                ),
                                570f, 10f, 0.0f);
                            pageStamp.AlterContents();
                        }
                        pdfSmartCopy.AddPage(importedPage);
                    }
                    if (reader.AcroForm != null)
                    {
                        pdfSmartCopy.CopyAcroForm(reader);
                    }

                    pdfSmartCopy.FreeReader(reader);
                }
                returnFlag = true;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Direct.PDFExtended.Library - MergePDFFiles - Exception:)" + e.Message);
            }
            finally
            {
                document?.Close();
                pdfSmartCopy?.Close();
                if (pdfReaderList != null)
                {
                    foreach (PdfReader pdfReader in pdfReaderList)
                        pdfReader.Close();
                }
            }
            return returnFlag;
        }

        [DirectDom("Set PDF Form Field Value")]
        [DirectDomMethod("Set PDF {Path} {File Name} Form Field {Field} Value {Value}")]
        [MethodDescription("Sets a value to a specified field of PDF form")]
        public static bool SetPDFFormFieldValue(
            string path,
            string fileName,
            string field,
            string value)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(field))
            {
                if (_log.IsErrorEnabled)
                    _log.ErrorFormat("SetPDFFormFieldValue - field is empty");
                return flag;
            }
            if (!ValidateInput(path, fileName, nameof(SetPDFFormFieldValue)))
                return flag;
            PdfReader reader = null;
            PdfStamper pdfStamper = null;
            AcroFields.Item userFormField = null;
            string str1 = Path.Combine(path, fileName);
            string str2 = Path.Combine(path, "tmp_" + fileName);
            try
            {

                reader = new PdfReader(str1);
                FileStream os = new FileStream(str2, FileMode.Create, FileAccess.ReadWrite);
                pdfStamper = new PdfStamper(reader, os);
                AcroFields acroFields = pdfStamper.AcroFields;
                if (acroFields.Fields.Count == 0)
                {
                    _log.ErrorFormat("SetPDFFormFieldValue - No fields in PDF File");
                }
                else
                {
                    acroFields.GenerateAppearances = true;
                    userFormField = acroFields.GetFieldItem(field);
                }
                if (userFormField == null)
                {
                    _log.ErrorFormat("SetPDFFormFieldValue - Unable to Find Field with Name " + field);
                    return flag;
                }

                flag = acroFields.SetField(field, value);
                pdfStamper.FormFlattening = false;

            }
            catch (Exception ex)
            {
                _log.ErrorFormat("SetPDFFormFieldValue - Exception:)" + ex.ToString());
            }
            finally
            {
                reader?.Close();
                pdfStamper?.Close();
                File.Copy(str2, str1, true);
                File.Delete(str2);
            }
            if (_log.IsInfoEnabled)
                _log.InfoFormat("SetPDFFormFieldValue - Path [{0}], File [{1}], Field [{2}], Value [{3}]", path, fileName, field, value);
            return flag;
        }

        [DirectDom("Check PDF File for Password Protection")]
        [DirectDomMethod("Check {Input File Full Path} for password protection")]
        [MethodDescription("Checks if the file in the filepath is password protected or not.")]
        public static bool IsPdfPasswordProtected(string path, string fileName)
        {
            bool isPasswordProtected = false;
            string fullFilePath = Path.Combine(path, fileName);
            PdfReader reader = null;
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Checking pdf file: " + fullFilePath + " for password protection");
                }
                if (ValidateInput(path, fileName, string.Empty))
                {
                    _log.Debug("Direct.PDFExtended.Library - File is Valid");
                    reader = new PdfReader(fullFilePath);
                    if (reader.IsEncrypted())
                    {
                        _log.Debug("Direct.PDFExtended.Library - Set Result to true");
                        isPasswordProtected = true;
                    }
                    else
                    {
                        _log.Debug("Direct.PDFExtended.Library - Set Result to false");
                    }
                }
            }
            catch (BadPasswordException)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - PDF is password-protected");
                }
                isPasswordProtected = true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Checking PDF File Exception", e);
            }
            finally
            {
                if (reader != null)
                {
                    reader?.Close();
                }
            }
            if (_log.IsDebugEnabled)
            {
                _log.Debug("Direct.PDFExtended.Library - Completed Checking PDF File");
            }
            return isPasswordProtected;
        }

        [DirectDom("Set Image into PDF Form Field Value")]
        [DirectDomMethod("Set Image into PDF Path: {Path}  File Name: {File Name} Form Field {Field} Image File Path: {Image Path}  Image File Name: {Image File Name}")]
        [MethodDescription("Sets an image to a specified field of PDF form")]
        public static bool SetImageIntoPDFFormFieldValue(string path, string fileName, string fieldName, string imgFilePath, string imgFileName)
        {
            bool flag = false;
            PdfReader reader = null;
            PdfStamper stamper = null;
            string fullFilePath = Path.Combine(path, fileName);
            string fullImageFilePath = Path.Combine(imgFilePath, imgFileName);
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Setting Image: " + fullImageFilePath + " to PDF: " + fullFilePath + " With form Field: " + fieldName + ".");
                }
                if (ValidateInput(path, fileName, string.Empty))
                {
                    _log.Debug("Direct.PDFExtended.Library - File is Valid");
                    if (ValidateImageInput(imgFilePath, imgFileName))
                    {
                        _log.Debug("Direct.PDFExtended.Library - Image File is Valid");
                        reader = new PdfReader(fullFilePath);
                        stamper = new PdfStamper(reader, new FileStream(fullFilePath + "_temp", FileMode.Create));
                        AcroFields formFields = stamper.AcroFields;
                        float[] fieldPositions = formFields.GetFieldPositions(fieldName);
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fullImageFilePath);
                        iTextSharp.text.Rectangle rect = reader.GetPageSizeWithRotation(1);
                        image.ScaleToFit(fieldPositions[3] - fieldPositions[1], fieldPositions[4] - fieldPositions[2]);
                        image.SetAbsolutePosition(fieldPositions[1], fieldPositions[4] - image.ScaledHeight);
                        formFields.RemoveField(fieldName);
                        PdfContentByte canvas = stamper.GetOverContent(1);
                        canvas.AddImage(image);
                        flag = true;
                    }

                }
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Setting Image in PDF Form Field Exception", e);
            }
            finally
            {
                stamper.Close();
                reader.Close();
                File.Delete(fullFilePath);
                File.Move(fullFilePath + "_temp", fullFilePath);
            }

            return flag;

        }

        [DirectDom("Remove form fields from document")]
        [DirectDomMethod("Remove form fields {Form Fields} from PDF file {Input File Path} {Input File Name}")]
        [MethodDescription("Remove form fields from PDF file.")]
        public static bool RemoveFormFieldsFromDocument(DirectCollection<string> fieldNames, string path, string fileName)
        {
            bool fieldsRemoved = false;
            PdfReader reader = null;
            PdfStamper stamper = null;
            string fullFilePath = Path.Combine(path, fileName);
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Removing all form fields from pdf file: " + fullFilePath);
                }
                if (!fieldNames.IsEmpty)
                {
                    if (ValidateInput(path, fileName, string.Empty))
                    {
                        _log.Debug("Direct.PDFExtended.Library - File is Valid");
                        reader = new PdfReader(fullFilePath);
                        stamper = new PdfStamper(reader, new FileStream(fullFilePath + "_temp", FileMode.Create));
                        AcroFields formFields = stamper.AcroFields;
                        foreach (string fieldName in fieldNames)
                        {
                            formFields.RemoveField(fieldName);
                        }
                        fieldsRemoved = true;
                    }
                }
                else
                {
                    throw new Exception("Form Fields list is empty");
                }
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Removing all form fields from PDF file failed with Exception:", e);
            }
            finally
            {
                stamper.Close();
                reader.Close();
                if (fieldsRemoved)
                {
                    File.Delete(fullFilePath);
                    File.Move(fullFilePath + "_temp", fullFilePath);
                }
            }
            if (_log.IsDebugEnabled)
            {
                _log.Debug("Direct.PDFExtended.Library - Completed Removing all pdf form fields.");
            }
            return fieldsRemoved;
        }

        [DirectDom("Add Form Fields")]
        [DirectDomMethod("Add to file: {File Path} the following form fields: {PDF Fields}")]
        [MethodDescription("Adds programmatically new form fields to input pdf document")]
        public static bool AddFormFields(string filePath, DirectCollection<PDFField> pdfFields)
        {

            if (string.IsNullOrEmpty(filePath))
            {
                _log.Debug("Direct.PDFExtended.Library - Add Form Fields: Path to document where new fields should be added is empty");
                return false;
            }

            bool result = false;
            bool shouldDeleteTempFile = false;
            bool shouldOverwriteInputFile = false;

            PdfReader reader = null;
            PdfStamper stamper = null;

            FileInfo fileInfo = new FileInfo(filePath);
            string tempFilePath = Path.Combine(fileInfo.Directory.FullName, Path.GetFileNameWithoutExtension(fileInfo.Name) + "_tmp" + fileInfo.Extension);

            int fieldCounter = 1;
            try
            {
                reader = new PdfReader(filePath);
                stamper = new PdfStamper(reader, new FileStream(tempFilePath, FileMode.Create));
                shouldDeleteTempFile = true;


                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Add Form Fields: start iterating over supplied list");
                }

                foreach (PDFField pdfField in pdfFields)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Add Form Fields: iterating field: " + fieldCounter);
                    }

                    if (string.IsNullOrEmpty(pdfField.Name))
                    {
                        throw new Exception("Direct.PDFExtended.Library - Add Form Fields: Field Name for field at position " + fieldCounter + " is empty!");
                    }


                    if (pdfField.Size.Width == 0 || pdfField.Size.Height == 0)
                    {
                        throw new Exception("Direct.PDFExtended.Library - Add Form Fields: Width and Height have to be bigger then 0 for field at position " + fieldCounter);
                    }

                    // (lower-left-x, lower-left-y, upper-right-x (llx + width), upper-right-y (lly + height), rotation angle 
                    TextField field = new TextField(
                        stamper.Writer,
                        new iTextSharp.text.Rectangle(
                            (float)pdfField.Position.X,
                            (float)pdfField.Position.Y,
                            (float)(pdfField.Position.X + pdfField.Size.Width),
                            (float)(pdfField.Position.Y + pdfField.Size.Height)
                        ),
                        pdfField.Name
                    );

                    if (pdfField.FieldProperties.TextAlignment.ToLower() == "right")
                    {
                        field.Alignment = Element.ALIGN_RIGHT;
                    }

                    if (!string.IsNullOrEmpty(pdfField.FieldProperties.CustomFont))
                    {
                        field.Font = BaseFont.CreateFont(pdfField.FieldProperties.CustomFont, BaseFont.CP1252, BaseFont.EMBEDDED);
                    }
                    else
                    {
                        if (pdfField.FieldProperties.FontBold)
                        {
                            field.Font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        }
                        else
                        {
                            field.Font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        }

                    }

                    field.FontSize = pdfField.FieldProperties.FontSize;

                    int options = 0;

                    if (pdfField.FieldProperties.IsMultiline)
                    {
                        options += BaseField.MULTILINE;
                    }

                    if (pdfField.FieldProperties.IsReadOnly)
                    {
                        options += BaseField.READ_ONLY;
                    }

                    if (pdfField.FieldProperties.IsRequired)
                    {
                        options += BaseField.REQUIRED;
                    }

                    field.Options = options;

                    stamper.AddAnnotation(field.GetTextField(), 1);

                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Add Form Fields: Field " + fieldCounter + " added with success");
                    }

                    fieldCounter++;
                }

                shouldOverwriteInputFile = true;
                result = true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Add Form Fields: Failed to add fields", e);
            }
            finally
            {
                stamper?.Close();
                reader?.Close();

                if (!shouldOverwriteInputFile && shouldDeleteTempFile)
                {
                    File.Delete(tempFilePath);
                }

                if (shouldDeleteTempFile && shouldOverwriteInputFile)
                {
                    File.Delete(filePath);
                    File.Move(tempFilePath, filePath);
                }

            }

            return result;
        }

        [DirectDom("Flatten PDF Form")]
        [DirectDomMethod("Flatten PDF form from file {Source File Path}")]
        [MethodDescription("Flattens PDF Form")]
        public static bool FlattenForm(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Flatten PDF: input file path is empty");
                }
                return false;
            }

            bool result = false;
            PdfReader reader = null;
            PdfStamper stamper = null;

            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Flatten PDF: Flattenning...");
                }
                byte[] pdfFile = File.ReadAllBytes(inputFilePath);
                reader = new PdfReader(pdfFile);
                stamper = new PdfStamper(reader, new FileStream(inputFilePath, FileMode.Create));

                stamper.AcroFields.GenerateAppearances = true;
                stamper.FormFlattening = true;

                reader.RemoveUnusedObjects();

                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Flatten PDF: Success");
                }

                result = true;
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Flatten PDF: failed to flatten form", e);
            }
            finally
            {
                reader?.Close();
                stamper?.Close();
            } 
            
            return result;
        }

        [DirectDom("Get Form Fields Names")]
        [DirectDomMethod("Get form fields names from file: {Full File Path}")]
        [MethodDescription("Gets and return form fields names from supplied document")]
        public static DirectCollection<string> GetFormFieldNames(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Direct.PDFExtended.Library - Get Form Field Names: input file path is empty");
                }
                return new DirectCollection<string>();
            }

            DirectCollection<string> result = new DirectCollection<string>();
            MemoryStream os = null;
            PdfReader reader = null;
            PdfStamper pdfStamper = null;
            try
            {
                os = new MemoryStream();
                reader = new PdfReader(inputFilePath);
                pdfStamper = new PdfStamper(reader, os);
                AcroFields acroFields = pdfStamper.AcroFields;
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"Direct.PDFExtended.Library - Get Form Field Names: fields count: {reader.AcroFields.Fields.Keys.Count}");
                }
                foreach (string key in (IEnumerable)reader.AcroFields.Fields.Keys)
                {
                    result.Add(key);
                }
            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Get Form Field Names: failed to get field name", e);
                result = new DirectCollection<string>();
            }
            finally
            {
                pdfStamper?.Close();
                reader?.Close();
            }

            return result;

        }

        [DirectDom("Convert Image To PDF")]
        [DirectDomMethod("Convert Image {Image Path} to PDF {Output Full File Name}")]
        [MethodDescription("Converts Image to PDF")]
        public static bool ConvertImageToPDF(string inputImageFilePath, string outputPDFFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(inputImageFilePath) || string.IsNullOrEmpty(outputPDFFilePath))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Image To PDF: missing arguments");
                    }
                    return false;
                }

                if (!System.Web.MimeMapping.GetMimeMapping(inputImageFilePath).StartsWith("image/"))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Image To PDF: provided image is not of valid extension");
                    }
                    return false;
                }

                iTextSharp.text.Rectangle pageSize = null;

                using (var srcImage = new Bitmap(inputImageFilePath))
                {
                    pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
                }
                using (var ms = new MemoryStream())
                {
                    var document = new Document(pageSize, 0, 0, 0, 0);
                    PdfWriter.GetInstance(document, ms).SetFullCompression();
                    document.Open();
                    var image = iTextSharp.text.Image.GetInstance(inputImageFilePath);
                    document.Add(image);
                    document.Close();

                    File.WriteAllBytes(outputPDFFilePath, ms.ToArray());
                }

                return true;

            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Convert Image To PDF: unexpected exception occured: ", e);
                return false;
            }

        }

        [DirectDom("Convert Word To PDF")]
        [DirectDomMethod("Convert Word File {Word Path} to PDF {Output Full File Name}")]
        [MethodDescription("Converts Word to PDF")]
        public static bool ConvertWordToPDF(string inputFilePath, string outputPDFFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputPDFFilePath))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Word To PDF: missing arguments");
                    }
                    return false;
                }

                if (!System.Web.MimeMapping.GetMimeMapping(inputFilePath).Equals("application/msword") && 
                    !System.Web.MimeMapping.GetMimeMapping(inputFilePath).Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Word To PDF: provided word file is not of valid extension");
                    }
                    return false;
                }

                object missingValue = System.Reflection.Missing.Value;

                Word.Application wordApp = new Word.Application
                {
                    Visible = false,
                    DisplayAlerts = Word.WdAlertLevel.wdAlertsNone
           
                };
                Word.Document doc = wordApp.Documents.Open(inputFilePath);
                doc.Activate();

                doc.SaveAs(outputPDFFilePath, Word.WdSaveFormat.wdFormatPDF, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue);

                doc.Close();
                wordApp.Quit();

                releaseComObject(doc);
                releaseComObject(wordApp);

                return true;

            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Convert Word To PDF: unexpected exception occured: ", e);
                return false;
            }

        }

        [DirectDom("Convert Excel To PDF")]
        [DirectDomMethod("Convert Excel File {Excel Path} to PDF {Output Full File Name}")]
        [MethodDescription("Converts Word to PDF")]
        public static bool ConvertExcelToPDF(string inputFilePath, string outputPDFFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputPDFFilePath))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Excel To PDF: missing arguments");
                    }
                    return false;
                }

                if (!System.Web.MimeMapping.GetMimeMapping(inputFilePath).Equals("application/vnd.ms-excel") &&
                    !System.Web.MimeMapping.GetMimeMapping(inputFilePath).Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Direct.PDFExtended.Library - Convert Excel To PDF: provided excel file is not of valid extension");
                    }
                    return false;
                }

                object missingValue = System.Reflection.Missing.Value;

                Excel.Application excelApp = new Excel.Application
                {
                    Visible = false,
                    DisplayAlerts = false

                };
                Excel.Workbook workbook = excelApp.Workbooks.Open(inputFilePath);
                workbook.Activate();

                workbook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, outputPDFFilePath, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue, missingValue);

                workbook.Close();
                excelApp.Quit();

                releaseComObject(workbook);
                releaseComObject(excelApp);

                return true;

            }
            catch (Exception e)
            {
                _log.Error("Direct.PDFExtended.Library - Convert Word To PDF: unexpected exception occured: ", e);
                return false;
            }

        }

        [DirectDom("Get Text Width")]
        [DirectDomMethod("Get width of text {Text} in field {Field Name} from PDF {PDF File Path}")]
        [MethodDescription("Calculates the width (in PDF points) of the given text when rendered inside the specified form field in the PDF.")]
        public static double GetTextWidth(string text, string fieldName, string pdfFilePath)
        {
            // Automatically pick up the method name
            string func = MethodBase.GetCurrentMethod().Name;
            _log.Debug($"Direct.PDFExtended.Library - {func}: Entering; text='{text}', field='{fieldName}', file='{pdfFilePath}'");

            // 1) Param validation
            if (string.IsNullOrWhiteSpace(pdfFilePath))
            {
                _log.Debug($"Direct.PDFExtended.Library - {func}: PDF path is empty");
                throw new ArgumentNullException(nameof(pdfFilePath), "PDF file path cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                _log.Debug($"Direct.PDFExtended.Library - {func}: Field name is empty");
                throw new ArgumentNullException(nameof(fieldName), "Field name cannot be null or empty.");
            }
            if (!File.Exists(pdfFilePath))
            {
                _log.Debug($"Direct.PDFExtended.Library - {func}: File not found at '{pdfFilePath}'");
                throw new FileNotFoundException($"PDF not found: {pdfFilePath}", pdfFilePath);
            }

            text = text ?? string.Empty;
            PdfReader reader = null;

            try
            {
                // 2) Open PDF (no IDisposable on PdfReader in v4.1.2.0)
                reader = new PdfReader(pdfFilePath);
                var fields = reader.AcroFields;

                // 3) Lookup the field item
                var item = fields.GetFieldItem(fieldName);
                if (item == null)
                {
                    string msg = $"Field '{fieldName}' not found in '{pdfFilePath}'.";
                    _log.Error($"Direct.PDFExtended.Library - {func}: {msg}");
                    throw new InvalidOperationException(msg);
                }

                // 4) Pull the merged dictionary out of the public ArrayList
                ArrayList mergedList = item.merged;
                if (mergedList == null || mergedList.Count == 0)
                {
                    string msg = $"Could not find merged dictionary for field '{fieldName}'.";
                    _log.Error($"Direct.PDFExtended.Library - {func}: {msg}");
                    throw new InvalidOperationException(msg);
                }
                var mergedDict = mergedList[0] as PdfDictionary;
                if (mergedDict == null)
                {
                    string msg = $"Merged entry was not a PdfDictionary for '{fieldName}'.";
                    _log.Error($"Direct.PDFExtended.Library - {func}: {msg}");
                    throw new InvalidOperationException(msg);
                }

                // 5) Decode into a TextField to get .Font and .FontSize
                var tf = new TextField(null, null, null);
                fields.DecodeGenericDictionary(mergedDict, tf);

                // 6) Fallback defaults
                BaseFont bf = tf.Font
                             ?? BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                float fontSize = tf.FontSize > 0 ? tf.FontSize : 12f;
                _log.Debug($"Direct.PDFExtended.Library - {func}: using font '{bf.PostscriptFontName}' @ {fontSize}pt");

                // 7) Measure
                float widthPt = bf.GetWidthPoint(text, fontSize);
                _log.Debug($"Direct.PDFExtended.Library - {func}: measured width = {widthPt}pt");

                return (double)widthPt;
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException || ex is InvalidOperationException))
            {
                _log.Error($"Direct.PDFExtended.Library - {func}: unexpected exception occured: ", ex);
                // rethrow as-is so callers see the underlying exception
                throw;
            }
            finally
            {
                // 8) Clean up
                reader?.Close();
            }
        }


        private static bool ValidateImageInput(
             string imageFilePath,
             string imageFileName
             )
        {
            _log.Debug("Direct.PDFExtended.Library - ValidateImageInput Parameters: " + imageFilePath + ", " + imageFileName);
            string fullFilePath = Path.Combine(imageFilePath, imageFileName);
            if (!string.IsNullOrEmpty(imageFileName) &&
                 imageFileName.Length >= 5 &&
                (imageFileName.ToUpper().EndsWith(".JPG") || imageFileName.ToUpper().EndsWith(".JPEG") ||
                 imageFileName.ToUpper().EndsWith(".PNG") || imageFileName.ToUpper().EndsWith(".GIF")) &&
                (File.Exists(imageFilePath != null ? Path.Combine(imageFilePath, imageFileName) : imageFileName)))
            {

                return true;
            }

            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat("Direct.PDFExtended.Library - ValidateImageInput - Path {1} is not valid, please enter valid pdf path", fullFilePath);
            }

            return false;
        }

        private static bool ValidateInput(
            string path,
            string fileName,
            string methodName,
            bool bCheckExistence = true)
        {
            if (!string.IsNullOrEmpty(fileName) &&
                fileName.Length >= 5 &&
                fileName.ToUpper().EndsWith(".PDF") &&
                (!bCheckExistence || File.Exists(path != null ? Path.Combine(path, fileName) : fileName)))
            {
                return true;
            }

            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat("Direct.PDFExtended.Library - ValidateInput.{0} - Path [{1}] is not valid, please enter valid pdf path", methodName, fileName);
            }

            return false;
        }

        public static byte[] RenameFields(string inputFilePath, int fieldNameExtender)
        {
            MemoryStream os = null;
            PdfReader reader = null;
            PdfStamper pdfStamper = null;
            try
            {
                os = new MemoryStream();
                reader = new PdfReader(inputFilePath);
                pdfStamper = new PdfStamper(reader, os);
                AcroFields acroFields = pdfStamper.AcroFields;
                foreach (string key in (IEnumerable)reader.AcroFields.Fields.Keys)
                    acroFields.RenameField(key, string.Format("{0}{1}", key, fieldNameExtender));
            }
            finally
            {
                pdfStamper.Close();
                reader.Close();
            }
            return os.ToArray();
        }
        private static void releaseComObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}