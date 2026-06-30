using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewTASUI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace NewTASUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult GetLicenseStatus()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    return Json(new { success = false, error = "Connection string missing" });
                }

                using var con = new SqlConnection(connStr);
                con.Open();

                using var cmd = new SqlCommand(@"
            SELECT TOP 1 LMStatus 
            FROM TASLicense 
            ORDER BY TASLicenseId DESC
        ", con);

                var result = cmd.ExecuteScalar();

                bool status = result != null && result != DBNull.Value && Convert.ToBoolean(result);

                return Json(new
                {
                    success = true,
                    lmStatus = status
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GetServerStatus()
        {
            try
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                con.Open();

                using var cmd = new SqlCommand(@"
            SELECT ServerTyp, IsOk, IsTASRunning
            FROM TASServerRedudancy
            WHERE TASServerRedundancyId IN (
                SELECT MAX(TASServerRedundancyId)
                FROM TASServerRedudancy
                GROUP BY ServerTyp
            )
        ", con);

                using var reader = cmd.ExecuteReader();
                string primaryColor = "warn";
                string secondaryColor = "warn";

                while (reader.Read())
                {
                    string type = reader["ServerTyp"]?.ToString() ?? "";

                    bool isOk = (bool)reader["IsOk"];
                    bool isRunning = reader["IsTASRunning"] != DBNull.Value && (bool)reader["IsTASRunning"];

                    string statusClass;

                    if (!isOk && !isRunning)
                        statusClass = "warn";      // RED
                    else if (isOk && !isRunning)
                        statusClass = "amber";     // YELLOW
                    else
                        statusClass = "ok";        // GREEN

                    if (type == "P")
                        primaryColor = statusClass;
                    else if (type == "S")
                        secondaryColor = statusClass;
                }

                return Json(new
                {
                    success = true,
                    primary = primaryColor,
                    secondary = secondaryColor
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetCompanyInfo()
        {
            try
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                con.Open();

                using var cmd = new SqlCommand(@"
            SELECT TOP 1 CompanyName, State
            FROM CompanyInfo
            ORDER BY CompanyId DESC
        ", con);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string company = reader["CompanyName"]?.ToString() ?? "";
                    string state = reader["State"]?.ToString() ?? "";

                    return Json(new
                    {
                        success = true,
                        text = $"{company} - {state}"
                    });
                }

                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
