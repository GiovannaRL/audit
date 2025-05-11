using OfflineXPlanner.Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class ExportDataItemsErrorForm : Form
    {
        private IEnumerable<Inventory> items;

        public ExportDataItemsErrorForm(IEnumerable<Inventory> items)
        {
            InitializeComponent();
            this.items = items;
        }

        private void ExportDataItemsErrorForm_Load(object sender, System.EventArgs e)
        {
            this.itemsErrorGridView.DataSource = this.items.Select(i => new {
                i.Status,
                i.Code,
                i.JSN,
                i.Resp,
                Comment = i.StatusComment,
                i.Manufacturer,
                i.ModelNumber,
                i.ModelName,
                i.Phase,
                i.Department,
                Room = string.IsNullOrEmpty(i.RoomNumber) ? i.RoomName : (i.RoomNumber + (string.IsNullOrEmpty(i.RoomName) ? "" : $" - {i.RoomName}")),
                i.U1,
                i.U2,
                i.U3,
                i.U4,
                i.U5,
                i.U6
            }).ToList();
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
