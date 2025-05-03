using System;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    public interface IOptionRepository : IDisposable
    {
        void AddPicture(GenericOption option, FileData picture);
        void InsertOptionPictureAssociation(short domain_id, GenericOption option, FileData picture);
        assets_options AddOption(GenericOption option, string addedBy);
        assets_options AddCustomOption(GenericOption option, string addedBy);
        void DeletePicture(assets_options option);
    }
}