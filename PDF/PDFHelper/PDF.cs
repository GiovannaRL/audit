using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Reflection;

namespace PDFHelper
{
    public class PDF
    {
        public static void ConvertTo14(string file_name, string output_pdf_name)
        {
            var file_stream = new FileStream(output_pdf_name, FileMode.Create);
            try
            {
                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(file_name);
                var encrypted = typeof(iTextSharp.text.pdf.PdfReader).GetField("encrypted", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                encrypted.SetValue(reader, false);

                // we retrieve the total number of pages
                int n = reader.NumberOfPages;
                // step 1: creation of a document-object
                iTextSharp.text.Document document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
                // step 2: we create a writer that listens to the document
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, file_stream);
                //write pdf that pdfsharp can understand
                writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_4);
                // step 3: we open the document
                document.Open();
                iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
                iTextSharp.text.pdf.PdfImportedPage page;

                int rotation;

                int i = 0;
                while (i < n)
                {
                    i++;
                    document.SetPageSize(reader.GetPageSizeWithRotation(i));
                    document.NewPage();

                    page = writer.GetImportedPage(reader, i);
                    rotation = reader.GetPageRotation(i);
                    if (rotation == 90 || rotation == 270)
                    {
                        cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                    }
                    else
                    {
                        cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                    }
                }
                document.Close();
            }
            catch(Exception ex)
            {
                try
                {
                    file_stream.Close();
                }
                catch
                {
                }
                try
                {
                    if (File.Exists(output_pdf_name))
                        File.Delete(output_pdf_name);
                }
                catch
                { }
                throw ex;
            }

        }

        public void MergeFiles(string path, string[] files, string [] titles, string output_file, long max_size)
        {
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = Path.Combine(path, files[i]);
            }
            output_file = Path.Combine(path, output_file);
            MergeFiles(files, titles, output_file, max_size);
        }

        public void MergeFiles(string[] files, string[] titles, string output_file, long max_size)
        {

            //int rep_file_count = 0;
            PdfDocument output = null;
            PdfDocument input = null;
            long size = 0;
            Progress = 0;
            foreach (string file in files)
            {
                if (output == null)
                    output = new PdfDocument();

                try
                {
                    input = PdfSharp.Pdf.IO.PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    var info = new FileInfo(file);
                    size += info.Length;
                }
                catch
                {
                    string file_only = Path.GetFileName(file);
                    string directory_only = Path.GetDirectoryName(file);
                    string converted = Path.Combine(new string[] { directory_only,
                            "converted", file_only });
                    if (!File.Exists(converted))
                    {
                        string converted_directory = Path.GetDirectoryName(converted);
                        if (!Directory.Exists(converted_directory))
                            Directory.CreateDirectory(converted_directory);
                        ConvertTo14(file, converted);
                    }
                    input = PdfSharp.Pdf.IO.PdfReader.Open(converted, PdfDocumentOpenMode.Import);
                    var info = new FileInfo(converted);
                    size += info.Length;
                }
                if (size > max_size)
                {
                    throw new OutOfMemoryException(string.Format("Failure to create file, the maximum number of bytes specified {0} has been exceeded {1}",
                        max_size, size));
                }

                for(int i=0; i < input.PageCount; ++i)
                {
                    var p  = input.Pages[i];
                    output.InsertPage(output.PageCount, p);
                    if (i == 0 && titles != null && titles[Progress].Length > 0)
                    {
                        output.Outlines.Add(titles[Progress], output.Pages[output.PageCount - 1]);
                    }
                }
                input.Dispose();
                ++Progress;
            }
            output.Save(output_file);
            System.Threading.Thread.Sleep(10);
        }
        public int Progress { get; set; }
    }
}
