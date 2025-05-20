using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Services;
using xPlannerAPI.Models;
using xPlannerAPI.Tests;
using xPlannerCommon;
using xPlannerCommon.Models;
using System.Data.Entity;

namespace xPlannerAPI.Tests.Repositories
{
    [TestClass]
    public class AssetInventoryImporterRespositoryTests
    {
        private int _projectIdAudaxWare;
        private int _projectIdMillCreek;
        const string _projectDescription = "AssetInventoryImporterRespositoryTests-F0635B72-447F-4CE5-AB9F-BEF20105FDC2";
        const string _importUser = "unittests@audaxware.com";
        int _currentDomain = 1;
        int _currentProject = -1;
        audaxwareEntities _db;

        void ContextEvent(object sender, ContextEventArgs args)
        {
            args.DomainId = _currentDomain;
            args.ShowAudaxwareInfo = true;
        }

        void ClearAssets()
        {
            ClearProject(true);
        }

        void ClearProject()
        {
            ClearProject(false);
        }

        void ClearProject(bool assetsOnly)
        {
            // If project already exists, then we delete it. Cannot use the controller to delete
            // as the controller only marks the project status as deleted
            var testProjects = _db.projects.Where(x => x.project_description == _projectDescription).ToList();
            foreach (var p in testProjects)
            {
                _db.project_room_inventory.RemoveRange(_db.project_room_inventory.Where(x => x.project_id == p.project_id));
                if (!assetsOnly)
                {
                    _db.projects.Remove(p);
                }
            }
            _db.assets.RemoveRange(_db.assets.Where(x => x.added_by == _importUser));
            _db.assets_category.RemoveRange(_db.assets_category.Where(x => x.added_by == _importUser));
            _db.assets_subcategory.RemoveRange(_db.assets_subcategory.Where(x => x.added_by == _importUser));
            _db.project_room.RemoveRange(_db.project_room.Where(x => x.added_by == _importUser));
            _db.project_department.RemoveRange(_db.project_department.Where(x => x.added_by == _importUser));
            _db.project_phase.RemoveRange(_db.project_phase.Where(x => x.added_by == _importUser));
            _db.manufacturers.RemoveRange(_db.manufacturers.Where(x => x.added_by == _importUser));
            _db.jsns.RemoveRange(_db.jsns.Where(x => x.added_by == _importUser));
            _db.SaveChanges();
        }

        [TestInitialize]
        public void Initialize()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent += ContextEvent;
            _db = new audaxwareEntities();
            _db.Database.CommandTimeout = 2000000;
            ClearProject();
            using (var tableRepo = new  TableRepository<project>())
            {
                var p = new project();
                p.project_description = _projectDescription;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 1;
                p = tableRepo.Add(p);
                _projectIdAudaxWare = p.project_id;
                p = new project();
                p.project_description = _projectDescription;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 24;
                p = tableRepo.Add(p);
                _projectIdMillCreek = p.project_id;
            }
        }


