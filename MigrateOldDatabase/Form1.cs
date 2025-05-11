using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Npgsql;
using System.Data.SqlClient;
using System.IO;
using OfficeOpenXml;
using xPlannerAPI.Security.Controller;

namespace MigrateOldDatabase
{
    public partial class Form1 : Form
    {
        public string postgres_conn;
        public string sql_conn;
        public string exec_prod;
        public string exec_prod1 = "EXEC sp_set_session_context 'domain_id', '1'; ";


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
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

        private void FillDomains()
        {
            domains.Items.Clear();

            if (database.SelectedValue.ToString() == "1")
            {
                postgres_conn = "User Id=postgres;server=localhost;Database=audaxware;Pooling=false;Password=postgres;";
                sql_conn = @"Data Source=(LOCAL)\SQLExpress;Initial Catalog=audaxware;Integrated Security=True";
                exec_prod = "";
            }
            else if (database.SelectedValue.ToString() == "2")
            {
                postgres_conn = "User Id=postgres;server=localhost;Database=audaxware_migration;Pooling=false;Password=postgres;";
                sql_conn = @"Server = tcp:audaxware.database.windows.net,1433; Initial Catalog = audaxware_dev; Persist Security Info = False; User ID =juliana; Password =My_test_Jose1!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
                exec_prod = "1";
            }
            else if (database.SelectedValue.ToString() == "3")
            {
                postgres_conn = "User Id=postgres;server=localhost;Database=audaxware_migration;Pooling=false;Password=postgres;";
                sql_conn = @"Server = tcp:audaxware.database.windows.net,1433; Initial Catalog = audaxware; Persist Security Info = False; User ID =juliana; Password =My_test_Jose1!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
                exec_prod = "1";
            }

            domains.DisplayMember = "Text";
            domains.ValueMember = "Value";

            var dell_db = new NpgsqlConnection(postgres_conn);
            dell_db.Open();

            var cmd = dell_db.CreateCommand();
            cmd.CommandText = "select * from domain order by domain_id";
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                domains.Items.Add(new { Text = reader["domain"].ToString(), Value = reader["domain_id"].ToString() });
            }

            reader.Close();
            dell_db.Dispose();

            domains.SelectedValue = 1;
        }

        private void FillProjects()
        {
            projects.Items.Clear();

            projects.DisplayMember = "Text";
            projects.ValueMember = "Value";


            var domain = (domains.SelectedItem as dynamic).Value;

            var dell_db = new NpgsqlConnection(postgres_conn);
            dell_db.Open();

            var cmd = dell_db.CreateCommand();
            cmd.CommandText = "select project_id, project_description from project where domain_id = '" + domain + "' order by project_description ";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                projects.Items.Add(new { Text = reader["project_description"].ToString(), Value = reader["project_id"].ToString() });
            }

            reader.Close();
            dell_db.Dispose();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Working...";

            var domain_id = (domains.SelectedItem as dynamic).Value;
            if (string.IsNullOrEmpty(domain_id))
            {
                label4.Text = "Choose a domain";
            }
            else
            {

                label5.Text = "Starting migration...";
                label5.Refresh();


                var dbCon = new NpgsqlConnection(postgres_conn);
                var cmd = dbCon.CreateCommand();
                NpgsqlDataReader objDR;

                var dbCon5 = new NpgsqlConnection(postgres_conn);
                var cmd5 = dbCon5.CreateCommand();
                NpgsqlDataReader objDR5;

                var dbCon4 = new NpgsqlConnection(postgres_conn);
                var cmd4 = dbCon4.CreateCommand();
                NpgsqlDataReader objDR4;

                var dbCon2 = new SqlConnection(sql_conn);
                var cmd2 = dbCon2.CreateCommand();
                SqlDataReader objDR2;
                Int32 total = 0;

                var dbCon3 = new SqlConnection(sql_conn);
                var cmd3 = dbCon3.CreateCommand();
                SqlDataReader objDR3;


                dbCon.Open();
                dbCon2.Open();
                dbCon3.Open();
                dbCon4.Open();
                dbCon5.Open();

                //if (exec_prod == "1")
                //{
                exec_prod = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "'; ";
                //}

                //SaveMsg("INICIO domain")
                label5.Text = label5.Text + "\rStarting domain";
                label5.Refresh();

                cmd.CommandText = "select * from domain where domain_id = " + domain_id;
                objDR = cmd.ExecuteReader();

                while (objDR.Read())
                {
                    cmd2.CommandText = exec_prod1 + " select count(*) as total from domain where domain_id = " + objDR["domain_id"];
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    total = (Int32)objDR2["total"];
                    objDR2.Close();

                    if (total == 0)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod1 + " insert into domain(domain_id, name, created_at, show_audax_info) values(@domain_id, @domain, @created_at, @show_audax_info);";
                        cmd2.Parameters.AddWithValue("@domain_id", objDR["domain_id"]);
                        cmd2.Parameters.AddWithValue("@domain", objDR["domain"]);
                        cmd2.Parameters.AddWithValue("@created_at", objDR["created_at"]);
                        cmd2.Parameters.AddWithValue("@show_audax_info", objDR["show_audax_info"]);
                        cmd2.ExecuteNonQuery();
                    }
                }
                objDR.Close();

                //INICIO ROLES
                //TODO JULIANA: PERGUNTAR PRA CAMILA SE O ID É GERADO AUTOMATICO PORQUE A TABELA ESTÁ DIFERENTE EM RELACAO AO BANCO ANTIGO
                //label5.Text = label5.Text + "\nStarting Roles";
                //label5.Refresh();
                //cmd.CommandText = @"select * from ""Roles""";
                //objDR = cmd.ExecuteReader();

                //while (objDR.Read())
                //{
                //    cmd2.CommandText = exec_prod1 + " select count(*) as total from AspNetRoles where Name = '" + objDR["Rolename"].ToString() + "'";
                //    objDR2 = cmd2.ExecuteReader();
                //    objDR2.Read();
                //    total = (Int32)objDR2["total"];
                //    objDR2.Close();

                //    if (total == 0)
                //    {
                //        cmd2.Parameters.Clear();
                //        cmd2.CommandText = exec_prod1 + " insert into AspNetRoles(Id, Name) values(@id, @name);";
                //        cmd2.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
                //        cmd2.Parameters.AddWithValue("@name", objDR["Rolename"]);
                //        cmd2.ExecuteNonQuery();
                //    }
                //}
                //objDR.Close();

                //INICIO SESSIONS
                //label5.Text = label5.Text + "\rStarting Sessions";
                //label5.Refresh();
                //cmd.CommandText = @"select * from ""Sessions""";
                //objDR = cmd.ExecuteReader();

                //while (objDR.Read())
                //{
                //    cmd2.CommandText = exec_prod1 + " select count(*) as total from Sessions where SessionId = '" + objDR["SessionId"].ToString() + "'";
                //    objDR2 = cmd2.ExecuteReader();
                //    objDR2.Read();
                //    total = (Int32)objDR2["total"];
                //    objDR2.Close();

                //    if (total == 0)
                //    {
                //        cmd2.Parameters.Clear();
                //        cmd2.CommandText = exec_prod1 + " insert into Sessions(SessionId, ApplicationName,Created,Expires,Timeout,Locked,LockId,LockDate,Data,Flags) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10);";
                //        cmd2.Parameters.AddWithValue("@value1", objDR["SessionId"]);
                //        cmd2.Parameters.AddWithValue("@value2", objDR["ApplicationName"]);
                //        cmd2.Parameters.AddWithValue("@value3", objDR["Created"]);
                //        cmd2.Parameters.AddWithValue("@value4", objDR["Expires"]);
                //        cmd2.Parameters.AddWithValue("@value5", objDR["Timeout"]);
                //        cmd2.Parameters.AddWithValue("@value6", objDR["Locked"]);
                //        cmd2.Parameters.AddWithValue("@value7", objDR["LockId"]);
                //        cmd2.Parameters.AddWithValue("@value8", objDR["LockDate"]);
                //        cmd2.Parameters.AddWithValue("@value9", objDR["Data"]);
                //        cmd2.Parameters.AddWithValue("@value10", objDR["Flags"]);
                //        cmd2.ExecuteNonQuery();
                //    }
                //}
                //objDR.Close();






