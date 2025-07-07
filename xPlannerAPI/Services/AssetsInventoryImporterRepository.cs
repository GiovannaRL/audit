using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Data.Entity.Core.Metadata.Edm;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;

namespace xPlannerAPI.Services
{
    public class AssetsInventoryImporterRepository : IAssetsInventoryImporterRepository, IDisposable
    {
        class RoomEntry
        {
            int phase_id;
            int department_id;
            int room_id;

            public RoomEntry( int phase_id, int department_id, int room_id)
            {
                this.phase_id = phase_id;
                this.department_id = department_id;
                this.room_id = room_id;
            }

            public void SetInventory(project_room_inventory inventory)
            {
                inventory.phase_id = phase_id;
                inventory.department_id = department_id;
                inventory.room_id = room_id;
            }

        }

        static Dictionary<string, string> _audaxWareFormat;
        static Dictionary<string, string> _audaxWareFormatReversed;
        static Dictionary<string, string> _millCreekFormat;
        static Dictionary<string, string> _millCreekFormatReversed;
        static Dictionary<string, int> _stringLimits = new Dictionary<string, int>();
        static Type _itemType = typeof(ImportItem);
        public const string ColumnNameId = "Id";
        public const string ColumnNameCode = "Code";
        public const string ColumnNameManufacturer = "Manufacturer";
        public const string ColumnNameDescription = "Description";
        public const string ColumnNameModelNumber = "ModelNumber";
        public const string ColumnNameModelName = "ModelName";
        public const string ColumnNameJSN = "JSN";
        public const string ColumnNameJSNNomeclature = "JSNNomeclature";
        public const string ColumnNamePlannedQty = "PlannedQty";
        public const string ColumnNameClass = "Class";
        public const string ColumnNameClin = "Clin";
        public const string ColumnNameUnitBudget = "UnitBudget";
        public const string ColumnNamePhase = "Phase"; // Mandatory
        public const string ColumnNameDepartment = "Department"; // Mandatory
        public const string ColumnNameRoomName = "RoomName"; // Mandatory
        public const string ColumnNameRoomNumber = "RoomNumber";
        public const string ColumnNameResp = "Resp"; // Mandatory
        public const string ColumnNameU1 = "U1";
        public const string ColumnNameU2 = "U2";
        public const string ColumnNameU3 = "U3";
        public const string ColumnNameU4 = "U4";
        public const string ColumnNameU5 = "U5";
        public const string ColumnNameU6 = "U6";
        public const string ColumnUnitOfMeasure = "UnitOfMeasure";
        public const string ColumnUnitMarkup  = "UnitMarkup";
        public const string ColumnUnitEscalation  = "UnitEscalation";
        public const string ColumnUnitTax  = "UnitTax";
        public const string ColumnUnitInstallNet  = "UnitInstallNet";
        public const string ColumnUnitInstallMarkup  = "UnitInstallMarkup";
        public const string ColumnUnitFreightNet  = "UnitFreightNet";
        public const string ColumnUnitFreightMarkup  = "UnitFreightMarkup";
        public const string ColumnECN  = "ECN";
        public const string ColumnCostCenter = "CostCenter";
        public const string ColumnCostCenterDescription = "CostCenterDescription";
        public const string Tags  = "Tags";
        public const string ColumnPlacement  = "Placement";
        public const string ColumnBiomedRequired  = "BiomedRequired";
        public const string ColumnComment  = "Comment";
        public const string ColumnFunctionalArea  = "FunctionalArea";
        public const string ColumnBlueprint = "Blueprint";
        public const string ColumnStaff = "Staff";
        public const string ColumnHeight  = "Height";
        public const string ColumnWidth  = "Width";
        public const string ColumnDepth  = "Depth";
        public const string ColumnMountingHeight  = "MountingHeight";
        public const string ColumnRoomArea = "RoomArea";
        public const string ColumnRoomCode = "RoomCode";
        public const string ColumnTemporaryLocation = "TemporaryLocation";
        public const string ColumnFinalDisposition = "FinalDisposition";
        public const string ColumnCADID = "CADID";
        public const string ColumnNetworkRequired = "NetworkRequired";

        public Dictionary<string, int> _networks = new Dictionary<string, int>() {
            {"", 0 },
            {"--", 0 },
            {"no", 0 },
            {"yes", 1},
            {"optional", 2}
        };

        public Dictionary<string, string> _placements = new Dictionary<string, string>() {
            {"None", "None"},
            {"Boom", "Boom"},
            {"Ceiling", "Ceiling"},
            {"Counter", "Counter"},
            {"Freestanding", "Freestanding"},
            {"Floor", "Floor"},
            {"Mobile", "Mobile"},
            {"On Cart", "On Cart"},
            {"Other Equipment", "Other Equipment"},
            {"Portable", "Portable"},
            {"Recessed", "Recessed"},
            {"Under-Counter", "Under-Counter"},
            {"Wall", "Wall"},
        };
        const int _maxSupportedRows = 15000;
        Dictionary<string, int> _columnNameToIndex = new Dictionary<string, int>();
        Dictionary<string, string> _selectedColumnFormat;
        Dictionary<string, string> _selectedColumnFormatReversed;
        Dictionary<string, assets_subcategory> _subcategories = new Dictionary<string, assets_subcategory>();
        Dictionary<int, project_room_inventory> _projectInventory = new Dictionary<int, project_room_inventory>();
        Dictionary<string, jsn> _existingJSNs = new Dictionary<string, jsn>();
        Dictionary<string, List<asset_import_search_Result>> _existingCatalogAssets = new Dictionary<string, List<asset_import_search_Result>>();
        Dictionary<string, assets_measurement> _existingUnits = new Dictionary<string, assets_measurement>();
        Dictionary<string, RoomEntry> _rooms = new Dictionary<string, RoomEntry>();
        Dictionary<string, int> _phases = new Dictionary<string, int>();
        Dictionary<string, int> _departments = new Dictionary<string, int>();
        Dictionary<string, manufacturer> _manufacturers = new Dictionary<string, manufacturer>();
        List<get_max_asset_codes_Result> _lastAssetsCodes = new List<get_max_asset_codes_Result>();
        Dictionary<string, cost_center> _projectCostCenters = new Dictionary<string, cost_center>();
        List<string> _projectTemporaryLocations = new List<string>();
        List<string> _projectFinalDispositions = new List<string>();
        ExcelWorksheet _worksheet;
        ExcelPackage _xlPackage;
        List<ImportAnalysisResult> _analysisResultTotal = new List<ImportAnalysisResult>();
        ImportAnalysisResult _analysisResult = new ImportAnalysisResult();
        string[] _requiredColumns = new string[]{ColumnNameRoomName, ColumnNameDepartment, ColumnNameResp};
        string[] _assetHintColumns = new string[] {ColumnNameCode, ColumnNameManufacturer,
            ColumnNameDescription, ColumnNameModelNumber, ColumnNameJSN, ColumnNameJSNNomeclature};
        string[] _requiredColumnsTarget;
        string[] _assetHintColumnsTargetFormat;
        int _fileRows = 0;
        audaxwareEntities _db;
        bool _showAudaxwareInfo;
        short _domainId;
        int _projectId;
        string _user;
        private bool _disposed = false;
        List<responsability> _responsibilities;
        static Dictionary<string, int> _classes = new Dictionary<string, int>() {
            {"N/A", 0},
            {"AW", 1},
            {"CC", 2},
            {"CM", 3},
            {"FF", 4},
            {"ME", 5},
            {"RP", 6},
            {"SW", 7}
        };

        static AssetsInventoryImporterRepository()
        {
            InitAudaxWareFormat();
            InitMillCreekFormat();
        }


