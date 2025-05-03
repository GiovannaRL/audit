using System;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IDocumentRepository : IDisposable
    {
        void DeleteRoomPicture(int domain_id, int project_id, int phase_id, int department_id, int room_id, int picture_id);
        void DeleteInventoryPicture(int domain_id, int project_id, int inventory_id, int picture_id);
        List<PictureInfo> GetRoomPictures(int domain_id, int project_id, int phase_id, int department_id, int room_id);
        List<InventoryPictureInfo> GetInventoryPictures(int domain_id, int project_id, int inventory_id);
        bool ChangeDocumentType(int domain_id, int project_id, int document_id, int newTypeId);
        bool RotatePhoto(int domain, int project_id, int document_id, int newRotate);
    }
}
