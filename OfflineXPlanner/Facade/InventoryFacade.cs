using Newtonsoft.Json;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using xPlannerCommon.Models;
using OfflineXPlanner.Extensions;

namespace OfflineXPlanner.Facade
{
    public class InventoryFacade
    {
        private static HttpClient _client = new HttpClient();
        private static readonly string importerEndpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.assetsInventoryImporter}/";
        private static readonly string inventoryEndpoint =
           $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.assetsInventory}/";

        static InventoryFacade()
        {
            _client.Timeout = new TimeSpan(1, 0, 0);
        }

        public static ExportAnalysisResult AnalyzeData(int project_id, byte[] excelFile, string excelFileName)
        {
            // Verify if there is a logged token
            if (!SecurityUtil.IsLogged())
            {
                //TODO: throw exception
            }

            using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                content.Add(new StreamContent(new MemoryStream(excelFile)), "inventory", excelFileName);

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
                var res = _client.Post($"{importerEndpoint}{RestAPIActions.Analyze}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project_id}/1", content);
                try
                {
                    var result = res.Content.ReadAs<List<ExportAnalysisResult>>();
                    return result[0];
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error to try upload data");
                    return null;
                }
            }
        }

        static List<Inventory> ReverseInventoryIdAndAccessId(List<Inventory> inventoryData)
        {
            foreach(var i in inventoryData)
            {
                var tmp = i.Id;
                i.Id = i.inventory_id;
                i.inventory_id = tmp != null ? (int)tmp : 0;
            }

            return inventoryData;
        }
        public static ExportAnalysisResult ExportData(int project_id, List<Inventory> inventoryData)
        {
            // Apply the InventoryId to Id, which is required by the import, later we revert back
            //inventoryData = ReverseInventoryIdAndAccessId(inventoryData);
            var data = JsonConvert.SerializeObject(inventoryData);
            var buffer = Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Cursor.Current = Cursors.WaitCursor;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = _client.Put($"{importerEndpoint}{RestAPIActions.Import}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project_id}", byteContent);
            Cursor.Current = Cursors.Default;
            inventoryData = ReverseInventoryIdAndAccessId(inventoryData);
            if (res.IsSuccessStatusCode)
            {
                return res.Content.ReadAs<ExportAnalysisResult>();
            }

            // TODO: Throw exception when fails
            return null;
        }

        public static bool UploadPictures(int project_id, int inventory_id, List<FileData> pictures)
        {
            // Verify if there is a logged token
            if (!SecurityUtil.IsLogged())
            {
                //TODO: throw exception
                return false;
            }

            var data = JsonConvert.SerializeObject(pictures);
            var buffer = Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = _client.Post($"{inventoryEndpoint}{RestAPIActions.Pictures}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project_id}/{inventory_id}", byteContent);
            return res.IsSuccessStatusCode;
        }
    }
}
