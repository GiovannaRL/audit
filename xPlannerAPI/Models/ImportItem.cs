namespace xPlannerAPI.Models
{
    public enum ImportItemStatus { Error, New, NewCatalog, Changed, NewCatalogWarning};
    public class ImportItem
    {
        public int? Id { get; set; }
        public ImportItemStatus Status { get; set; }
        public string StatusComment { get; set; }
        public string Code { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string ModelNumber { get; set; }
        public string ModelName { get; set; }
        public string JSN { get; set; }
        public string JSNNoSuffix { get; set; }
        public string JSNSuffix { get; set; }
        public string JSNNomeclature { get; set; }
        public int PlannedQty { get; set; }
        public string Class { get; set; }
        public string Clin { get; set; }
        public decimal? UnitBudget { get; set; }
        public string Phase { get; set; }
        public string Department { get; set; }
        public string RoomNumber { get; set; }
        public string RoomName { get; set; }
        public string Resp { get; set; }
        public string U1 { get; set; }
        public string U2 { get; set; }
        public string U3 { get; set; }
        public string U4 { get; set; }
        public string U5 { get; set; }
        public string U6 { get; set; }
        public decimal? UnitMarkup { get; set; }
        public decimal? UnitEscalation { get; set; }
        public decimal? UnitTax { get; set; }
        public decimal? UnitInstallNet { get; set; }
        public decimal? UnitInstallMarkup { get; set; }
        public decimal? UnitFreightNet { get; set; }
        public decimal? UnitFreightMarkup { get; set; }

        public string UnitOfMeasure { get; set; }
        public string[] MatchingCodes { get; set; }
        public string ECN { get; set; }

        public string CostCenter { get; set; }
        public string CostCenterDescription { get; set; }
        public string Tags { get; set; }
        public string Placement { get; set; }
        public bool? BiomedRequired { get; set; }
        public string Comment { get; set; }
        public string FunctionalArea { get; set; }
        public string Blueprint { get; set; }
        public string Staff { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Depth { get; set; }
        public string MountingHeight { get; set; }
        public decimal? RoomArea { get; set;  }
        public string RoomCode { get; set; }
        public string TemporaryLocation { get; set; }
        public string FinalDisposition { get; set; }
        public string CADID { get; set; }
        public string Worksheet { get; set; }
        public int? NetworkOption { get; set; }
        public string NetworkRequired { get; set; }

        internal bool HasId
        {
            get { return Id != null && Id > 0; }
        }
        internal string CatalogKey
        {
            get
            {
                if (string.IsNullOrEmpty(Code))
                {
                    return JSN + "\t" + Manufacturer + "\t" + ModelNumber + "\t" + ModelName + "\t" + Description;
                }
                else
                {
                    return Code;
                }
            }
        }

        public ImportItem()
        {
            UnitOfMeasure = "EA";
            StatusComment = "";
            Manufacturer = "";
            ModelName = "";
            ModelNumber = "";
            Department = "";
            RoomNumber = "";
            RoomName = "";
        }
        public void SetError(string description)
        {
            Status = ImportItemStatus.Error;
            StatusComment = description;
        }

        public void SetNewCatalogWarning(string description)
        {
            Status = ImportItemStatus.NewCatalogWarning;
            StatusComment = description;
        }

        public bool IsNewCatalog
        {
            get { return Status == ImportItemStatus.NewCatalog || Status == ImportItemStatus.NewCatalogWarning; }
        }
        
        public void SetStatus(ImportItemStatus status, string message)
        {
            // If we have id, then we are always changin an existing asset
            if (HasId && status != ImportItemStatus.Error)
            {
                Status = ImportItemStatus.Changed;
            }
            else
            {
                Status = status;
                StatusComment = message;
            }
        }

        public ImportItem Clone()
        {
            return (ImportItem)this.MemberwiseClone();
        }
    }
}
