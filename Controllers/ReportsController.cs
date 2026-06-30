using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using NewTASUI.Models;
using System.Data;
using Microsoft.Data.SqlClient;


namespace NewTASUI.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IConfiguration _configuration;

        public ReportsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult GetBays()
        {
            var bays = new List<object>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT BayId, BayName FROM Bay ORDER BY BayId", con))
                {
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bays.Add(new
                            {
                                id = reader["BayId"],
                                name = reader["BayName"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(bays);
        }
        //  API FOR UI (TABLE)
        [HttpPost]
        public IActionResult GetReportData([FromBody] ReportFilterModel filter)
        {
            var data = new List<object>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("usp_Rpt_BayWise", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    DateTime? fromDate = string.IsNullOrEmpty(filter?.FromDate)
                        ? null
                        : DateTime.Parse(filter.FromDate);

                    DateTime? toDate = string.IsNullOrEmpty(filter?.ToDate)
                        ? null
                        : DateTime.Parse(filter.ToDate);

                    cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                dateAndTime = reader["DateAndTime"],
                                gantry = reader["Gantry"],
                                bayNo = reader["BayNo"],
                                fanNumber = reader["FANNUMBER"],
                                truckRegNo = reader["TruckRegNo"],
                                preset = reader["Preset"],
                                baseQty = reader["BaseQty"],
                                blendQty = reader["BlendQty"],
                                add1Qty = reader["Add1Qty"],
                                add2Qty = reader["Add2Qty"],
                                qtyFilled = reader["QtyFilled"],
                                topUpQty = reader["TopUpQty"],
                                decantQty = reader["DecantQty"],
                                effectiveQty = reader["EffectiveQty"]
                            });
                        }
                    }
                }
            }

            return Json(data);
        }

        // ✅ PDF EXPORT (USING SAME SP)
        public IActionResult TestPDF()
        {
            var report = new LocalReport();

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "RptBayWise.rdlc"
            );

            report.ReportPath = path;

            // 🔥 CALL SAME METHOD (NO DUPLICATION)
            var data = GetDataFromSP();

            report.DataSources.Add(
                new ReportDataSource("DataSetBayWise", data)
            );

            report.SetParameters(new[]
            {
                new ReportParameter("CompanyName", "BPCL WARANGAL DEPOT"),
                new ReportParameter("CompanyAddress1", "Warangal, Telangana"),
                new ReportParameter("CompanyAddress2", "India"),
                new ReportParameter("FromDate", DateTime.Now.ToString("dd-MM-yyyy")),
                new ReportParameter("ToDate", DateTime.Now.ToString("dd-MM-yyyy")),
                new ReportParameter("BayNo", "ALL")
            });

            var result = report.Render("PDF");

            return File(result, "application/pdf", "BayWiseReport.pdf");
        }

        // 🔥 COMMON METHOD FOR SP (REUSABLE)
        private List<object> GetDataFromSP()
        {
            var data = new List<object>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("usp_Rpt_BayWise", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                DateAndTime = reader["DateAndTime"],
                                Gantry = reader["Gantry"],
                                BayNo = reader["BayNo"],
                                FANNUMBER = reader["FANNUMBER"],
                                TruckRegNo = reader["TruckRegNo"],
                                Preset = reader["Preset"],
                                BaseQty = reader["BaseQty"],
                                BlendQty = reader["BlendQty"],
                                Add1Qty = reader["Add1Qty"],
                                Add2Qty = reader["Add2Qty"],
                                QtyFilled = reader["QtyFilled"],
                                TopUpQty = reader["TopUpQty"],
                                DecantQty = reader["DecantQty"],
                                EffectiveQty = reader["EffectiveQty"]
                            });
                        }
                    }
                }
            }

            return data;
        }

        public IActionResult BayWise()
        {
            return View();
        }
    }
}