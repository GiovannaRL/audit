using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFHelper;
using System.IO;
using System.Diagnostics;

namespace PDFHelper_Tests
{
    [TestClass]
    public class Basic
    {
        string TestPath
        {
            get
            {
                var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, @"..\..\PDFHelper_Tests\TestFiles");
                return path;
            }
        }

        string [] Files
        {
            get
            {
                return Directory.GetFiles(TestPath, "*.pdf");
            }
        }



        void Convert(long max_size)
        {
            PDF pdf = new PDF();
            string output = Path.Combine(TestPath, "Output.pdf");
            File.Delete(output);
            pdf.MergeFiles(Files, Files, output, max_size);
            Assert.IsTrue(File.Exists(output), "Error, file was not created");
            long total_size = 0;
            foreach(var file in Files)
            {
                if (string.Compare(file, output, true) == 0)
                    continue;
                FileInfo fi2 = new FileInfo(file);
                total_size += fi2.Length;
            }
            FileInfo fi = new FileInfo(output);
            Assert.IsTrue(fi.Length >= (total_size*0.9), "Invalid output file size");
        }
        
        [TestMethod]
        public void ExceededFileSize()
        {
            try
            {
                Convert(10*1024);
                Assert.Fail("Merge should have failed, maximum size was exceeded");
            }
            catch (OutOfMemoryException )
            {
            }
            catch (Exception ex)
            {
                Assert.Fail("Wrong exception generated => " + ex.ToString());
            }
        }

        [TestMethod]
        public void HappyPath()
        {
            Convert(500 * 1024);
        }
    }
}
