using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using System;
using System.IO;
using System.Windows.Forms;

namespace OfflineXPlanner.Utils
{
    public static class IOUtil
    {
        private static string DATABASE_FILENAME = "audaxware_offline.accdb";
        private static string DATABASE_FILENAME_EXT = "accdb";

        public static string GetRootDirectory()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string rootLower = root.ToLower();

            if (rootLower.Contains("bin"))
            {
                root = root.Substring(0, rootLower.IndexOf("bin"));
            }

            return root;
        }

        public static string GetExcelFilePath(int domain)
        {
            var excelPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audaxware");
            CreateDirectory(excelPath);

            excelPath = Path.Combine(excelPath, "ImportGeneratedFiles");
            CreateDirectory(excelPath);

            return Path.Combine(excelPath, $"ImportFileDomain_{domain}.xlsx");
        }

        private static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static void MoveDatabaseFile() {

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Audaxware");

            CreateDirectory(directory);

            string destPath = Path.Combine(directory, DATABASE_FILENAME);

            if (!File.Exists(destPath))
            {
                File.Copy(Path.Combine(GetRootDirectory(), DATABASE_FILENAME), destPath);
            } else {

                IVersionDAO versionDAO = new VersionDAO();
                int version = versionDAO.GetCurrentInstalledVersion();
                int newVersion = versionDAO.GetNewVersion();

                if (version < newVersion) {

                    MessageBox.Show("Your current database is not longer compatible with this version and will be reset");
                    File.Move(destPath, destPath.Replace($".{DATABASE_FILENAME_EXT}", $"_v{version}.{DATABASE_FILENAME_EXT}"));
                    File.Copy(Path.Combine(GetRootDirectory(), DATABASE_FILENAME), destPath);                       
                }   
            }
        }
    }
}
