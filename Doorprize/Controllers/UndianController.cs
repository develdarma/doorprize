using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doorprize.Controllers
{
    public class UndianController : Controller
    {
        //
        // GET: /Undian/
        private SqlConnection con;
        public ActionResult Index()
        {
            return View();
        }

        private void connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["DB_DOORPRIZEConnectionString"].ToString();
            con = new SqlConnection(constr);

        }


        [HttpGet]
        public ActionResult ReadHadiah()
        {
            DoorprizeDataContext db = new DoorprizeDataContext();
            return Json(db.TBL_M_HADIAHs.Select(x => new
            {
                PID = x.PID,
                HADIAH = x.HADIAH,
                QTY = x.QTY,
                STATUS = x.STATUS
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReadUndian()
        {
            DoorprizeDataContext db = new DoorprizeDataContext();
            return Json(db.VW_DATA_UNDIs.Select(x => new
            {
                PID = x.PID,
                NRP = x.NRP,
                NAMA = x.NAMA,
                DEPT = x.DEPT,
                JABATAN = x.JABATAN,
            }).ToList().OrderBy(x=> x.NRP), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReadPemenang(string PID)
        {
            DoorprizeDataContext db = new DoorprizeDataContext();
            return Json(db.TBL_T_PEMENANGs.Select(x => new
            {
                PID_HADIAH = x.PID_HADIAH,
                NRP = x.NRP,
                NAMA = x.NAMA
            }).Where(x=>x.PID_HADIAH == PID).ToList(), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult ReadUndian()
        //{
        //    DoorprizeDataContext db = new DoorprizeDataContext();
        //    return Json(db.TBL_T_DATA_UNDIs.Select(x => new
        //    {
        //        PID = x.PID,
        //        NRP = x.NRP,
        //        NAMA = x.NAMA,
        //        DEPT = x.DEPT,
        //        JABATAN = x.JABATAN,
        //    }).ToList(), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public bool InsertPemenang(string PID_HADIAH, string DATA)
        {
            connection();
            SqlCommand com = new SqlCommand("SP_INSERT_PEMENANG", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@PID_HADIAH", PID_HADIAH);
            com.Parameters.AddWithValue("@DATA", DATA);
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
        public bool UpdateStock(string PID_HADIAH)
        {
            connection();
            SqlCommand com = new SqlCommand("SP_UPDATE_STOCK_HADIAH", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@PID", PID_HADIAH);
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
	}
}