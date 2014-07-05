using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SpeedPdfMerge
{
    class Program
    {
        // sample usage: SpeedPdfMerge.exe -SOURCE "file1.pdf" "file2.pdf" -DEST "newfile.pdf"
        static int Main(string[] arrParams)
        {
            if (arrParams == null || arrParams.Length < 1)
            {
                Console.WriteLine("STATUS:ERROR - No files. Usage: SpeedPdfMerge.exe -SOURCE file1.pdf file2.pdf -DEST output.pdf");
                return 0;
            }

            List<string> listSourceFiles = new List<string>();

            string strDestFile = "";

            string strParamKey = "";
            foreach (string strParam in arrParams)
            {
                if (strParam == "-SOURCE")
                {
                    strParamKey = "source";
                    continue;
                }

                if (strParam == "-DEST")
                {
                    strParamKey = "dest";
                    continue;
                }

                if (strParamKey == "dest")
                {
                    strDestFile = strParam;
                    continue;
                }

                if (strParamKey == "source")
                {
                    listSourceFiles.Add(strParam);
                    continue;
                }
            }

            if (String.IsNullOrEmpty(strDestFile) == true)
            {
                Console.WriteLine("STATUS:ERROR - Destination file -DEST is required");
                return 0;
            }

            if (listSourceFiles.Count < 1)
            {
                Console.WriteLine("STATUS:ERROR - Source files -SOURCE is required");
                return 0;
            }

            // You can convert it back to an array if you would like to
            string[] arrFiles = listSourceFiles.ToArray();

            // statistic
            int numFiles = 0;
            int numPages = 0;
            DateTime dateStart = DateTime.Now;

            // ----
            Document document = new Document();
            MemoryStream output = new MemoryStream();
            try
            {
                try
                {
                    // Initialize pdf writer
                    PdfWriter writer = PdfWriter.GetInstance(document, output);
                    //writer.PageEvent = new PdfPageEvents();

                    // set pdf version
                    //writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_6);

                    // Open document to write
                    document.Open();
                    PdfContentByte content = writer.DirectContent;

                    // Iterate through all pdf documents
                    foreach (string strFilename in arrFiles)
                    {

                        if (System.IO.File.Exists(strFilename) == false)
                        {
                            if (document != null)
                            {
                                document.Close();
                            }

                            Console.WriteLine("STATUS:ERROR - File not found: " + strFilename);
                            return 0;
                        }

                        numFiles++;

                        // Create pdf reader
                        // File.ReadAllBytes(strFilename)
                        PdfReader reader = new PdfReader(strFilename);
                        int numberOfPages = reader.NumberOfPages;

                        // Iterate through all pages
                        for (int currentPageIndex = 1; currentPageIndex <= numberOfPages; currentPageIndex++)
                        {
                            numPages++;

                            // Determine page size for the current page
                            Rectangle psize = reader.GetPageSizeWithRotation(currentPageIndex);

                            document.SetPageSize(psize);

                            // Create page
                            document.NewPage();
                            PdfImportedPage importedPage = writer.GetImportedPage(reader, currentPageIndex);

                            switch (psize.Rotation)
                            {
                                case 0:
                                    content.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);
                                    break;
                                case 90:
                                    content.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(1).Height);
                                    break;
                                case 180:
                                    content.AddTemplate(importedPage, -1f, 0, 0, -1f, 0, 0);
                                    break;
                                case 270:
                                    content.AddTemplate(importedPage, 0, 1.0F, -1.0F, 0, reader.GetPageSizeWithRotation(1).Width, 0);
                                    break;
                                default:
                                    break;
                            }

                            // This block is not working correctly
                            // Determine page orientation
                            //int pageOrientation = reader.GetPageRotation(currentPageIndex);
                            //if ((pageOrientation == 90) || (pageOrientation == 270))
                            //{
                            //content.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(currentPageIndex).Height);
                            //}
                            //else
                            //{
                            //content.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);
                            //}
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception("There has an unexpected exception occured during the pdf merging process.", exception);
                }
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                }

            }

            byte[] arrContent = output.ToArray();

            // Write out PDF from memory stream.
            using (FileStream fs = File.Create(strDestFile))
            {
                fs.Write(arrContent, 0, (int)arrContent.Length);
            }

            // ...and start a viewer.
            //Process.Start(strDestFile);

            // timer
            DateTime dateStop = DateTime.Now;
            long elapsedTicks = dateStop.Ticks - dateStart.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

            // write status to console
            Console.WriteLine("STATUS:OK");
            Console.WriteLine("OUTPUT:{0}", strDestFile);
            Console.WriteLine("FILES:{0}", numFiles);
            Console.WriteLine("PAGES:{0}", numPages);
            Console.WriteLine("TIME:{0}", elapsedSpan.TotalSeconds);

            return 1;
        }
    }
}
