using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;
using System.Data;

namespace NewTASUI.Controllers
{
    public class MastersController : Controller
    {
        private readonly IConfiguration _configuration;

        public MastersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ── GET /Masters/Bay ─────────────────────────────────────────────────
        public IActionResult Bay()
        {
            var bayTypes = FetchBayTypes();
            return View(bayTypes);
        }

        // ── GET /Masters/Trucks ───────────────────────────────────────────────
        public IActionResult Trucks()
        {
            return View();
        }
        // ── GET /Masters/Bcu ───────────────────────────────────────────────
        public IActionResult Bcu()
        {
            return View();
        }

        // ── GET /Masters/GetBays ─────────────────────────────────────────────
        // Returns all bays as JSON for the table (called by JS on page load)
        [HttpGet]
        public IActionResult GetBays()
        {
            var bays = new List<object>();

            using (var con = new SqlConnection(ConnStr()))
            using (var cmd = new SqlCommand(@"
                SELECT  b.BayId,
                        b.BayName,
                        b.BayNumber,
                        ISNULL(b.BayDesc, '')   AS BayDesc,
                        b.BayTypeId,
                        ISNULL(bt.Name, '') AS BayType,
                        b.IsActive
                FROM    Bay b
                LEFT JOIN BayType bt ON bt.Id = b.BayTypeId
                WHERE   b.IsDelete = 0
                ORDER BY b.BayId", con))
            {
                con.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bays.Add(new
                    {
                        bayId = reader["BayId"],
                        bayName = reader["BayName"].ToString(),
                        bayNumber = reader["BayNumber"],
                        bayDesc = reader["BayDesc"].ToString(),
                        bayTypeId = reader["BayTypeId"] == DBNull.Value ? 0 : (int)reader["BayTypeId"],
                        bayType = reader["BayType"].ToString(),
                        isActive = (bool)reader["IsActive"]
                    });
                }
            }

            return Json(bays);
        }

        // ── GET /Masters/GetBayTypes ─────────────────────────────────────────
        // Returns BayType lookup list as JSON
        [HttpGet]
        public IActionResult GetBayTypes()
        {
            var types = FetchBayTypes();
            return Json(types.Select(t => new { t.BayTypeId, t.BayTypeName }));
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var list = new List<object>();

            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT ProductId, ProductName FROM Products WHERE ISNULL(IsDelete,0)=0 ORDER BY ProductName", con);

            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new
                {
                    id = Convert.ToInt32(r["ProductId"]),
                    name = r["ProductName"].ToString()
                });
            }

            return Json(list);
        }

        [HttpGet]
        public IActionResult GetBayMaterials(int bayId)
        {
            var list = new List<BayMaterialListModel>();

            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  bp.BCUProductId,
                        bcu.BCUId,
                        ISNULL(bcu.BCUName,'') AS BCUName,
                        bp.ProductId,
                        ISNULL(p.ProductName,'') AS ProductName,
                        ISNULL(bp.IsAdditive,0) AS IsAdditive,
                        ISNULL(bp.BCUArmNo,0) AS BCUArmNo,
                        bp.BCURecipeNo
                FROM    BCUs bcu
                INNER JOIN BCUProducts bp ON bp.BCUId = bcu.BCUId AND ISNULL(bp.IsDelete,0)=0
                INNER JOIN Products p ON p.ProductId = bp.ProductId AND ISNULL(p.IsDelete,0)=0
                WHERE   ISNULL(bcu.IsDelete,0)=0
                  AND   bcu.BayId = @BayId
                ORDER BY p.ProductName, bcu.BCUName, bp.BCURecipeNo", con);

            cmd.Parameters.AddWithValue("@BayId", bayId);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new BayMaterialListModel
                {
                    BCUProductId = Convert.ToInt32(r["BCUProductId"]),
                    BCUId = Convert.ToInt32(r["BCUId"]),
                    BCUName = r["BCUName"].ToString() ?? "",
                    ProductId = Convert.ToInt32(r["ProductId"]),
                    ProductName = r["ProductName"].ToString() ?? "",
                    IsAdditive = Convert.ToBoolean(r["IsAdditive"]),
                    BCUArmNo = Convert.ToInt32(r["BCUArmNo"]),
                    BCURecipeNo = r["BCURecipeNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["BCURecipeNo"])
                });
            }

            return Json(list);
        }

        // ── POST /Masters/SaveBay ────────────────────────────────────────────
        // Insert (BayId == null) or Update (BayId has value)
        [HttpPost]
        public IActionResult SaveBay([FromBody] BaySaveModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BayName))
                return Json(new { success = false, message = "Bay Name is required." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();

                if (model.BayId == null || model.BayId == 0)
                {
                    // ── INSERT ──────────────────────────────────────────────
                    using var cmd = new SqlCommand(@"
                        INSERT INTO Bay
                            (BayLocationId, BayName, BayDesc, BayNumber,
                             BayTypeId, IsActive, IsDelete,
                             CreatedBy, CreatedDate)
                        VALUES
                            (1, @BayName, @BayDesc,
                             (SELECT ISNULL(MAX(BayNumber),0)+1 FROM Bay),
                             @BayTypeId, @IsActive, 0,
                             1, GETDATE())", con);

                    cmd.Parameters.AddWithValue("@BayName", model.BayName.Trim());
                    cmd.Parameters.AddWithValue("@BayDesc", model.BayDesc?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@BayTypeId", model.BayTypeId);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // ── UPDATE ──────────────────────────────────────────────
                    using var cmd = new SqlCommand(@"
                        UPDATE Bay SET
                            BayName     = @BayName,
                            BayDesc     = @BayDesc,
                            BayTypeId   = @BayTypeId,
                            IsActive    = @IsActive,
                            UpdatedBy   = 1,
                            UpdatedDate = GETDATE()
                        WHERE BayId = @BayId
                          AND IsDelete = 0", con);

                    cmd.Parameters.AddWithValue("@BayId", model.BayId);
                    cmd.Parameters.AddWithValue("@BayName", model.BayName.Trim());
                    cmd.Parameters.AddWithValue("@BayDesc", model.BayDesc?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@BayTypeId", model.BayTypeId);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint violation — BayName already exists
                return Json(new { success = false, message = "Bay Name already exists." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST /Masters/DeleteBay ──────────────────────────────────────────
        // Soft delete
        [HttpPost]
        public IActionResult DeleteBay([FromBody] int bayId)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE Bay SET
                        IsDelete    = 1,
                        IsActive    = 0,
                        DeletedBy   = 1,
                        DeletedDate = GETDATE()
                    WHERE BayId = @BayId", con);

                cmd.Parameters.AddWithValue("@BayId", bayId);
                con.Open();
                cmd.ExecuteNonQuery();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────
        private string ConnStr() =>
            _configuration.GetConnectionString("DefaultConnection")!;

        private List<BayTypeModel> FetchBayTypes()
        {
            var list = new List<BayTypeModel>();

            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT Id, Name, ISNULL(Description,'') AS Description FROM BayType ORDER BY Name", con);

            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BayTypeModel
                {
                    BayTypeId = (int)reader["Id"],
                    BayTypeName = reader["Name"].ToString()!,
                    BayTypeDesc = reader["Description"].ToString()!
                });
            }

            return list;
        }
    }
}
