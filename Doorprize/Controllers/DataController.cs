using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doorprize.Controllers
{
    public class DataController : Controller
    {
        //
        // GET: /Data/
        private SqlConnection con;
        public ActionResult UnggahData()
        {
            return View();
        }
        public ActionResult DataPemenang()
        {
            return View();
        }

        private void connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["DB_DOORPRIZEConnectionString"].ToString();
            con = new SqlConnection(constr);

        }

        [HttpGet]
        public ActionResult RawData()
        {
            DoorprizeDataContext _dbcontext = new DoorprizeDataContext();
            var data = (from e in _dbcontext.TBL_T_RAW_DATAs select new { e.BARCODE, e.LOCATION, e.DATE}).ToList();
            return Json(new { status = "sukses", data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ListPemenang()
        {
            DoorprizeDataContext _dbcontext = new DoorprizeDataContext();
            var data = (from e in _dbcontext.VW_DATA_PEMENANGs select new { e.HADIAH, e.NRP, e.NAMA, e.DEPT, e.JABATAN }).ToList();
            return Json(new { status = "sukses", data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public bool SubmitRaw()
        {
            connection();
            SqlCommand com = new SqlCommand("SP_DUMPING_DATA_SCAN", con);
            com.CommandType = CommandType.StoredProcedure;
            con.Open();
            int i = com.ExecuteNonQuery();
            con.Close();
            if (i >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult UnggahListData(HttpPostedFileBase postedFile)
        {
            string filePath = string.Empty;
            if (postedFile != null)
            {
                string path = Server.MapPath("@ViewBag.pathParent/Files/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);

                string conString = string.Empty;
                switch (extension)
                {
                    case ".xls": //Excel 97-03.
                        conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                        break;
                    case ".xlsx": //Excel 07 and above.
                        conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                        break;
                }

                DataTable dt = new DataTable();
                conString = string.Format(conString, filePath);

                using (OleDbConnection connExcel = new OleDbConnection(conString))
                {
                    using (OleDbCommand cmdExcel = new OleDbCommand())
                    {
                        using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                        {
                            cmdExcel.Connection = connExcel;

                            //Get the name of First Sheet.
                            connExcel.Open();
                            DataTable dtExcelSchema;
                            dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            connExcel.Close();

                            //Read Data from First Sheet.
                            connExcel.Open();
                            cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                            odaExcel.SelectCommand = cmdExcel;
                            odaExcel.Fill(dt);
                            connExcel.Close();
                        }
                    }
                }

                conString = ConfigurationManager.ConnectionStrings["DB_DOORPRIZEConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(conString))
                {
                    try
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                        {
                            //Set the database table name.
                            sqlBulkCopy.DestinationTableName = "dbo.TBL_T_RAW_DATA";
                            var ID = Guid.NewGuid();
                            //[OPTIONAL]: Map the Excel columns with that of the database table
                            sqlBulkCopy.ColumnMappings.Add("BARCODE", "BARCODE");
                            sqlBulkCopy.ColumnMappings.Add("LOCATION", "LOCATION");
                            sqlBulkCopy.ColumnMappings.Add("QUANTITY", "QUANTITY");
                            sqlBulkCopy.ColumnMappings.Add("DATE", "DATE");

                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);
                            con.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append("<script type = 'text/javascript'>");
                        sb.Append("window.onload=function(){");
                        sb.Append("alert('");
                        sb.Append(ex.ToString());
                        sb.Append("')};");
                        sb.Append("</script>");
                        //return Content("<script type='text/javascript'>alert('Hello there');</script>"); //You can get the alert with this line also
                        return Content(sb.ToString(), "text/javascript");
                    }
                }
            }

            return RedirectToAction("UnggahData", "Data");
        }
	}
}