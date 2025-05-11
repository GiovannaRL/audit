using ExpertPdf.HtmlToPdf.PdfDocument;
using System;
using System.IO;
using Azure.Storage.Blobs;

namespace xPlannerAPI.Interfaces
{
    interface IFileStreamRepository : IDisposable
    {
        BlobClient GetBlob(string container_name, string filename);
        // create_cover(asset item);
        string GetBlobSasUri(BlobClient blob);
        void DeleteBlob(string container, string fileName);
        Document ConvertToPDF(StringWriter file, string path);
        Document ConvertToPDF(StringWriter file);
        bool DeleteAllTemporaryDocs(string directory);
        bool DeleteTemporaryDoc(string directory, string filename);
        string SaveDocumentTemporarily(Document file, string directory, string fileName);
        bool SaveDocumentTemporarily(Document file, string path);
        bool DeleteFile(string path);
        DirectoryInfo CreateLocallyDirectory(string directoryPath);
        bool MergeFiles(string[] paths, string saveInto, string[] titles = null);
        void UploadToCloud(string filePath, string container, string filename);
        void UploadToCloud(FileStream file, string container, string filename);
    }
}
