using OfficeOpenXml;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace MigrateOldDatabase
{
    public partial class MillCreek : Form
    {
        public string sql_conn;
        public string exec_prod = "EXEC sp_set_session_context 'domain_id', '24'; ";


        public MillCreek()
        {
            InitializeComponent();
        }

        private void MillCreek_Load(object sender, EventArgs e)
        {
            database.DisplayMember = "Text";
            database.ValueMember = "Value";

            var items = new[] {
                    new { Text = "Local", Value = "1" },
                    new { Text = "Server DEV", Value = "2" },
                    new { Text = "Server PROD", Value = "3" }
                };

            database.DataSource = items;

        }

        private string validate_data(object data = null) {
            var new_data = "";

            if (data == null)
                return null;

            switch (data.ToString().Trim())
            {
                case "n/a":
                case ".":
                    new_data = null;
                    break;
                default:
                    new_data = data.ToString();
                    break;
            }

            return new_data;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Migrating...";


            if (database.SelectedValue.ToString() == "1")
            {
                sql_conn = @"Data Source=(LOCAL)\SQLExpress;Initial Catalog=audaxware;Integrated Security=True";
            }
            else if (database.SelectedValue.ToString() == "2")
            {
                sql_conn = @"Server = tcp:audaxware.database.windows.net,1433; Initial Catalog = audaxware_dev; Persist Security Info = False; User ID =juliana; Password =My_test_Jose1!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
            }
            else if (database.SelectedValue.ToString() == "3")
            {
                sql_conn = @"Server = tcp:audaxware.database.windows.net,1433; Initial Catalog = audaxware; Persist Security Info = False; User ID =juliana; Password =My_test_Jose1!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
            }


            var dbCon1 = new SqlConnection(sql_conn);
            var cmd1 = dbCon1.CreateCommand();
            SqlDataReader objDR1;

            var dbCon2 = new SqlConnection(sql_conn);
            var cmd2 = dbCon2.CreateCommand();
            SqlDataReader objDR2;


            dbCon1.Open();
            dbCon2.Open();


            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(@"C:\import\JSN Data.xlsx")))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["JSN"];

                int numRows = worksheet.Dimension.End.Row;

                while (numRows > 1)
                {
                    if (worksheet.Cells[numRows, 1].Value != null && !worksheet.Cells[numRows, 1].Value.ToString().Equals(""))
                    {
                        cmd1.Parameters.Clear();
                        cmd1.CommandText = exec_prod + " insert into jsn(jsn_code, nomenclature, mhs_cat, va_cat, fundsourmhs, fundsourva, amps1, amps2, archived, assigned_individual, btuperhour, cen_depth, cen_height, cen_width, comments, date_appr, description, freq_review, hertz, hertz_dep, hertz_swit, in_depth, inches_height, inches_width, last_modified, nsn, pending, phase1, phase2, unit_issue, utility1, utility2, utility3, utility4, utility5, utility6, utility7, volts, watts, wct_csi_link, weight_av, weight_met, year_review, domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17,@value18,@value19,@value20,@value21,@value22,@value23,@value24,@value25,@value26,@value27,@value28,@value29,@value30,@value31,@value32,@value33,@value34,@value35,@value36,@value37,@value38,@value39,@value40,@value41,@value42,@value43,@value44);";
                        cmd1.Parameters.AddWithValue("@value44", 24);
                        cmd1.Parameters.AddWithValue("@value1", worksheet.Cells[numRows, 1].Value);
                        cmd1.Parameters.AddWithValue("@value2", worksheet.Cells[numRows, 2].Value);
                        cmd1.Parameters.AddWithValue("@value3", (worksheet.Cells[numRows, 3].Value != null ? worksheet.Cells[numRows, 3].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value4", (worksheet.Cells[numRows, 4].Value != null ? worksheet.Cells[numRows, 4].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value5", (worksheet.Cells[numRows, 5].Value != null ? worksheet.Cells[numRows, 5].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value6", (worksheet.Cells[numRows, 6].Value != null ? worksheet.Cells[numRows, 6].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value7", (worksheet.Cells[numRows, 7].Value != null ? worksheet.Cells[numRows, 7].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value8", (worksheet.Cells[numRows, 8].Value != null ? worksheet.Cells[numRows, 8].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value9", (worksheet.Cells[numRows, 9].Value != null ? worksheet.Cells[numRows, 9].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value10", (worksheet.Cells[numRows, 10].Value != null ? worksheet.Cells[numRows, 10].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value11", (worksheet.Cells[numRows, 11].Value != null ? worksheet.Cells[numRows, 11].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value12", (worksheet.Cells[numRows, 12].Value != null ? worksheet.Cells[numRows, 12].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value13", (worksheet.Cells[numRows, 13].Value != null ? worksheet.Cells[numRows, 13].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value14", (worksheet.Cells[numRows, 14].Value != null ? worksheet.Cells[numRows, 14].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value15", (worksheet.Cells[numRows, 15].Value != null ? worksheet.Cells[numRows, 15].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value16", (worksheet.Cells[numRows, 16].Value != null ? worksheet.Cells[numRows, 16].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value17", (worksheet.Cells[numRows, 17].Value != null ? worksheet.Cells[numRows, 17].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value18", (worksheet.Cells[numRows, 18].Value != null ? worksheet.Cells[numRows, 18].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value19", (worksheet.Cells[numRows, 19].Value != null ? worksheet.Cells[numRows, 19].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value20", (worksheet.Cells[numRows, 20].Value != null ? worksheet.Cells[numRows, 20].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value21", (worksheet.Cells[numRows, 21].Value != null ? worksheet.Cells[numRows, 21].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value22", (worksheet.Cells[numRows, 22].Value != null ? worksheet.Cells[numRows, 22].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value23", (worksheet.Cells[numRows, 23].Value != null ? worksheet.Cells[numRows, 23].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value24", (worksheet.Cells[numRows, 24].Value != null ? worksheet.Cells[numRows, 24].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value25", (worksheet.Cells[numRows, 25].Value != null ? worksheet.Cells[numRows, 25].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value26", (worksheet.Cells[numRows, 26].Value != null ? worksheet.Cells[numRows, 26].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value27", (worksheet.Cells[numRows, 27].Value != null ? worksheet.Cells[numRows, 27].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value28", (worksheet.Cells[numRows, 28].Value != null ? worksheet.Cells[numRows, 28].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value29", (worksheet.Cells[numRows, 29].Value != null ? worksheet.Cells[numRows, 29].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value30", (worksheet.Cells[numRows, 30].Value != null ? worksheet.Cells[numRows, 30].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value31", (validate_data(worksheet.Cells[numRows, 31].Value) != null ? worksheet.Cells[numRows, 31].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value32", (validate_data(worksheet.Cells[numRows, 32].Value) != null ? worksheet.Cells[numRows, 32].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value33", (validate_data(worksheet.Cells[numRows, 33].Value) != null ? worksheet.Cells[numRows, 33].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value34", (validate_data(worksheet.Cells[numRows, 34].Value) != null ? worksheet.Cells[numRows, 34].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value35", (validate_data(worksheet.Cells[numRows, 35].Value) != null ? worksheet.Cells[numRows, 35].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value36", (validate_data(worksheet.Cells[numRows, 36].Value) != null ? worksheet.Cells[numRows, 36].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value37", (validate_data(worksheet.Cells[numRows, 37].Value) != null ? worksheet.Cells[numRows, 37].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value38", worksheet.Cells[numRows, 38].Value.ToString() + "/" + worksheet.Cells[numRows, 39].Value.ToString());
                        cmd1.Parameters.AddWithValue("@value39", worksheet.Cells[numRows, 40].Value.ToString() + "/" + worksheet.Cells[numRows, 41].Value.ToString());
                        cmd1.Parameters.AddWithValue("@value40", (worksheet.Cells[numRows, 42].Value != null ? worksheet.Cells[numRows, 42].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value41", (worksheet.Cells[numRows, 43].Value != null ? worksheet.Cells[numRows, 43].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value42", (worksheet.Cells[numRows, 44].Value != null ? worksheet.Cells[numRows, 44].Value : DBNull.Value));
                        cmd1.Parameters.AddWithValue("@value43", (worksheet.Cells[numRows, 45].Value != null ? worksheet.Cells[numRows, 45].Value : DBNull.Value));

                        cmd1.ExecuteNonQuery();
                    }
                    numRows--;
                }
            }


            dbCon1.Close();
            dbCon2.Close();

            label5.Text = "The end";

            button1.Text = "JSN Table";
        }
    }
}
