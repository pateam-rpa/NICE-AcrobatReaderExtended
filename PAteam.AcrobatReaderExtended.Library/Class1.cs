using Direct.Interface;
using Direct.Shared;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private static readonly int nMagorFileVersion = (int)char.GetNumericValue(FileVersionInfo.GetVersionInfo("itextsharp.dll").FileVersion[0]);

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

        [DirectDom("Merges List of files")]
        [DirectDomMethod("Merges {list of pdf files} into {pdf file}")]
        [MethodDescription("Adds blank pages after every page.")]
        public static bool Merge(DirectCollection<String> InFiles, String OutFile)
        {
            try
            {
                FileStream stream = new FileStream(OutFile, FileMode.Create);
                Document doc = new Document();
                PdfCopy pdf = new PdfCopy(doc, stream);
            
                doc.Open();

                PdfReader reader = null;
                PdfImportedPage page = null;

                foreach (var file in InFiles)
                {
                    reader = new PdfReader(file);

                    for (int i = 0; i < reader.NumberOfPages; i++)
                    {
                        page = pdf.GetImportedPage(reader, i + 1);
                        pdf.AddPage(page);
                    }

                    pdf.FreeReader(reader);
                    reader.Close();
                };
                doc.Close();
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
    }
}