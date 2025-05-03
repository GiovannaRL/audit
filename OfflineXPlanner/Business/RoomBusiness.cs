using System.Collections.Generic;
using System.Data;
using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.IO;
using xPlannerCommon.Models;
using System.Diagnostics;
using OfflineXPlanner.Facade;
using System.Windows.Forms;

namespace OfflineXPlanner.Business
{
    public class RoomBusiness
    {
        public static Room DuplicateRoom(int project_id, int department_id, int room_id, Room newRoomInfo)
        {
            IRoomDAO roomDAO = new RoomDAO();
            return roomDAO.DuplicateItem(project_id, department_id, room_id, newRoomInfo);
        }

        public static DataTable Get(int projectId, int departmentId)
        {
            var dao = new RoomDAO();

            return dao.Get(projectId, departmentId);
        }

        public static bool Insert(Room room)
        {
            var dao = new RoomDAO();
            return dao.Insert(room) != null;
        }

        public static bool Update(Room room)
        {
            var dao = new RoomDAO();
            return dao.Update(room);
        }

        private static bool UploadRoomPictures(Room room, List<string> paths)
        {
            if (room == null || ListUtil.isEmptyOrNull(paths))
            {
                return true;
            }

            UploadRoomPicturesReq request = new UploadRoomPicturesReq
            {
                roomName = room.Name,
                roomNumber = room.Number,
                departmentName = room.Dpto.description,
                pictures = new List<FileData>()
            };

            bool isSuccess = true;

            var reportFile = PictureUtil.GetUploadErrorReportFilePath(room.ProjectId);
            foreach (string path in paths)
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);

                    request.pictures.Add(new FileData
                    {
                        fileExtension = "jpg",
                        base64File = Convert.ToBase64String(bytes)
                    });

                    int retry = 0;

                    while (retry < 3)
                    {
                        if (RoomFacade.UploadPictures(room.ProjectId, request))
                        {
                            break;
                        }
                        ++retry;
                    }
                    if (retry >= 3)
                    {
                        File.AppendAllText(reportFile, $"Failed to upload file: {path}\n");
                        isSuccess = false;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Erro to read file: {path}", e);
                    File.AppendAllText(reportFile, $"Error to read file: {path}\n");
                    isSuccess = false;
                }
            }
            
            return isSuccess;
        }

        public static void ExportRoomPictures(int project_id)
        {
            IRoomDAO roomDAO = new RoomDAO();
            var rooms = roomDAO.GetAllFromProjectWithDepartment(project_id);

            if (!ListUtil.isEmptyOrNull(rooms)) {

                ProgressBarForm progressBarForm = new ProgressBarForm("Uploading room's pictures", rooms.Count);
                progressBarForm.Show();
                var reportFile = PictureUtil.GetUploadErrorReportFilePath(project_id);

                bool isSuccess = true;
                foreach (var room in rooms) {
                    var paths = PictureUtil.GetPictureFileNames(room.ProjectId, room.DepartmentId, room.Id);
                    if (!UploadRoomPictures(room, paths))
                    {
                        isSuccess = false;
                        File.AppendAllText(reportFile, $" Error for the following room: Id:{room.Id}, Department Id: {room.DepartmentId}, Name: {room.Name}, Number: {room.Number}\n");
                    }

                    progressBarForm.PerformStep();
                }

                if (!isSuccess)
                {
                    MessageBox.Show($"The upload has been completed but some images failed to upload. Please see the report at \'{reportFile}\'", "Upload Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                progressBarForm.Close();
            }
        }
    }
}
