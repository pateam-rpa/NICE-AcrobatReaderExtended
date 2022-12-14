using Direct.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using log4net;

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
                            Rectangle rectangle = new Rectangle(520f, 6f, 570f, 18f);
                            rectangle.BackgroundColor = Color.WHITE;
                            overContent.Rectangle(rectangle);
                            ColumnText.ShowTextAligned(
                                overContent, 
                                2, 
                                new Phrase(
                                    new Chunk(string.Format("{0}", num++), 
                                    FontFactory.GetFont("Helvetica", 7f, 0, Color.BLACK))
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
    }
}