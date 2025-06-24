using OfflineXPlanner.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace OfflineXPlanner.Utils
{
    public static class PictureUtil
    {
        private static string _maskImagesPathRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Audaxware");

        private static string _maskImagesPath = Path.Combine(_maskImagesPathRoot, "project_{0}", "department_{1}", "room_{2}");

        public static List<string> GetPictureFileNames(int project_id, int department_id, int room_id, int? inventory_itemID = null)
        {
            var path = GetRoomPictureDirectory(project_id, department_id, room_id);
            if (inventory_itemID != null)
            {
                path = GetInventoryPictureDirectory(path, inventory_itemID.GetValueOrDefault());
            }

            if (path == null)
                return new List<string>();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly).ToList();
        }

        public static string GetUploadErrorReportFilePath(int project_id)
        {
            var reportFile = Path.Combine(_maskImagesPathRoot, $"Project_Picture_Report_{project_id}");
            reportFile = Path.ChangeExtension(reportFile, "csv");
            return reportFile;
        }

        public static string GetRoomPictureDirectory(int project_id, int department_id, int room_id)
        {
            return string.Format(_maskImagesPath, project_id, department_id, room_id);
        }

        public static string GetDeleteItemsDirectory(string deletedItemPath){ 

            var splitedPath = deletedItemPath.Split(new string[] { "department" }, StringSplitOptions.None);
            var completePath = splitedPath[0] + "deletedItems\\department" + splitedPath[1];

            return completePath;
        }

        public static bool MoveRoomDirectory(int project_id, int old_department_id, int room_id, int new_department_id)
        {
            string oldRoomPath = GetRoomPictureDirectory(project_id, old_department_id, room_id);
            string newDepartmentPath = Path.Combine(_maskImagesPathRoot, $"project_{project_id}", $"department_{new_department_id}");
            string newRoomPath = Path.Combine(newDepartmentPath, $"room_{room_id}");

            try
            {
                if (!Directory.Exists(oldRoomPath))
                    return false;

                if (!Directory.Exists(newDepartmentPath))
                    Directory.CreateDirectory(newDepartmentPath);

                if (Directory.Exists(newRoomPath))
                    Directory.Delete(newRoomPath, true); 

                Directory.Move(oldRoomPath, newRoomPath);

                return Directory.Exists(newRoomPath) && !Directory.Exists(oldRoomPath);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string GetInventoryPictureDirectory(string roomDirectoryPath, int itemId)
        {
            return Path.Combine(roomDirectoryPath, itemId.ToString());
        }

        public static Bitmap RotateImage(Image image, float angle)
        {
            // center of the image
            float rotateAtX = image.Width / 2;
            float rotateAtY = image.Height / 2;
            return RotateImage(image, rotateAtX, rotateAtY, angle);
        }

        private static Bitmap RotateImage(Image image, float rotateAtX, float rotateAtY, float angle)
        {
            int W, H, X, Y;
            double dW = image.Width;
            double dH = image.Height;

            double degrees = Math.Abs(angle);
            if (degrees <= 90)
            {
                double radians = 0.0174532925 * degrees;
                double dSin = Math.Sin(radians);
                double dCos = Math.Cos(radians);
                W = (int)(dH * dSin + dW * dCos);
                H = (int)(dW * dSin + dH * dCos);
                X = (W - image.Width) / 2;
                Y = (H - image.Height) / 2;
            }
            else
            {
                degrees -= 90;
                double radians = 0.0174532925 * degrees;
                double dSin = Math.Sin(radians);
                double dCos = Math.Cos(radians);
                W = (int)(dW * dSin + dH * dCos);
                H = (int)(dH * dSin + dW * dCos);
                X = (W - image.Width) / 2;
                Y = (H - image.Height) / 2;
            }

            //create a new empty bitmap to hold rotated image
            Bitmap bmpRet = new Bitmap(W, H);
            bmpRet.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(bmpRet);

            //Put the rotation point in the "center" of the image
            g.TranslateTransform(rotateAtX + X, rotateAtY + Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-rotateAtX - X, -rotateAtY - Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0 + X, 0 + Y));

            return bmpRet;
        }
    }
}
