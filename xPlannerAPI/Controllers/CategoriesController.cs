using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Services;
using HelperAPI = xPlannerAPI.App_Data.Helper;
using System.Web;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogCategories)]
    public class CategoriesController : TableGenericController<assets_category>
    {
        public CategoriesController() : base(new[] { "domain_id", "category_id" }, new[] { "domain_id" }, new[] { "domain" }, true, true) { }

        protected override bool UpdateReferences(assets_category item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var fieldsToCheck = new string[] { "HVAC", "Plumbing", "Gases", "IT", "Electrical", "Support", "Physical",
                "Environmental" };

            var properties = item.GetType().GetProperties();
            foreach (var property in properties)
            {
                var dataValue = property.GetValue(item, null);
                if (fieldsToCheck.Contains(property.Name) && dataValue == null)
                {
                    property.SetValue(item, "E", null);
                }
            }
            return true;
        }

        [ActionName("All")]
        public override IEnumerable<assets_category> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var categories = base.GetAll(id1, id2, id3, id4, id5).ToList();
            categories.ForEach(ac => ac.domain = new domain { domain_id = ac.domain_id, name = Helper.GetEnterpriseName(ac.domain.name) });
            return categories;
        }

        /* Returns the categories that is allowed to add a subcategory in that domain */
        [ActionName("ToSub")]
        public IEnumerable<assets_category> GetToSub(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return GetAll(id1, id2, id3, id4, id5);
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<asset>())
            {
                if (!repository.GetAll(new string[] { }, new int[] { }, new[] { "assets_subcategory.assets_category" })
                    .Any(a => a.assets_subcategory.assets_category.domain_id == id1 && a.assets_subcategory.assets_category.category_id == id2))
                {
                    base.Delete(id1, id2, id3, id4, id5);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Conflict, "The category could not be deleted. There is assigned asset!");
        }

        [ActionName("Export")]
        public HttpResponseMessage GetExport(short id1)
        {
            using (var rep = new TableRepository<assets_category>())
            {
                var categories = rep.GetAll(new[] { "domain_id" }, new[] { id1 }, new[] { "assets_subcategory" }, true);

                using (var expRepository = new ExportImportRepository())
                {
                    var filePath = ExportImportRepository.ExportCategories(id1, categories);

                    using (var fileRep = new FileStreamRepository())
                    {
                        var stream = fileRep.GetMemoryStreamFile(filePath);

                        var message = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StreamContent(stream)
                        };

                        message.Content.Headers.ContentLength = stream.Length;
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                        message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = $"Categories_{id1}_export,xlsx",
                            Size = stream.Length
                        };

                        fileRep.DeleteFile(filePath);

                        return message;
                    }
                }
            }
        }

        [ActionName("Import")]
        public async Task<HttpResponseMessage> PostImport(short id1)
        {
            try
            {
                var timestamp = DateTime.Now;
                var directoryPath = Path.Combine(Domain.GetRoot(), "ImportFiles", "Categories");
                using (var fileRep = new FileStreamRepository())
                {
                    fileRep.CreateLocalDirectory(directoryPath);
                    var filePath = Path.Combine(directoryPath, string.Format("Categories_{0}_{1}_{2}.xlsx", id1, timestamp.ToShortDateString().Replace("/", ""), timestamp.ToShortTimeString().Replace(":", "")));
                    using (var fileStream = File.Create(filePath))
                    {
                        var requestStreamXX = await Request.Content.ReadAsMultipartAsync();
                        var requestStream = requestStreamXX.Contents[0].ReadAsStreamAsync().Result;
                        requestStream.CopyTo(fileStream);
                    }

                    await ExportImportRepository.ImportCategories(id1, filePath, UserId);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                //fileRep.DeleteFile(filePath);
            }
            catch (IOException e)
            {
                if (e.InnerException != null && e.InnerException is HttpException && (e.InnerException as HttpException).WebEventCode == 3004)
                {
                    var message = string.Format(StringMessages.ImportFileSizeExceeded, HelperAPI.GetMaxRequestLength());
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, new { ErrorMessage = message });
                }

                throw e;
            }
        }
    }
}