                //INICIO USERS
                //TODO: criar uma funcao baseada na SetPassword no AccountController para preencher o campo PasswordHash
                //    public void SetPassword_tmp(string senha, string user_id)
                //{
                //    UserManager.AddPassword(user_id, senha);
                //}
                //TODO: TBM É NECESSARIO PREENCHER O CAMPO SecurityStamp
                //update AspNetUsers set PasswordHash = 'ABtJAN1WR6lA3QjhPqaI54qgD1A2UvUHwA2XxYnrPRUDsoLXMZfkhzeH6IV6ISVvjw==', SecurityStamp = 'cbfb4a4e-33ea-465f-840c-2b050df21c43'
                //where UserName = 'juliana.barros@audaxware.com'
                label5.Text = label5.Text + "\rStarting Users";
                label5.Refresh();
                cmd.CommandText = @"select distinct u.* from ""Users"" u, domain d where (split_part(""Username"", '@', 2) = d.domain and d.domain_id =" + domain_id + ") or u.domain_id = " + domain_id;
                objDR = cmd.ExecuteReader();
                var user_ids = new List<string>();
                while (objDR.Read())
                {
                    user_ids.Add(objDR["pId"].ToString());
                    cmd2.CommandText = exec_prod1 + " select count(*) as total from AspNetUsers where UserName = '" + objDR["UserName"].ToString() + "'";
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    total = (Int32)objDR2["total"];
                    objDR2.Close();

                    if (total == 0)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod1 + " insert into AspNetUsers(Id,Username,Email,Comment,LastActivityDate,LastLoginDate,CreationDate,IsOnLine,LockoutEnabled,LockoutEndDateUtc,IsPasswordTemporary,accept_user_license,first_name,last_name,domain_id) values(@value1, @value2, @value4, @value5, @value10, @value11, @value13, @value14, @value15, @value16, @value21, @value22, @value23, @value24, @value25);";
                        cmd2.Parameters.AddWithValue("@value1", objDR["pId"]);
                        cmd2.Parameters.AddWithValue("@value2", objDR["Username"]);
                        cmd2.Parameters.AddWithValue("@value4", objDR["Email"]);
                        cmd2.Parameters.AddWithValue("@value5", objDR["Comment"]);
                        cmd2.Parameters.AddWithValue("@value10", objDR["LastActivityDate"]);
                        cmd2.Parameters.AddWithValue("@value11", objDR["LastLoginDate"]);
                        cmd2.Parameters.AddWithValue("@value13", objDR["CreationDate"]);
                        cmd2.Parameters.AddWithValue("@value14", objDR["IsOnLine"]);
                        cmd2.Parameters.AddWithValue("@value15", objDR["IsLockedOut"]);
                        cmd2.Parameters.AddWithValue("@value16", objDR["LastLockedOutDate"]);
                        cmd2.Parameters.AddWithValue("@value21", objDR["IsPasswordTemporary"]);
                        cmd2.Parameters.AddWithValue("@value22", objDR["accept_user_license"]);
                        cmd2.Parameters.AddWithValue("@value23", objDR["first_name"]);
                        cmd2.Parameters.AddWithValue("@value24", objDR["last_name"]);
                        cmd2.Parameters.AddWithValue("@value25", objDR["domain_id"]);
                        cmd2.ExecuteNonQuery();

                        var accountController = new AccountController();
                        //accountController.SetPassword_tmp(objDR["Password"].ToString(), objDR["pId"].ToString());
                    }
                }
                objDR.Close();


                //INICIO USERSINROLES
                //TODO JULIANA: a principio tera outra estrutura, nao será necessario migrar, mas checar
                //cmd.CommandText = @"select * from ""UsersInRoles""";
                //objDR = cmd.ExecuteReader();

                //while (objDR.Read())
                //{
                //    cmd2.CommandText = exec_prod1 + " select count(*) as total from AspNetUserRoles where Username = '" + objDR["Username"].ToString() + "'";
                //    objDR2 = cmd2.ExecuteReader();
                //    objDR2.Read();
                //    total = (Int32)objDR2["total"];
                //    objDR2.Close();

                //    if (total == 0)
                //    {
                //        cmd2.Parameters.Clear();
                //        cmd2.CommandText = exec_prod1 + " insert into AspNetUserRoles(Username,Rolename,ApplicationName) values(@value1, @value2,@value3);";
                //        cmd2.Parameters.AddWithValue("@value1", objDR["Username"]);
                //        cmd2.Parameters.AddWithValue("@value2", objDR["Rolename"]);
                //        cmd2.Parameters.AddWithValue("@value3", objDR["ApplicationName"]);
                //        cmd2.ExecuteNonQuery();
                //    }
                //}

                //INICIO ASPNETUSERCLAIMS
                cmd.CommandText = @"select * from ""Users""  u where u.domain_id =" + domain_id;
                objDR = cmd.ExecuteReader();

                while (objDR.Read())
                {
                    cmd2.CommandText = exec_prod1 + " select count(*) as total from AspNetUserClaims where UserId = '" + objDR["pId"].ToString() + "'";
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    total = (Int32)objDR2["total"];
                    objDR2.Close();

                    if (total == 0)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod1 + " INSERT INTO AspNetUserClaims(UserId, ClaimType, ClaimValue) SELECT Id, 'Enterprise', d.domain_id FROM AspNetUsers as u, domain as d WHERE SUBSTRING(Username, CHARINDEX('@', Email)+1, 256) = name and Username = '" + objDR["Username"].ToString() + "'";
                        cmd2.ExecuteNonQuery();
                    }
                }
                objDR.Close();

                //INICIO role_pages
                //cmd.CommandText = @"select * from role_pages";
                //objDR = cmd.ExecuteReader();

                //while (objDR.Read())
                //{
                //    cmd2.CommandText = exec_prod1 + " select count(*) as total from role_pages where role_name = '" + objDR["role_name"].ToString() + "' and page = '" + objDR["page"].ToString() + "' and level = '" + objDR["level"].ToString() + "'";
                //    objDR2 = cmd2.ExecuteReader();
                //    objDR2.Read();
                //    total = (Int32)objDR2["total"];
                //    objDR2.Close();

                //    if (total == 0)
                //    {
                //        cmd2.Parameters.Clear();
                //        cmd2.CommandText = exec_prod1 + " insert into role_pages(role_name,page,level) values(@value1, @value2, @value3);";
                //        cmd2.Parameters.AddWithValue("@value1", objDR["role_name"]);
                //        cmd2.Parameters.AddWithValue("@value2", objDR["page"]);
                //        cmd2.Parameters.AddWithValue("@value3", objDR["level"]);
                //        cmd2.ExecuteNonQuery();
                //    }
                //}

                //objDR.Close();


                if (equipment_code.Checked)
                {



                    //INICIO ASSET_CODES
                    label5.Text = label5.Text + "\r Starting equipment_codes";
                    label5.Refresh();
                    cmd.CommandText = "select * from equipment_codes where domain_id =" + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_codes where prefix = '" + objDR["prefix"].ToString() + "' and domain_id = " + objDR["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into assets_codes(prefix,description,next_seq,domain_id) values(@value1, @value2, @value3,@value4);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["prefix"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["next_seq"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["domain_id"]);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();
                }


                if (categories.Checked)
                {



                    //INICIO ASSETS_CATEGORY
                    label5.Text = label5.Text + "\r Starting equipment_category";
                    label5.Refresh();
                    cmd.CommandText = "select * from equipment_category where domain_id =" + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_category where category_id = '" + objDR["category_id"].ToString() + "' and domain_id = " + objDR["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets_category ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into assets_category(category_id, description, domain_id) values(@value1, @value2, @value3);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["category_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["domain_id"]);
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets_category OFF;";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //INICIO ASSETS_SUBCATEGORY
                    label5.Text = label5.Text + "\r Starting equipment_subcategory";
                    label5.Refresh();
                    cmd.CommandText = "select * from equipment_subcategory where domain_id =" + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_subcategory where category_id = " + objDR["category_id"].ToString() + " and subcategory_id = " + objDR["subcategory_id"].ToString() + " and domain_id = " + objDR["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets_subcategory ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into assets_subcategory(subcategory_id, category_id, description, domain_id, category_domain_id, use_category_settings) values(@value1, @value2, @value3,@value4, @value5,@value6);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["subcategory_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["category_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value6", 0);
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets_subcategory OFF;";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();
                }

                if (manufacturer.Checked)
                {



                    //INICIO MANUFACTURER
                    label5.Text = label5.Text + "\r Starting manufacturer";
                    label5.Refresh();
                    cmd.CommandText = "select * from manufacturer where domain_id =" + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from manufacturer where manufacturer_id = " + objDR["manufacturer_id"].ToString() + " and domain_id = " + objDR["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT manufacturer ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into manufacturer(manufacturer_id,manufacturer_description, date_added, added_by, comment, domain_id) values(@value1, @value2, @value3,@value4,@value5,@value6);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["manufacturer_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["manufacturer_description"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["domain_id"]);
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT manufacturer OFF;";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //INICIO VENDOR
                    label5.Text = label5.Text + "\r Starting vendor";
                    label5.Refresh();
                    cmd.CommandText = "select * from vendor where domain_id =" + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from vendor where vendor_id = " + objDR["vendor_id"].ToString() + " and domain_id = " + objDR["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT vendor ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into vendor(vendor_id,name,territory,hospitals,date_added,added_by,comment,domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["vendor_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["territory"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["hospitals"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["domain_id"]);
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT vendor OFF;";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                }

                //INICIO assets_measurement
                label5.Text = label5.Text + "\r Starting equipment_measurement";
                label5.Refresh();
                cmd.CommandText = "select * from equipment_measurement";
                objDR = cmd.ExecuteReader();

                while (objDR.Read())
                {
                    cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_measurement where eq_unit_measure_id = " + objDR["eq_unit_measure_id"];
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    total = (Int32)objDR2["total"];
                    objDR2.Close();

                    if (total == 0)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod1 + " insert into assets_measurement(eq_unit_measure_id,eq_unit_desc) values(@value1, @value2);";
                        cmd2.Parameters.AddWithValue("@value1", objDR["eq_unit_measure_id"]);
                        cmd2.Parameters.AddWithValue("@value2", objDR["eq_unit_desc"]);
                        cmd2.ExecuteNonQuery();
                    }
                }
                objDR.Close();

                if (chkassets.Checked)
                {



                    //INICIO ASSETS
                    label5.Text = label5.Text + "\r Starting assets";
                    label5.Refresh();
                    //cmd.CommandText = "select equipment_id,equipment_code,manufacturer_id,equipment_desc,subcategory_id,height,width,depth,weight,serial_number,min_cost,max_cost,avg_cost,last_cost,default_resp,cut_sheet,date_added,added_by,cad_block,water,plumbing,data,electrical,mobile,blocking,medgas,supports,discontinued,last_budget_update,photo,eq_measurement_id,water_option,plumbing_option,data_option,electrical_option,mobile_option,blocking_option,medgas_option,supports_option,revit,placement,clearance_left,clearance_right,clearance_front,clearance_back,clearance_top,clearance_bottom,volts,phases,hertz,amps,volt_amps,watts,cfm,btus,misc_ase,misc_ada,misc_seismic,misc_antimicrobial,misc_ecolabel,misc_ecolabel_desc,mapping_code,medgas_oxygen,medgas_nitrogen,medgas_air,medgas_n2o,medgas_vacuum,medgas_wag,medgas_co2,medgas_other,medgas_steam,medgas_natgas,plu_hot_water,plu_drain,plu_cold_water,plu_return,plu_treated_water,plu_relief,plu_chilled_water,serial_name,useful_life,loaded_weight,ship_weight,alternate_equip,updated_at,domain_id,manufacturer_domain_id,no_options,no_colors from equipment where domain_id =" + domains.SelectedItem + " order by equipment_id";
                    cmd4.CommandText = "select equipment_id, domain_id, equipment_code from equipment where domain_id =" + domain_id + " order by equipment_id";
                    cmd4.CommandTimeout = 40;
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        var asset_code = objDR4["equipment_code"].ToString();
                        if (domain_id.ToString() == "1")
                            asset_code = asset_code.Substring(0, 3) + "0" + asset_code.Substring(3, asset_code.Length - 3);
                        else if (domain_id.ToString() != "1" && asset_code.Substring(asset_code.Length - 1, 1) == "C")
                            asset_code = asset_code.Substring(0, 3) + "0" + asset_code.Substring(3, asset_code.Length - 3);
                        else
                            asset_code = asset_code.Substring(0, 3) + (9999 + (10000 - Convert.ToInt32(asset_code.Substring(3, asset_code.Length - 3)))).ToString();


                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets where (asset_id = " + objDR4["equipment_id"].ToString() + ") and domain_id = " + objDR4["domain_id"].ToString();
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd.CommandText = "select case when s.description = c.description then c.description || ', ' || e.equipment_desc else c.description || ', ' || s.description || ', ' || e.equipment_desc end description, s.description, c.description, e.* from equipment e inner join equipment_subcategory s on e.subcategory_id = s.subcategory_id and s.domain_id = e.domain_id inner join equipment_category c on c.category_id = s.category_id and c.domain_id = e.domain_id where e.equipment_id =" + objDR4["equipment_id"].ToString() + " and e.domain_id = " + objDR4["domain_id"].ToString();
                            cmd.CommandTimeout = 40;
                            objDR = cmd.ExecuteReader();
                            objDR.Read();

                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into assets(asset_id,asset_code,manufacturer_id,asset_description,subcategory_id,height,width,depth,weight,serial_number,min_cost,max_cost,avg_cost,last_cost,default_resp,cut_sheet,date_added,added_by,comment,cad_block,water,plumbing,data,electrical,mobile,blocking,medgas,supports,discontinued,last_budget_update,photo,eq_measurement_id,water_option,plumbing_option,data_option,electrical_option,mobile_option,blocking_option,medgas_option,supports_option,revit,placement,clearance_left,clearance_right,clearance_front,clearance_back,clearance_top,clearance_bottom,volts,phases,hertz,amps,volt_amps,watts,cfm,btus,misc_ase,misc_ada,misc_seismic,misc_antimicrobial,misc_ecolabel,misc_ecolabel_desc,mapping_code,medgas_oxygen,medgas_nitrogen,medgas_air,medgas_n2o,medgas_vacuum,medgas_wag,medgas_co2,medgas_other,medgas_steam,medgas_natgas,plu_hot_water,plu_drain,plu_cold_water,plu_return,plu_treated_water,plu_relief,plu_chilled_water,serial_name,useful_life,loaded_weight,ship_weight,alternate_asset,updated_at,domain_id,manufacturer_domain_id,no_options,no_colors, subcategory_domain_id, asset_suffix) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17,@value18,@value19,@value20,@value21,@value22,@value23,@value24,@value25,@value26,@value27,@value28,@value29,@value30,@value31,@value32,@value33,@value34,@value35,@value36,@value37,@value38,@value39,@value40,@value41,@value42,@value43,@value44,@value45,@value46,@value47,@value48,@value49,@value50,@value51,@value52,@value53,@value54,@value55,@value56,@value57,@value58,@value59,@value60,@value61,@value62,@value63,@value64,@value65,@value66,@value67,@value68,@value69,@value70,@value71,@value72,@value73,@value74,@value75,@value76,@value77,@value78,@value79,@value80,@value81,@value82,@value83,@value84,@value85,@value86,@value87,@value88,@value89,@value90,@value91, @value92);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value2", asset_code);
                            cmd2.Parameters.AddWithValue("@value3", objDR["manufacturer_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["subcategory_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["height"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["width"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["depth"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["weight"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["serial_number"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["min_cost"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["max_cost"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["avg_cost"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["last_cost"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["default_resp"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["cut_sheet"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value18", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value19", objDR["comment"].ToString());
                            cmd2.Parameters.AddWithValue("@value20", objDR["cad_block"]);
                            cmd2.Parameters.AddWithValue("@value21", objDR["water"]);
                            cmd2.Parameters.AddWithValue("@value22", objDR["plumbing"]);
                            cmd2.Parameters.AddWithValue("@value23", objDR["data"]);
                            cmd2.Parameters.AddWithValue("@value24", objDR["electrical"]);
                            cmd2.Parameters.AddWithValue("@value25", objDR["mobile"]);
                            cmd2.Parameters.AddWithValue("@value26", objDR["blocking"]);
                            cmd2.Parameters.AddWithValue("@value27", objDR["medgas"]);
                            cmd2.Parameters.AddWithValue("@value28", objDR["supports"]);

                            if (objDR["discontinued"].ToString() == "Y")
                                cmd2.Parameters.AddWithValue("@value29", 1);
                            else
                                cmd2.Parameters.AddWithValue("@value29", objDR["discontinued"]);
                            cmd2.Parameters.AddWithValue("@value30", objDR["last_budget_update"]);
                            cmd2.Parameters.AddWithValue("@value31", objDR["photo"]);
                            cmd2.Parameters.AddWithValue("@value32", objDR["eq_measurement_id"]);
                            cmd2.Parameters.AddWithValue("@value33", objDR["water_option"]);
                            cmd2.Parameters.AddWithValue("@value34", objDR["plumbing_option"]);
                            cmd2.Parameters.AddWithValue("@value35", objDR["data_option"]);
                            cmd2.Parameters.AddWithValue("@value36", objDR["electrical_option"]);
                            cmd2.Parameters.AddWithValue("@value37", objDR["mobile_option"]);
                            cmd2.Parameters.AddWithValue("@value38", objDR["blocking_option"]);
                            cmd2.Parameters.AddWithValue("@value39", objDR["medgas_option"]);
                            cmd2.Parameters.AddWithValue("@value40", objDR["supports_option"]);
                            cmd2.Parameters.AddWithValue("@value41", objDR["revit"]);
                            cmd2.Parameters.AddWithValue("@value42", objDR["placement"]);
                            cmd2.Parameters.AddWithValue("@value43", objDR["clearance_left"]);
                            cmd2.Parameters.AddWithValue("@value44", objDR["clearance_right"]);
                            cmd2.Parameters.AddWithValue("@value45", objDR["clearance_front"]);
                            cmd2.Parameters.AddWithValue("@value46", objDR["clearance_back"]);
                            cmd2.Parameters.AddWithValue("@value47", objDR["clearance_top"]);
                            cmd2.Parameters.AddWithValue("@value48", objDR["clearance_bottom"]);
                            cmd2.Parameters.AddWithValue("@value49", objDR["volts"]);
                            cmd2.Parameters.AddWithValue("@value50", objDR["phases"]);
                            cmd2.Parameters.AddWithValue("@value51", objDR["hertz"]);
                            cmd2.Parameters.AddWithValue("@value52", objDR["amps"]);
                            cmd2.Parameters.AddWithValue("@value53", objDR["volt_amps"]);
                            cmd2.Parameters.AddWithValue("@value54", objDR["watts"]);
                            cmd2.Parameters.AddWithValue("@value55", objDR["cfm"]);
                            cmd2.Parameters.AddWithValue("@value56", objDR["btus"]);
                            cmd2.Parameters.AddWithValue("@value57", objDR["misc_ase"]);
                            cmd2.Parameters.AddWithValue("@value58", objDR["misc_ada"]);
                            cmd2.Parameters.AddWithValue("@value59", objDR["misc_seismic"]);
                            cmd2.Parameters.AddWithValue("@value60", objDR["misc_antimicrobial"]);
                            cmd2.Parameters.AddWithValue("@value61", objDR["misc_ecolabel"]);
                            cmd2.Parameters.AddWithValue("@value62", objDR["misc_ecolabel_desc"]);
                            cmd2.Parameters.AddWithValue("@value63", objDR["mapping_code"]);
                            cmd2.Parameters.AddWithValue("@value64", objDR["medgas_oxygen"]);
                            cmd2.Parameters.AddWithValue("@value65", objDR["medgas_nitrogen"]);
                            cmd2.Parameters.AddWithValue("@value66", objDR["medgas_air"]);
                            cmd2.Parameters.AddWithValue("@value67", objDR["medgas_n2o"]);
                            cmd2.Parameters.AddWithValue("@value68", objDR["medgas_vacuum"]);
                            cmd2.Parameters.AddWithValue("@value69", objDR["medgas_wag"]);
                            cmd2.Parameters.AddWithValue("@value70", objDR["medgas_co2"]);
                            cmd2.Parameters.AddWithValue("@value71", objDR["medgas_other"]);
                            cmd2.Parameters.AddWithValue("@value72", objDR["medgas_steam"]);
                            cmd2.Parameters.AddWithValue("@value73", objDR["medgas_natgas"]);
                            cmd2.Parameters.AddWithValue("@value74", objDR["plu_hot_water"]);
                            cmd2.Parameters.AddWithValue("@value75", objDR["plu_drain"]);
                            cmd2.Parameters.AddWithValue("@value76", objDR["plu_cold_water"]);
                            cmd2.Parameters.AddWithValue("@value77", objDR["plu_return"]);
                            cmd2.Parameters.AddWithValue("@value78", objDR["plu_treated_water"]);
                            cmd2.Parameters.AddWithValue("@value79", objDR["plu_relief"]);
                            cmd2.Parameters.AddWithValue("@value80", objDR["plu_chilled_water"]);
                            cmd2.Parameters.AddWithValue("@value81", objDR["serial_name"]);
                            cmd2.Parameters.AddWithValue("@value82", objDR["useful_life"]);
                            cmd2.Parameters.AddWithValue("@value83", objDR["loaded_weight"]);
                            cmd2.Parameters.AddWithValue("@value84", objDR["ship_weight"]);
                            cmd2.Parameters.AddWithValue("@value85", objDR["alternate_equip"]);
                            cmd2.Parameters.AddWithValue("@value86", objDR["updated_at"]);
                            cmd2.Parameters.AddWithValue("@value87", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value88", objDR["manufacturer_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value89", objDR["no_options"]);
                            cmd2.Parameters.AddWithValue("@value90", objDR["no_colors"]);
                            cmd2.Parameters.AddWithValue("@value91", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value92", objDR["equipment_desc"]);

                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT assets OFF;";
                            cmd2.ExecuteNonQuery();

                            objDR.Close();
                        }
                    }
                    objDR4.Close();
                }

                if (manufacturer.Checked)
                {


                    //INICIO vendor_contact
                    cmd.CommandText = "select * from vendor_contact where vendor_domain_id = " + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from vendor_contact where vendor_id = " + objDR["vendor_id"] + " and vendor_domain_id = " + objDR["vendor_domain_id"] + " and name ='" + objDR["name"].ToString() + "'";
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into vendor_contact(name,vendor_id,contact_type,title,email,address,city,state,zipcode,phone,fax,date_added,added_by,comment,mobile,vendor_domain_id,contact_domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["vendor_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["contact_type"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["title"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["email"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["address"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["city"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["state"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["zipcode"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["phone"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["fax"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["mobile"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["vendor_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["contact_domain_id"]);

                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //INICIO assets_vendor
                    cmd.CommandText = "select * from vendor_equipment where equipment_domain_id in(1," + domain_id + ") and vendor_domain_id in (1," + domain_id + ")";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        if (objDR["vendor_domain_id"].ToString() == objDR["equipment_domain_id"].ToString() && domain_id != "1")
                            total = 1;
                        if (total == 0)
                        {
                            cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_vendor where vendor_id = " + objDR["vendor_id"] + " and vendor_domain_id = " + objDR["vendor_domain_id"] + " and asset_id = " + objDR["equipment_id"] + " and asset_domain_id = " + objDR["equipment_domain_id"];
                            objDR2 = cmd2.ExecuteReader();
                            objDR2.Read();
                            total = (Int32)objDR2["total"];
                            objDR2.Close();
                        }
                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into assets_vendor(asset_id,vendor_id,min_cost,max_cost,avg_cost,date_added,added_by,comment,model_number,asset_domain_id,vendor_domain_id,last_cost) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["vendor_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["min_cost"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["max_cost"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["avg_cost"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["model_number"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["equipment_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["vendor_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["last_cost"]);

                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //inicio manufacturer_contact
                    cmd.CommandText = "select * from manufacturer_contact where manufacturer_domain_id = " + domain_id + " order by name";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from manufacturer_contact where manufacturer_id = " + objDR["manufacturer_id"] + " and manufacturer_domain_id = " + objDR["manufacturer_domain_id"] + " and name ='" + objDR["name"].ToString() + "'";
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into manufacturer_contact(name,manufacturer_id,contact_type,title,email,address,city,state,phone,fax,zipcode,date_added,added_by,comment,mobile,manufacturer_domain_id,contact_domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["manufacturer_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["contact_type"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["title"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["email"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["address"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["city"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["state"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["phone"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["fax"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["zipcode"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["mobile"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["manufacturer_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["contact_domain_id"]);

                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();
                }


                //if (chkassets.Checked)
                {


                    //INICIO related_assets
                    //cmd.CommandText = "select * from related_equipment where related_domain_id = " + domain_id;
                    //objDR = cmd.ExecuteReader();

                    //while (objDR.Read())
                    //{
                    //    cmd2.CommandText = exec_prod1 + " select count(*) as total from related_assets where asset_id = " + objDR["equipment_id"] + " and domain_id = " + objDR["domain_id"] + " and related_asset_id =" + objDR["related_equipment_id"] + " and related_domain_id =" + objDR["related_domain_id"];
                    //    objDR2 = cmd2.ExecuteReader();
                    //    objDR2.Read();
                    //    total = (Int32)objDR2["total"];
                    //    objDR2.Close();

                    //    if (total == 0)
                    //    {
                    //        cmd2.Parameters.Clear();
                    //        cmd2.CommandText = exec_prod1 + " insert into related_assets(asset_id,domain_id,related_asset_id, related_domain_id) values(@value1, @value2, @value3,@value4);";
                    //        cmd2.Parameters.AddWithValue("@value1", objDR["equipment_id"]);
                    //        cmd2.Parameters.AddWithValue("@value2", objDR["domain_id"]);
                    //        cmd2.Parameters.AddWithValue("@value3", objDR["related_equipment_id"]);
                    //        cmd2.Parameters.AddWithValue("@value4", objDR["related_domain_id"]);

                    //        cmd2.ExecuteNonQuery();
                    //    }
                    //}
                    //objDR.Close();

                    //INICIO ASSETS_OPTIONS
                    label5.Text = label5.Text + "\rStarting ASSETS_OPTIONS";
                    label5.Refresh();

                    cmd4.CommandText = "select equipment_options_id from equipment_options where domain_id = " + domain_id + " order by equipment_options_id";
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_options where data_type = 'A' and domain_id = " + domain_id + " and old_id = " + objDR4["equipment_options_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd.CommandText = " select a.*, case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id from equipment_options a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where a.equipment_options_id = " + objDR4["equipment_options_id"];
                            objDR = cmd.ExecuteReader();
                            objDR.Read();


                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into assets_options(old_id,asset_id,code,description,added_by,date_added,domain_id,data_type) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_options_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["asset_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["code"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["asset_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value8", "A");

                            cmd2.ExecuteNonQuery();

                            objDR.Close();
                        }
                    }
                    objDR4.Close();


                    //INICIO ASSETS_OPTIONS - OLD TABLE EQUIPMENT_COLORS
                    cmd.CommandText = "select a.*, case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id from equipment_colors a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where a.domain_id = " + domain_id + " order by a.equipment_colors_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_options where data_type = 'C' and domain_id = " + domain_id + " and old_id = " + objDR["equipment_colors_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into assets_options(old_id,asset_id,code,description,added_by,date_added,domain_id,data_type) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_colors_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["asset_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["code"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["asset_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value8", "C");

                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();
                }

                //INICIO lu_state
                cmd.CommandText = "select * from lu_state";
                objDR = cmd.ExecuteReader();

                while (objDR.Read())
                {
                    cmd2.CommandText = exec_prod1 + " select count(*) as total from lu_state where abrv = '" + objDR["abrv"] + "'";
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    total = (Int32)objDR2["total"];
                    objDR2.Close();

                    if (total == 0)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod1 + " insert into lu_state(state, abrv) values(@value1,@value2);";
                        cmd2.Parameters.AddWithValue("@value1", objDR["state"]);
                        cmd2.Parameters.AddWithValue("@value2", objDR["abrv"]);

                        cmd2.ExecuteNonQuery();

                    }
                }
                objDR.Close();

                foreach (var project in projects.CheckedItems)
                {
                    var selected_project = (project as dynamic).Value;


                    //INICIO CLIENT
                    label5.Text = label5.Text + "\r Starting client";
                    label5.Refresh();
                    cmd.CommandText = "select distinct client from project where domain_id = " + domain_id + " and project_id = " + selected_project;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from client where replace(name,'''','#1') = '" + objDR["client"].ToString().Replace("'", "#1") + "' and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into client(name, domain_id) values(@value1,@value2);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["client"]);
                            cmd2.Parameters.AddWithValue("@value2", domain_id.ToString());

                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();

                    //INICIO FACILITY
                    label5.Text = label5.Text + "\r Starting facility";
                    label5.Refresh();
                    cmd.CommandText = "select distinct hospital from project where domain_id = " + domain_id + " and project_id = " + selected_project;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = @" select count(*) as total from facility where replace(name, '''', '') = '" + objDR["hospital"].ToString().Replace("'", "") + "' and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " insert into facility(name, domain_id) values(@value1,@value2);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["hospital"].ToString());
                            cmd2.Parameters.AddWithValue("@value2", domain_id.ToString());

                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    var client_id = 0;
                    var facility_id = 0;
                    //INICIO PROJECT
                    label5.Text = label5.Text + "\r Starting project";
                    label5.Refresh();
                    cmd.CommandText = "select * from project where domain_id = " + domain_id + " and project_id = " + selected_project + " order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project where project_id = " + objDR["project_id"] + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.CommandText = exec_prod1 + " select id from client where replace(name, '''', '') = '" + objDR["client"].ToString().Replace("'", "") + "' and domain_id = " + domain_id;
                            objDR2 = cmd2.ExecuteReader();
                            objDR2.Read();
                            client_id = (Int32)objDR2["id"];
                            objDR2.Close();

                            cmd2.CommandText = exec_prod1 + " select id from facility where replace(name, '''', '') = '" + objDR["hospital"].ToString().Replace("'", "") + "' and domain_id = " + domain_id;
                            objDR2 = cmd2.ExecuteReader();
                            objDR2.Read();
                            facility_id = (Int32)objDR2["id"];
                            objDR2.Close();

                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT project ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into project(project_id,project_description,project_start,project_end,status,client_project_number,facility_project_number,hsg_project_number,address1,address2,city,state,zip,default_cost_field,medial_budget,freight_budget,warehouse_budget,tax_budget,warranty_budget,misc_budget,date_added,added_by,comment,domain_id, client_id, client_domain_id, facility_id, facility_domain_id) values(@value1, @value2, @value3, @value4, @value5, @value7, @value9, @value10, @value11, @value12, @value13, @value14, @value15, @value16, @value17, @value18, @value19, @value20, @value21, @value22, @value23, @value24, @value25,@value26,@value27,@value28,@value29,@value30);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["project_description"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["project_start"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["project_end"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["status"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["client_project_number"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["hospital_project_number"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["hsg_project_number"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["address1"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["address2"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["city"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["state"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["zip"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["default_cost_field"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["medial_budget"]);
                            cmd2.Parameters.AddWithValue("@value18", objDR["freight_budget"]);
                            cmd2.Parameters.AddWithValue("@value19", objDR["warehouse_budget"]);
                            cmd2.Parameters.AddWithValue("@value20", objDR["tax_budget"]);
                            cmd2.Parameters.AddWithValue("@value21", objDR["warranty_budget"]);
                            cmd2.Parameters.AddWithValue("@value22", objDR["misc_budget"]);
                            cmd2.Parameters.AddWithValue("@value23", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value24", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value25", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value26", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value27", client_id);
                            cmd2.Parameters.AddWithValue("@value28", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value29", facility_id);
                            cmd2.Parameters.AddWithValue("@value30", objDR["domain_id"]);

                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project OFF";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                    //INICIO DEPARTMENT_TYPE
                    label5.Text = label5.Text + "\r Starting department_type";
                    label5.Refresh();
                    cmd.CommandText = "select * from department_type where domain_id = " + domain_id + " order by department_type_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from department_type where department_type_id = " + objDR["department_type_id"] + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT department_type ON";
                            cmd2.CommandText = cmd2.CommandText + " insert into department_type(department_type_id,description,domain_id) values(@value1, @value2, @value3);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["department_type_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["domain_id"]);

                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT department_type OFF";
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //INICIO PROJECT_PHASE
                    label5.Text = label5.Text + "\r Starting project_phase";
                    label5.Refresh();
                    cmd.CommandText = "select * from project_phase where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id, phase_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_phase where project_id = " + objDR["project_id"] + " and phase_id = " + objDR["phase_id"] + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT project_phase ON;";

                            cmd2.CommandText = cmd2.CommandText + " insert into project_phase(project_id,phase_id,description,start_date,end_date,plan_end,plan_start,sd_date,dd_date,cd_date,equip_move_in_date,occupancy_date,date_added,added_by,comment,ofci_delivery,domain_id) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10, @value11, @value12, @value13, @value14, @value15, @value16,@value17);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["start_date"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["end_date"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["plan_end"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["plan_start"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["sd_date"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["dd_date"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["cd_date"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["equip_move_in_date"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["occupancy_date"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["ofci_delivery"]);
                            cmd2.Parameters.AddWithValue("@value17", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_phase OFF";
                            cmd2.ExecuteNonQuery();


                        }
                    }
                    objDR.Close();



                    //INICIO PROJECT_DEPARTMENT
                    label5.Text = label5.Text + "\r Starting project_department";
                    label5.Refresh();
                    cmd4.CommandText = "select project_id, department_id, phase_id from project_department where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id, phase_id";
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_department where project_id = " + objDR4["project_id"] + " and department_id = " + objDR4["department_id"] + " and phase_id = " + objDR4["phase_id"] + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd.CommandText = " select * from project_department where project_id = " + objDR4["project_id"] + " and department_id = " + objDR4["department_id"] + " and phase_id = " + objDR4["phase_id"];
                            objDR = cmd.ExecuteReader();
                            objDR.Read();


                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT project_department ON;";

                            cmd2.CommandText = cmd2.CommandText + " insert into project_department(project_id,department_id,description,department_type_id,phase_id,area,contact_name,contact_email,contact_phone,date_added,added_by,comment,department_type_domain_id,domain_id) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10, @value11, @value12, @value13,@value14);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["department_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["department_type_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["area"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["contact_name"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["contact_email"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["contact_phone"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["department_type_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value14", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_department OFF";
                            cmd2.ExecuteNonQuery();

                            objDR.Close();
                        }
                    }
                    objDR4.Close();


                    //INICIO PROJECT_ROOM
                    label5.Text = label5.Text + "\rStarting project_room";
                    label5.Refresh();
                    cmd4.CommandText = "select project_id, room_id from project_room where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id, room_id";
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_room where project_id = " + objDR4["project_id"] + " and room_id = " + objDR4["room_id"] + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd.CommandText = " select * from project_room where project_id = " + objDR4["project_id"] + " and room_id = " + objDR4["room_id"];
                            objDR = cmd.ExecuteReader();
                            objDR.Read();

                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT project_room ON;";

                            cmd2.CommandText = cmd2.CommandText + " insert into project_room(project_id,department_id,room_id,drawing_room_name,drawing_room_number,final_room_name,final_room_number,date_added,added_by,comment,phase_id,domain_id) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10, @value11,@value12);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["department_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["room_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["drawing_room_name"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["drawing_room_number"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["final_room_name"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["final_room_number"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value12", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_room OFF";
                            cmd2.ExecuteNonQuery();

                            objDR.Close();

                        }
                    }
                    objDR4.Close();


                    //INICIO users_track
                    cmd.CommandText = " select * from users_track where SUBSTRING (username,position('@' in username)+1,LENGTH (username)) in(select domain from domain where domain_id in (1," + domain_id + "));";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from users_track where id = " + objDR["id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = cmd2.CommandText + " insert into users_track(username,created_at,deleted_at) values(@value2, @value3,@value4);";
                            cmd2.Parameters.AddWithValue("@value2", objDR["username"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["created_at"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["deleted_at"]);
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //INICIO project_user
                    foreach (var user_id in user_ids)
                    {
                        cmd.CommandText = "select * from project_user where user_pid = '" + user_id + "' and project_id = " + selected_project;
                        objDR = cmd.ExecuteReader();

                        while (objDR.Read())
                        {
                            cmd2.CommandText = exec_prod1 + " select count(*) as total from project_user where project_id = " + objDR["project_id"] + " and user_pid = '" + objDR["user_pid"].ToString() + "'";
                            objDR2 = cmd2.ExecuteReader();
                            objDR2.Read();
                            total = (Int32)objDR2["total"];
                            objDR2.Close();

                            if (total == 0)
                            {
                                cmd2.Parameters.Clear();

                                cmd2.CommandText = exec_prod1 + " insert into project_user(project_id, user_pid, project_domain_id) values(@value1, @value2, @value3);";
                                cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                                cmd2.Parameters.AddWithValue("@value2", objDR["user_pid"]);
                                cmd2.Parameters.AddWithValue("@value3", domain_id);
                                cmd2.ExecuteNonQuery();

                            }
                        }
                        objDR.Close();
                    }





                    //INICIO assets_project
                    label5.Text = label5.Text + "\r Starting project_equipment";
                    label5.Refresh();
                    cmd.CommandText = "select * from project_equipment where domain_id = " + domain_id + " and project_id = " + selected_project + " order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_project where project_id = " + objDR["project_id"] + " and domain_id = " + objDR["domain_id"] + " and asset_id = " + objDR["equipment_id"] + " and asset_domain_id = " + objDR["equipment_domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " insert into assets_project(asset_id, asset_domain_id, project_id, domain_id) values(@value1, @value2,@value3,@value4);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["equipment_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["domain_id"]);
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //INICIO global_contact
                    cmd4.CommandText = "select contact_id from global_contact where domain_id = " + domain_id + " order by contact_id";
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from global_contact where contact_id = " + objDR4["contact_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd.CommandText = "select * from global_contact where contact_id = " + objDR4["contact_id"];
                            objDR = cmd.ExecuteReader();
                            objDR.Read();


                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT global_contact ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into global_contact(contact_id, name, contact_type, company, title, email, address, city, state, zip, phone, fax, app_access, date_added, added_by, comment, mobile, domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17,@value18);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["contact_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["contact_type"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["company"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["title"].ToString());
                            cmd2.Parameters.AddWithValue("@value6", objDR["email"].ToString());
                            cmd2.Parameters.AddWithValue("@value7", objDR["address"].ToString());
                            cmd2.Parameters.AddWithValue("@value8", objDR["city"].ToString());
                            cmd2.Parameters.AddWithValue("@value9", objDR["state"].ToString());
                            cmd2.Parameters.AddWithValue("@value10", objDR["zip"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["phone"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["fax"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["app_access"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["mobile"]);
                            cmd2.Parameters.AddWithValue("@value18", objDR["domain_id"]);

                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT global_contact OFF;";
                            cmd2.ExecuteNonQuery();

                            objDR.Close();

                        }
                    }
                    objDR4.Close();


                    //INICIO project_contact
                    cmd.CommandText = "select * from project_contact where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        //cmd3.CommandText = exec_prod1 + " select contact_id from global_contact where old_contact_id = " + objDR["contact_id"];
                        //objDR3 = cmd3.ExecuteReader();
                        //objDR3.Read();
                        //var contact_id = (Int32)objDR3["contact_id"];
                        //objDR3.Close();
                        var contact_id = (Int32)objDR["contact_id"];


                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_contact where project_id = " + objDR["project_id"] + " and contact_id = " + contact_id + " and domain_id = " + domain_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " insert into project_contact(project_id,contact_id,domain_id) values(@value1,@value2,@value3);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["contact_id"]);
                            cmd2.Parameters.AddWithValue("@value3", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //cost_center
                    cmd.CommandText = "select * from cost_center where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from cost_center where id = " + objDR["id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT cost_center ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into cost_center(id,code,description, project_id, is_default,domain_id) values(@value1, @value2, @value3,@value4,@value5,@value6);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["code"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["is_default"]);
                            cmd2.Parameters.AddWithValue("@value6", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT cost_center OFF";
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();




                    //project_addresses
                    cmd.CommandText = "select * from project_addresses where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_addresses where id = " + objDR["id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT project_addresses ON;";
                            cmd2.CommandText = cmd2.CommandText + " insert into project_addresses(id,domain_id,nickname,description,project_id,address1,address2,city,state,zip,is_default) values(@value1, @value2, @value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["id"]);
                            cmd2.Parameters.AddWithValue("@value2", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value3", objDR["nickname"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["address1"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["address2"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["city"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["state"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["zip"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["is_default"]);
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_addresses OFF";
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //project_addresses2
                    cmd.CommandText = "select * from project where domain_id = " + domain_id + " and project_id = " + selected_project + " and address1 is not null order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_addresses where project_id = " + objDR["project_id"] + " and domain_id=" + objDR["domain_id"] + " and address1 = '" + objDR["address1"] + "'";
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.CommandText = exec_prod1 + "update project_addresses set is_default = 0 where project_id = " + objDR["project_id"] + " and domain_id=" + objDR["domain_id"];
                            cmd2.ExecuteNonQuery();

                            cmd2.Parameters.Clear();

                            cmd2.CommandText = cmd2.CommandText + " insert into project_addresses(domain_id,nickname,description,project_id,address1,address2,city,state,zip,is_default) values(@value2, @value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11);";
                            cmd2.Parameters.AddWithValue("@value2", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value3", "From Project");
                            cmd2.Parameters.AddWithValue("@value4", "From Project");
                            cmd2.Parameters.AddWithValue("@value5", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["address1"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["address2"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["city"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["state"]);
                            if (objDR["zip"].ToString() == "")
                                cmd2.Parameters.AddWithValue("@value10", " ");
                            else
                                cmd2.Parameters.AddWithValue("@value10", objDR["zip"]);

                            cmd2.Parameters.AddWithValue("@value11", 1);
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //phase_documents
                    cmd.CommandText = "select * from equipment_drawing where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from phase_documents where drawing_id = " + objDR["drawing_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT phase_documents ON";
                            cmd2.CommandText = cmd2.CommandText + " insert into phase_documents(project_id, phase_id,drawing_id,filename,date_added,domain_id) values(@value1, @value2, @value3,@value4,@value5,@value6);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["drawing_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["filename"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value6", domain_id.ToString());
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT phase_documents OFF";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                    //project_room_inventory
                    label5.Text = label5.Text + "\rStarting project_room_inventory";
                    label5.Refresh();

                    cmd4.CommandText = "select a.project_id, a.phase_id, a.room_id, a.department_id, a.equipment_id, a.domain_id  from project_room_inventory a, equipment e where a.equipment_id = e.equipment_id and a.domain_id = e.domain_id and a.project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id,room_id, equipment_id";
                    objDR4 = cmd4.ExecuteReader();

                    while (objDR4.Read())
                    {
                        cmd.CommandText = "select a.*, case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id, array_to_string(tag, ', ') as tag1, case when a.resp is null then e.default_resp else a.resp end as resp2  from project_room_inventory a, equipment e where a.equipment_id = e.equipment_id and a.domain_id = e.domain_id and a.project_id = " + objDR4["project_id"] + " and a.phase_id = " + objDR4["phase_id"] + " and a.department_id = " + objDR4["department_id"] + " and a.room_id = " + objDR4["room_id"] + " and a.equipment_id = " + objDR4["equipment_id"] + " and a.domain_id = " + objDR4["domain_id"];
                        objDR = cmd.ExecuteReader();
                        objDR.Read();

                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_room_inventory where project_id = " + objDR["project_id"] + " and domain_id in(1, " + domain_id + ") and room_id = " + objDR["room_id"] + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id in(1, " + objDR["asset_domain_id"] + ")";
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " insert into project_room_inventory(project_id,department_id,room_id,asset_id,status,resp,budget_qty,dnp_qty,unit_budget,buyout_delta,estimated_delivery_date,current_location,inventory_type,date_added,added_by,comment,lease_qty,cost_center_id,tag,cad_id,domain_id,phase_id,asset_domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value9,@value11,@value12,@value13,@value14,@value16,@value17,@value18,@value19,@value20,@value21,@value22,@value23,@value24,@value25,@value26);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["department_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["room_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["asset_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["status"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["resp2"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["budget_qty"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["dnp_qty"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["unit_budget"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["buyout_delta"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["estimated_delivery_date"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["current_location"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["inventory_type"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value18", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value19", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value20", objDR["lease_qty"]);
                            cmd2.Parameters.AddWithValue("@value21", objDR["cost_center_id"]);
                            cmd2.Parameters.AddWithValue("@value22", objDR["tag1"].ToString());
                            cmd2.Parameters.AddWithValue("@value23", objDR["cad_id"]);
                            cmd2.Parameters.AddWithValue("@value24", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value25", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value26", objDR["asset_domain_id"]);

                            cmd2.ExecuteNonQuery();
                        }
                        objDR.Close();
                    }
                    objDR4.Close();


                    //project_room_inventory - UPDATE NONE_OPTIONS
                    cmd.CommandText = "select case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id, a.* from project_room_inventory a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where equipment_color_ids = '{0}' and equipment_option_ids = '{0}' and project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id,room_id, a.equipment_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " update project_room_inventory set none_option = 1 where project_id = " + objDR["project_id"] + " and domain_id = " + domain_id + " and phase_id = " + objDR["phase_id"] + " and department_id = " + objDR["department_id"] + " and room_id = " + objDR["room_id"] + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id = " + objDR["asset_domain_id"];
                        cmd2.ExecuteNonQuery();

                    }
                    objDR.Close();

                    //inventory_options - FOR OPTIONS
                    cmd.CommandText = "select case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id,  REPLACE(REPLACE(equipment_option_ids::VARCHAR, '{',''),'}','') as options, * from project_room_inventory a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where equipment_option_ids <> '{0}' AND equipment_option_ids IS NOT NULL and project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id,room_id, a.equipment_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select inventory_id from project_room_inventory where project_id = " + objDR["project_id"] + " and domain_id = " + domain_id + " and phase_id = " + objDR["phase_id"] + " and department_id = " + objDR["department_id"] + " and room_id = " + objDR["room_id"] + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id = " + objDR["asset_domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        var inventory_id = (Int32)objDR2["inventory_id"];
                        objDR2.Close();

                        var options = objDR["options"].ToString().Split(',');

                        foreach (var item in options)
                        {
                            //there is an inconsistence on the old database
                            //need to check if the options still exists first
                            cmd4.CommandText = "select count(*) as total from equipment_options where equipment_options_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                            objDR4 = cmd4.ExecuteReader();
                            objDR4.Read();
                            if (Convert.ToInt32(objDR4["total"]) > 0)
                            {
                                //GET CORRECT OPTION ID
                                cmd2.CommandText = exec_prod1 + " select asset_option_id from assets_options where data_type = 'A' and old_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                                objDR2 = cmd2.ExecuteReader();
                                objDR2.Read();
                                if (objDR2.HasRows)
                                {
                                    var new_option_id = (Int32)objDR2["asset_option_id"];

                                    objDR2.Close();

                                    cmd2.CommandText = exec_prod1 + " select count(*) as total from inventory_options where inventory_id = " + inventory_id + " and option_id = " + new_option_id + " and domain_id = " + objDR["asset_domain_id"];
                                    objDR2 = cmd2.ExecuteReader();
                                    objDR2.Read();
                                    total = (Int32)objDR2["total"];
                                    objDR2.Close();

                                    if (total == 0)
                                    {
                                        cmd2.Parameters.Clear();

                                        cmd2.CommandText = exec_prod1 + " insert into inventory_options(inventory_id,option_id, domain_id, quantity) values(@value1,@value2,@value3,@value4);";
                                        cmd2.Parameters.AddWithValue("@value1", inventory_id);
                                        cmd2.Parameters.AddWithValue("@value2", new_option_id);
                                        cmd2.Parameters.AddWithValue("@value3", objDR["asset_domain_id"]);
                                        cmd2.Parameters.AddWithValue("@value4", 1);
                                        cmd2.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    //pequena gambi porque foram criados options no domain da audaxware qeu a hsg tá usando mas nao pode migrar pq vai dar conflito com o que ja tem no sistema novo
                                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_file");
                                    string file = Path.Combine(path, "error.txt");
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);

                                    if (!File.Exists(file))
                                        File.Create(file);

                                    objDR2.Close();
                                    cmd5.CommandText = "select * from equipment_options where equipment_options_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                                    objDR5 = cmd5.ExecuteReader();
                                    objDR5.Read();
                                    var error_msg = objDR["equipment_id"].ToString() + ";" + objDR["equipment_code"].ToString() + ";" +
                                        ";" + objDR5["equipment_options_id"].ToString() + ";" + ";" + objDR5["code"].ToString() + ";" +
                                        ";" + objDR5["description"].ToString() + ";" + inventory_id + ";" + objDR5["domain_id"] + ";";
                                    objDR5.Close();

                                    cmd2.CommandText = "select * from project_room_inventory where inventory_id = " + inventory_id;
                                    objDR2 = cmd2.ExecuteReader();
                                    objDR2.Read();
                                    error_msg += objDR2["project_id"] + ";" + objDR2["phase_id"] + ";" +
                                        objDR2["department_id"] + ";" + objDR2["room_id"] + ";";
                                    objDR2.Close();
                                    File.AppendAllText(file, error_msg + Environment.NewLine);
                                }

                            }
                            objDR4.Close();
                        }
                    }
                    objDR.Close();


                    //inventory_options - FOR COLORS
                    cmd.CommandText = "select case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id,  REPLACE(REPLACE(equipment_color_ids::VARCHAR, '{',''),'}','') as options, * from project_room_inventory a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where equipment_color_ids <> '{0}' AND equipment_color_ids IS NOT NULL and project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id,room_id, a.equipment_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {

                        cmd2.CommandText = exec_prod1 + " select inventory_id from project_room_inventory where project_id = " + objDR["project_id"] + " and domain_id = " + domain_id + " and phase_id = " + objDR["phase_id"] + " and department_id = " + objDR["department_id"] + " and room_id = " + objDR["room_id"] + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id = " + objDR["asset_domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        var inventory_id = (Int32)objDR2["inventory_id"];
                        objDR2.Close();

                        var options = objDR["options"].ToString().Split(',');

                        foreach (var item in options)
                        {
                            //there is an inconsistence on the old database
                            //need to check if the options still exists first
                            cmd4.CommandText = "select count(*) as total from equipment_colors where equipment_colors_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                            objDR4 = cmd4.ExecuteReader();
                            objDR4.Read();
                            if (Convert.ToInt32(objDR4["total"]) > 0)
                            {


                                //GET CORRECT COLOR ID
                                cmd2.CommandText = exec_prod1 + " select asset_option_id from assets_options where data_type = 'C' and old_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                                objDR2 = cmd2.ExecuteReader();
                                objDR2.Read();
                                if (objDR2.HasRows)
                                {
                                    var new_option_id = (Int32)objDR2["asset_option_id"];
                                    objDR2.Close();



                                    cmd2.CommandText = exec_prod1 + " select count(*) as total from inventory_options where inventory_id = " + inventory_id + " and option_id = " + new_option_id + " and domain_id = " + objDR["asset_domain_id"];
                                    objDR2 = cmd2.ExecuteReader();
                                    objDR2.Read();
                                    total = (Int32)objDR2["total"];
                                    objDR2.Close();

                                    if (total == 0)
                                    {
                                        cmd2.Parameters.Clear();

                                        cmd2.CommandText = exec_prod1 + " insert into inventory_options(inventory_id,option_id, domain_id, quantity) values(@value1,@value2,@value3,@value4);";
                                        cmd2.Parameters.AddWithValue("@value1", inventory_id);
                                        cmd2.Parameters.AddWithValue("@value2", new_option_id);
                                        cmd2.Parameters.AddWithValue("@value3", objDR["asset_domain_id"]);
                                        cmd2.Parameters.AddWithValue("@value4", 1);
                                        cmd2.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    //pequena gambi porque foram criados options no domain da audaxware qeu a hsg tá usando mas nao pode migrar pq vai dar conflito com o que ja tem no sistema novo
                                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_file");
                                    string file = Path.Combine(path, "error_color.txt");
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);

                                    if (!File.Exists(file))
                                        File.Create(file);

                                    objDR2.Close();
                                    cmd5.CommandText = "select * from equipment_colors where equipment_colors_id = " + item + " and domain_id = " + objDR["asset_domain_id"];
                                    objDR5 = cmd5.ExecuteReader();
                                    objDR5.Read();
                                    var error_msg = objDR["equipment_id"].ToString() + ";" + objDR["equipment_code"].ToString() + ";" +
                                        ";" + objDR5["equipment_colors_id"].ToString() + ";" + ";" + objDR5["code"].ToString() + ";" +
                                        ";" + objDR5["description"].ToString() + ";" + inventory_id + ";" + objDR5["domain_id"] + ";";
                                    objDR5.Close();

                                    cmd2.CommandText = "select * from project_room_inventory where inventory_id = " + inventory_id;
                                    objDR2 = cmd2.ExecuteReader();
                                    objDR2.Read();
                                    error_msg += objDR2["project_id"] + ";" + objDR2["phase_id"] + ";" +
                                        objDR2["department_id"] + ";" + objDR2["room_id"] + ";";
                                    objDR2.Close();
                                    File.AppendAllText(file, error_msg + Environment.NewLine);
                                }
                            }
                            objDR4.Close();
                        }
                    }
                    objDR.Close();



                    //project_room_inventory - UPDATE OPTION_IDS
                    cmd3.CommandText = exec_prod1 + "SELECT  inventory_id,STUFF((SELECT ', ' + CAST(option_id AS VARCHAR(10)) [text()] FROM inventory_options WHERE inventory_id = t.inventory_id and domain_id = " + domain_id + " order by inventory_id, option_id FOR XML PATH(''), TYPE) .value('.','NVARCHAR(MAX)'),1,2,' ') options FROM inventory_options t GROUP BY inventory_id";

                    objDR3 = cmd3.ExecuteReader();

                    while (objDR3.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " update project_room_inventory set option_ids = '" + objDR3["options"] + "' where inventory_id = " + objDR3["inventory_id"] + " and option_ids is null";
                        cmd2.ExecuteNonQuery();

                    }
                    objDR3.Close();



                    //purchase_order
                    label5.Text = label5.Text + "\r Starting purchase_order";
                    label5.Refresh();
                    cmd.CommandText = "select * from purchase_order where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by po_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from purchase_order where project_id = " + objDR["project_id"] + " and ((old_po_id is not null and old_po_id = " + objDR["po_id"] + ") or (old_po_id is null and po_id = " + objDR["po_id"] + "))";
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            //cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT purchase_order ON";
                            cmd2.CommandText = cmd2.CommandText + " insert into purchase_order(old_po_id,project_id,po_number,quote_number,description,vendor_id,quote_requested_date,quote_received_date,po_requested_date,po_received_date,freight,warehouse,tax,warranty,misc,status,date_added,added_by,comment,quote_file,po_file,upd_asset_value,vendor_domain_id,po_requested_number,domain_id,quote_amount, ship_to) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17,@value18,@value19,@value20,@value21,@value22,@value23,@value24,@value25,@value26, @value27);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["po_id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["po_number"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["quote_number"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["vendor_id"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["quote_requested_date"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["quote_received_date"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["po_requested_date"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["po_received_date"]);
                            cmd2.Parameters.AddWithValue("@value11", objDR["freight"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["warehouse"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["tax"]);
                            cmd2.Parameters.AddWithValue("@value14", objDR["warranty"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["misc"]);
                            cmd2.Parameters.AddWithValue("@value16", objDR["status"]);
                            cmd2.Parameters.AddWithValue("@value17", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value18", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value19", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value20", objDR["quote_file"]);
                            cmd2.Parameters.AddWithValue("@value21", objDR["po_file"]);
                            cmd2.Parameters.AddWithValue("@value22", objDR["upd_equipment_value"]);
                            cmd2.Parameters.AddWithValue("@value23", objDR["vendor_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value24", objDR["po_requested_number"]);
                            cmd2.Parameters.AddWithValue("@value25", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value26", objDR["quote_amount"]);
                            cmd2.Parameters.AddWithValue("@value27", objDR["ship_to"]);

                            cmd2.ExecuteNonQuery();

                            //cmd2.CommandText = exec_prod1 + "SET IDENTITY_INSERT purchase_order OFF;";
                            //cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //inventory_purchase_order
                    label5.Text = label5.Text + "\r Starting inventory_purchase_order";
                    label5.Refresh();
                    cmd.CommandText = "select case when e.new_equipment_id is null then a.equipment_id else e.new_equipment_id end as asset_id, case when e.new_equipment_id is null then a.domain_id else e.new_equipment_domain_id end as asset_domain_id, a.* from inventory_purchase_order a inner join equipment e on e.equipment_id = a.equipment_id and e.domain_id = a.domain_id where project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {

                        cmd3.CommandText = exec_prod1 + "select inventory_id from project_room_inventory where project_id = " + objDR["project_id"] + " and department_id = " + objDR["department_id"] + " and room_id = " + objDR["room_id"] + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id = " + objDR["asset_domain_id"];
                        objDR3 = cmd3.ExecuteReader();
                        objDR3.Read();
                        var inventory_id = objDR3["inventory_id"];
                        objDR3.Close();

                        cmd3.CommandText = exec_prod1 + "select po_id from purchase_order where project_id = " + objDR["project_id"] + " and ((old_po_id is not null and old_po_id = " + objDR["po_id"] + ") or (old_po_id is null and po_id = " + objDR["po_id"] + "))";
                        objDR3 = cmd3.ExecuteReader();
                        objDR3.Read();
                        var po_id = objDR3["po_id"];
                        objDR3.Close();

                        cmd2.CommandText = exec_prod1 + " select count(*) as total from inventory_purchase_order where inventory_id = " + inventory_id + " and po_id = " + po_id + " and asset_id = " + objDR["asset_id"] + " and asset_domain_id = " + objDR["asset_domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();


                            cmd2.CommandText = exec_prod1 + " insert into inventory_purchase_order(project_id,po_id,po_qty,po_unit_amt,po_status,date_added,added_by,inventory_id,asset_id,asset_domain_id,po_domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value2", po_id);
                            cmd2.Parameters.AddWithValue("@value3", objDR["po_qty"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["po_unit_amt"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["po_status"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value8", inventory_id);
                            cmd2.Parameters.AddWithValue("@value9", objDR["asset_id"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["asset_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value11", domain_id.ToString());
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    objDR.Close();


                    //room_it_connectivity_boxes
                    cmd.CommandText = "select b.*, pr.phase_id as connected_phase_id, pr.department_id as connected_department_id from boxes b left join project_room pr on pr.project_id = b.project_id and pr.room_id = b.connection_room_id where b.project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from room_it_connectivity_boxes where old_box_id = " + objDR["id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            //cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT room_it_connectivity_boxes ON";
                            cmd2.CommandText = cmd2.CommandText + " insert into room_it_connectivity_boxes(old_box_id, name, project_id, department_id, room_id, max_jack_connections, box_number, connected_room_id, phase_id, domain_id, connected_phase_id, connected_department_id, connected_project_id) values(@value1, @value2, @value3,@value4,@value5,@value6,@value7, @value8, @value9,@value10,@value11,@value12, @value13);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value3", objDR["project_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["department_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["room_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["max_jack_connections"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["box_number"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["connection_room_id"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["phase_id"]);
                            cmd2.Parameters.AddWithValue("@value10", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value11", objDR["connected_phase_id"]);
                            cmd2.Parameters.AddWithValue("@value12", objDR["connected_department_id"]);
                            cmd2.Parameters.AddWithValue("@value13", objDR["project_id"]);
                            cmd2.ExecuteNonQuery();
                            //cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT room_it_connectivity_boxes OFF";
                            //cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //assets_it_connectivity
                    cmd.CommandText = "select * from equipment_box a, boxes b where b.id = a.box_id and b.project_id in(select project_id from project where domain_id = " + domain_id + " and project_id = " + selected_project + ") order by project_id";
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd3.CommandText = exec_prod1 + "select box_id from room_it_connectivity_boxes where old_box_id = " + objDR["box_id"];
                        objDR3 = cmd3.ExecuteReader();
                        objDR3.Read();
                        var box_id = objDR3["box_id"];
                        objDR3.Close();


                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_it_connectivity where old_id = " + objDR["id"] + " and box_id = " + box_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            //cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT assets_it_connectivity ON";
                            cmd2.CommandText = exec_prod1 + " insert into assets_it_connectivity(asset_id, box_id, jack_number, connected_asset_id, domain_id, connected_domain_id, added_by, jack_label) values(@value2, @value3,@value4,@value5,@value6,@value7, @value8, @value9);";
                            //cmd2.Parameters.AddWithValue("@value1", objDR["id"]);
                            cmd2.Parameters.AddWithValue("@value2", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value3", box_id);
                            cmd2.Parameters.AddWithValue("@value4", objDR["jack_number"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["connected_equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["connected_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value9", objDR["jack_label"]);
                            cmd2.ExecuteNonQuery();
                            //cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT assets_it_connectivity OFF";
                            //cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                    //user_project_mine - starts empty
                    //User_gridView - starts empty
                    //inventory_tab_display_prefs - doesn't exist anymore


                    //TEMPLATES - OLD template_room
                    cmd.CommandText = "select * from template_room where department_type_domain_id = " + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_room where is_template = 1 and room_id = " + objDR["template_room_id"] + " and department_type_domain_id_template = " + objDR["department_type_domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_room ON";
                            cmd2.CommandText = cmd2.CommandText + " insert into project_room(project_id,department_id,room_id,drawing_room_name,drawing_room_number,date_added,added_by,comment,phase_id,domain_id,is_template,department_type_id_template, department_type_domain_id_template) values(@value1, @value2, @value3, @value4, @value5, @value8, @value9, @value10, @value11,@value12,@value13,@value14,@value15);";
                            cmd2.Parameters.AddWithValue("@value1", 1); //project_id
                            cmd2.Parameters.AddWithValue("@value2", 1); //department_id
                            cmd2.Parameters.AddWithValue("@value3", objDR["template_room_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["description"]);
                            cmd2.Parameters.AddWithValue("@value5", "-");
                            cmd2.Parameters.AddWithValue("@value8", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value9", "Migration");
                            cmd2.Parameters.AddWithValue("@value10", objDR["comment"]);
                            cmd2.Parameters.AddWithValue("@value11", 1); //phase_id
                            cmd2.Parameters.AddWithValue("@value12", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value13", 1); //is_template
                            cmd2.Parameters.AddWithValue("@value14", objDR["department_type_id"]);
                            cmd2.Parameters.AddWithValue("@value15", objDR["department_type_domain_id"]);
                            cmd2.ExecuteNonQuery();
                            cmd2.CommandText = exec_prod1 + " SET IDENTITY_INSERT project_room OFF";
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                    //TEMPLATES - template_room_inventory
                    label5.Text = label5.Text + "\rStarting templates";
                    label5.Refresh();
                    cmd.CommandText = "select *  from template_room_inventory where domain_id = " + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from project_room_inventory where project_id = 1 and room_id = " + objDR["template_room_id"] + " and asset_id = " + objDR["equipment_id"] + " and asset_domain_id = " + objDR["domain_id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " insert into project_room_inventory(project_id,department_id,room_id,asset_id,status,resp,budget_qty,current_location,date_added,added_by,domain_id,phase_id,asset_domain_id) values(@value1,@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13);";
                            cmd2.Parameters.AddWithValue("@value1", 1);
                            cmd2.Parameters.AddWithValue("@value2", 1);
                            cmd2.Parameters.AddWithValue("@value3", objDR["template_room_id"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value5", "A");
                            cmd2.Parameters.AddWithValue("@value6", "OFOI");
                            cmd2.Parameters.AddWithValue("@value7", objDR["qty"]);
                            cmd2.Parameters.AddWithValue("@value8", "Plan");
                            cmd2.Parameters.AddWithValue("@value9", objDR["date_added"]);
                            cmd2.Parameters.AddWithValue("@value10", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value11", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value12", 1);
                            cmd2.Parameters.AddWithValue("@value13", objDR["domain_id"]);

                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();


                    //TEMPLATES - template_boxes / room_it_connectivity_boxes
                    cmd.CommandText = "select a.* from template_boxes a, template_room b where a.template_room_id = b.template_room_id and b.department_type_domain_id = " + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd2.CommandText = exec_prod1 + " select count(*) as total from room_it_connectivity_boxes where old_template_box_id = " + objDR["id"];
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = exec_prod1 + " insert into room_it_connectivity_boxes(name, project_id, department_id, room_id, max_jack_connections, box_number, phase_id, domain_id, old_template_box_id) values(@value1, @value2, @value3,@value4,@value5,@value6,@value7, @value8, @value9);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["name"]);
                            cmd2.Parameters.AddWithValue("@value2", 1);
                            cmd2.Parameters.AddWithValue("@value3", 1);
                            cmd2.Parameters.AddWithValue("@value4", objDR["template_room_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["max_jack_connections"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["box_number"]);
                            cmd2.Parameters.AddWithValue("@value7", 1);
                            cmd2.Parameters.AddWithValue("@value8", domain_id.ToString());
                            cmd2.Parameters.AddWithValue("@value9", objDR["id"]);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();

                    //TEMPLATES - template_equipment_box / assets_it_connectivity
                    cmd.CommandText = "select a.* from template_equipment_box a, template_boxes b, template_room c where a.box_id = b.id and b.template_room_id = c.template_room_id and c.department_type_domain_id = " + domain_id;
                    objDR = cmd.ExecuteReader();

                    while (objDR.Read())
                    {
                        cmd3.CommandText = exec_prod1 + "select box_id from room_it_connectivity_boxes where project_id = 1 and old_template_box_id = " + objDR["box_id"];
                        objDR3 = cmd3.ExecuteReader();
                        objDR3.Read();
                        var box_id = objDR3["box_id"];
                        objDR3.Close();


                        cmd2.CommandText = exec_prod1 + " select count(*) as total from assets_it_connectivity where box_id = " + box_id;
                        objDR2 = cmd2.ExecuteReader();
                        objDR2.Read();
                        total = (Int32)objDR2["total"];
                        objDR2.Close();

                        if (total == 0)
                        {
                            cmd2.Parameters.Clear();

                            cmd2.CommandText = cmd2.CommandText + " insert into assets_it_connectivity(asset_id, box_id, jack_number, connected_asset_id, domain_id, connected_domain_id, added_by, jack_label) values(@value1, @value2, @value3,@value4,@value5,@value6,@value7, @value8);";
                            cmd2.Parameters.AddWithValue("@value1", objDR["equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value2", box_id);
                            cmd2.Parameters.AddWithValue("@value3", objDR["jack_number"]);
                            cmd2.Parameters.AddWithValue("@value4", objDR["connected_equipment_id"]);
                            cmd2.Parameters.AddWithValue("@value5", objDR["domain_id"]);
                            cmd2.Parameters.AddWithValue("@value6", objDR["connected_domain_id"]);
                            cmd2.Parameters.AddWithValue("@value7", objDR["added_by"]);
                            cmd2.Parameters.AddWithValue("@value8", objDR["jack_label"]);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                    objDR.Close();
                }

                //MIGRATE SUBCATEGORY ADITIONALS
                var hvac = "D";
                var plumbing = "D";
                var gases = "D";
                var it = "D";
                var electrical = "D";
                var support = "D";
                var physical = "D";
                var environmental = "D";

                var hvac_fields = new List<string>() { "water", "water_option", "cfm", "btus" };
                var plumbing_fields = new List<string>() { "plumbing", "plumbing_option", "plu_hot_water", "plu_cold_water", "plu_drain", "plu_return", "plu_treated_water", "plu_chilled_water", "plu_relief" };
                var gases_fields = new List<string>() { "medgas", "medgas_option", "medgas_oxygen", "medgas_air", "medgas_n2o", "medgas_co2", "medgas_wag", "medgas_other", "medgas_nitrogen", "medgas_vacuum", "medgas_steam", "medgas_natgas" };
                var it_fields = new List<string>() { "data", "data_option" };
                var electrical_fields = new List<string>() { "electrical", "electrical_option", "hertz", "amps", "volt_amps", "watts" };
                var support_fields = new List<string>() { "blocking", "blocking_option", "supports", "supports_option", "misc_seismic" };
                var physical_fields = new List<string>() { "misc_ase", "misc_ada", "mobile", "mobile_option", "height", "width", "depth", "clearance_top", "clearance_bottom", "clearance_right", "clearance_left", "clearance_front", "clearance_back", "loaded_weight", "ship_weight", "category_attribute" };
                var environmental_fields = new List<string>() { "misc_antimicrobial", "misc_ecolabel", "misc_ecolabel_desc" };


                label5.Text = label5.Text + "\r Starting aditionals for cat e subcat";
                label5.Refresh();
                //cmd2.CommandText = exec_prod1 + " select subcategory_id from assets_subcategory where domain_id = " + domain_id;
                //objDR2 = cmd2.ExecuteReader();

                cmd2.CommandText = exec_prod1 + "update assets_subcategory set HVAC = 'E', Plumbing = 'E', Gases =  'E', IT =  'E', Electrical =  'E', Support =  'E', Physical =  'E', Environmental =  'E' where domain_id = " + domain_id;
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_category set HVAC = 'E', Plumbing = 'E', Gases =  'E', IT =  'E', Electrical =  'E', Support =  'E', Physical =  'E', Environmental =  'E' where domain_id = " + domain_id;
                cmd2.ExecuteNonQuery();


                //while (objDR2.Read())
                //{
                //    hvac = "D";
                //    plumbing = "D";
                //    gases = "D";
                //    it = "D";
                //    electrical = "D";
                //    support = "D";
                //    physical = "D";
                //    environmental = "D";

                //cmd3.CommandText = exec_prod1 + "select asset_id,subcategory_id,height,width,depth,weight,water,plumbing,data,electrical,mobile,blocking,medgas,supports,discontinued,water_option,plumbing_option,data_option,electrical_option,mobile_option,blocking_option,medgas_option,supports_option,placement,clearance_left,clearance_right,clearance_front,clearance_back,clearance_top,clearance_bottom,volts,phases,hertz,amps,volt_amps,watts,cfm,btus,misc_ase,misc_ada,misc_seismic,misc_antimicrobial,misc_ecolabel,misc_ecolabel_desc,mapping_code,medgas_oxygen,medgas_nitrogen,medgas_air,medgas_n2o,medgas_vacuum,medgas_wag,medgas_co2,medgas_other,medgas_steam,medgas_natgas,plu_hot_water,plu_drain,plu_cold_water,plu_return,plu_treated_water,plu_relief,plu_chilled_water,serial_name,useful_life,loaded_weight,ship_weight,domain_id,subcategory_domain_id,category_attribute from assets where domain_id = " + domain_id + " and subcategory_id = " + objDR2["subcategory_id"];
                //objDR3 = cmd3.ExecuteReader();

                //while (objDR3.Read())
                //{
                //    foreach (var item in hvac_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            hvac = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in plumbing_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            plumbing = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in gases_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            gases = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in it_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            it = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in electrical_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            electrical = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in support_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            support = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in physical_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            physical = "E";
                //            break;
                //        }
                //    }

                //    foreach (var item in environmental_fields)
                //    {
                //        if (IsDataPresent(objDR3[item]))
                //        {
                //            environmental = "E";
                //            break;
                //        }
                //    }

                //    var dbCon5 = new SqlConnection(sql_conn);
                //    var cmd5 = dbCon5.CreateCommand();
                //    dbCon5.Open();
                //    cmd5.CommandText = exec_prod1 + "update assets_subcategory set HVAC = '" + hvac + "', Plumbing = '" + plumbing + "', Gases =  '" + gases + "', IT =  '" + it + "', Electrical =  '" + electrical + "', Support =  '" + support + "', Physical =  '" + physical + "', Environmental =  '" + environmental + "' where subcategory_id = " + objDR3["subcategory_id"] + " and domain_id =  " + objDR3["subcategory_domain_id"];
                //    cmd5.ExecuteNonQuery();
                //    dbCon5.Dispose();

                //}
                //objDR3.Close();
                //}


                //objDR2.Close();


                label5.Text = label5.Text + "\r Starting special chars";
                label5.Refresh();

                //special chars
                cmd2.Parameters.Clear();

                cmd2.CommandText = exec_prod1 + "update purchase_order set quote_number = replace(quote_number, '@@1', '™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update purchase_order set comment = replace(comment, '@@1', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#1', '®')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#2', 'â€')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#3', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#4', '…')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project set comment = replace(comment, '#1', ' - ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project set comment = replace(comment, '#2', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update global_contact set company = replace(company, '#1', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update global_contact set company = replace(company, '#3', '''')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update global_contact set title = replace(title, '#1', '''')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update global_contact set mobile = replace(mobile, '#1', '(512) 529-9823')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update global_contact set phone = replace(phone, '#1', '(512) 324-7048')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_department set description = replace(description, '#2', '''')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_department set comment = replace(comment, '#1', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room set comment = replace(comment, '#1', 'º')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room set comment = replace(comment, '#2', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#27', 'Angle scale reads 1° increments.')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#25', 'Cards are 8 1/2 x 11‚ (22 x 28 cm)')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#3', '”,')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#4', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = @"update assets set comment = replace(comment, '#5', '""')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#7', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#9', '©')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#10', ' ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#11', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#13', '½')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#14', '¼')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#15', 'º')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#16', '“')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#17', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#18', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#19', 'µ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#20', '±')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#21', '—')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#22', '‘')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#23', '¹')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#24', 'é')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#26', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#28', '³')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#29', 'ä')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#30', '¾')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#1', '®')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#2', '™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#31', 'The N7500 cabinets are equipped with multiple handpieces, assistants instrumen­tation, self-contained patient water, CPU storage, monitor and keyboard supports, and properly-sized drawers to hold all materials necessary for treating patients.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#32', 'Stereoscopic continuous 4.5 – 20x zoom Nikon optics for optimal viewing and image magnification. Dual zoom knobs on the optical head allow for left or right handed operation. Convenient fine focus control handle sharpens the image. USB Video Camera option. Includes: compact center stand with a spring loaded main shaft for gross height adjustment, with the addition of fine height adjustment.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' aa ', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' bb ', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' cc ', '”™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' dd ', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' ee ', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' ff ', '/')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' gg ', 'œ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' hh ', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, ' ii ', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set asset_description = replace(asset_description, '#22', '''')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' aa ', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' bb ', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' cc ', '”™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' dd ', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' ee ', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' ff ', '/')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' gg ', 'œ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' hh ', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_name = replace(serial_name, ' ii ', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set serial_number = replace(serial_number, '#1', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set electrical = replace(electrical, '#1', '×')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set electrical = replace(electrical, '#2', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set volts = replace(volts, '*1', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set volts = replace(volts, '*2', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set plumbing = replace(plumbing, '#1', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#3', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#4', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = @"update assets_options set description = replace(description, '#5', '""')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#6', 'cm²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#7', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#8', 'Ã')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#9', '©')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#10', ' ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#11', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#12', 'œ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#13', '½')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#14', '¼')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#15', 'º')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#16', '“')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#17', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#1', '®')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set description = replace(description, '#2', '™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room set comment = replace(comment, '#1', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets_options set code = replace(code, '*4', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update manufacturer_contact set title = replace(title, '*1', ' – ') where manufacturer_domain_id = 1";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#1', 'No digital timer. No infra red sensor…  we want knee operated controlled.')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update project_room_inventory set comment = replace(comment, '#2', '”')";
                cmd2.ExecuteNonQuery();
            }
            button1.Text = "START";

            MessageBox.Show("Fim");
        }

        private bool IsDataPresent(object data)
        {
            var data2 = data.ToString().Trim();
            var isNotEmpty = true;

            if (data2 == "" || data2 == "0" || data2.ToLower() == "false")
                isNotEmpty = false;

            return isNotEmpty;
        }

        private void domains_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillProjects();
        }

        private void database_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillDomains();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int i = 0; i < projects.Items.Count; i++)
                {
                    projects.SetItemChecked(i, true);
                }
            }
            else
            {
                FillProjects();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var domain_id = (domains.SelectedItem as dynamic).Value;
            if (string.IsNullOrEmpty(domain_id))
            {
                label4.Text = "Choose a domain";
            }
            else
            {

                label5.Text = "Starting migration...";
                label5.Refresh();


                var dbCon4 = new NpgsqlConnection(postgres_conn);
                var cmd4 = dbCon4.CreateCommand();
                NpgsqlDataReader objDR4;

                var dbCon2 = new SqlConnection(sql_conn);
                var cmd2 = dbCon2.CreateCommand();

                dbCon2.Open();
                dbCon4.Open();

                exec_prod = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "'; ";

                //fix tag
                cmd4.CommandText = "select project_id, department_id, phase_id, room_id, equipment_id, domain_id, array_to_string(tag, ', ') as tag from project_room_inventory where project_id in(select project_id from project where domain_id =" + domain_id + ") and tag is not null";
                objDR4 = cmd4.ExecuteReader();

                while (objDR4.Read())
                {
                    cmd2.CommandText = exec_prod + "update project_room_inventory set tag = '" + objDR4["tag"].ToString() + "' where project_id = " + objDR4["project_id"] + " and phase_id = " + objDR4["phase_id"] + " and department_id = " + objDR4["department_id"] + " and room_id = " + objDR4["room_id"] + " and asset_id =" + objDR4["equipment_id"].ToString() + " and (tag = 'System.String[]' or tag is null or tag = '') and asset_domain_id = " + objDR4["domain_id"].ToString();
                    cmd2.ExecuteNonQuery();
                }
                objDR4.Close();

                label5.Text = "\rFoi";
                label5.Refresh();



            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            var domain_id = (domains.SelectedItem as dynamic).Value;
            if (string.IsNullOrEmpty(domain_id))
            {
                label4.Text = "Choose a domain";
            }
            else
            {

                label5.Text = "Starting migration...";
                label5.Refresh();


                var dbCon4 = new NpgsqlConnection(postgres_conn);
                var cmd4 = dbCon4.CreateCommand();
                NpgsqlDataReader objDR4;

                var dbCon2 = new SqlConnection(sql_conn);
                var cmd2 = dbCon2.CreateCommand();
                var asset_id = "";

                dbCon2.Open();
                dbCon4.Open();

                exec_prod = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "'; ";

                //fix comment
                cmd4.CommandText = "select equipment_id, domain_id, comment from equipment where domain_id =" + domain_id + " order by equipment_id";
                objDR4 = cmd4.ExecuteReader();

                while (objDR4.Read())
                {
                    asset_id = objDR4["equipment_id"].ToString();
                    cmd2.CommandText = exec_prod + "update assets set comment = '" + objDR4["comment"].ToString().Replace("'", "''") + "' where asset_id = " + objDR4["equipment_id"] + " and domain_id = " + objDR4["domain_id"].ToString();
                    cmd2.ExecuteNonQuery();
                }
                objDR4.Close();

                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#27', 'Angle scale reads 1° increments.')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#25', 'Cards are 8 1/2 x 11‚ (22 x 28 cm)')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#3', '”,')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#4', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = @"update assets set comment = replace(comment, '#5', '""')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#7', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#9', '©')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#10', ' ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#11', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#13', '½')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#14', '¼')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#15', 'º')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#16', '“')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#17', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#18', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#19', 'µ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#20', '±')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#21', '—')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#22', '‘')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#23', '¹')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#24', 'é')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#26', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#28', '³')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#29', 'ä')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#30', '¾')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#1', '®')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#2', '™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#31', 'The N7500 cabinets are equipped with multiple handpieces, assistants instrumen­tation, self-contained patient water, CPU storage, monitor and keyboard supports, and properly-sized drawers to hold all materials necessary for treating patients.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set comment = replace(comment, '#32', 'Stereoscopic continuous 4.5 – 20x zoom Nikon optics for optimal viewing and image magnification. Dual zoom knobs on the optical head allow for left or right handed operation. Convenient fine focus control handle sharpens the image. USB Video Camera option. Includes: compact center stand with a spring loaded main shaft for gross height adjustment, with the addition of fine height adjustment.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod1 + "update assets set plumbing = replace(plumbing, '#1', '°')";
                cmd2.ExecuteNonQuery();

                label5.Text = "\rFoi";
                label5.Refresh();



            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var domain_id = (domains.SelectedItem as dynamic).Value;
            if (string.IsNullOrEmpty(domain_id))
            {
                label4.Text = "Choose a domain";
            }
            else
            {

                label5.Text = "Starting migration...";
                label5.Refresh();


                var dbCon = new NpgsqlConnection(postgres_conn);
                var cmd = dbCon.CreateCommand();
                NpgsqlDataReader objDR;

                var dbCon3 = new NpgsqlConnection(postgres_conn);
                var cmd3 = dbCon3.CreateCommand();
                NpgsqlDataReader objDR3;

                var dbCon4 = new NpgsqlConnection(postgres_conn);
                var cmd4 = dbCon4.CreateCommand();
                NpgsqlDataReader objDR4;

                var dbCon2 = new SqlConnection(sql_conn);
                var cmd2 = dbCon2.CreateCommand();
                SqlDataReader objDR2;

                var dbCon5 = new SqlConnection(sql_conn);
                var cmd5 = dbCon5.CreateCommand();
                SqlDataReader objDR5;

                var asset_id = "";
                var total = 0;

                dbCon.Open();
                dbCon2.Open();
                dbCon3.Open();
                dbCon4.Open();
                dbCon5.Open();

                exec_prod = "EXEC sp_set_session_context 'domain_id', '1'; ";

                //INICIO ASSETS
                label5.Text = label5.Text + "\r Starting assets";
                label5.Refresh();
                cmd4.CommandText = "select equipment_id, domain_id, new_equipment_id, new_equipment_domain_id from equipment where domain_id = 3 and new_equipment_id is null order by equipment_id";
                cmd4.CommandTimeout = 40;
                objDR4 = cmd4.ExecuteReader();

                while (objDR4.Read())
                {
                    //if (objDR4["new_equipment_id"].ToString() != "")
                    //{
                    //    cmd2.CommandText = exec_prod + " select count(*) as total from assets where asset_id = " + objDR4["new_equipment_id"].ToString() + " and domain_id = " + objDR4["new_equipment_domain_id"].ToString();
                    //    objDR2 = cmd2.ExecuteReader();
                    //    objDR2.Read();
                    //    total = (Int32)objDR2["total"];
                    //    objDR2.Close();
                    ////}


                    //if (total == 0)
                    //{
                    cmd.CommandText = "select * from equipment where equipment_id =" + objDR4["equipment_id"].ToString() + " and domain_id = " + objDR4["domain_id"].ToString();
                    cmd.CommandTimeout = 40;
                    objDR = cmd.ExecuteReader();
                    objDR.Read();


                    //get subcategory id for domain 1
                    cmd3.CommandText = "select a.description as category, b.description as subcategory from equipment_category a inner join equipment_subcategory b on a.category_id =  b.category_id and a.domain_id = b.domain_id where b.domain_id = 3 and b.subcategory_id = " + objDR["subcategory_id"];
                    objDR3 = cmd3.ExecuteReader();
                    objDR3.Read();
                    var category = objDR3["category"].ToString();
                    var subcategory = objDR3["subcategory"].ToString();
                    objDR3.Close();

                    cmd3.CommandText = "select b.subcategory_id from equipment_category a inner join equipment_subcategory b on a.category_id =  b.category_id and a.domain_id = b.domain_id where a.domain_id = 1 and a.description || b.description = ('" + category + subcategory + "')";
                    objDR3 = cmd3.ExecuteReader();
                    var subcategory_id = "";
                    if (objDR3.Read())
                        subcategory_id = objDR3["subcategory_id"].ToString();

                    objDR3.Close();

                    if (subcategory_id == "")
                    {
                        var category_id = "";

                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod + " select category_id from assets_category where description = '" + category + "' and domain_id = 1";
                        objDR2 = cmd2.ExecuteReader();
                        if (objDR2.Read())
                            category_id = objDR2["category_id"].ToString();
                        objDR2.Close();

                        if (category_id == "")
                        {
                            cmd2.CommandText = exec_prod + " insert into assets_category(description, domain_id) values(@value1, @value2);";
                            cmd2.Parameters.AddWithValue("@value1", category);
                            cmd2.Parameters.AddWithValue("@value2", 1);
                            cmd2.ExecuteNonQuery();

                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod + " select max(category_id) as max from assets_category where domain_id = 1";
                            objDR2 = cmd2.ExecuteReader();
                            if (objDR2.Read())
                                category_id = objDR2["max"].ToString();
                            objDR2.Close();
                        }


                        cmd2.Parameters.Clear();
                        cmd2.CommandText = exec_prod + " select subcategory_id from assets_subcategory where category_id = " + category_id + " and description = '" + subcategory + "' and domain_id = 1";
                        objDR2 = cmd2.ExecuteReader();
                        if (objDR2.Read())
                            subcategory_id = objDR2["subcategory_id"].ToString();
                        objDR2.Close();

                        if (subcategory_id == "")
                        {
                            cmd2.CommandText = exec_prod + " insert into assets_subcategory(category_id, description, domain_id, category_domain_id, use_category_settings) values(@value2, @value3,@value4, @value5,@value6);";
                            cmd2.Parameters.AddWithValue("@value1", subcategory_id);
                            cmd2.Parameters.AddWithValue("@value2", category_id);
                            cmd2.Parameters.AddWithValue("@value3", subcategory);
                            cmd2.Parameters.AddWithValue("@value4", 1);
                            cmd2.Parameters.AddWithValue("@value5", 1);
                            cmd2.Parameters.AddWithValue("@value6", 0);
                            cmd2.ExecuteNonQuery();

                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod + " select max(subcategory_id) as max from assets_subcategory where domain_id = 1";
                            objDR2 = cmd2.ExecuteReader();
                            if (objDR2.Read())
                                subcategory_id = objDR2["max"].ToString();
                            objDR2.Close();
                        }

                    }



                    //get manufacturer id for domain 1
                    var manufacturer_id = objDR["manufacturer_id"].ToString();
                    if (objDR["manufacturer_domain_id"].ToString() != "1")
                    {
                        cmd3.CommandText = "select b.manufacturer_id, a.manufacturer_description, a.date_added, a.added_by, a.comment from manufacturer a left join manufacturer b on b.manufacturer_description = a.manufacturer_description and b.domain_id = 1 where a.manufacturer_id = " + objDR["manufacturer_id"] + " and a.domain_id= 3";
                        objDR3 = cmd3.ExecuteReader();
                        objDR3.Read();
                        manufacturer_id = objDR3["manufacturer_id"].ToString();


                        if (manufacturer_id == "")
                        {
                            cmd2.Parameters.Clear();
                            cmd2.CommandText = exec_prod + " select * from manufacturer where manufacturer_description = '" + objDR3["manufacturer_description"].ToString() + "' and domain_id = 1";
                            objDR2 = cmd2.ExecuteReader();
                            if (objDR2.Read())
                                manufacturer_id = objDR2["manufacturer_id"].ToString();

                            objDR2.Close();

                            if (manufacturer_id == "")
                            {
                                cmd2.Parameters.Clear();
                                cmd2.CommandText = exec_prod + " insert into manufacturer(manufacturer_description, date_added, added_by, comment, domain_id) values(@value2, @value3,@value4,@value5,@value6);";
                                cmd2.Parameters.AddWithValue("@value2", objDR3["manufacturer_description"]);
                                cmd2.Parameters.AddWithValue("@value3", objDR3["date_added"]);
                                cmd2.Parameters.AddWithValue("@value4", objDR3["added_by"]);
                                cmd2.Parameters.AddWithValue("@value5", objDR3["comment"]);
                                cmd2.Parameters.AddWithValue("@value6", 1);
                                cmd2.ExecuteNonQuery();

                                cmd2.CommandText = exec_prod + " select max(manufacturer_id) as max from manufacturer where domain_id = 1";
                                objDR2 = cmd2.ExecuteReader();
                                objDR2.Read();
                                manufacturer_id = objDR2["max"].ToString();
                                objDR2.Close();
                            }

                        }

                        objDR3.Close();

                    }

                    cmd2.Parameters.Clear();
                    cmd2.CommandText = exec_prod + " insert into assets(asset_code,manufacturer_id,asset_description,subcategory_id,height,width,depth,weight,serial_number,min_cost,max_cost,avg_cost,last_cost,default_resp,cut_sheet,date_added,added_by,comment,cad_block,water,plumbing,data,electrical,mobile,blocking,medgas,supports,discontinued,last_budget_update,photo,eq_measurement_id,water_option,plumbing_option,data_option,electrical_option,mobile_option,blocking_option,medgas_option,supports_option,revit,placement,clearance_left,clearance_right,clearance_front,clearance_back,clearance_top,clearance_bottom,volts,phases,hertz,amps,volt_amps,watts,cfm,btus,misc_ase,misc_ada,misc_seismic,misc_antimicrobial,misc_ecolabel,misc_ecolabel_desc,mapping_code,medgas_oxygen,medgas_nitrogen,medgas_air,medgas_n2o,medgas_vacuum,medgas_wag,medgas_co2,medgas_other,medgas_steam,medgas_natgas,plu_hot_water,plu_drain,plu_cold_water,plu_return,plu_treated_water,plu_relief,plu_chilled_water,serial_name,useful_life,loaded_weight,ship_weight,alternate_asset,updated_at,domain_id,manufacturer_domain_id,no_options,no_colors, subcategory_domain_id) values(@value2,@value3,@value4,@value5,@value6,@value7,@value8,@value9,@value10,@value11,@value12,@value13,@value14,@value15,@value16,@value17,@value18,@value19,@value20,@value21,@value22,@value23,@value24,@value25,@value26,@value27,@value28,@value29,@value30,@value31,@value32,@value33,@value34,@value35,@value36,@value37,@value38,@value39,@value40,@value41,@value42,@value43,@value44,@value45,@value46,@value47,@value48,@value49,@value50,@value51,@value52,@value53,@value54,@value55,@value56,@value57,@value58,@value59,@value60,@value61,@value62,@value63,@value64,@value65,@value66,@value67,@value68,@value69,@value70,@value71,@value72,@value73,@value74,@value75,@value76,@value77,@value78,@value79,@value80,@value81,@value82,@value83,@value84,@value85,@value86,@value87,@value88,@value89,@value90,@value91);";

                    cmd5.CommandText = exec_prod + "select cast((replicate('0',5-len(coalesce(MAX(SUBSTRING(ASSET_CODE, 4, 5)),0)+1))) as varchar) + cast((coalesce(MAX(SUBSTRING(ASSET_CODE, 4, 5)),0)+1) as varchar) as max_asset_code from assets where domain_id = 1 and asset_code like '" + objDR["equipment_code"].ToString().Substring(0, 3) + "%'";
                    objDR5 = cmd5.ExecuteReader();
                    objDR5.Read();
                    var asset_code = objDR["equipment_code"].ToString().Substring(0, 3) + objDR5["max_asset_code"].ToString();
                    objDR5.Close();

                    //asset_code = asset_code.Substring(0, 3) + "0" + asset_code.Substring(3, asset_code.Length - 3);

                    cmd2.Parameters.AddWithValue("@value2", asset_code);
                    cmd2.Parameters.AddWithValue("@value3", manufacturer_id);
                    cmd2.Parameters.AddWithValue("@value4", objDR["equipment_desc"]);
                    cmd2.Parameters.AddWithValue("@value5", subcategory_id);
                    cmd2.Parameters.AddWithValue("@value6", objDR["height"]);
                    cmd2.Parameters.AddWithValue("@value7", objDR["width"]);
                    cmd2.Parameters.AddWithValue("@value8", objDR["depth"]);
                    cmd2.Parameters.AddWithValue("@value9", objDR["weight"]);
                    cmd2.Parameters.AddWithValue("@value10", objDR["serial_number"]);
                    cmd2.Parameters.AddWithValue("@value11", objDR["min_cost"]);
                    cmd2.Parameters.AddWithValue("@value12", objDR["max_cost"]);
                    cmd2.Parameters.AddWithValue("@value13", objDR["avg_cost"]);
                    cmd2.Parameters.AddWithValue("@value14", objDR["last_cost"]);
                    cmd2.Parameters.AddWithValue("@value15", objDR["default_resp"]);
                    cmd2.Parameters.AddWithValue("@value16", objDR["cut_sheet"]);
                    cmd2.Parameters.AddWithValue("@value17", objDR["date_added"]);
                    cmd2.Parameters.AddWithValue("@value18", objDR["added_by"]);
                    cmd2.Parameters.AddWithValue("@value19", objDR["comment"]);
                    cmd2.Parameters.AddWithValue("@value20", objDR["cad_block"]);
                    cmd2.Parameters.AddWithValue("@value21", objDR["water"]);
                    cmd2.Parameters.AddWithValue("@value22", objDR["plumbing"]);
                    cmd2.Parameters.AddWithValue("@value23", objDR["data"]);
                    cmd2.Parameters.AddWithValue("@value24", objDR["electrical"]);
                    cmd2.Parameters.AddWithValue("@value25", objDR["mobile"]);
                    cmd2.Parameters.AddWithValue("@value26", objDR["blocking"]);
                    cmd2.Parameters.AddWithValue("@value27", objDR["medgas"]);
                    cmd2.Parameters.AddWithValue("@value28", objDR["supports"]);
                    if (objDR["discontinued"].ToString() == "Y")
                        cmd2.Parameters.AddWithValue("@value29", 1);
                    else
                        cmd2.Parameters.AddWithValue("@value29", objDR["discontinued"]);
                    cmd2.Parameters.AddWithValue("@value30", objDR["last_budget_update"]);
                    cmd2.Parameters.AddWithValue("@value31", objDR["photo"]);
                    cmd2.Parameters.AddWithValue("@value32", objDR["eq_measurement_id"]);
                    cmd2.Parameters.AddWithValue("@value33", objDR["water_option"]);
                    cmd2.Parameters.AddWithValue("@value34", objDR["plumbing_option"]);
                    cmd2.Parameters.AddWithValue("@value35", objDR["data_option"]);
                    cmd2.Parameters.AddWithValue("@value36", objDR["electrical_option"]);
                    cmd2.Parameters.AddWithValue("@value37", objDR["mobile_option"]);
                    cmd2.Parameters.AddWithValue("@value38", objDR["blocking_option"]);
                    cmd2.Parameters.AddWithValue("@value39", objDR["medgas_option"]);
                    cmd2.Parameters.AddWithValue("@value40", objDR["supports_option"]);
                    cmd2.Parameters.AddWithValue("@value41", objDR["revit"]);
                    cmd2.Parameters.AddWithValue("@value42", objDR["placement"]);
                    cmd2.Parameters.AddWithValue("@value43", objDR["clearance_left"]);
                    cmd2.Parameters.AddWithValue("@value44", objDR["clearance_right"]);
                    cmd2.Parameters.AddWithValue("@value45", objDR["clearance_front"]);
                    cmd2.Parameters.AddWithValue("@value46", objDR["clearance_back"]);
                    cmd2.Parameters.AddWithValue("@value47", objDR["clearance_top"]);
                    cmd2.Parameters.AddWithValue("@value48", objDR["clearance_bottom"]);
                    cmd2.Parameters.AddWithValue("@value49", objDR["volts"]);
                    cmd2.Parameters.AddWithValue("@value50", objDR["phases"]);
                    cmd2.Parameters.AddWithValue("@value51", objDR["hertz"]);
                    cmd2.Parameters.AddWithValue("@value52", objDR["amps"]);
                    cmd2.Parameters.AddWithValue("@value53", objDR["volt_amps"]);
                    cmd2.Parameters.AddWithValue("@value54", objDR["watts"]);
                    cmd2.Parameters.AddWithValue("@value55", objDR["cfm"]);
                    cmd2.Parameters.AddWithValue("@value56", objDR["btus"]);
                    cmd2.Parameters.AddWithValue("@value57", objDR["misc_ase"]);
                    cmd2.Parameters.AddWithValue("@value58", objDR["misc_ada"]);
                    cmd2.Parameters.AddWithValue("@value59", objDR["misc_seismic"]);
                    cmd2.Parameters.AddWithValue("@value60", objDR["misc_antimicrobial"]);
                    cmd2.Parameters.AddWithValue("@value61", objDR["misc_ecolabel"]);
                    cmd2.Parameters.AddWithValue("@value62", objDR["misc_ecolabel_desc"]);
                    cmd2.Parameters.AddWithValue("@value63", objDR["mapping_code"]);
                    cmd2.Parameters.AddWithValue("@value64", objDR["medgas_oxygen"]);
                    cmd2.Parameters.AddWithValue("@value65", objDR["medgas_nitrogen"]);
                    cmd2.Parameters.AddWithValue("@value66", objDR["medgas_air"]);
                    cmd2.Parameters.AddWithValue("@value67", objDR["medgas_n2o"]);
                    cmd2.Parameters.AddWithValue("@value68", objDR["medgas_vacuum"]);
                    cmd2.Parameters.AddWithValue("@value69", objDR["medgas_wag"]);
                    cmd2.Parameters.AddWithValue("@value70", objDR["medgas_co2"]);
                    cmd2.Parameters.AddWithValue("@value71", objDR["medgas_other"]);
                    cmd2.Parameters.AddWithValue("@value72", objDR["medgas_steam"]);
                    cmd2.Parameters.AddWithValue("@value73", objDR["medgas_natgas"]);
                    cmd2.Parameters.AddWithValue("@value74", objDR["plu_hot_water"]);
                    cmd2.Parameters.AddWithValue("@value75", objDR["plu_drain"]);
                    cmd2.Parameters.AddWithValue("@value76", objDR["plu_cold_water"]);
                    cmd2.Parameters.AddWithValue("@value77", objDR["plu_return"]);
                    cmd2.Parameters.AddWithValue("@value78", objDR["plu_treated_water"]);
                    cmd2.Parameters.AddWithValue("@value79", objDR["plu_relief"]);
                    cmd2.Parameters.AddWithValue("@value80", objDR["plu_chilled_water"]);
                    cmd2.Parameters.AddWithValue("@value81", objDR["serial_name"]);
                    cmd2.Parameters.AddWithValue("@value82", objDR["useful_life"]);
                    cmd2.Parameters.AddWithValue("@value83", objDR["loaded_weight"]);
                    cmd2.Parameters.AddWithValue("@value84", objDR["ship_weight"]);
                    cmd2.Parameters.AddWithValue("@value85", objDR["alternate_equip"]);
                    cmd2.Parameters.AddWithValue("@value86", objDR["updated_at"]);
                    cmd2.Parameters.AddWithValue("@value87", 1);
                    cmd2.Parameters.AddWithValue("@value88", 1);
                    cmd2.Parameters.AddWithValue("@value89", objDR["no_options"]);
                    cmd2.Parameters.AddWithValue("@value90", objDR["no_colors"]);
                    cmd2.Parameters.AddWithValue("@value91", 1);

                    cmd2.ExecuteNonQuery();

                    cmd2.CommandText = exec_prod + " select max(asset_id) as max from assets where domain_id = 1";
                    objDR2 = cmd2.ExecuteReader();
                    objDR2.Read();
                    asset_id = objDR2["max"].ToString();
                    objDR2.Close();

                    cmd3.CommandText = "update equipment set new_equipment_id = " + asset_id + ", new_asset_code = '" + asset_code + "', new_equipment_domain_id = 1, migrated=true  where equipment_id = " + objDR["equipment_id"] + " and domain_id = 3";
                    cmd3.ExecuteNonQuery();

                    objDR.Close();
                }
                //}
                objDR4.Close();

                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#27', 'Angle scale reads 1° increments.')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#25', 'Cards are 8 1/2 x 11‚ (22 x 28 cm)')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#3', '”,')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#4', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = @"update assets set comment = replace(comment, '#5', '""')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#7', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#9', '©')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#10', ' ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#11', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#13', '½')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#14', '¼')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#15', 'º')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#16', '“')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#17', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#18', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#19', 'µ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#20', '±')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#21', '—')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#22', '‘')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#23', '¹')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#24', 'é')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#26', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#28', '³')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#29', 'ä')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#30', '¾')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#1', '®')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#2', '™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#31', 'The N7500 cabinets are equipped with multiple handpieces, assistants instrumen­tation, self-contained patient water, CPU storage, monitor and keyboard supports, and properly-sized drawers to hold all materials necessary for treating patients.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set comment = replace(comment, '#32', 'Stereoscopic continuous 4.5 – 20x zoom Nikon optics for optimal viewing and image magnification. Dual zoom knobs on the optical head allow for left or right handed operation. Convenient fine focus control handle sharpens the image. USB Video Camera option. Includes: compact center stand with a spring loaded main shaft for gross height adjustment, with the addition of fine height adjustment.');";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set plumbing = replace(plumbing, '#1', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' aa ', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' bb ', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' cc ', '”™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' dd ', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' ee ', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' ff ', '/')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' gg ', 'œ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' hh ', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set asset_description = replace(asset_description, ' ii ', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' aa ', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' bb ', '°')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' cc ', '”™')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' dd ', '”')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' ee ', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' ff ', '/')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' gg ', 'œ')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' hh ', '’')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_name = replace(serial_name, ' ii ', '²')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set serial_number = replace(serial_number, '#1', '-')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set electrical = replace(electrical, '#1', '×')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set volts = replace(volts, '*1', '–')";
                cmd2.ExecuteNonQuery();
                cmd2.CommandText = exec_prod + "update assets set volts = replace(volts, '*2', '-')";
                cmd2.ExecuteNonQuery();

                label5.Text = "\rFoi";
                label5.Refresh();

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            label5.Text = "Starting migration...";
            label5.Refresh();


            var dbCon = new NpgsqlConnection(postgres_conn);
            var cmd = dbCon.CreateCommand();
            NpgsqlDataReader objDR;

            var dbCon2 = new SqlConnection(sql_conn);
            var cmd2 = dbCon2.CreateCommand();

            dbCon.Open();
            dbCon2.Open();

            exec_prod = "EXEC sp_set_session_context 'domain_id', '1'; ";

            //INICIO ASSETS
            label5.Text = label5.Text + "\r Starting assets";
            label5.Refresh();
            cmd.CommandText = "select equipment_id, min_cost, max_cost, avg_cost, last_cost from equipment where domain_id = 1";
            cmd.CommandTimeout = 40;
            objDR = cmd.ExecuteReader();

            while (objDR.Read())
            {
                cmd2.Parameters.Clear();
                cmd2.CommandText = exec_prod + "update assets set min_cost = @min_cost, max_cost = @max_cost, avg_cost = @avg_cost, last_cost = @last_cost where domain_id = 1 and asset_id = " + objDR["equipment_id"];
                cmd2.Parameters.AddWithValue("@min_cost", objDR["min_cost"]);
                cmd2.Parameters.AddWithValue("@max_cost", objDR["max_cost"]);
                cmd2.Parameters.AddWithValue("@avg_cost", objDR["avg_cost"]);
                cmd2.Parameters.AddWithValue("@last_cost", objDR["last_cost"]);
                cmd2.ExecuteNonQuery();

            }

            label5.Text = "The end";
            label5.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label5.Text = "Starting migration...";
            label5.Refresh();


            var dbCon = new NpgsqlConnection(postgres_conn);
            var cmd = dbCon.CreateCommand();
            NpgsqlDataReader objDR;

            var dbCon2 = new SqlConnection(sql_conn);
            var cmd2 = dbCon2.CreateCommand();

            dbCon.Open();
            dbCon2.Open();

            exec_prod = "EXEC sp_set_session_context 'domain_id', '1'; ";

            //INICIO ASSETS
            label5.Text = label5.Text + "\r Starting assets";
            label5.Refresh();
            cmd.CommandText = "select equipment_id, cut_sheet from equipment where domain_id = 1 and cut_sheet is not null";
            cmd.CommandTimeout = 40;
            objDR = cmd.ExecuteReader();

            while (objDR.Read())
            {
                cmd2.Parameters.Clear();
                cmd2.CommandText = exec_prod + "update assets set cut_sheet = @cutsheet where domain_id = 1  and ('0123456789') LIKE '%' + substring(cut_sheet,1,1) + '%' and asset_id = " + objDR["equipment_id"];
                cmd2.Parameters.AddWithValue("@cutsheet", objDR["cut_sheet"]);
                cmd2.ExecuteNonQuery();

            }

            label5.Text = "The end";
            label5.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles("C:\\Need to upload to new site");
            string line = "";
            foreach (var item in files)
            {
                var file_name_full = item.Split('\\')[item.Split('\\').Length - 1];
                var file_name = file_name_full.Split('.')[0];
                var file_ext = file_name_full.Split('.')[1];


                switch (file_ext)
                {
                    case "jpg":
                        line += "update assets set photo = '" + file_name_full + "' where asset_code = '" + file_name + "';\n ";
                        break;
                    case "pdf":
                        line += "update assets set cut_sheet = '" + file_name_full + "' where asset_code = '" + file_name + "';\n ";
                        break;
                    default:
                        if (file_name != "arquivos")
                            line += "ERROOOOORRRRR:  " + file_name_full + ";\n ";
                        break;
                }
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Need to upload to new site\arquivos.txt"))
            {
                file.WriteLine(line);
            }

            label5.Text = "The end";
            label5.Refresh();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            //postgres_conn = "User Id=postgres;server=localhost;Database=audaxware;Pooling=false;Password=postgres;";
            var filePath = @"C:\import\files\SunagMed171106.xlsx";

            var domain_id = (domains.SelectedItem as dynamic).Value;
            if (string.IsNullOrEmpty(domain_id))
            {
                label4.Text = "Choose a domain";
            }
            else
            {

                label5.Text = "Starting migration...";
                label5.Refresh();

                var project_id = 0;
                var phase_id = 0;
                var subcategory_id = 0;
                var dbCon = new SqlConnection(sql_conn);
                var cmd = dbCon.CreateCommand();
                dbCon.Open();

                exec_prod = "EXEC sp_set_session_context 'domain_id', '1'; ";

                //create project, phase, category, subcategory, facility, 
                //prod
                if (database.SelectedValue.ToString() == "1")
                {
                    project_id = 342; //
                    phase_id = 22350; //
                    subcategory_id = 11122; //
                }
                else if (database.SelectedValue.ToString() == "2")
                {
                    //todo
                }
                else if (database.SelectedValue.ToString() == "3")
                {
                    project_id = 325;//342; //
                    phase_id = 660;//22350; //
                    subcategory_id = 6194;//11122; //
                }

                //local

                var added_by = "juliana.barros@audaxware.com";


                using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["final"];

                    int numRows = worksheet.Dimension.End.Row;
                    while (numRows > 1)
                    {
                        if (worksheet.Cells[numRows, 2].Value != null && !worksheet.Cells[numRows, 2].Value.ToString().Equals(""))//&& !ignore.Contains(worksheet.Cells[numRows, 2].Value.ToString()))
                        {
                            var cad_id = worksheet.Cells[numRows, 1].Value == null ? "" : worksheet.Cells[numRows, 1].Value.ToString();
                            var equipment = worksheet.Cells[numRows, 2].Value.ToString();
                            var qty = worksheet.Cells[numRows, 3].Value.ToString();
                            var room_name = worksheet.Cells[numRows, 4].Value == null ? "" : worksheet.Cells[numRows, 4].Value.ToString();
                            var room_number = worksheet.Cells[numRows, 5].Value == null ? "" : worksheet.Cells[numRows, 5].Value.ToString();
                            var manufacturer = worksheet.Cells[numRows, 6].Value == null ? "" : worksheet.Cells[numRows, 6].Value.ToString();
                            var model_name = worksheet.Cells[numRows, 7].Value == null ? "" : worksheet.Cells[numRows, 7].Value.ToString();
                            var model_number = worksheet.Cells[numRows, 8].Value == null ? "" : worksheet.Cells[numRows, 8].Value.ToString();
                            var cost = worksheet.Cells[numRows, 9].Value == null ? "0" : worksheet.Cells[numRows, 9].Value.ToString();
                            var cost_center = worksheet.Cells[numRows, 11].Value == null ? "" : worksheet.Cells[numRows, 11].Value.ToString();
                            var tag = worksheet.Cells[numRows, 12].Value == null ? "" : worksheet.Cells[numRows, 12].Value.ToString();
                            var resp = worksheet.Cells[numRows, 13].Value == null ? "" : worksheet.Cells[numRows, 13].Value.ToString();
                            var dept = worksheet.Cells[numRows, 14].Value.ToString();


                            var manufacturer_id = "";
                            cmd.Parameters.Clear();
                            cmd.CommandText = exec_prod + "select manufacturer_id from manufacturer where manufacturer_description = @value1 and domain_id = 15";
                            cmd.Parameters.AddWithValue("@value1", manufacturer);
                            var reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                manufacturer_id = reader["manufacturer_id"].ToString();
                                reader.Close();
                            }
                            else
                            {
                                reader.Close();
                                cmd.Parameters.Clear();
                                cmd.CommandText = exec_prod + "insert into manufacturer(manufacturer_description, date_added, added_by, domain_id) values(@value1, @value2, @value3, @value4)";
                                cmd.Parameters.AddWithValue("@value1", manufacturer);
                                cmd.Parameters.AddWithValue("@value2", DateTime.Now);
                                cmd.Parameters.AddWithValue("@value3", added_by);
                                cmd.Parameters.AddWithValue("@value4", 15);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = exec_prod + "select max(manufacturer_id) max from manufacturer where domain_id = 15";
                                reader = cmd.ExecuteReader();
                                reader.Read();
                                manufacturer_id = reader["max"].ToString();
                                reader.Close();
                            }

                            var equipment_id = "";
                            cmd.Parameters.Clear();
                            cmd.CommandText = exec_prod + "select asset_id from assets where asset_suffix = @value1 and domain_id = 15";
                            cmd.Parameters.AddWithValue("@value1", equipment);
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                equipment_id = reader["asset_id"].ToString();
                                reader.Close();
                            }
                            else
                            {
                                reader.Close();
                                var asset_code = "";
                                cmd.Parameters.Clear();
                                cmd.CommandText = exec_prod + "select 'MED' + convert(varchar, COALESCE(MAX(substring(asset_code, 4, 5)),9999)+1) AS CODE from assets where substring(asset_code, 1, 3) = 'MED' and domain_id = 15";
                                reader = cmd.ExecuteReader();
                                reader.Read();
                                asset_code = reader["CODE"].ToString();
                                reader.Close();

                                cmd.Parameters.Clear();
                                cmd.CommandText = exec_prod + "insert into assets(asset_code, manufacturer_id, manufacturer_domain_id, asset_description, subcategory_id, subcategory_domain_id, serial_number, serial_name, default_resp, date_added, added_by, eq_measurement_id, domain_id, asset_suffix) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10, @value11, @value12, @value14, @value15)";
                                cmd.Parameters.AddWithValue("@value1", asset_code);
                                cmd.Parameters.AddWithValue("@value2", manufacturer_id);
                                cmd.Parameters.AddWithValue("@value3", 15);
                                cmd.Parameters.AddWithValue("@value4", "MED, " + equipment);
                                cmd.Parameters.AddWithValue("@value5", subcategory_id);
                                cmd.Parameters.AddWithValue("@value6", 15);
                                cmd.Parameters.AddWithValue("@value7", model_number);
                                cmd.Parameters.AddWithValue("@value8", model_name);
                                cmd.Parameters.AddWithValue("@value9", resp);
                                cmd.Parameters.AddWithValue("@value10", DateTime.Now);
                                cmd.Parameters.AddWithValue("@value11", added_by);
                                cmd.Parameters.AddWithValue("@value12", 1);
                                cmd.Parameters.AddWithValue("@value14", 15);
                                cmd.Parameters.AddWithValue("@value15", equipment);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = exec_prod + "select max(asset_id) max from assets where domain_id = 15";
                                reader = cmd.ExecuteReader();
                                reader.Read();
                                equipment_id = reader["max"].ToString();
                                reader.Close();
                            }


                            //get department
                            var department_id = "";
                            cmd.CommandText = exec_prod + "select department_id from project_department where project_id = " + project_id + " and phase_id = " + phase_id + " and lower(description) = lower('" + dept + "') and domain_id = 15";
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                department_id = reader["department_id"].ToString();
                                reader.Close();
                            }
                            else
                            {
                                reader.Close();
                                cmd.Parameters.Clear();
                                cmd.CommandText = exec_prod + "insert into project_department(project_id, phase_id, description, department_type_id, department_type_domain_id, domain_id, date_added, added_by) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8)";
                                cmd.Parameters.AddWithValue("@value1", project_id);
                                cmd.Parameters.AddWithValue("@value2", phase_id);
                                cmd.Parameters.AddWithValue("@value3", dept);
                                cmd.Parameters.AddWithValue("@value4", 22);
                                cmd.Parameters.AddWithValue("@value5", 1);
                                cmd.Parameters.AddWithValue("@value6", 15);
                                cmd.Parameters.AddWithValue("@value7", DateTime.Now);
                                cmd.Parameters.AddWithValue("@value8", added_by);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = exec_prod + "select max(department_id) max from project_department where domain_id = 15";
                                reader = cmd.ExecuteReader();
                                reader.Read();
                                department_id = reader["max"].ToString();
                                reader.Close();
                            }

                            //get cost_center
                            //var cc_id = "";
                            //cmd.CommandText = exec_prod + "select id from cost_center where project_id = " + project_id + "and code = '" + cost_center + "' and domain_id = 15";
                            //reader = cmd.ExecuteReader();
                            //if (reader.Read())
                            //{
                            //    cc_id = reader["id"].ToString();
                            //    reader.Close();
                            //}
                            //else
                            //{
                            //    reader.Close();
                            //    cmd.Parameters.Clear();
                            //    cmd.CommandText = exec_prod + "insert into cost_center(code, description, project_id, domain_id) values(@value1, @value2, @value3, @value4)";
                            //    cmd.Parameters.AddWithValue("@value1", cost_center);
                            //    cmd.Parameters.AddWithValue("@value2", cost_center);
                            //    cmd.Parameters.AddWithValue("@value3", project_id);
                            //    cmd.Parameters.AddWithValue("@value4", 15);
                            //    cmd.ExecuteNonQuery();

                            //    cmd.CommandText = exec_prod + "select max(id) max from cost_center where domain_id = 15";
                            //    reader = cmd.ExecuteReader();
                            //    reader.Read();
                            //    cc_id = reader["max"].ToString();
                            //    reader.Close();
                            //}


                            //get room
                            var room_id = "";
                            cmd.CommandText = exec_prod + "select room_id from project_room where project_id = " + project_id + " and phase_id = " + phase_id + " and department_id = " + department_id + " and lower(drawing_room_number) = lower('" + room_number + "') and lower(drawing_room_name) = lower('" + room_name + "')";
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                room_id = reader["room_id"].ToString();
                                reader.Close();
                            }
                            else
                            {
                                reader.Close();
                                cmd.Parameters.Clear();
                                cmd.CommandText = exec_prod + "insert into project_room(project_id, phase_id, department_id, drawing_room_number, drawing_room_name, final_room_number, final_room_name, room_quantity, domain_id, date_added, added_by) values(@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9, @value10, @value11)";
                                cmd.Parameters.AddWithValue("@value1", project_id);
                                cmd.Parameters.AddWithValue("@value2", phase_id);
                                cmd.Parameters.AddWithValue("@value3", department_id);
                                cmd.Parameters.AddWithValue("@value4", room_number);
                                cmd.Parameters.AddWithValue("@value5", room_name);
                                cmd.Parameters.AddWithValue("@value6", room_number);
                                cmd.Parameters.AddWithValue("@value7", room_name);
                                cmd.Parameters.AddWithValue("@value8", 1);
                                cmd.Parameters.AddWithValue("@value9", 15);
                                cmd.Parameters.AddWithValue("@value10", DateTime.Now);
                                cmd.Parameters.AddWithValue("@value11", added_by);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = exec_prod + "select max(room_id) max from project_room where domain_id = 15";
                                reader = cmd.ExecuteReader();
                                reader.Read();
                                room_id = reader["max"].ToString();
                                reader.Close();
                            }

                            //check if equipment already exist
                            var total = 0;
                            if (cad_id.Length > 25)
                            {
                                cad_id = cad_id.Substring(1, 25);
                            }
                            cmd.CommandText = exec_prod + "select count(*) as total from project_room_inventory where domain_id = 15 and project_id = " + project_id + " and phase_id = " + phase_id + " and department_id = " + department_id + " and room_id = " + room_id + " and asset_id = " + equipment_id + " and cad_id = '" + cad_id + "'";
                            reader = cmd.ExecuteReader();
                            reader.Read();
                            total = Convert.ToInt32(reader["total"]);
                            reader.Close();
                            if (total == 0)
                            {
                                if (cost == "in line 11")
                                {
                                    cost = "0";
                                }
                                cmd.CommandText = exec_prod + "insert into project_room_inventory(project_id, department_id, room_id, asset_id, status, budget_qty, unit_budget, current_location,date_added, added_by, cad_id, phase_id, domain_id, tag, resp, cost_center_id, asset_domain_id)";
                                cmd.CommandText = cmd.CommandText + " values(" + project_id + ", " + department_id + ", " + room_id + ", " + equipment_id + ", 'A', " + qty + ", " + cost + ", 'Plan', getdate(), '" + added_by + "','" + cad_id + "', " + phase_id + ", 15, '" + tag + "', '" + resp + "', null, 15)";
                                cmd.ExecuteNonQuery();
                            }
                        }
                        numRows--;

                    }
                }
                dbCon.Dispose();
                label5.Text = "The end";
                label5.Refresh();
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            //pequena gambi porque foram criados options no domain da audaxware qeu a hsg tá usando mas nao pode migrar pq vai dar conflito com o que ja tem no sistema novo
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_file");
            string file = Path.Combine(path, "inexistent_assets.txt");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(file))
                File.Create(file);


            var dbCon = new NpgsqlConnection(postgres_conn);
            var cmd = dbCon.CreateCommand();
            NpgsqlDataReader objDR;

            var dbCon2 = new SqlConnection(sql_conn);
            var cmd2 = dbCon2.CreateCommand();
            SqlDataReader objDR2;
            dbCon.Open();
            dbCon2.Open();

            cmd.CommandText = "select distinct equipment_id from project_room_inventory where domain_id = 1 and project_id in(select project_id from project where domain_id = 3)";
            objDR = cmd.ExecuteReader();
            while (objDR.Read())
            {
                cmd2.CommandText = exec_prod1 + "select count(*) as total from assets where domain_id = 1 and asset_id = " + objDR["equipment_id"];
                objDR2 = cmd2.ExecuteReader();
                objDR2.Read();
                if (objDR2["total"].ToString() == "0")
                {
                    var error_msg = objDR["equipment_id"].ToString() + ",";

                    File.AppendAllText(file, error_msg + Environment.NewLine);
                }
                objDR2.Close();
            }

            dbCon.Dispose();
            dbCon2.Dispose();
            label5.Text = "The end";
            label5.Refresh();

        }
    }
}