        static void InitStringLimits(audaxwareEntities db)
        {
            lock (_stringLimits)
            {
                // We have already initialized
                if (_stringLimits.Count > 0)
                {
                    return;
                }

                var metadata = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext.MetadataWorkspace;
                var phaseMetadata = metadata.GetItem<EntityType>("AudaxWare.project_phase", DataSpace.CSpace);
                _stringLimits.Add("Phase", (int)phaseMetadata.Properties["description"].TypeUsage.Facets["MaxLength"].Value);

                var departmentMetadata = metadata.GetItem<EntityType>("AudaxWare.project_department", DataSpace.CSpace);
                _stringLimits.Add("Department", (int)departmentMetadata.Properties["description"].TypeUsage.Facets["MaxLength"].Value);

                var roomMetadata = metadata.GetItem<EntityType>("AudaxWare.project_room", DataSpace.CSpace);
                _stringLimits.Add("RoomNumber", (int)roomMetadata.Properties["drawing_room_number"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("RoomName", (int)roomMetadata.Properties["drawing_room_name"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("FunctionalArea", (int)roomMetadata.Properties["functional_area"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Blueprint", (int)roomMetadata.Properties["blueprint"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Staff", (int)roomMetadata.Properties["staff"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("RoomCode", (int)roomMetadata.Properties["room_code"].TypeUsage.Facets["MaxLength"].Value);
               
                var jsnMetadata = metadata.GetItem<EntityType>("AudaxWare.jsn", DataSpace.CSpace);
                _stringLimits.Add("JSNNoSuffix", (int)jsnMetadata.Properties["jsn_code"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("JSNNomeclature", (int)jsnMetadata.Properties["nomenclature"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U1", (int)jsnMetadata.Properties["utility1"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U2", (int)jsnMetadata.Properties["utility2"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U3", (int)jsnMetadata.Properties["utility3"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U4", (int)jsnMetadata.Properties["utility4"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U5", (int)jsnMetadata.Properties["utility5"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("U6", (int)jsnMetadata.Properties["utility6"].TypeUsage.Facets["MaxLength"].Value);

                var manufacturersMetadata = metadata.GetItem<EntityType>("AudaxWare.manufacturer", DataSpace.CSpace);
                _stringLimits.Add("Manufacturer", (int)manufacturersMetadata.Properties["manufacturer_description"].TypeUsage.Facets["MaxLength"].Value);

                var categoriesMetadata = metadata.GetItem<EntityType>("AudaxWare.assets_category", DataSpace.CSpace);
                _stringLimits.Add("Category", (int)categoriesMetadata.Properties["description"].TypeUsage.Facets["MaxLength"].Value);

                var subcategoriesMetadata = metadata.GetItem<EntityType>("AudaxWare.assets_subcategory", DataSpace.CSpace);
                _stringLimits.Add("SubCategory", (int)subcategoriesMetadata.Properties["description"].TypeUsage.Facets["MaxLength"].Value);


                var costCenterMetadata = metadata.GetItem<EntityType>("AudaxWare.cost_center", DataSpace.CSpace);
                _stringLimits.Add("CostCenter", (int)costCenterMetadata.Properties["code"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("CostCenterDescription", (int)costCenterMetadata.Properties["description"].TypeUsage.Facets["MaxLength"].Value);

                var assetsMetadata = metadata.GetItem<EntityType>("AudaxWare.asset", DataSpace.CSpace);
                _stringLimits.Add("ModelName", (int)assetsMetadata.Properties["model_name"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("ModelNumber", (int)assetsMetadata.Properties["model_number"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("JSNSuffix", (int)assetsMetadata.Properties["jsn_suffix"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Description", (int)assetsMetadata.Properties["asset_description"].TypeUsage.Facets["MaxLength"].Value);


                var inventoryMetadata = metadata.GetItem<EntityType>("AudaxWare.project_room_inventory", DataSpace.CSpace);
                _stringLimits.Add("Clin", (int)inventoryMetadata.Properties["clin"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Resp", (int)inventoryMetadata.Properties["resp"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Comment", (int)inventoryMetadata.Properties["comment"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Tags", (int)inventoryMetadata.Properties["tag"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("Placement", (int)inventoryMetadata.Properties["placement"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("ECN", (int)inventoryMetadata.Properties["ECN"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("TemporaryLocation", (int)inventoryMetadata.Properties["temporary_location"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("FinalDisposition", (int)inventoryMetadata.Properties["final_disposition"].TypeUsage.Facets["MaxLength"].Value);
                _stringLimits.Add("CADID", (int)inventoryMetadata.Properties["cad_id"].TypeUsage.Facets["MaxLength"].Value);
            }
        }

        static void InitAudaxWareFormat()
        {
            _audaxWareFormat = new Dictionary<string, string>();
            _audaxWareFormat.Add("Code", ColumnNameCode);
            _audaxWareFormat.Add("Manufacturer", ColumnNameManufacturer);
            _audaxWareFormat.Add("Inventory ID", ColumnNameId);
            _audaxWareFormat.Add("Description", ColumnNameDescription);
            _audaxWareFormat.Add("Model No.", ColumnNameModelNumber);
            _audaxWareFormat.Add("Model Name", ColumnNameModelName);
            _audaxWareFormat.Add("JSN", ColumnNameJSN);
            _audaxWareFormat.Add("JSN Nomeclature", ColumnNameJSNNomeclature);
            _audaxWareFormat.Add("Planned qty", ColumnNamePlannedQty);
            _audaxWareFormat.Add("Class", ColumnNameClass);
            _audaxWareFormat.Add("CLIN", ColumnNameClin);
            _audaxWareFormat.Add("Unit Budget (Net)", ColumnNameUnitBudget);
            _audaxWareFormat.Add("Phase", ColumnNamePhase);
            _audaxWareFormat.Add("Department", ColumnNameDepartment);
            _audaxWareFormat.Add("Room No.", ColumnNameRoomNumber);
            _audaxWareFormat.Add("Room Name", ColumnNameRoomName);
            _audaxWareFormat.Add("Resp", ColumnNameResp);
            _audaxWareFormat.Add("U1", ColumnNameU1);
            _audaxWareFormat.Add("U2", ColumnNameU2);
            _audaxWareFormat.Add("U3", ColumnNameU3);
            _audaxWareFormat.Add("U4", ColumnNameU4);
            _audaxWareFormat.Add("U5", ColumnNameU5);
            _audaxWareFormat.Add("U6", ColumnNameU6);
            _audaxWareFormat.Add("UnitOfMeasure", ColumnUnitOfMeasure);
            _audaxWareFormat.Add("Unit Markup (%)", ColumnUnitMarkup);
            _audaxWareFormat.Add("Unit Escalation (%)", ColumnUnitEscalation);
            _audaxWareFormat.Add("Unit Tax (%)", ColumnUnitTax);
            _audaxWareFormat.Add("Unit Install (Net)", ColumnUnitInstallNet);
            _audaxWareFormat.Add("Unit Install Markup (%)", ColumnUnitInstallMarkup);
            _audaxWareFormat.Add("Unit Freight (Net)", ColumnUnitFreightNet);
            _audaxWareFormat.Add("Unit Freight Markup (%)", ColumnUnitFreightMarkup);
            _audaxWareFormat.Add("ECN", ColumnECN);
            _audaxWareFormat.Add("Cost Center", ColumnCostCenter);
            _audaxWareFormat.Add("Cost Center Description", ColumnCostCenterDescription);
            _audaxWareFormat.Add("TAG", Tags);
            _audaxWareFormat.Add("Placement", ColumnPlacement);
            _audaxWareFormat.Add("Biomed Required", ColumnBiomedRequired);
            _audaxWareFormat.Add("Comment", ColumnComment);
            _audaxWareFormat.Add("Functional Area", ColumnFunctionalArea);
            _audaxWareFormat.Add("Blueprint", ColumnBlueprint);
            _audaxWareFormat.Add("Staff", ColumnStaff);
            _audaxWareFormat.Add("Height(in)", ColumnHeight);
            _audaxWareFormat.Add("Width(in)", ColumnWidth);
            _audaxWareFormat.Add("Depth(in)", ColumnDepth);
            _audaxWareFormat.Add("Mounting Height(in)", ColumnMountingHeight);
            _audaxWareFormat.Add("Room Area", ColumnRoomArea);
            _audaxWareFormat.Add("Room Code", ColumnRoomCode);
            _audaxWareFormat.Add("Temporary Location", ColumnTemporaryLocation);
            _audaxWareFormat.Add("Final Disposition", ColumnFinalDisposition);
            _audaxWareFormat.Add("CAD ID", ColumnCADID);
            _audaxWareFormat.Add("Network Required", ColumnNetworkRequired);
            _audaxWareFormatReversed = new Dictionary<string, string>();
            foreach (var keyPair in _audaxWareFormat)
            {
                _audaxWareFormatReversed.Add(keyPair.Value, keyPair.Key);
            }
        }

        static void InitMillCreekFormat()
        {
            _millCreekFormat = new Dictionary<string, string>();
            _millCreekFormat.Add("Code", ColumnNameCode);
            _millCreekFormat.Add("Manufacturer", ColumnNameManufacturer);
            _millCreekFormat.Add("Inventory ID", ColumnNameId);
            _millCreekFormat.Add("Description", ColumnNameDescription);
            _millCreekFormat.Add("Model", ColumnNameModelNumber);
            _millCreekFormat.Add("Model Name", ColumnNameModelName);
            _millCreekFormat.Add("JSN", ColumnNameJSN);
            _millCreekFormat.Add("JSN Description", ColumnNameJSNNomeclature);
            _millCreekFormat.Add("Q", ColumnNamePlannedQty);
            _millCreekFormat.Add("Class", ColumnNameClass);
            _millCreekFormat.Add("CLIN", ColumnNameClin);
            _millCreekFormat.Add("IO UP", ColumnNameUnitBudget);
            _millCreekFormat.Add("IOPhase", ColumnNamePhase);
            _millCreekFormat.Add("Dept Desc", ColumnNameDepartment);
            _millCreekFormat.Add("Room No", ColumnNameRoomNumber);
            _millCreekFormat.Add("Room Desc", ColumnNameRoomName);
            _millCreekFormat.Add("LogCat", ColumnNameResp);
            _millCreekFormat.Add("Util1", ColumnNameU1);
            _millCreekFormat.Add("Util2", ColumnNameU2);
            _millCreekFormat.Add("Util3", ColumnNameU3);
            _millCreekFormat.Add("Util4", ColumnNameU4);
            _millCreekFormat.Add("Util5", ColumnNameU5);
            _millCreekFormat.Add("Util6", ColumnNameU6);
            _millCreekFormat.Add("Unit", ColumnUnitOfMeasure);
            _millCreekFormat.Add("RxR Mark Up", ColumnUnitMarkup);
            _millCreekFormat.Add("RxR Escalation", ColumnUnitEscalation);
            _millCreekFormat.Add("Tax", ColumnUnitTax);
            _millCreekFormat.Add("Install UP", ColumnUnitInstallNet);
            _millCreekFormat.Add("Install Mark UP", ColumnUnitInstallMarkup);
            _millCreekFormat.Add("Freight UP", ColumnUnitFreightNet);
            _millCreekFormat.Add("Freight Mark Up", ColumnUnitFreightMarkup);
            _millCreekFormat.Add("ECN", ColumnECN);
            _millCreekFormat.Add("Fund Code", ColumnCostCenter);
            _millCreekFormat.Add("Fund Description", ColumnCostCenterDescription);
            _millCreekFormat.Add("TAG", Tags);
            _millCreekFormat.Add("Install Method", ColumnPlacement);
            _millCreekFormat.Add("Biomed Check", ColumnBiomedRequired);
            _millCreekFormat.Add("Comments", ColumnComment);
            _millCreekFormat.Add("Functional Area", ColumnFunctionalArea);
            _millCreekFormat.Add("Blueprint", ColumnBlueprint);
            _millCreekFormat.Add("Staff", ColumnStaff);
            _millCreekFormat.Add("Height", ColumnHeight);
            _millCreekFormat.Add("Width", ColumnWidth);
            _millCreekFormat.Add("Depth", ColumnDepth);
            _millCreekFormat.Add("Mounting Height", ColumnMountingHeight);
            _millCreekFormat.Add("Room Area", ColumnRoomArea);
            _millCreekFormat.Add("Room Code", ColumnRoomCode);
            _millCreekFormat.Add("Temporary Location", ColumnTemporaryLocation);
            _millCreekFormat.Add("Final Disposition", ColumnFinalDisposition);
            _millCreekFormat.Add("CAD ID", ColumnCADID);
            _millCreekFormat.Add("Lan", ColumnNetworkRequired);
            _millCreekFormatReversed = new Dictionary<string, string>();
            foreach(var keyPair in _millCreekFormat)
            {
                _millCreekFormatReversed.Add(keyPair.Value, keyPair.Key);
            }
        }

        void SetColumnsFormat(ImportColumnsFormat format)
        {
            switch(format)
            {
                case ImportColumnsFormat.AudaxWare:
                    _selectedColumnFormat = _audaxWareFormat;
                    _selectedColumnFormatReversed = _audaxWareFormatReversed;
                    break;
                case ImportColumnsFormat.MillCreek:
                    _selectedColumnFormat = _millCreekFormat;
                    _selectedColumnFormatReversed = _millCreekFormatReversed;
                    break;
                default:
                    _selectedColumnFormat = null;
                    break;
            }
            _requiredColumnsTarget = new string[_requiredColumns.Length];
            for(int i=0; i < _requiredColumns.Length; ++i)
            {
                _requiredColumnsTarget[i] = _selectedColumnFormatReversed[_requiredColumns[i]];
            }
            _assetHintColumnsTargetFormat = new string[_assetHintColumns.Length];
            for(int i = 0; i < _assetHintColumns.Length; ++i)
            {
                _assetHintColumnsTargetFormat[i] = _selectedColumnFormatReversed[_assetHintColumns[i]];
            }
        }

        static public string GetColumnNameInFormat(ImportColumnsFormat format, string columnName)
        {
            Dictionary<string, string> formatReversed;
            switch(format)
            {
                case ImportColumnsFormat.AudaxWare:
                    formatReversed = _audaxWareFormatReversed;
                    break;
                case ImportColumnsFormat.MillCreek:
                    formatReversed = _millCreekFormatReversed;
                    break;
                default:
                    return columnName;
            }
            return formatReversed[columnName];
        }

        public AssetsInventoryImporterRepository()
        {
        }

        private bool CheckRequiredColumns()
        {
            var missingRequired = from required in _requiredColumns where !_columnNameToIndex.ContainsKey(required) select _selectedColumnFormatReversed[required];
            if (missingRequired.Count() > 0)
            {
                SetResult(ImportAnalysisResultStatus.RequiredColumnsMissing, string.Format(StringMessages.ImportMissingRequiredColumns,
                        String.Join(",", missingRequired)));
                return false;
            }

            if (!_assetHintColumns.Any(x => _analysisResult.UsedColumns.Contains(x)))
            {
                SetResult(ImportAnalysisResultStatus.AssetHintColumnMissing, string.Format(StringMessages.ImportAssetHintColumnMissing,
                        String.Join(",", _assetHintColumnsTargetFormat)));
                return false;
            }
            return true;
        }

        public bool EvaluateColumns() {
            int numColumns = _worksheet.Dimension.End.Column;
            _columnNameToIndex.Clear();
            var unusedColumns = new List<string>();
            var usedColumns = new List<string>();
            for(var col=0; col < numColumns; ++col)
            {
                if (_worksheet.Cells[1, col + 1].Value == null)
                {
                    continue;
                }
                string columnName = _worksheet.Cells[1, col + 1].Value.ToString();
                if (_selectedColumnFormat.ContainsKey(columnName))
                {
                    if (_columnNameToIndex.ContainsKey(_selectedColumnFormat[columnName]))
                    {
                        SetResult(ImportAnalysisResultStatus.Invalid, string.Format(StringMessages.ImportDuplicateColumn,
                                columnName));
                        return false;
                    }
                    _columnNameToIndex.Add(_selectedColumnFormat[columnName], col+1);
                    usedColumns.Add(columnName);
                }
                else
                {
                    unusedColumns.Add(columnName);
                }
            }
            _analysisResult.UnusedColumns = unusedColumns;
            _analysisResult.UsedColumns = usedColumns;

            return CheckRequiredColumns();
        } 

        void SetResult(ImportAnalysisResultStatus status, string errorMessage)
        {
            _analysisResult.Status = status;
            _analysisResult.ErrorMessage = errorMessage;
        }

        void LoadProjectInventory()
        {
            var projectAssets = _db.project_room_inventory.Where(x => (x.domain_id == _domainId) && (x.project_id == _projectId));
            foreach (var inventory in projectAssets)
            {
                _projectInventory.Add(inventory.inventory_id, inventory);
            }
        }

        void LoadProjectCostCenter()
        {
            var costCenters = _db.cost_center.Where(x => (x.domain_id == _domainId) && (x.project_id == _projectId));
            foreach (var costCenter in costCenters)
            {
                _projectCostCenters.Add(costCenter.code.ToUpper(), costCenter);
            }
        }

        void LoadProjectTemporaryLocations()
        {
            _projectTemporaryLocations = _db.temporary_location.Where(tl => tl.domain_id == _domainId && tl.project_id == _projectId).Select(tl => tl.name.ToUpper().Trim()).ToList();
        }

        void LoadProjectFinalDispositions()
        {
            _projectFinalDispositions = _db.final_disposition.Where(fd => fd.domain_id == _domainId && fd.project_id == _projectId).Select(fd => fd.name.ToUpper().Trim()).ToList();
        }

        bool CheckFile(string xlsFilePath)
        {        
            try
            {
                if (!File.Exists(xlsFilePath))
                {
                    SetResult(ImportAnalysisResultStatus.Invalid, StringMessages.ImportFileCouldNotBeRead);
                    Trace.TraceError($"Error trying to analyze file for import, file does not exist (File: {xlsFilePath})");
                    return false;
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                _xlPackage = new ExcelPackage(new FileInfo(xlsFilePath));

                return true;

            }
            catch (Exception ex)
            {
                if (ex is System.IO.InvalidDataException || (ex.InnerException != null &&
                    ex.InnerException is System.IO.InvalidDataException))
                {
                    SetResult(ImportAnalysisResultStatus.InvalidFile, StringMessages.ImportInvalidFile);
                    Trace.TraceError($"[Import] Error to read file during import: {ex.Message}");
                }
                else
                {
                    SetResult(ImportAnalysisResultStatus.InternalError, StringMessages.ImportInitializationError);
                    Trace.TraceError($"[Import] Exception generated during initialization: {ex.Message}");
                }
                return false;
            }
        }

        bool LoadFileData(ImportColumnsFormat columnsFormat)
        {
            try
            {
                
                if (_fileRows > _maxSupportedRows)
                {
                    SetResult(ImportAnalysisResultStatus.Invalid, StringMessages.ImportMaxRows);
                    return false;
                }

                if (_worksheet.Cells.Count() == 0)
                {
                    SetResult(ImportAnalysisResultStatus.Invalid, StringMessages.ImportEmptyWorksheet);
                    return false;
                }

                SetColumnsFormat(columnsFormat);
                if (!EvaluateColumns())
                    return false;
                if (_columnNameToIndex.ContainsKey(ColumnNameId))
                {
                    LoadProjectInventory();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (ex is System.IO.InvalidDataException || (ex.InnerException != null &&
                    ex.InnerException is System.IO.InvalidDataException))
                {
                    SetResult(ImportAnalysisResultStatus.InvalidFile, StringMessages.ImportInvalidFile);
                    Trace.TraceError($"[Import] Error to read file during import: {ex.Message}");
                }
                else
                {
                    SetResult(ImportAnalysisResultStatus.InternalError, StringMessages.ImportInitializationError);
                    Trace.TraceError($"[Import] Exception generated during initialization: {ex.Message}");
                }
                return false;
            }
        }

        public void Init(int domainId, int projectId)
        {
            _db = new audaxwareEntities();
            _db.Database.CommandTimeout = 600;
            InitStringLimits(_db);


            _showAudaxwareInfo = Helper.ShowAudaxWareInfo(domainId);
            _domainId = (short)domainId;
            _projectId = projectId;

            _responsibilities = _db.responsabilities.Where(x => (x.domain_id == _domainId || (_showAudaxwareInfo == true && x.domain_id == 1))).ToList();

            var jsns = _db.jsns.Where(x => x.domain_id == 1 || x.domain_id == _domainId).ToList();
            foreach (var jsnEntry in jsns)
            {
                if (_existingJSNs.ContainsKey(jsnEntry.jsn_code))
                {
                    if (jsnEntry.domain_id == _domainId)
                    {
                        _existingJSNs[jsnEntry.jsn_code] = jsnEntry;
                    }
                }
                else
                {
                    _existingJSNs.Add(jsnEntry.jsn_code, jsnEntry);
                }
            }
        }

        string ParseUtilityValue(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "N/A";
            }
            return input.ToUpper();
        }

        bool CheckUtility(ImportItem item,  int number, string jsnUtility, string itemUtility)
        {
            jsnUtility = ParseUtilityValue(jsnUtility);
            itemUtility = ParseUtilityValue(itemUtility);
            if (!String.IsNullOrEmpty(itemUtility) && (jsnUtility ?? "") != itemUtility)
            {
                return false;
            }
            return true;
        }
        void SetAssetUtility(ImportItem item,  int number, string jsnUtility, string itemUtility, out bool? ow, out string utility)
        {
            if (CheckUtility(item, number, jsnUtility, itemUtility))
            {
                ow = false;
                utility = null;
            }
            else
            {
                ow = true;
                utility = itemUtility;
            }
        }


        bool ValidateLimits(ImportItem item)
        {
            var itemProperties = item.GetType().GetProperties();
            foreach(var prop in itemProperties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    if (_stringLimits.ContainsKey(prop.Name))
                    {
                        var val = prop.GetValue(item);
                        if (val != null)
                        {
                            var length = val.ToString().Length;
                            if (length > _stringLimits[prop.Name])
                            {
                                item.SetError(string.Format(StringMessages.ImportFieldLimitExceeded, prop.Name, length, _stringLimits[prop.Name]));
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        bool ValidateItemJSN(ImportItem item, out bool createJSN)
        {
            createJSN = false;
            // No JSN
            if (String.IsNullOrEmpty(item.JSN))
                return true;
            item.JSN = item.JSN.Trim().ToUpper();
            if (item.JSN.Length > 5)
            {
                item.JSNNoSuffix = item.JSN.Substring(0, 5);
                item.JSNSuffix = item.JSN.Substring(6);
                if (item.JSNSuffix.Length > 4)
                {
                    item.SetStatus(ImportItemStatus.Error, StringMessages.ImportJSNSuffixTooLong);
                    return false;
                }
            }
            else
            {
                item.JSNNoSuffix = item.JSN;
                item.JSNSuffix = null;
            }

            // New JSN
            if (!_existingJSNs.ContainsKey(item.JSNNoSuffix))
            {
                item.SetStatus(ImportItemStatus.NewCatalog, string.Format(StringMessages.ImportJSNNotFound, item.JSNNoSuffix));
                createJSN = true;
                return true;
            }
            
            var jsn_data = _existingJSNs[item.JSNNoSuffix];
            if (string.IsNullOrEmpty(item.Description) && jsn_data != null)
            {
                item.Description = jsn_data.nomenclature;
            }
            if (!CheckUtility(item, 1, jsn_data.utility1, item.U1))
            {
                return true;
            }
            if (!CheckUtility(item, 2, jsn_data.utility2, item.U2))
            {
                return true;
            }
            if (!CheckUtility(item, 3, jsn_data.utility3, item.U3))
            {
                return true;
            }
            if (!CheckUtility(item, 4, jsn_data.utility4, item.U4))
            {
                return true;
            }
            if (!CheckUtility(item, 5, jsn_data.utility5, item.U5))
            {
                return true;
            }
            if (!CheckUtility(item, 6, jsn_data.utility6, item.U6))
            {
                return true;
            }
            return true;
        }

        bool IsPropertyNullOrEmpty(ImportItem item, string property)
        {
            var prop = _itemType.GetProperty(property);
            if (prop == null)
            {
                return true;
            }
            var val = prop.GetValue(item);

            if(val == null)
            {
                return true;
            }

            if (val is string)
            {
                if (String.IsNullOrEmpty(val as string))
                {
                    return true;
                }
            }
            else if (val is int)
            {
                if (((int) val) == 0)
                {
                    return true;
                }
            }
            return false;
        }


        bool ValidateItemRequiredColumns(ImportItem item)
        {
            var missingRequired = from required in _requiredColumns where IsPropertyNullOrEmpty(item, required) select
                                  _selectedColumnFormatReversed == null ? required : _selectedColumnFormatReversed[required];
            // If we have the item id, we do not check for the required columns
            // as the required values will be properly set for the existing item
            // we are updating
            if (missingRequired.Count() > 0 && item.Id == null)
            {
                item.SetError(string.Format(StringMessages.ImportMissingItemRequiredColumns,
                        String.Join(",", missingRequired)));
                return false;
            }

            if (!_assetHintColumns.Any(x => !IsPropertyNullOrEmpty(item, x)))
            {
                item.SetError(string.Format(StringMessages.ImportItemAssetHintColumnMissing,
                        String.Join(",", _assetHintColumnsTargetFormat)));
                return false;
            }

            return true;
        }

        void SplitDescription( string description, out string categoryDescription, out string subcategoryDescription, out string assetSuffix)
        {
            var splittedDescription = description.Split(',').ToList();
            if (splittedDescription.Count >= 3)
            {
                assetSuffix = splittedDescription.Last().Trim();
                splittedDescription.RemoveAt(splittedDescription.Count - 1);
            }
            else
            {
                assetSuffix = null;
            }

            if (splittedDescription.Count >= 2)
            {
                subcategoryDescription = splittedDescription.Last().Trim();
                splittedDescription.RemoveAt(splittedDescription.Count - 1);
            }
            else
            {
                subcategoryDescription = null;
            }

            if (splittedDescription.Count >= 1)
                categoryDescription = String.Join(",", splittedDescription).Trim();
            else
                categoryDescription = null;
            if (subcategoryDescription == null)
                subcategoryDescription = categoryDescription;
        }


        int GetCostCenter(string code, string description)
        {
            if(_projectCostCenters.ContainsKey(code.ToUpper()))
            {
                return _projectCostCenters[code.ToUpper()].id;
            }
            var costCenter = new cost_center();
            costCenter.code = code;
            costCenter.domain_id = _domainId;
            costCenter.description = description;
            costCenter.project_id = _projectId;
            _db.Entry(costCenter).State = EntityState.Added;
            _db.SaveChanges();
            _projectCostCenters.Add(code.ToUpper(), costCenter);
            return costCenter.id;
        }

        void SaveTemporaryLocation(string name)
        {
            var temporaryLocation = new temporary_location();
            temporaryLocation.name = name.Trim();
            temporaryLocation.domain_id = _domainId;
            temporaryLocation.project_id = _projectId;
            _db.Entry(temporaryLocation).State = EntityState.Added;
            _db.SaveChanges();
            _projectTemporaryLocations.Add(name.ToUpper().Trim());
        }

        void SaveFinalDisposition(string name)
        {
            var finalDisposition = new final_disposition();
            finalDisposition.name = name.Trim();
            finalDisposition.domain_id = _domainId;
            finalDisposition.project_id = _projectId;
            _db.Entry(finalDisposition).State = EntityState.Added;
            _db.SaveChanges();
            _projectFinalDispositions.Add(name.ToUpper().Trim());
        }


        string GetAssetSuffix(string description)
        {
            string categoryDescription, subcategoryDescription, assetSuffix;
            SplitDescription(description, out categoryDescription, out subcategoryDescription, out assetSuffix);
            return assetSuffix;
        }

        private assets_subcategory GetSubcategory(ImportItem item, jsn jsn = null, bool create = false)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                if (jsn == null)
                {
                    Trace.TraceError("Error to import item, Description and JSN cannot both be empty");
                    return null;
                }
                item.Description = jsn.nomenclature;
            }
            assets_subcategory ret = null;
            if (!string.IsNullOrEmpty(item.Description) && _subcategories.TryGetValue(item.Description, out ret))
            {
                if (!create || ret != null)
                {
                    return ret;
                }
                _subcategories.Remove(item.Description);
            }
            var description = item.Description; 
            var categoryDescription = "";
            var subcategoryDescription = "";
            var assetSuffix = "";
            SplitDescription(item.Description, out categoryDescription, out subcategoryDescription, out assetSuffix);

            // In case we have to create a subcategory, we will use this to select the best one
            assets_category create_category = null;

            var categories = _db.assets_category.Where(x => string.Compare(x.description, categoryDescription, true) == 0 &&
            (x.domain_id == 1 ||
                x.domain_id == _domainId)).ToList();
            if (categories.Count() == 0)
            {
                if (!create)
                {
                    _subcategories.Add(item.Description, null);
                    return ret;
                }
                create_category = new assets_category();
                create_category.domain_id = _domainId;
                create_category.description = categoryDescription;
                create_category.Electrical = "E";
                create_category.Environmental = "E";
                create_category.Gases = "E";
                create_category.HVAC = "E";
                create_category.IT = "E";
                create_category.Physical = "E";
                create_category.Plumbing = "E";
                create_category.Support = "E";
                create_category.added_by = _user;
                create_category.date_added = DateTime.Now;

                _db.Entry(create_category).State = EntityState.Added;
                _db.SaveChanges();
                categories = _db.assets_category.Where(x => string.Compare(x.description, categoryDescription, true) == 0 &&
                    (x.domain_id == 1 || x.domain_id == _domainId)).ToList();
            }

            foreach (var category in categories)
            {
                // TODO(JLT): We need to enhance this to be more precise on the category we will pick
                if (create_category == null)
                    create_category = category;

                var assetSubcategories = _db.assets_subcategory.Where(x => x.category_id == category.category_id &&
                    string.Compare(x.description, subcategoryDescription, true) == 0 && 
                   (x.domain_id == 1 || x.domain_id == _domainId)).ToList();
                assets_subcategory subcategory = null;
                if (assetSubcategories.Count() > 0)
                {
                    subcategory = assetSubcategories?.First();
                }
                if (subcategory != null)
                {
                    if (string.IsNullOrEmpty(subcategory.asset_code) && !string.IsNullOrEmpty(category.asset_code))
                    {
                         //subcategory.asset_code = category.asset_code;
                    }
                    ret = subcategory;
                    _subcategories.Add(item.Description, ret);
                    return ret;
                }
            }

            if (create)
            {
                ret = new assets_subcategory();
                ret.category_id = create_category.category_id;
                ret.category_domain_id = create_category.domain_id;
                ret.domain_id = _domainId;
                // Some imported items might not have a subcategory, so we add an empty subcategory
                ret.description = string.IsNullOrEmpty(subcategoryDescription) ? " " : subcategoryDescription;
                ret.Electrical = "E";
                ret.Environmental = "E";
                ret.Gases = "E";
                ret.HVAC = "E";
                ret.IT = "E";
                ret.Physical = "E";
                ret.Plumbing = "E";
                ret.Support = "E";
                ret.use_category_settings = false;
                ret.asset_code = jsn?.asset_code;
                ret.added_by = _user;
                ret.date_added = DateTime.Now;
                _db.Entry(ret).State = EntityState.Added;
                _db.SaveChanges();
                _subcategories.Add(item.Description, ret);
            }
            else
            {
                _subcategories.Add(item.Description, null);
            }

            return ret;
        }

        private bool ValidateItemCatalog(ImportItem item)
        {
            List<asset_import_search_Result>  assets;
            if (string.IsNullOrEmpty(item.Manufacturer))
            {
                item.Manufacturer = item.Id != null ? _projectInventory[(int)item.Id].manufacturer_description : "TBD";
            }

            if (string.IsNullOrEmpty(item.ModelName) && item.Id != null)
            {
                item.ModelName = _projectInventory[(int)item.Id].model_name;
            }

            if (string.IsNullOrEmpty(item.ModelNumber) & item.Id != null)
            {
                item.ModelNumber = _projectInventory[(int)item.Id].model_number;
            }
            string key = item.CatalogKey;

            if (_existingCatalogAssets.ContainsKey(key))
            {
                assets = _existingCatalogAssets[key];
            }
            else
            {
                int? jsnId = null;
                short? jsnDomainId = null;
                if (!string.IsNullOrEmpty(item.JSN))
                {
                    if (_existingJSNs.ContainsKey(item.JSN))
                    {
                        jsnId = _existingJSNs[item.JSN].Id;
                        jsnDomainId = _existingJSNs[item.JSN].domain_id;
                    }
                    else
                    {
                        jsnId = -1;
                        jsnDomainId = -1;
                    }
                }
                assets = _db.asset_import_search((short)_domainId, _showAudaxwareInfo, item.Code,
                        item.Manufacturer, item.ModelNumber, item.ModelName, item.Description,
                        jsnId, jsnDomainId, string.IsNullOrEmpty(item.JSNSuffix) ? null : item.JSNSuffix, _projectId).ToList();
                _existingCatalogAssets.Add(key, assets);
                if (assets.Count() == 0 && item.HasId && _projectInventory.ContainsKey((int)item.Id))
                {
                    var inventory = _projectInventory[(int)item.Id];
                    var existingItemAsset = _db.assets.Where(x => x.asset_id == inventory.asset_id && x.domain_id == inventory.asset_domain_id).FirstOrDefault();
                    AddCatalogSearchResult(item.CatalogKey, existingItemAsset.asset_code, inventory.asset_id, inventory.asset_domain_id, inventory.resp, existingItemAsset.jsn_id);
                }
            }
            if (assets.Count() == 0)
            {
                item.SetStatus(ImportItemStatus.NewCatalog, StringMessages.ImportNoMatchingAssets);
            }
            else if (assets.Count() == 1)
            {
                item.Code = assets[0].asset_code;
                // When we set the code, the key changes to be based on the asset code
                // this ensures we have the key added in both cases
                if (!_existingCatalogAssets.ContainsKey(item.CatalogKey))
                {
                    _existingCatalogAssets.Add(item.CatalogKey, assets);
                }

                if (string.IsNullOrEmpty(item.Resp))
                {
                    item.Resp = assets[0].default_resp;
                }
                if (item.IsNewCatalog)
                    item.Status = ImportItemStatus.New;
            }
            else
            {
                item.SetStatus(ImportItemStatus.Error, StringMessages.ImportMultipleAssetsFound);
                Trace.TraceError($"ImportMultipleAssetsFound.Description: {item.CatalogKey}.");
            }
            item.MatchingCodes = assets.Select(x => x.asset_code).ToArray();
            return true;
        }

        private bool ValidateCategorySubcategory(ImportItem item)
        {
            // if status is not new, we will not need to create category and subcategory
            if (!item.IsNewCatalog)
                return true;

            if (String.IsNullOrEmpty(item.Description ))
            {
                item.SetError(StringMessages.ImportMissingCategorySubcategory);
                return false;
            }
            var description = item.Description; 
            var categoryDescription = "";
            var subcategoryDescription = "";
            var assetSuffix = "";

            SplitDescription(item.Description, out categoryDescription, out subcategoryDescription, out assetSuffix);
            
            if (string.IsNullOrEmpty(categoryDescription) || string.IsNullOrEmpty(subcategoryDescription))
            {
                item.SetError(string.Format(StringMessages.ImportDescriptionInvalid, item.Description));
                return false;
            }
            if (categoryDescription.Length > _stringLimits["Category"])
            {
                item.SetError(string.Format(StringMessages.ImportDescriptionCategoryInvalid, item.Description, _stringLimits["Category"]));
                return false;
            }
            if (subcategoryDescription.Length > _stringLimits["SubCategory"])
            {
                item.SetError(string.Format(StringMessages.ImportDescriptionSubcategoryInvalid, item.Description, _stringLimits["SubCategory"]));
                return false;
            }
            return true;
        }
        
        string NormalizeDescription(string description)
        {
            if (description == null)
                return "";
            if (description.IndexOf(',') < 0)
            {
                var tmp = new System.Text.StringBuilder (description);
                var index = description.IndexOf('/');
                if (index > 0)
                {
                    tmp[index] = ',';
                }
                else
                {
                    index = description.IndexOf(';');
                    if (index > 0)
                    {
                        tmp[index] = ',';
                    }
                }
                return tmp.ToString();
            }
            else
            {
                return description;
            }
        }

        public void ValidateItem(ImportItem item)
        {
            if (!item.HasId)
                item.Status = ImportItemStatus.New;
            else
                item.Status = ImportItemStatus.Changed;

            if (item.HasId && !_projectInventory.ContainsKey((int)item.Id))
            {
                item.SetError(StringMessages.ImportInvalidAssetId);
                return;
            }

            if (String.IsNullOrEmpty(item.Description))
            {
                item.Description = item.JSNNomeclature;
            }

            item.Description = NormalizeDescription(item.Description);
            var resp = item.Resp?.PadRight(10);

            if (resp == null || _responsibilities.Where(x => x.name.Trim() == resp.Trim()).FirstOrDefault() == null)
            {
                item.SetError(StringMessages.ImportMissingResponsability);
                return;
            }
            
            if (string.IsNullOrEmpty(item.Placement))
            {
                item.Placement = "None";
            }


            if(!_placements.ContainsValue(item.Placement))
            {
                item.SetError(string.Format(StringMessages.ImportPlacementInvalid, item.Placement));
                return;
            }

            if (!String.IsNullOrEmpty(item.Class))
            {
                // Remove the description added during the export
                int descriptionSeparator = item.Class.IndexOf(':');
                if (descriptionSeparator > 0)
                {
                    item.Class = item.Class.Substring(0, descriptionSeparator);
                }
            }

            if (!String.IsNullOrEmpty(item.Class) && !_classes.ContainsKey(item.Class))
            {
                item.SetError(string.Format(StringMessages.ImportInvalidClassSpecified, string.Join(",", _classes.Keys)));
                return;
            }

            if (!ValidateItemRequiredColumns(item))
            {
                return;
            }

            bool createJSN;
            if (!ValidateItemJSN(item, out createJSN))
            {
                return;
            }

            if (!ValidateItemCatalog(item))
            {
                return;
            }

            if (!ValidateCategorySubcategory(item))
            {
                return;
            }

            if (string.IsNullOrEmpty(item.UnitOfMeasure))
            {
                item.UnitOfMeasure = "ea.";
            }

            if (!_existingUnits.ContainsKey(item.UnitOfMeasure))
            {
                var measurement = _db.assets_measurement.Where(x => x.eq_unit_desc.ToLower().Contains(item.UnitOfMeasure.ToLower()));
                if (measurement.Count() == 0)
                {
                    item.SetError(string.Format(StringMessages.ImportInvalidUnitOfMeasurement, item.UnitOfMeasure));
                    return;
                }
                else
                {
                    _existingUnits.Add(item.UnitOfMeasure, measurement.FirstOrDefault());
                }
            }

            if (!ValidateLimits(item))
            {
                return;
            }

            if (item.Status == ImportItemStatus.New)
            {
                if (item.HasId)
                {
                    item.Status = ImportItemStatus.Changed;
                }
                else
                {
                    if (item.MatchingCodes == null || item.MatchingCodes.Length == 0)
                    {
                        item.Status = ImportItemStatus.NewCatalog;
                    }
                }
            }
        }


        Object GetCellValue(int row, string propName)
        {
            object val = _worksheet.Cells[row, _columnNameToIndex[propName]].Value;
            return val;
        }

        void ComputeItem(ImportItem item)
        {
            switch(item.Status)
            {
                case ImportItemStatus.NewCatalog:
                case ImportItemStatus.NewCatalogWarning:
                    // Items added to the catalog are also added to the inventory
                    ++_analysisResult.TotalNewCatalog;
                    ++_analysisResult.TotalNew;
                    break;
                case ImportItemStatus.New:
                    ++_analysisResult.TotalNew;
                    break;
                case ImportItemStatus.Changed:
                    ++_analysisResult.TotalChanged;
                    break;
                case ImportItemStatus.Error:
                    ++_analysisResult.TotalErrors;
                    break;
            }

        }

        ImportItem CreateItem(int row)
        {
            var item = new ImportItem();
            var itemType = item.GetType();
            item.Worksheet = _analysisResult.WorkSheetName;
            foreach( var col in _analysisResult.UsedColumns )
            {
                var propName = _selectedColumnFormat[col];
                var prop = _itemType.GetProperty(propName);
                object val = GetCellValue(row, propName);
                try
                {
                    // Special handling because it is nullable
                    if (prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(int?))
                    {
                        if (val == null)
                        {
                            prop.SetValue(item, null);
                        }
                        else
                        {
                            if (prop.PropertyType == typeof(decimal?) && val.ToString().Length > 0)
                                prop.SetValue(item, (decimal)(double)val);
                            else if (val.ToString().Length > 0)
                                prop.SetValue(item, Convert.ChangeType(val, typeof(int)));
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(item, Convert.ChangeType(val, prop.PropertyType));

                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                    {
                        if (val != null)
                        {
                            var strVal = val.ToString().ToUpper();
                            if (strVal == "TRUE" || strVal == "1")
                            {
                                prop.SetValue(item, true);
                            }
                            else
                            {
                                prop.SetValue(item, false);
                            }
                        }
                    }
                    else
                    {
                        prop.SetValue(item, Convert.ChangeType(val ?? "0", prop.PropertyType));
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Exception parsing column values for row {row}. Exception: {ex.Message}");
                    item.SetError(string.Format(StringMessages.ImportInvalidCellValue, val, col));
                    return item;
                }
            }

            ValidateItem(item);
            return item;
        }

        static void TraceException(Exception ex)
        {
            var inner = ex.InnerException;
            if (inner != null)
            {
                Trace.TraceError(StringMessages.ImporterExceptionInner, ex.Message, inner.Message, ex.StackTrace);
            }
            else
            {
                Trace.TraceError(StringMessages.ImporterException, ex.Message, ex.StackTrace);
            }
        }

        public List<ImportAnalysisResult> Analyze(int domainId, int projectId, string xlsFilePath, ImportColumnsFormat columnsFormat)
        {
            try
            {
                Init(domainId, projectId);
                if (!CheckFile(xlsFilePath))
                {
                    _analysisResultTotal.Add(_analysisResult);
                    return _analysisResultTotal;
                }
                
                foreach (var sheet in _xlPackage.Workbook.Worksheets)
                {
                    var items = new List<ImportItem>();
                    _analysisResult = new ImportAnalysisResult();
                    _analysisResult.Items = items;
                    _worksheet = sheet;
                    _analysisResult.WorkSheetName = sheet.Name.ToString();
                    _fileRows = sheet.Dimension.End.Row;

                    if (LoadFileData(columnsFormat))
                    {
                        for (int row = 1; row < _fileRows; row++)
                        {
                            var item = CreateItem(row + 1);
                            // If we have the PP cost center, we are breaking the assets into individual entries.
                            // This is to allow them to be properly mapped
                            if (!item.HasId && item.PlannedQty > 1 && item.CostCenter == "PP")
                            {
                                for (int i = 0; i < item.PlannedQty; i++)
                                {
                                    var cloned = item.Clone();
                                    cloned.PlannedQty = 1;
                                    _analysisResult.Items.Add(cloned);
                                    ComputeItem(cloned);
                                }
                            }
                            else
                            {
                                _analysisResult.Items.Add(item);
                                ComputeItem(item);
                            }
                        }
                    }
                    _analysisResultTotal.Add(_analysisResult);
                }
                return _analysisResultTotal;                
            }
            catch (Exception ex)
            {
                TraceException(ex);
                throw ex;
            }
        }

        private jsn GetJSN(ImportItem item)
        {
            if (string.IsNullOrEmpty(item.JSN))
                return null;

            bool createJSN;
            ValidateItemJSN(item, out createJSN);

            if (createJSN)
            {
                var jsnData = new jsn();
                jsnData.jsn_code = item.JSNNoSuffix;
                jsnData.domain_id = (short)_domainId;
                jsnData.nomenclature = item.JSNNomeclature;
                if (string.IsNullOrEmpty(jsnData.nomenclature))
                {
                    jsnData.nomenclature = item.Description;
                }
                jsnData.utility1 = item.U1;
                jsnData.utility2 = item.U2;
                jsnData.utility3 = item.U3;
                jsnData.utility4 = item.U4;
                jsnData.utility5 = item.U5;
                jsnData.utility6 = item.U6;
                jsnData.asset_code = null;
                jsnData.asset_code_domain_id = 1;
                jsnData.added_by = _user;
                jsnData.date_added = DateTime.Now;
                _db.Entry(jsnData).State = EntityState.Added;
                _db.SaveChanges();
                _existingJSNs[item.JSNNoSuffix] = jsnData;
            }
            return _existingJSNs[item.JSNNoSuffix];
        }

        private asset GetAsset(ImportItem item, jsn jsn, manufacturer manufacturer) {
            var asset = new asset();
            var newCatalog = item.IsNewCatalog;
            if (item.Status == ImportItemStatus.Changed)
            {
                if (_existingCatalogAssets.ContainsKey(item.CatalogKey))
                {
                    newCatalog = _existingCatalogAssets[item.CatalogKey].Count() == 0;
                }
                else
                {
                    Trace.TraceError("Item not found in the list of existing assets and it must be there after validation is sucessfull");
                    return null;
                }
            }

            if (!newCatalog)
            {
                if (_existingCatalogAssets.ContainsKey(item.CatalogKey))
                {
                    var foundAsset = _existingCatalogAssets[item.CatalogKey].First();
                    asset.asset_id = foundAsset.asset_id;
                    asset.asset_code = foundAsset.asset_code;
                    asset.domain_id = foundAsset.domain_id;
                    asset.default_resp = foundAsset.default_resp;
                    asset.jsn_id = foundAsset.jsn_id;
                    // If asset was found, but does not have a JSN and it is a domain asset, then we update
                    // the asset to add the jsn
                    if (asset.jsn_id == null && jsn != null)
                    {
                        var domainAsset = _db.assets.Where(x => x.asset_id == asset.asset_id && x.domain_id == _domainId).FirstOrDefault();
                        // The asset can be null if it does not belong to the domain being imported
                        if (domainAsset != null)
                        {
                            asset = domainAsset;
                            asset.jsn = jsn;
                            asset.imported_by_project_id = _projectId;
                            _db.Entry(asset).State = EntityState.Modified;
                            _db.SaveChanges();
                        }
                    }
                }
                else
                {
                    throw new System.ApplicationException("Item status indicates the asset exists but it cannot be found neither by asset code nor by Catalog Key");
                }
            }
            else
            {
                var subcategory = GetSubcategory(item, jsn, true);
                if (subcategory == null)
                {
                    Trace.TraceError($"Error to create category/subcateogry for asset {item.Description}");
                    return null;
                }

                var measurement = _existingUnits[item.UnitOfMeasure];

                var code = jsn?.asset_code;
                // No code assigned to the JSN or code = EQP
                if (string.IsNullOrEmpty(code))
                {
                    code = subcategory?.asset_code;
                }

                if (string.IsNullOrEmpty(code))
                {
                    code = "EQP";
                }
                else
                {
                    code = code.ToUpper();
                }
                var last_code = _lastAssetsCodes.Where(x => x.prefix.ToUpper() == code).FirstOrDefault();
                if (last_code == null)
                {
                    var prefix = code;
                    code = code + (_domainId == 1 ? "00001" : "10000");
                    _lastAssetsCodes.Add(new get_max_asset_codes_Result() { prefix = prefix, max_asset_code = code });
                }
                else
                {
                    int aux;
                    Int32.TryParse(last_code.max_asset_code.Substring(3, last_code.max_asset_code.Length - 3), out aux);
                    var nextCode = (aux + 1).ToString("D" + 5);
                    code = code + nextCode;
                    _lastAssetsCodes[_lastAssetsCodes.FindIndex(x => x.prefix.ToUpper() == last_code.prefix)].max_asset_code = code;
                }

                asset.asset_code = code;
                // The standard JSN, with no changes belog to the AudaxWare database
                asset.domain_id = _domainId;
                asset.manufacturer_id = manufacturer.manufacturer_id;
                asset.manufacturer_domain_id = manufacturer.domain_id;
                asset.default_resp = item.Resp;
                asset.eq_measurement_id = measurement.eq_unit_measure_id;
                if (jsn != null)
                {
                    asset.jsn_id = jsn.Id;
                    asset.jsn_domain_id = jsn.domain_id;
                }
                if (jsn != null)
                {
                    bool? overwrite;
                    string utilityValue;
                    SetAssetUtility(item, 1, jsn.utility1, item.U1, out overwrite, out utilityValue);
                    asset.jsn_utility1 = utilityValue;
                    asset.jsn_utility1_ow = overwrite;
                    SetAssetUtility(item, 2, jsn.utility2, item.U2, out overwrite, out utilityValue);
                    asset.jsn_utility2 = utilityValue;
                    asset.jsn_utility2_ow = overwrite;
                    SetAssetUtility(item, 3, jsn.utility3, item.U3, out overwrite, out utilityValue);
                    asset.jsn_utility3 = utilityValue;
                    asset.jsn_utility3_ow = overwrite;
                    SetAssetUtility(item, 4, jsn.utility4, item.U4, out overwrite, out utilityValue);
                    asset.jsn_utility4 = utilityValue;
                    asset.jsn_utility4_ow = overwrite;
                    SetAssetUtility(item, 5, jsn.utility5, item.U5, out overwrite, out utilityValue);
                    asset.jsn_utility5 = utilityValue;
                    asset.jsn_utility5_ow = overwrite;
                    SetAssetUtility(item, 6, jsn.utility6, item.U6, out overwrite, out utilityValue);
                    asset.jsn_utility6 = utilityValue;
                    asset.jsn_utility6_ow = overwrite;
                }
                asset.placement = item.Placement;
                asset.height = item.Height ?? null;
                asset.width = item.Width ?? null;
                asset.depth = item.Depth ?? null;
                asset.mounting_height = item.MountingHeight ?? null;
                asset.network_option = item.NetworkOption ?? null;

                // This should never happen as we would capture on the ValidateItem, hence we do not check
                if (!String.IsNullOrEmpty(item.Class))
                {
                    asset.@class = _classes[item.Class];
                }

                asset.subcategory_domain_id = subcategory.domain_id;
                asset.subcategory_id = subcategory.subcategory_id;
                asset.asset_description = string.IsNullOrEmpty(item.Description) && jsn != null ? jsn.nomenclature : item.Description;
                asset.asset_suffix = GetAssetSuffix(asset.asset_description);
                asset.added_by = _user;
                asset.date_added = DateTime.Now;
                asset.updated_at = DateTime.Now;
                asset.model_name = item.ModelName;
                asset.model_number = item.ModelNumber;
                asset.jsn_suffix = item.JSNSuffix;
                //update asset attributes if jsn exists
                if (jsn != null)
                {
                    asset.jsn = jsn;
                    using (JSNRepository rep = new JSNRepository())
                    {
                        rep.UpdateAsset(asset);
                    }
                }
                asset.imported_by_project_id = _projectId;
                _db.Entry(asset).State = EntityState.Added;
                _db.SaveChanges();
                AddCatalogSearchResult(item.CatalogKey, asset.asset_code, asset.asset_id, asset.domain_id, asset.default_resp, asset.jsn_id);
            }
            return asset;
        }

        void AddCatalogSearchResult(string key, string code, int asset_id, short domain_id, string default_resp, int? jsn_id)
        {
            var searchResult = new asset_import_search_Result()
            {
                asset_code = code,
                asset_id = asset_id,
                domain_id = domain_id,
                default_resp = default_resp,
                jsn_id = jsn_id
            };


            // If we have several items pointing to a New Catalog item, this ensures the catalog item only gets created once
            if (_existingCatalogAssets.ContainsKey(key))
            {
                if (_existingCatalogAssets[key].Count() > 0)
                {
                    Trace.TraceError($"Programming error, we are adding an asset but there is already one in the catalog {key}");
                }
                else
                {
                    _existingCatalogAssets[key].Add(searchResult);
                }
            }
            else
            {
                var searchResultList = new List<asset_import_search_Result>();
                searchResultList.Add(searchResult);
                _existingCatalogAssets.Add(key, searchResultList);
            }
        }

        private int GetPhase(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                description = "Unassigned";
                if (_phases.ContainsKey(description))
                {
                    return _phases[description];
                }
                var phases = _db.project_phase.Where(x => x.domain_id == _domainId && x.project_id == _projectId);
                if (phases.Count() > 0)
                {
                    var firstOrDefault = phases.FirstOrDefault();
                    _phases.Add(description, firstOrDefault.phase_id);
                    return firstOrDefault.phase_id;
                }
            }

            if (_phases.ContainsKey(description))
            {
                return _phases[description];
            }

            var item = _db.project_phase.Where(x => x.domain_id == _domainId && x.project_id == _projectId
                && x.description == description).FirstOrDefault();

            if (item == null)
            {
                item = new project_phase();
                item.domain_id = _domainId;
                item.project_id = _projectId;
                item.description = description;
                item.start_date = DateTime.Now;//????
                item.end_date = DateTime.Now;//????
                item.added_by = _user;
                item.date_added = DateTime.Now;

                _db.Entry(item).State = EntityState.Added;
                _db.SaveChanges();
            }

            _phases.Add(description, item.phase_id);

            return item.phase_id;
        }

        private int GetDepartment(int phaseId, string description)
        {
            var key = $"{phaseId}:{description}";

            if (_departments.ContainsKey(key))
            {
                return _departments[key];
            }

            var item = _db.project_department.Where(x => x.domain_id == _domainId &&
                x.project_id == _projectId && x.phase_id == phaseId && x.description == description).FirstOrDefault();

            if (item == null)
            {
                item = new project_department();
                item.domain_id = _domainId;
                item.project_id = _projectId;
                item.phase_id = phaseId;
                item.description = description;
                item.department_type_id = 53;//????
                item.department_type_domain_id = 1;//???
                item.added_by = _user;
                item.date_added = DateTime.Now;

                _db.Entry(item).State = EntityState.Added;
                _db.SaveChanges();
            }

            _departments.Add(key, item.department_id);
            return item.department_id;
        }

        private void GetRoom(string phase, string department, string roomName, string roomNumber, string roomFunctionalArea, string blueprint, string staff, string roomCode, project_room_inventory inventory, decimal? roomArea = null)
        {
            string key = $"{phase??""}-{department}-{roomName}-{roomNumber}";
            RoomEntry room;
            if (_rooms.ContainsKey(key))
            {
                room = _rooms[key];
                room.SetInventory(inventory);
                return;
            }

            int phaseId = GetPhase(phase);
            int departmentId = GetDepartment(phaseId, department);
            var item = _db.project_room.Where(x => x.domain_id == _domainId && x.project_id == _projectId
                && x.phase_id == phaseId && x.department_id == departmentId && x.drawing_room_number == roomNumber
                && x.drawing_room_name == roomName).FirstOrDefault();
            if (item == null)
            {
                item = new project_room();
                item.domain_id = _domainId;
                item.project_id = _projectId;
                item.phase_id = phaseId;
                item.department_id = departmentId;
                item.drawing_room_number = roomNumber;
                item.drawing_room_name = roomName;
                item.functional_area = roomFunctionalArea;
                item.blueprint = blueprint;
                item.staff = staff;
                item.room_area = roomArea;
                item.room_code = roomCode;
                item.room_quantity = 1;
                item.added_by = _user;
                item.date_added = DateTime.Now;

                _db.Entry(item).State = EntityState.Added;
                _db.SaveChanges();
            }
            else if (item.functional_area != roomFunctionalArea || item.blueprint != blueprint || item.staff != staff || item.room_area != roomArea || item.room_code != roomCode)
            {
                item.functional_area = GetUpdateStringValue(item.functional_area, roomFunctionalArea);
                item.blueprint = GetUpdateStringValue(item.blueprint, blueprint);
                item.staff = GetUpdateStringValue(item.staff, staff);
                if (roomArea != null)
                {
                    item.room_area = roomArea;
                }
                item.room_code = GetUpdateStringValue(item.room_code, roomCode);
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
            }

            room = new RoomEntry(phaseId, departmentId, item.room_id);
            room.SetInventory(inventory);
            _rooms.Add(key, room);
        }


        string GetUpdateStringValue( string current, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return current;
            }
            newValue = newValue.Trim();
            if (newValue.Length == 0)
            {
                return current;
            }
            if (string.Compare(newValue, "(Clear)", true) == 0)
            {
                return "";
            }
            return newValue;
        }

        void SetInventoryFields(project_room_inventory inventory, ImportItem item, manufacturer manufacturer)
        {
            if (item.Department != null && (item.RoomName != null || item.RoomNumber != null))
            {
                GetRoom(item.Phase, item.Department, item.RoomName, item.RoomNumber, item.FunctionalArea, item.Blueprint, item.Staff, item.RoomCode, inventory, item.RoomArea);
            }
            else
            {
                Trace.TraceError("Department and room name not found for the item");
                return;
            }

            UpdateOWFields(inventory, item.Description, "asset_description");
            UpdateOWFields(inventory, item.Manufacturer, "manufacturer_description");
            UpdateOWFields(inventory, item.ModelName, "model_name");
            UpdateOWFields(inventory, item.ModelNumber, "model_number");
            if (item.PlannedQty < 0 && inventory.budget_qty != null)
            {
                item.PlannedQty = (int)inventory.budget_qty;
            }
            inventory.budget_qty = Math.Max(0, item.PlannedQty);
            inventory.clin = GetUpdateStringValue(inventory.clin, item.Clin);
            inventory.resp = GetUpdateStringValue(inventory.resp, item.Resp);
            inventory.unit_budget = item.UnitBudget ?? inventory.unit_budget;
            inventory.unit_escalation = item.UnitEscalation ?? inventory.unit_escalation;
            inventory.unit_freight_markup = item.UnitFreightMarkup ?? inventory.unit_freight_markup;
            inventory.unit_freight_net = item.UnitFreightNet ?? inventory.unit_freight_net;
            inventory.unit_install_markup = item.UnitInstallMarkup ?? inventory.unit_install_markup;
            inventory.unit_install_net = item.UnitInstallNet ?? inventory.unit_install_net;
            inventory.unit_markup = item.UnitMarkup ?? inventory.unit_markup;
            inventory.unit_tax = item.UnitTax ?? inventory.unit_tax;
            inventory.total_budget_amt = item.UnitBudget * Math.Max(1, item.PlannedQty);
            inventory.comment = GetUpdateStringValue(inventory.comment, item.Comment);
            inventory.tag = GetUpdateStringValue(inventory.tag, item.Tags);
            if (item.Placement != null)
            {
                inventory.placement_ow = true;
                inventory.placement = _placements[item.Placement];
            }
            inventory.biomed_check_required = item.BiomedRequired ?? inventory.biomed_check_required;
            inventory.ECN = GetUpdateStringValue(inventory.ECN, item.ECN);
            if (!string.IsNullOrEmpty(item.TemporaryLocation))
            {
                inventory.temporary_location = item.TemporaryLocation;
                if (!_projectTemporaryLocations.Contains(item.TemporaryLocation.ToUpper().Trim())) 
                {
                    SaveTemporaryLocation(item.TemporaryLocation);
                }
            }
            if (!string.IsNullOrEmpty(item.FinalDisposition))
            {
                inventory.final_disposition = item.FinalDisposition;
                if (!_projectFinalDispositions.Contains(item.FinalDisposition.ToUpper().Trim()))
                {
                    SaveFinalDisposition(item.FinalDisposition);

                }               

            }
            

            inventory.cad_id = GetUpdateStringValue(inventory.cad_id, item.CADID);
            if (!string.IsNullOrEmpty(item.CostCenter))
            {
                inventory.cost_center_id = GetCostCenter(item.CostCenter, item.CostCenterDescription);
            }
            UpdateOWFields(inventory, item.Height, "height");
            UpdateOWFields(inventory, item.Width, "width");
            UpdateOWFields(inventory, item.Depth, "depth");
            UpdateOWFields(inventory, item.MountingHeight, "mounting_height");
            UpdateOWIntFields(inventory, item.NetworkOption, "network_option");
            if (!String.IsNullOrEmpty(item.Class))
            {
                inventory.@class = _classes[item.Class];
                inventory.class_ow = true;
            }

            if (item.JSN != null || item.U1 != null || item.U2 != null || item.U3 != null || item.U4 != null
                || item.U5 != null || item.U6 != null)
            {
                inventory.jsn_ow = true;
                inventory.jsn_code = GetUpdateStringValue(inventory.jsn_code, item.JSN);
                inventory.jsn_utility1 = GetUpdateStringValue(inventory.jsn_utility1, item.U1);
                inventory.jsn_utility2 = GetUpdateStringValue(inventory.jsn_utility2, item.U2);
                inventory.jsn_utility3 = GetUpdateStringValue(inventory.jsn_utility3, item.U3);
                inventory.jsn_utility4 = GetUpdateStringValue(inventory.jsn_utility4, item.U4);
                inventory.jsn_utility5 = GetUpdateStringValue(inventory.jsn_utility5, item.U5);
                inventory.jsn_utility6 = GetUpdateStringValue(inventory.jsn_utility6, item.U6);
            }
        }

        void UpdateInventory(asset asset, ImportItem item, manufacturer manufacturer) {
            var inventory = _projectInventory[(int)item.Id];
            inventory.asset_id = asset.asset_id;
            inventory.asset_domain_id = asset.domain_id;
            SetInventoryFields(inventory, item, manufacturer);
            _db.Entry(inventory).State = EntityState.Modified;
        }

        private void UpdateOWFields(project_room_inventory inventory, string data, string fieldName)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var newType = inventory.GetType();
            var newProp = newType.GetProperty(fieldName);
            if (newProp != null)
            {
                newProp.SetValue(inventory, data);
                newProp = newType.GetProperty(fieldName + "_ow");
                if (newProp != null)
                {
                    newProp.SetValue(inventory, true);
                }
            }
        }

        private void UpdateOWIntFields(project_room_inventory inventory, int? networkOption, string fieldName)
        {
            if (networkOption == null)
            {
                return;
            }
            var newType = inventory.GetType();
             var newProp = newType.GetProperty(fieldName);
            if (newProp != null)
            {
                newProp.SetValue(inventory, networkOption);
                newProp = newType.GetProperty(fieldName + "_ow");
                if (newProp != null)
                {
                    newProp.SetValue(inventory, true);
                }
            }

        }

        private void CreateInventory(asset asset, ImportItem item, manufacturer manufacturer) {
            var inventory = new project_room_inventory();
            inventory.added_by = _user;
            inventory.asset_domain_id = asset.domain_id;
            inventory.asset_id = asset.asset_id;
            inventory.current_location = "Plan";
            inventory.domain_id = _domainId;
            inventory.project_id = _projectId;
            inventory.status = "A";
            inventory.date_added = DateTime.Now;
            SetInventoryFields(inventory, item, manufacturer);
            _db.Entry(inventory).State = EntityState.Added;
            _db.SaveChanges();
            item.Id = inventory.inventory_id;
        }

        private manufacturer GetManufacturer(string manufacturerDescription)
        {
            if(_manufacturers.ContainsKey(manufacturerDescription))
            {
                return _manufacturers[manufacturerDescription];
            }

            var manufacturer = new manufacturer();
            if (string.IsNullOrEmpty(manufacturerDescription))
            {
                manufacturer = _db.manufacturers.Where(x => (x.domain_id == 1 ||
                    (_showAudaxwareInfo == true && x.domain_id == 1)) && x.manufacturer_description == "TBD").FirstOrDefault();
                _manufacturers.Add(manufacturerDescription, manufacturer);
                return manufacturer;
            }

            manufacturer = _db.manufacturers.Where(x => x.domain_id == _domainId && x.manufacturer_description == manufacturerDescription).FirstOrDefault();
            if (manufacturer == null)
            {
                manufacturer = new manufacturer();
                manufacturer.domain_id = _domainId;
                manufacturer.manufacturer_description = manufacturerDescription;
                manufacturer.added_by = _user;
                manufacturer.date_added = DateTime.Now;

                _db.Entry(manufacturer).State = EntityState.Added;
                _db.SaveChanges();
            }
            _manufacturers.Add(manufacturerDescription, manufacturer);
            return manufacturer;
        }

        public ImportAnalysisResult Import(ImportItem[] items, int domainId, int projectId, string user, NotificationRepository notificationRepo)
        {
             Init(domainId, projectId);
            _user = user;
            var aux = new List<ImportItem>();
            _analysisResult.Items = aux;
            _analysisResult.Items.AddRange(items);
            SetResult(ImportAnalysisResultStatus.Ok, "");
            int exceptionItemIndex = -1;
            if (!_db.projects.Any(x => x.domain_id == domainId && x.project_id == projectId))
            {
                Trace.TraceError($"Call to import project to invalid project id (domain: {domainId}, project id: {projectId}");
                SetResult(ImportAnalysisResultStatus.Invalid, StringMessages.ImportInvalidDomainProject);
                return _analysisResult;
            }

            if (items.Any(x => x.HasId))
            {
                LoadProjectInventory();
            }

            LoadProjectCostCenter();
            LoadProjectTemporaryLocations();
            LoadProjectFinalDispositions();
            var lastNotification = DateTime.Now;

            using (var _dbTransaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _lastAssetsCodes = _db.get_max_asset_codes(domainId).ToList();

                    for (int i=0; i < items.Length; ++i)
                    {
                        exceptionItemIndex = i;
                        var item = items[i];
                        ValidateItem(item);
                        if (item.Status == ImportItemStatus.Error)
                        {
                            Trace.TraceError($"Item is invalid, import has been cancelled: {item.StatusComment}");
                            SetResult(ImportAnalysisResultStatus.Invalid, string.Format(StringMessages.ImportInvalidItemWithIndex, i));
                            return _analysisResult;
                        }
                        var jsn = GetJSN(item);
                        var manufacturer = GetManufacturer(item.Manufacturer);
                        var asset = GetAsset(item, jsn, manufacturer);
                        if (asset == null)
                        {
                            SetResult(ImportAnalysisResultStatus.InternalError, string.Format(StringMessages.ImportNoAssetReturnedWithIndex, i));
                            Trace.TraceError($"Could not import item, asset returned null {item.CatalogKey}");
                            return _analysisResult;
                        }

                        //change inventory
                        if (item.Status == ImportItemStatus.Changed)
                            UpdateInventory(asset, item, manufacturer);
                        else
                            CreateInventory(asset, item, manufacturer);
                        var elapsedTime = DateTime.Now - lastNotification;
                        if (elapsedTime.TotalMinutes > 10)
                        {
                            lastNotification = DateTime.Now;
                            if (notificationRepo != null)
                            {
                                var msg = string.Format(StringMessages.ImportStatusReport, i, items.Length);
                                notificationRepo.Notify(msg);
                            }
                        }
                    }

                    _db.SaveChanges();
                    _dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    TraceException(ex);
                    _dbTransaction.Rollback();
                    SetResult(ImportAnalysisResultStatus.InternalError, string.Format(StringMessages.ImportExceptionWithExceptionIndex, exceptionItemIndex));
                }
            }
            return _analysisResult;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (_db != null)
                    {
                        _db.Dispose();
                    }
                    if (_worksheet != null)
                    {
                        _worksheet.Dispose();
                    }
                    if (_xlPackage != null)
                    {
                        _xlPackage.Dispose();
                    }
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