        [TestCleanup]
        public void Cleanup()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent -= ContextEvent;
            ClearProject();
            _db.Dispose();
        }

        List<ImportAnalysisResult> GetAnalysisResult(ImportAnalysisResultStatus expectedResult, string resourceFile, Interfaces.ImportColumnsFormat format)
        {
            _currentDomain = format == Interfaces.ImportColumnsFormat.MillCreek ? 24 : 1;
            _currentProject = format == Interfaces.ImportColumnsFormat.MillCreek ? _projectIdMillCreek : _projectIdAudaxWare;
            var file = EmbeddedResourceHelper.GetTempFilePathFromResource(resourceFile);
            using (var repo = new AssetsInventoryImporterRepository())
            {
                var result = repo.Analyze(_currentDomain, _currentProject, file, format);
                foreach (var item in result)
                {
                    Assert.AreEqual(expectedResult, item.Status);
                }
                File.Delete(file);
                return result;
            }
        }

        void Import(string resourceFile, Interfaces.ImportColumnsFormat format)
        {
            var analysisResult = GetAnalysisResult(ImportAnalysisResultStatus.Ok, resourceFile, format).FirstOrDefault();
            // Removes all items with errors
            analysisResult.Items = analysisResult.Items.Where(x => x.Status != ImportItemStatus.Error).ToList();
            _currentDomain = format == Interfaces.ImportColumnsFormat.MillCreek ? 24 : 1;
            using (var repo = new AssetsInventoryImporterRepository())
            {
                var result = repo.Import(analysisResult.Items.ToArray(), _currentDomain,
                   _currentProject, _importUser, null);
                Assert.AreEqual(ImportAnalysisResultStatus.Ok, result.Status);
            }
        }

        void Import(ImportItem item, short domainId, ImportAnalysisResultStatus expectedStatus = ImportAnalysisResultStatus.Ok, int? projectId = null)
        {
            _currentDomain = domainId;
            if (projectId != null)
                _currentProject = (int)projectId;
            else
                _currentProject = _currentDomain == 1 ? _projectIdAudaxWare : _projectIdMillCreek;
            ImportItem[] items = new ImportItem[] { item };
            Import(items, domainId, expectedStatus, _currentProject);
        }

        void Import(ImportItem[] items, short domainId, ImportAnalysisResultStatus expectedStatus = ImportAnalysisResultStatus.Ok, int? projectId = null)
        {
            using (var repo = new AssetsInventoryImporterRepository())
            {
                var result = repo.Import(items, domainId,
                   projectId ?? (domainId == 1 ? _projectIdAudaxWare : _projectIdMillCreek), _importUser, null);
                Assert.AreEqual(expectedStatus, result.Status);
            }
        }

        List<ImportAnalysisResult> GetResult(ImportAnalysisResultStatus expectedResult, string resourceFile)
        {
            return GetAnalysisResult(expectedResult, resourceFile, Interfaces.ImportColumnsFormat.AudaxWare);
        }

        [TestMethod]
        public void AnalyzeNegativeInvalidPath()
        {
            var repo = new AssetsInventoryImporterRepository();
            _currentDomain = 1;
            var result = repo.Analyze(1, 1, "InvalidPath", Interfaces.ImportColumnsFormat.AudaxWare);
            Assert.AreEqual(ImportAnalysisResultStatus.Invalid, result.FirstOrDefault().Status);
        }

        [TestMethod]
        public void AnalyzeNegativeInvalidFile()
        {
            GetResult(ImportAnalysisResultStatus.InvalidFile, "xPlannerAPI.Tests.Repositories.TestData.InvalidExcelFile.xlsx");
        }

        [TestMethod]
        public void AnalyzeNegativeUnsupportedFile()
        {
            GetResult(ImportAnalysisResultStatus.InvalidFile, "xPlannerAPI.Tests.Repositories.TestData.UnsupportedFormat.xls");
        }

        void CheckMissingRequired(SortedSet<string> missingColumns, string embeddedResource)
        {
            string[] requiredColumns = new string[]{ AssetsInventoryImporterRepository.ColumnNameRoomName, AssetsInventoryImporterRepository.ColumnNameDepartment,
                AssetsInventoryImporterRepository.ColumnNameResp};
            var result = GetResult(ImportAnalysisResultStatus.RequiredColumnsMissing, embeddedResource).FirstOrDefault();
            foreach(var col in requiredColumns)
            {
                var targetName = AssetsInventoryImporterRepository.GetColumnNameInFormat(Interfaces.ImportColumnsFormat.AudaxWare, col);
                Assert.IsTrue(result.ErrorMessage.Contains(targetName) ^ !missingColumns.Contains(col), $"Missing column was not properly detected, message returned Col {targetName}=>{result.ErrorMessage}");
            }
        }

        [TestMethod]
        public void AnalyzeNegativeRequiredColumnsAllMissing()
        {
            var missingColumns = new SortedSet<string> { AssetsInventoryImporterRepository.ColumnNameRoomName, AssetsInventoryImporterRepository.ColumnNameDepartment,
                AssetsInventoryImporterRepository.ColumnNameResp};
            CheckMissingRequired(missingColumns, "xPlannerAPI.Tests.Repositories.TestData.RequiredMissingAll.xlsx");
        }


        [TestMethod]
        public void AnalyzeNegativeRequiredColumnsAllRespRoomName()
        {
            var missingColumns = new SortedSet<string>{ AssetsInventoryImporterRepository.ColumnNameRoomName, 
                AssetsInventoryImporterRepository.ColumnNameResp};
            CheckMissingRequired(missingColumns, "xPlannerAPI.Tests.Repositories.TestData.RequiredMissingRespRoomName.xlsx");
        }

        [TestMethod]
        public void AnalyzeNegativeRequiredColumnsNoAssetHint()
        {
            GetResult(ImportAnalysisResultStatus.AssetHintColumnMissing, "xPlannerAPI.Tests.Repositories.TestData.AssetHintMissing.xlsx");
        }

        [TestMethod]
        public void AnalyzeMillCreekVanceLab()
        {
            ClearAssets();
            var result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.VanceLab.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            Assert.AreEqual(129, result.FirstOrDefault().TotalNewCatalog, "Error on the number of items imported");
        }

        [TestMethod]
        public void AnalyzeMillCreekVanceLabTwoSheets()
        {
             ClearAssets();
            var result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.VanceLabTwoSheets.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            int totalItems = 0;
            foreach (var item in result)
            {
                Assert.AreEqual(ImportAnalysisResultStatus.Ok, item.Status);
                totalItems = totalItems + item.TotalNewCatalog;
            }

            Assert.AreEqual(129, totalItems, "Error on the number of items imported");
        }




        [TestMethod]
        public void AnalyzeMillCreekMissingSubcategoryInDescription()
        {
            // TODO(JLT) - Modify this to have multiple lines with our different items and check the results
            //ClearAssets();
            //var result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.MissingSubcategoryInDescription.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            //Assert.AreEqual(0, result.TotalNewCatalog, "We should have only one item with invalid description");
            //Assert.AreEqual(1, result.TotalErrors, "Total number of errors should be 1");
            //Assert.AreEqual(0, result.TotalNew, "We should have only one item with invalid description");
        }

        [TestMethod]
        public void ImportSimpleAsset()
        {
            var result = GetResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.SimpleNew.xlsx").FirstOrDefault();
            Assert.AreEqual(1, result.Items.Count, "Number of items to import mismatch the worksheet");
            var item = result.Items[0];
            Assert.AreEqual(ImportItemStatus.New, result.Items[0].Status, "Status returned mismatch");
            Assert.AreEqual("AID00001", item.Code, "Code mismatch");
        }

        // We are matching the project_id now and this test fails. We have to revisit this algorithm in the future
        //[TestMethod]
        //public void ImportMultipleByDescription()
        //{

        //    var result = GetResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.MultipleByDescription.xlsx");
        //    Assert.AreEqual(1, result.Items.Count, "Number of items to import mismatch the worksheet");
        //    Assert.AreEqual(ImportItemStatus.Error, result.Items[0].Status, "Status returned mismatch");
        //    Assert.IsNotNull(result.Items[0].MatchingCodes, "Matching codes returned null");
        //    Assert.IsTrue(result.Items[0].MatchingCodes.Length > 1, "Wrong number of matching codes");
        //    Assert.IsTrue(result.Items[0].MatchingCodes.Contains("MAN00032"), "Wrong matching code returned");
        //}


        [TestMethod]
        public void ImportSimpleExistingAsset()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.SimpleNew.xlsx", Interfaces.ImportColumnsFormat.AudaxWare);
            var assets = _db.assets.Where(x => x.added_by == _importUser);
            Assert.AreEqual(0, assets.Count(), "We should not have any assets for the test user in our database");
            var roomInventory = _db.project_room_inventory.Include("asset").Where(x => x.project_id == _projectIdAudaxWare);
            Assert.AreEqual(1, roomInventory.Count(), "We should have one asset in the inventory");
            var inventory_item = roomInventory.First();
            Assert.AreEqual("AID00001", inventory_item.asset.asset_code);
            Assert.AreEqual(220, inventory_item.unit_budget);
            Assert.AreEqual(2, inventory_item.budget_qty);
            Assert.AreEqual(440, inventory_item.total_budget_amt);
            ClearAssets();
        }


        public void ImportJSN()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.SimpleJSNAdd.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var assets = _db.assets.Include("jsn").Where(x => x.added_by == _importUser);
            Assert.AreEqual(1, assets.Count(), "We should have one asset");
            var asset = assets.First();
            Assert.AreEqual("A1012", asset.jsn.jsn_code, "JSN does not match");
            var roomInventory = _db.project_room_inventory.Include("asset").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "We should not have added any assets");
            var inventory_item = roomInventory.First();
            Assert.IsTrue(inventory_item.asset.asset_code.StartsWith("PHN"));
            Assert.AreEqual(400, inventory_item.unit_budget);
            Assert.AreEqual(20, inventory_item.budget_qty);
            Assert.AreEqual(8000, inventory_item.total_budget_amt);
            ClearAssets();
        }

        [TestMethod]
        public void ImportMillCreekVanceLab()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.VanceLab.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var assets = _db.assets.Include("jsn").Where(x => x.added_by == "unittests@audaxware.com");
            Assert.AreEqual(85, assets.Count(), "We should have one asset");
            var verifiedAssets = assets.Where(x => x.jsn.jsn_code == "A1012");
            Assert.AreEqual(1, verifiedAssets.Count());
            var asset = verifiedAssets.First();
            Assert.AreEqual("A1012", asset.jsn.jsn_code, "JSN does not match");
            var roomInventory = _db.project_room_inventory.Include("asset").Where(x => x.project_id == _projectIdMillCreek && asset.asset_id == x.asset_id);
            Assert.AreEqual(1, roomInventory.Count(), "Number of assets mismatch");
            var inventory_item = roomInventory.First();
            Assert.IsTrue(inventory_item.asset.asset_code.StartsWith("PHN"));
            Assert.AreEqual(400, inventory_item.unit_budget);
            Assert.AreEqual(8, inventory_item.budget_qty);
            Assert.AreEqual(400*8, inventory_item.total_budget_amt);
            ClearAssets();
        }


        [TestMethod]
        public void ImportMultipleMismatchJSNUtility()
        {
            ClearAssets();
            var items = new ImportItem[] {
            new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                Description = "This is a Category for Testing Purposes, This is  a Subcategory for testing purposes",
                JSN = "A1012",
                U1 = "A",
                U2 = "B",
                U3 = "",
                U4 = "",
                U5 = "",
                U6 = ""
            },
            new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room2",
                Description = "This is a Category for Testing Purposes, This is  a Subcategory for testing purposes",
                JSN = "A1012",
                U1 = "A",
                U2 = "B",
                U3 = "",
                U4 = "",
                U5 = "",
                U6 = ""
            }
            };

            Import(items, 24);
            var assets = _db.assets.Where(x => x.added_by == _importUser).ToList();
            Assert.AreEqual(1, assets.Count(), "Invalid number of assets returned");
            ClearAssets();
        }


        [TestMethod]
        public void ImportCreateAssetWithJSN()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                Description = "This is a Category for Testing Purposes, This is  a Subcategory for testing purposes",
                UnitBudget = 800,
                JSN = "A9999",
                U1 = "A",
                U2 = "B",
                U3 = "C",
                U4 = "D",
                U5 = "E",
                U6 = "F"
            };

            Import(item, 1);

            var roomInventory = _db.project_room_inventory.Include("asset").Include("asset.jsn").Where(x => x.project_id == _projectIdAudaxWare).ToList();
            Assert.AreEqual(1, roomInventory.Count(), "We should have one and only one asset added");
            var inventory_item = roomInventory.First();
            Assert.IsTrue(inventory_item.asset.asset_code.StartsWith("EQP"), inventory_item.asset.asset_code);
            Assert.AreEqual(800, inventory_item.unit_budget);
            Assert.AreEqual(2, inventory_item.budget_qty);
            Assert.IsNull(inventory_item.asset.jsn.asset_code);
            Assert.AreEqual("A", inventory_item.asset.jsn.utility1);
            Assert.AreEqual("B", inventory_item.asset.jsn.utility2);
            Assert.AreEqual("C", inventory_item.asset.jsn.utility3);
            Assert.AreEqual("D", inventory_item.asset.jsn.utility4);
            Assert.AreEqual("E", inventory_item.asset.jsn.utility5);
            Assert.AreEqual("F", inventory_item.asset.jsn.utility6);
            ClearAssets();
        }

        [TestMethod]
        public void ImportInvalidProjectId()
        {
            var item = new ImportItem
            {
                Status = ImportItemStatus.New,
                Code = "AID00001",
                PlannedQty = 2,
                Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                UnitBudget = 800,
                UnitEscalation = 3,
                UnitFreightMarkup = 4,
                UnitFreightNet = 5,
                UnitMarkup = 6,
                UnitInstallMarkup = 7,
                UnitInstallNet = 8,
                UnitTax = 9
            };
            Import(item, 1, ImportAnalysisResultStatus.Invalid ,1111999111);

        }

        [TestMethod]
        public void ImportExplodePPFundCode()
        {
            // If the asset has fund code PP and quantity greater than 1, we should import multiple entries for that asset
            // We do this because the PP fund code is for relocation
            ClearAssets();
            var analysisResult = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.SimpleExplodePPFundCode.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            var items = analysisResult.Items;
            Assert.AreEqual(5, items.Count());
            foreach(var item in items)
            {
                Assert.AreEqual(1, item.PlannedQty, "Quantity is invalid");
                Assert.AreEqual(400, item.UnitBudget, "Quantity is invalid");
            }
            ClearAssets();
        }

        [TestMethod]
        public void ImportSecondTime()
        {
            // If we have an existing asset with the same JSN, the result should not be to create the asset again
            ClearAssets();
            var analysisResult = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.SimpleNewJSNAndAsset.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            var items = analysisResult.Items;
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(ImportItemStatus.NewCatalog, items[0].Status);
            Import("xPlannerAPI.Tests.Repositories.TestData.SimpleNewJSNAndAsset.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var assets = _db.assets.Include("jsn").Where(x => x.added_by == _importUser);
            Assert.AreEqual(1, assets.Count(), "We should have one asset");
            var asset = assets.First();
            Assert.AreEqual("A8899", asset.jsn.jsn_code, "JSN does not match");
            analysisResult = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.SimpleNewJSNAndAsset.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            items = analysisResult.Items;
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(ImportItemStatus.New, items[0].Status);
            ClearAssets();
        }

        [TestMethod]
        public void ImportUpdateAsset()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.New,
                Description = "Test1, Test2",
                PlannedQty = 2,
                Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                UnitBudget = 800,
                UnitEscalation = 3,
                UnitFreightMarkup = 4,
                UnitFreightNet = 5,
                UnitMarkup = 6,
                UnitInstallMarkup = 7,
                UnitInstallNet = 8,
                UnitTax = 9,
                BiomedRequired = true,
                CostCenter = "MyCost",
                Tags = "Tag1, Tag2",
                Placement = "Counter",
                Comment = "This is a test comment",
                FunctionalArea = "This is my functional area",
                Height = "10",
                Width = "20",
                Depth = "30",
                MountingHeight = "40",
                ECN = "TestECN",
                TemporaryLocation = "Temporary",
            };

            Import(item, 1);
            var roomInventory = _db.project_room_inventory.Include("cost_center").Include("asset").Where(x => x.project_id == _projectIdAudaxWare).ToList();
            Assert.AreEqual(1, roomInventory.Count(), "We should have one and only one asset added");
            var inventory_item = roomInventory.First();
            var phase = _db.project_phase.Where(x => x.phase_id == inventory_item.phase_id && x.domain_id == 1).First();
            var department = _db.project_department.Where(x => x.department_id == inventory_item.department_id && x.domain_id == 1).First();
            var room = _db.project_room.Where(x => x.room_id == inventory_item.room_id && x.domain_id == 1).First();
            Assert.AreEqual(2, inventory_item.budget_qty);
            Assert.AreEqual(800, inventory_item.unit_budget);
            Assert.AreEqual("OFCI", inventory_item.resp.Trim());
            Assert.AreEqual("Phase1", phase.description);
            Assert.AreEqual("Department1", department.description);
            Assert.AreEqual("Room1", room.drawing_room_name);
            Assert.AreEqual("1234", room.drawing_room_number);
            Assert.AreEqual("Y68", inventory_item.clin);
            Assert.AreEqual(2*800, inventory_item.total_budget_amt);
            Assert.AreEqual(3, inventory_item.unit_escalation);
            Assert.AreEqual(4, inventory_item.unit_freight_markup);
            Assert.AreEqual(5, inventory_item.unit_freight_net);
            Assert.AreEqual(6, inventory_item.unit_markup);
            Assert.AreEqual(7, inventory_item.unit_install_markup);
            Assert.AreEqual(8, inventory_item.unit_install_net);
            Assert.AreEqual(9, inventory_item.unit_tax);
            Assert.AreEqual("MyCost", inventory_item.cost_center.code);
            Assert.AreEqual("Tag1, Tag2", inventory_item.tag);
            Assert.AreEqual("Counter", inventory_item.placement);
            Assert.AreEqual("This is a test comment", inventory_item.comment);
            Assert.AreEqual("This is my functional area", room.functional_area);
            Assert.AreEqual("10", inventory_item.asset.height);
            Assert.AreEqual("20", inventory_item.asset.width);
            Assert.AreEqual("30", inventory_item.asset.depth);
            Assert.AreEqual("40", inventory_item.asset.mounting_height);
            item = new ImportItem
            {
                Id = inventory_item.inventory_id,
                Status = ImportItemStatus.Changed,
                Description = "Test1, Test2",
                PlannedQty = 3,
                Resp = "CFCI",
                Clin = "X67",
                Phase = "Phase2",
                Department = "Department2",
                RoomName = "Room2",
                RoomNumber = "5678",
                UnitBudget = 900,
                UnitEscalation = 10,
                UnitFreightMarkup = 11,
                UnitFreightNet = 12,
                UnitMarkup = 13,
                UnitInstallMarkup = 14,
                UnitInstallNet = 15,
                UnitTax = 16,
                BiomedRequired = false,
                CostCenter = "MyCost-2",
                Tags = "Tag1, Tag2-2",
                Placement = "Under-Counter",
                Comment = "This is a test comment-2",
                FunctionalArea = "This is my functional area-2",
                Height = "11",
                Width = "21",
                Depth = "31",
                MountingHeight = "41"
            };
            Import(item, 1);
            // Disposes and creates a new context to avoid issues with caching
            _db.Dispose();
            _db = new audaxwareEntities();
            roomInventory = _db.project_room_inventory.Include("cost_center").Include("asset").Where(x => x.project_id == _projectIdAudaxWare).ToList();
            Assert.AreEqual(1, roomInventory.Count(), "We should have one and only one asset added");
            inventory_item = roomInventory.First();
            phase = _db.project_phase.Where(x => x.phase_id == inventory_item.phase_id && x.domain_id == 1).First();
            department = _db.project_department.Where(x => x.department_id == inventory_item.department_id && x.domain_id == 1).First();
            room = _db.project_room.Where(x => x.room_id == inventory_item.room_id && x.domain_id == 1).First();
            Assert.AreEqual("CFCI", inventory_item.resp.Trim());
            Assert.AreEqual("Phase2", phase.description);
            Assert.AreEqual("Department2", department.description);
            Assert.AreEqual("Room2", room.drawing_room_name);
            Assert.AreEqual("5678", room.drawing_room_number);
            Assert.AreEqual("X67", inventory_item.clin);
            Assert.AreEqual(3, inventory_item.budget_qty);
            Assert.AreEqual(900, inventory_item.unit_budget);
            Assert.AreEqual(3*900, inventory_item.total_budget_amt);
            Assert.AreEqual(10, inventory_item.unit_escalation);
            Assert.AreEqual(11, inventory_item.unit_freight_markup);
            Assert.AreEqual(12, inventory_item.unit_freight_net);
            Assert.AreEqual(13, inventory_item.unit_markup);
            Assert.AreEqual(14, inventory_item.unit_install_markup);
            Assert.AreEqual(15, inventory_item.unit_install_net);
            Assert.AreEqual(16, inventory_item.unit_tax);
            Assert.AreEqual("MyCost-2", inventory_item.cost_center.code);
            Assert.AreEqual("Tag1, Tag2-2", inventory_item.tag);
            Assert.AreEqual("Under-Counter", inventory_item.placement);
            Assert.AreEqual("This is a test comment-2", inventory_item.comment);
            Assert.AreEqual("This is my functional area-2", room.functional_area);
            Assert.AreEqual("10", inventory_item.asset.height);
            Assert.AreEqual("20", inventory_item.asset.width);
            Assert.AreEqual("30", inventory_item.asset.depth);
            Assert.AreEqual("40", inventory_item.asset.mounting_height);
            ClearAssets();
        }

        [TestMethod]
        public void ImportDescriptionMismatch()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, B",
                PlannedQty = 2,
                Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                Manufacturer = "Test",
                ModelName = "MyModel",
                UnitBudget = 800,
                UnitEscalation = 3,
                UnitFreightMarkup = 4,
                UnitFreightNet = 5,
                UnitMarkup = 6,
                UnitInstallMarkup = 7,
                UnitInstallNet = 8,
                UnitTax = 9
            };
            Import(item, 1);
            item.Status = ImportItemStatus.Error;
            item.JSNNomeclature = "AB";
            item.Id = null;
            using (var repo = new AssetsInventoryImporterRepository())
            {
                repo.Init(1, _projectIdAudaxWare);
                try
                {
                    repo.ValidateItem(item);
                }
                catch(Exception ex)
                {
                    Assert.Fail($"Error to validate item: ${ex.Message}");
                }
                Assert.AreEqual(ImportItemStatus.New, item.Status, "Item with different description should be valid");
            }
            ClearAssets();
        }

        [TestMethod]
        public void ImportCategoryMismatch()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "Example of category too long test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test,subcategory",
                PlannedQty = 2,
                //Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                Manufacturer = "Test",
                ModelName = "MyModel",
                UnitBudget = 800,
            };
            
            using (var repo = new AssetsInventoryImporterRepository())
            {
                repo.Init(1, _projectIdAudaxWare);
                repo.ValidateItem(item);
                Assert.AreEqual(ImportItemStatus.Error, item.Status, "Category length is too long");
            }
            ClearAssets();
        }

        [TestMethod]
        public void ImportSubCategoryMismatch()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, Example of subcategory too long test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test test",
                PlannedQty = 2,
                //Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                Manufacturer = "Test",
                ModelName = "MyModel",
                UnitBudget = 800,
            };

            using (var repo = new AssetsInventoryImporterRepository())
            {
                repo.Init(1, _projectIdAudaxWare);
                repo.ValidateItem(item);
                Assert.AreEqual(ImportItemStatus.Error, item.Status, "SubCategory length is too long");
            }
            ClearAssets();
        }

        [TestMethod]
        public void ImportSameAssetTwice()
        {
            ClearAssets();
            var result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.SameAssetTwice.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            Assert.AreEqual(2, result.TotalNewCatalog, "Error on the number of items imported");
            Import("xPlannerAPI.Tests.Repositories.TestData.SameAssetTwice.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            // We have a new asset, but it is the same in both lines, so only one should be added to the catalog
            var importedAssets = _db.assets.Where(x => x.added_by == _importUser);
            Assert.AreEqual(1, importedAssets.Count(), "Number of assets returned mismatch");
            ClearAssets();
        }

        [TestMethod]
        public void ImportMillCreekVanceLabTwice()
        {
            ClearAssets();
            var result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.VanceLab.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            Assert.AreEqual(129, result.TotalNewCatalog, "Error on the number of items imported");
            Import("xPlannerAPI.Tests.Repositories.TestData.VanceLab.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            result = GetAnalysisResult(ImportAnalysisResultStatus.Ok, "xPlannerAPI.Tests.Repositories.TestData.VanceLab.xlsx", Interfaces.ImportColumnsFormat.MillCreek).FirstOrDefault();
            // Now they are all new, as the import has created the assets in the catalog
            Assert.AreEqual(129, result.TotalNew, "Error on the number of items imported");
            Assert.AreEqual(0, result.TotalNewCatalog, "Number of new items must be zero");
            ClearAssets();
        }

        [TestMethod]
        public void ImportJSNSuffix()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.SimpleJSNSuffix.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var importedAssets = _db.assets.Where(x => x.added_by == _importUser);
            Assert.AreEqual(1, importedAssets.Count(), "Number of assets returned mismatch");
            var asset = importedAssets.First();
            Assert.AreEqual("1", asset.jsn_suffix, "JSN Suffix mismatch");
            var newJSNs = _db.jsns.Where(x => x.added_by == _importUser);
            Assert.AreEqual(0, newJSNs.Count(), "Number of assets returned mismatch");
            ClearAssets();
        }


        [TestMethod]
        public void ImportJSNMismatch()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.SimpleJSNMismatch1.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var importedAssets = _db.assets.Where(x => x.added_by == _importUser);
            Assert.AreEqual(1, importedAssets.Count(), "Number of assets returned mismatch");
            var importedAsset = importedAssets.FirstOrDefault();
            Assert.AreEqual(true, importedAsset.jsn_utility1_ow, "Invalid overwrite value for utility 1");
            Assert.AreEqual("A", importedAsset.jsn_utility1, "Invalid utility value for utility 1");
            Assert.AreEqual(true, importedAsset.jsn_utility2_ow, "Invalid overwrite value for utility 2");
            Assert.AreEqual("B", importedAsset.jsn_utility2, "Invalid utility value for utility 2");
            Assert.AreEqual(false, importedAsset.jsn_utility3_ow, "Invalid overwrite value for utility 3");
            Assert.IsTrue(string.IsNullOrEmpty(importedAsset.jsn_utility3), "Invalid utility value for utility 3");
            Assert.AreEqual(false, importedAsset.jsn_utility4_ow, "Invalid overwrite value for utility 4");
            Assert.IsTrue(string.IsNullOrEmpty(importedAsset.jsn_utility4), "Invalid utility value for utility 4");
            Assert.AreEqual(false, importedAsset.jsn_utility5_ow, "Invalid overwrite value for utility 5");
            Assert.IsTrue(string.IsNullOrEmpty(importedAsset.jsn_utility5), "Invalid utility value for utility 5");
            Assert.AreEqual(false, importedAsset.jsn_utility6_ow, "Invalid overwrite value for utility 6");
            Assert.IsTrue(string.IsNullOrEmpty(importedAsset.jsn_utility6), "Invalid utility value for utility 6");
            ClearAssets();
        }

        /// <summary>
        /// The fields with suffix override are fields that are avaialble in the catalog but that can be overwritten
        /// on the inventory level
        /// </summary>
        [TestMethod]
        public void ImportReplaceCatalogAsset()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                Code = "AID00001",
                PlannedQty = 1,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
            };
            Import(item, 24);
            item.Code = "AID00002";
            Import(item, 24);
            var roomInventory = _db.project_room_inventory.Include("asset").Include("asset.manufacturer").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "Only one asset was imported");
            var roomInventoryItem = roomInventory.FirstOrDefault();
            Assert.AreEqual("AID00002", roomInventoryItem.asset.asset_code, "Only one asset was imported");
            ClearAssets();
        }


        /// <summary>
        /// The fields with suffix override are fields that are avaialble in the catalog but that can be overwritten
        /// on the inventory level
        /// </summary>
        [TestMethod]
        public void ImportOverrides()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, B",                // Override for the description
                ModelNumber = "JKL888",  
                ModelName = "This is my model name", 
                Manufacturer = "Test manufacturer",
                PlannedQty = 2,
                Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                UnitBudget = 800,
                UnitEscalation = 3,
                UnitFreightMarkup = 4,
                UnitFreightNet = 5,
                UnitMarkup = 6,
                UnitInstallMarkup = 7,
                UnitInstallNet = 8,
                UnitTax = 9
            };
            Import(item, 24);
            var roomInventory = _db.project_room_inventory.Include("asset").Include("asset.manufacturer").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "Only one asset was imported");
            var roomInventoryItem = roomInventory.FirstOrDefault();
            Assert.AreEqual(item.JSNNomeclature, roomInventoryItem.asset_description, "Description was not imported properly");
            Assert.AreEqual(item.ModelNumber, roomInventoryItem.asset.serial_number, "Model number was not imported properly");
            Assert.AreEqual(item.ModelName, roomInventoryItem.asset.serial_name, "Model number was not imported properly");
            Assert.AreEqual(item.Manufacturer, roomInventoryItem.asset.manufacturer.manufacturer_description, "Model number was not imported properly");
            var assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, assetInventory.Count(), "Only one asset was imported");
            var assetInventoryItem = assetInventory.FirstOrDefault();
            Assert.AreEqual(item.JSNNomeclature, assetInventoryItem.asset_description, "Description was not imported properly");
            //Assert.AreEqual(item.ModelNumber, assetInventoryItem.serial_number, "Model number was not imported properly");
            //Assert.AreEqual(item.ModelName, assetInventoryItem.serial_name, "Model number was not imported properly");
            //Assert.AreEqual(item.Manufacturer, assetInventoryItem.manufacturer_description, "Model number was not imported properly");
            _db.project_room_inventory.Remove(roomInventoryItem);
            _db.SaveChanges();
            // The description is set internally by the import call, so I reset it here
            item.Id = null;
            item.Description = null;
            item.JSNNomeclature = "2A; B";                // Override for the description
            Import(item, 24);
            roomInventory = _db.project_room_inventory.Include("asset").Include("asset.manufacturer").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "Only one asset was imported");
            roomInventoryItem = roomInventory.FirstOrDefault();
            Assert.AreEqual("2A, B", roomInventoryItem.asset_description, "Description was not imported properly");
            Assert.AreEqual(item.ModelNumber, roomInventoryItem.asset.serial_number, "Model number was not imported properly");
            Assert.AreEqual(item.ModelName, roomInventoryItem.asset.serial_name, "Model number was not imported properly");
            Assert.AreEqual(item.Manufacturer, roomInventoryItem.asset.manufacturer.manufacturer_description, "Model number was not imported properly");
            assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek).AsNoTracking();
            Assert.AreEqual(1, assetInventory.Count(), "Only one asset was imported");
            assetInventoryItem = assetInventory.FirstOrDefault();
            Assert.AreEqual("2A, B", assetInventoryItem.asset_description, "Description was not imported properly");
            //Assert.AreEqual(item.ModelNumber, assetInventoryItem.serial_number, "Model number was not imported properly");
            //Assert.AreEqual(item.ModelName, assetInventoryItem.serial_name, "Model number was not imported properly");
            //Assert.AreEqual(item.Manufacturer, assetInventoryItem.manufacturer_description, "Model number was not imported properly");
            ClearAssets();
        }

        [TestMethod]
        public void ImportDifferentJSN_Utility()
        {
            ClearAssets();
            var item = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, B",                // Override for the description
                ModelNumber = "JKL888",                 // Override for the model number
                ModelName = "This is my model name",    // Override for the model name
                Manufacturer = "Test manufacturer",
                PlannedQty = 2,
                Clin = "Y68",
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                UnitBudget = 800,
                UnitEscalation = 3,
                UnitFreightMarkup = 4,
                UnitFreightNet = 5,
                UnitMarkup = 6,
                UnitInstallMarkup = 7,
                UnitInstallNet = 8,
                UnitTax = 9,
                U1 = "A"
            };
            Import(item, 24);
            var roomInventory = _db.project_room_inventory.Include("asset").Include("asset.manufacturer").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "Only one asset was imported");
            var roomInventoryItem = roomInventory.FirstOrDefault();
            Assert.AreEqual(item.JSNNomeclature, roomInventoryItem.asset_description, "Description was not imported properly");
            Assert.AreEqual(item.ModelNumber, roomInventoryItem.asset.serial_number, "Model number was not imported properly");
            Assert.AreEqual(item.ModelName, roomInventoryItem.asset.serial_name, "Model number was not imported properly");
            Assert.AreEqual(item.Manufacturer, roomInventoryItem.asset.manufacturer.manufacturer_description, "Model number was not imported properly");
            var assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, assetInventory.Count(), "Only one asset was imported");
            var assetInventoryItem = assetInventory.FirstOrDefault();
            Assert.AreEqual(item.JSNNomeclature, assetInventoryItem.asset_description, "Description was not imported properly");
            //Assert.AreEqual(item.ModelNumber, assetInventoryItem.serial_number, "Model number was not imported properly");
            //Assert.AreEqual(item.ModelName, assetInventoryItem.serial_name, "Model number was not imported properly");
            //Assert.AreEqual(item.Manufacturer, assetInventoryItem.manufacturer_description, "Model number was not imported properly");
            _db.project_room_inventory.Remove(roomInventoryItem);
            _db.SaveChanges();
            // The description is set internally by the import call, so I reset it here
            item.Id = null;
            item.Description = null;
            item.JSNNomeclature = "2A; B";                // Override for the description
            item.ModelNumber = "2JKL888";                 // Override for the model number
            item.ModelName = "2This is my model name";    // Override for the model name
            item.Manufacturer = "2Test manufacturer";
            Import(item, 24);
            roomInventory = _db.project_room_inventory.Include("asset").Include("asset.manufacturer").Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, roomInventory.Count(), "Only one asset was imported");
            roomInventoryItem = roomInventory.FirstOrDefault();
            Assert.AreEqual("2A, B", roomInventoryItem.asset_description, "Description was not imported properly");
            Assert.AreEqual(item.ModelNumber, roomInventoryItem.asset.serial_number, "Model number was not imported properly");
            Assert.AreEqual(item.ModelName, roomInventoryItem.asset.serial_name, "Model number was not imported properly");
            Assert.AreEqual(item.Manufacturer, roomInventoryItem.asset.manufacturer.manufacturer_description, "Model number was not imported properly");
            assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, assetInventory.Count(), "Only one asset was imported");
            assetInventoryItem = assetInventory.FirstOrDefault();
            Assert.AreEqual("2A, B", assetInventoryItem.asset_description, "Description was not imported properly");
            //Assert.AreEqual(item.ModelNumber, assetInventoryItem.serial_number, "Model number was not imported properly");
            //Assert.AreEqual(item.ModelName, assetInventoryItem.serial_name, "Model number was not imported properly");
            //Assert.AreEqual(item.Manufacturer, assetInventoryItem.manufacturer_description, "Model number was not imported properly");
            ClearAssets();
        }

        [TestMethod]
        public void ImportAssetDuplicate()
        {
            ClearAssets();
            Import("xPlannerAPI.Tests.Repositories.TestData.AssetDuplicate.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            var importedAssets = _db.assets.Where(x => x.added_by == _importUser);
            // Assert.AreEqual(1, importedAssets.Count(), "Number of assets returned mismatch");
            var count = importedAssets.Count();
            Import("xPlannerAPI.Tests.Repositories.TestData.AssetDuplicate.xlsx", Interfaces.ImportColumnsFormat.MillCreek);
            importedAssets = _db.assets.Where(x => x.added_by == _importUser);
            Assert.AreEqual(count, importedAssets.Count(), "Number of assets returned mismatch");
            ClearAssets();
        }

        [TestMethod]
        public void ImportSameAssetDifferentSettings()
        {
            ClearAssets();
            var items = new ImportItem[2];
            items[0] = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, B",                // Override for the description
                ModelNumber = "JKL888",                 // Override for the model number
                ModelName = "This is my model name",    // Override for the model name
                Manufacturer = "Test manufacturer",
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                U1 = "A",
                U2 = "B",
                U3 = "C",
                U4 = "D",
                U5 = "E",
                U6 = "F",
                Clin = "Y68",
                Height = "5",
                Width = "6",
                Depth = "7",
                MountingHeight = "8",
                Class = "AW"
            };
            items[1] = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "A, B",                // Override for the description
                ModelNumber = "JKL888",                 // Override for the model number
                ModelName = "This is my model name",    // Override for the model name
                Manufacturer = "Test manufacturer",
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                U1 = "B",
                U2 = "C",
                U3 = "D",
                U4 = "E",
                U5 = "F",
                U6 = "G",
                Clin = "Y69",
                Height = "15",
                Width = "16",
                Depth = "17",
                MountingHeight = "18",
                Class = "N/A"
            };
            Import(items, 24);
            var importedAssets = _db.assets.Where(x => x.added_by == _importUser).AsNoTracking().OrderBy(x => x.jsn_utility1);
            Assert.AreEqual(1, importedAssets.Count(), "Invalid number of assets created in the catalog");
            var assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek).AsNoTracking();
            Assert.AreEqual(2, assetInventory.Count(), "Inventoy error, we must have two assets");
            var i = assetInventory.ToArray();
            Assert.AreEqual("A", i[0].jsn_utility1, "Utility 1 is invalid");
            Assert.AreEqual("B", i[0].jsn_utility2, "Utility 2 is invalid");
            Assert.AreEqual("C", i[0].jsn_utility3, "Utility 3 is invalid");
            Assert.AreEqual("D", i[0].jsn_utility4, "Utility 4 is invalid");
            Assert.AreEqual("E", i[0].jsn_utility5, "Utility 5 is invalid");
            Assert.AreEqual("F", i[0].jsn_utility6, "Utility 6 is invalid");
            Assert.AreEqual("Y68", i[0].clin, "Clin is invalid");
            Assert.AreEqual("5", i[0].height, "height is invalid");
            Assert.AreEqual("6", i[0].width, "width is invalid");
            Assert.AreEqual("7", i[0].depth, "depth is invalid");
            Assert.AreEqual("8", i[0].mounting_height, "mounting height is invalid");
            Assert.AreEqual(1, i[0].@class, "class is invalid");
            Assert.AreEqual("B", i[1].jsn_utility1, "Utility 1 is invalid");
            Assert.AreEqual("C", i[1].jsn_utility2, "Utility 2 is invalid");
            Assert.AreEqual("D", i[1].jsn_utility3, "Utility 3 is invalid");
            Assert.AreEqual("E", i[1].jsn_utility4, "Utility 4 is invalid");
            Assert.AreEqual("F", i[1].jsn_utility5, "Utility 5 is invalid");
            Assert.AreEqual("G", i[1].jsn_utility6, "Utility 6 is invalid");
            Assert.AreEqual("Y69", i[1].clin, "Clin is invalid");
            Assert.AreEqual("15", i[1].height, "height is invalid");
            Assert.AreEqual("16", i[1].width, "width is invalid");
            Assert.AreEqual("17", i[1].depth, "depth is invalid");
            Assert.AreEqual("18", i[1].mounting_height, "mounting height is invalid");
            Assert.AreEqual(0, i[1].@class, "class is invalid");
            items[0].U1 = "C";
            items[1].U1 = "D";
            // Update values on existing assets
            Import(items, 24);
            importedAssets = _db.assets.Where(x => x.added_by == _importUser).AsNoTracking().OrderBy(x => x.jsn_utility1);
            Assert.AreEqual(1, importedAssets.Count(), "Invalid number of assets created in the catalog");
            assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek).AsNoTracking();
            Assert.AreEqual(2, assetInventory.Count(), "Inventoy error, we must have two assets");
            i = assetInventory.ToArray();
            Assert.AreEqual("C", i[0].jsn_utility1, "Utility 1 is invalid");
            Assert.AreEqual("B", i[0].jsn_utility2, "Utility 2 is invalid");
            Assert.AreEqual("C", i[0].jsn_utility3, "Utility 3 is invalid");
            Assert.AreEqual("D", i[0].jsn_utility4, "Utility 4 is invalid");
            Assert.AreEqual("E", i[0].jsn_utility5, "Utility 5 is invalid");
            Assert.AreEqual("F", i[0].jsn_utility6, "Utility 6 is invalid");
            Assert.AreEqual("Y68", i[0].clin, "Clin is invalid");
            Assert.AreEqual("5", i[0].height, "height is invalid");
            Assert.AreEqual("6", i[0].width, "width is invalid");
            Assert.AreEqual("7", i[0].depth, "depth is invalid");
            Assert.AreEqual("8", i[0].mounting_height, "mounting height is invalid");
            Assert.AreEqual(1, i[0].@class, "class is invalid");
            Assert.AreEqual("D", i[1].jsn_utility1, "Utility 1 is invalid");
            Assert.AreEqual("C", i[1].jsn_utility2, "Utility 2 is invalid");
            Assert.AreEqual("D", i[1].jsn_utility3, "Utility 3 is invalid");
            Assert.AreEqual("E", i[1].jsn_utility4, "Utility 4 is invalid");
            Assert.AreEqual("F", i[1].jsn_utility5, "Utility 5 is invalid");
            Assert.AreEqual("G", i[1].jsn_utility6, "Utility 6 is invalid");
            Assert.AreEqual("Y69", i[1].clin, "Clin is invalid");
            Assert.AreEqual("15", i[1].height, "height is invalid");
            Assert.AreEqual("16", i[1].width, "width is invalid");
            Assert.AreEqual("17", i[1].depth, "depth is invalid");
            Assert.AreEqual("18", i[1].mounting_height, "mounting height is invalid");
            Assert.AreEqual(0, i[1].@class, "class is invalid");

            ClearAssets();
        }

        [TestMethod]
        public void ImportSetJSN()
        {
            ClearAssets();
            var items = new ImportItem[1];
            items[0] = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                Description = "A, B",                // Override for the description
                ModelNumber = "JKL888",                 // Override for the model number
                ModelName = "This is my model name",    // Override for the model name
                Manufacturer = "Test manufacturer",
                PlannedQty = 1,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
            };
            Import(items, 24);
            var assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, assetInventory.Count(), "Inventory error, we must have two assets");
            var inventoryItem = assetInventory.FirstOrDefault();
            var phaseId = inventoryItem.phase_id;
            var departmentId = inventoryItem.department_id;
            var roomId = inventoryItem.room_id;
            items[0].JSN = "A9999";
            items[0].Phase = null;
            items[0].Department = null;
            items[0].RoomName = null;
            items[0].RoomNumber = null;
            Import(items, 24);
            assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek);
            Assert.AreEqual(1, assetInventory.Count(), "Inventory error, we must have two assets");
            inventoryItem = assetInventory.FirstOrDefault();
            Assert.AreEqual(phaseId, inventoryItem.phase_id);
            Assert.AreEqual(departmentId, inventoryItem.department_id);
            Assert.AreEqual(roomId, inventoryItem.room_id);
            ClearAssets();
        }
        [TestMethod]
        public void ReimportWithOutManufacturer()
        {
            ClearAssets();
            var items = new ImportItem[2];
            items[0] = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9998",
                JSNNomeclature = "A, B",                
                ModelNumber = "JKL888",                 
                ModelName = "This is my model name 1",
                Manufacturer = "Test manufacturer 1",
                PlannedQty = 2,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                U1 = "A",
                U2 = "B",
                U3 = "C",
                U4 = "D",
                U5 = "E",
                U6 = "F",
                Clin = "Y68",
                Height = "5",
                Width = "6",
                Depth = "7",
                MountingHeight = "8",
                Class = "AW"
            };
            items[1] = new ImportItem
            {
                Status = ImportItemStatus.NewCatalog,
                JSN = "A9999",
                JSNNomeclature = "2A, 2B",                
                ModelNumber = "JKL889",                 
                ModelName = "This is my model name 2",    
                Manufacturer = "Test manufacturer 2",
                PlannedQty = 1,
                Resp = "OFCI",
                Phase = "Phase1",
                Department = "Department1",
                RoomName = "Room1",
                RoomNumber = "1234",
                U1 = "B",
                U2 = "C",
                U3 = "D",
                U4 = "E",
                U5 = "F",
                U6 = "G",
                Clin = "Y69",
                Height = "15",
                Width = "16",
                Depth = "17",
                MountingHeight = "18",
                Class = "N/A"
            };
            Import(items, 24);
            var assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek).ToArray();
            Assert.AreEqual(2, assetInventory.Count(), "Inventory error, we must have two assets");
            for (int i = 0; i < items.Count(); i++)
            {
                items[i].Status = ImportItemStatus.Changed;
                items[i].Manufacturer = null;
                items[i].ModelName = null;
                items[i].ModelNumber = null;
            };

            Import(items, 24);
            assetInventory = _db.asset_inventory.Where(x => x.project_id == _projectIdMillCreek).ToArray();
            Assert.AreEqual("Test manufacturer 1", assetInventory[0].manufacturer_description, "Manufacturer was not kept.");
            Assert.AreEqual("This is my model name 1", assetInventory[0].serial_name, "Model name was not kept.");
            Assert.AreEqual("JKL888", assetInventory[0].serial_number, "Model number was not kept.");
            Assert.AreEqual("Test manufacturer 2", assetInventory[1].manufacturer_description, "Manufacturer was not kept.");
            Assert.AreEqual("This is my model name 2", assetInventory[1].serial_name, "Model name was not kept.");
            Assert.AreEqual("JKL889", assetInventory[1].serial_number, "Model number was not kept.");
            ClearAssets();


        }

    }
}
