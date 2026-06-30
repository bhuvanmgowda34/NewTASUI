using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class TanksController : Controller
    {
        private readonly IConfiguration _cfg;
        public TanksController(IConfiguration cfg) { _cfg = cfg; }
        private string ConnStr() => _cfg.GetConnectionString("DefaultConnection")!;

        // ── Page ─────────────────────────────────────────────────────────
        public IActionResult Index() => View("~/Views/Masters/Tanks.cshtml");

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetProducts
        // Returns active, non-deleted products for dropdown.
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetProducts()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                @"SELECT ProductId, RTRIM(ProductName) AS ProductName
                  FROM   Products
                  WHERE  ISNULL(IsDelete,0) = 0
                    AND  ISNULL(IsActive,0) = 1
                  ORDER BY ProductName", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["ProductId"]), name = r["ProductName"].ToString() });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetTankTypes
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetTankTypes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT TankTypeId, RTRIM(TankType) AS TankType FROM TankTypes ORDER BY TankTypeId", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["TankTypeId"]), name = r["TankType"].ToString() });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetTankModes
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetTankModes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT TankModeId, RTRIM(TankModeName) AS TankModeName FROM TankMode ORDER BY TankModeId", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["TankModeId"]), name = r["TankModeName"].ToString() });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetGaugeTypes
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetGaugeTypes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT GuageTypeId, RTRIM(GuageType) AS GuageType FROM GuageTypes ORDER BY GuageTypeId", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["GuageTypeId"]), name = r["GuageType"].ToString() });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetTanks
        // Returns all non-deleted tanks.
        // Tank Mode (live) = latest TankMode from TankParamRecording
        // (updated every 15 mins by automation process).
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetTanks()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"SELECT
    t.TankId,
    t.TankNo,
    t.TankName,
    t.SapTankNo,
    t.TankDescription AS Description,
    t.ProductId,
    p.ProductName,
    t.TankType,
    tt.TankType AS TankTypeName,
    t.TankModeName AS TankModeId,
    tm.TankModeName AS TankModeLabel,

    t.GaugeType,
    gt.GuageType AS GaugeTypeName,
    t.Capacity,
    t.MinLevel,
    t.MaxLevel,
    t.SafeHeightLevel,
    ISNULL(t.IsActive, 0) AS IsActive

FROM Tanks t

LEFT JOIN Products p 
    ON p.ProductId = t.ProductId AND ISNULL(p.IsDelete,0)=0

LEFT JOIN TankTypes tt 
    ON tt.TankTypeId = t.TankType

LEFT JOIN TankMode tm 
    ON tm.TankModeId = t.TankModeName   -- 🔥 IMPORTANT

LEFT JOIN GuageTypes gt 
    ON gt.GuageTypeId = t.GaugeType

WHERE ISNULL(t.IsDelete, 0) = 0

ORDER BY t.TankNo;", con);

            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                // LiveTankMode from TankParamRecording overrides the configured mode
                // when a live reading is available (automation updates every 15 min).
                var tankModeLabel = r["TankModeLabel"].ToString();

                list.Add(new
                {
                    tankId = Convert.ToInt32(r["TankId"]),
                    tankNo = r["TankNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankNo"]),
                    tankName = r["TankName"].ToString(),
                    sapTankNo = r["SapTankNo"].ToString(),
                    description = r["Description"].ToString(),
                    productId = r["ProductId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ProductId"]),
                    productName = r["ProductName"].ToString(),
                    tankType = r["TankType"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankType"]),
                    tankTypeName = r["TankTypeName"].ToString(),
                    tankModeName = r["TankModeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankModeId"]),
                    TankModeLabel = tankModeLabel,          // live or configured
                    gaugeType = r["GaugeType"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["GaugeType"]),
                    gaugeTypeName = r["GaugeTypeName"].ToString(),
                    capacity = r["Capacity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Capacity"]),
                    minLevel = r["MinLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MinLevel"]),
                    maxLevel = r["MaxLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MaxLevel"]),
                    safeHeightLevel = r["SafeHeightLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SafeHeightLevel"]),
                    isActive = (bool)r["IsActive"]
                });
            }
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Tanks/GetTankDetail/{id}
        // Single tank for form fill / modal.
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetTankDetail(int id)
        {
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT
                    t.TankId,
                    t.TankNo,
                    RTRIM(t.TankName)                   AS TankName,
                    ISNULL(RTRIM(t.SapTankNo), '')      AS SapTankNo,
                    ISNULL(RTRIM(t.TankDescription),'') AS Description,
                    t.ProductId,
                    ISNULL(RTRIM(p.ProductName), '')    AS ProductName,
                    t.TankType,
                    ISNULL(RTRIM(tt.TankType), '')      AS TankTypeName,
                    t.TankModeName AS TankModeId,
                    tm.TankModeName AS TankModeLabel,
                    t.GaugeType,
                    ISNULL(RTRIM(gt.GuageType), '')     AS GaugeTypeName,
                    t.Capacity,
                    t.MinLevel,
                    t.MaxLevel,
                    t.SafeHeightLevel,
                    ISNULL(t.IsActive, 0)               AS IsActive
                FROM Tanks t
                LEFT JOIN Products   p  ON p.ProductId   = t.ProductId
                LEFT JOIN TankTypes  tt ON tt.TankTypeId  = t.TankType
                LEFT JOIN TankMode   tm ON tm.TankModeId  = t.TankModeName
                LEFT JOIN GuageTypes gt ON gt.GuageTypeId = t.GaugeType
                WHERE t.TankId = @Id AND ISNULL(t.IsDelete,0) = 0", con);

            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return NotFound();

            return Json(new
            {
                tankId = Convert.ToInt32(r["TankId"]),
                tankNo = r["TankNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankNo"]),
                tankName = r["TankName"].ToString(),
                sapTankNo = r["SapTankNo"].ToString(),
                description = r["Description"].ToString(),
                productId = r["ProductId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ProductId"]),
                productName = r["ProductName"].ToString(),
                tankType = r["TankType"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankType"]),
                tankTypeName = r["TankTypeName"].ToString(),
                tankModeName = r["TankModeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TankModeId"]),
                tankModeLabel = r["TankModeLabel"].ToString(),
                gaugeType = r["GaugeType"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["GaugeType"]),
                gaugeTypeName = r["GaugeTypeName"].ToString(),
                capacity = r["Capacity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Capacity"]),
                minLevel = r["MinLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MinLevel"]),
                maxLevel = r["MaxLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MaxLevel"]),
                safeHeightLevel = r["SafeHeightLevel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SafeHeightLevel"]),
                isActive = (bool)r["IsActive"]
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Tanks/SaveTank
        // INSERT (tankId == 0) or UPDATE via main form.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult SaveTank([FromBody] TankSaveModel m)
        {
            if (m == null)
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();

                bool isExisting = false;
                if (m.TankId > 0)
                {
                    using var chk = new SqlCommand(
                        "SELECT COUNT(1) FROM Tanks WHERE TankId=@Id AND ISNULL(IsDelete,0)=0",
                        con, tx);
                    chk.Parameters.AddWithValue("@Id", m.TankId);
                    isExisting = Convert.ToInt32(chk.ExecuteScalar()) > 0;
                }

                if (!isExisting)
                {
                    using var ins = new SqlCommand(@"
                        INSERT INTO Tanks
                            (TankNo, TankName, TankDescription, SapTankNo,
                             ProductId, TankType, TankModeName, GaugeType,
                             Capacity, MinLevel, MaxLevel, SafeHeightLevel,
                             IsActive, IsDelete, CreatedBy, CreatedDate)
                        VALUES
                            (@TankNo, @TankName, @Desc, @SapNo,
                             @ProdId, @TankType, @TankMode, @GaugeType,
                             @Capacity, @MinLevel, @MaxLevel, @SafeHeight,
                             @IsActive, 0, 1, GETDATE())", con, tx);
                    SetTankParams(ins, m);
                    ins.ExecuteNonQuery();
                }
                else
                {
                    using var upd = new SqlCommand(@"
                        UPDATE Tanks SET
                            TankNo          = @TankNo,
                            TankName        = @TankName,
                            TankDescription = @Desc,
                            SapTankNo       = @SapNo,
                            ProductId       = @ProdId,
                            TankType        = @TankType,
                            TankModeName    = @TankMode,
                            GaugeType       = @GaugeType,
                            Capacity        = @Capacity,
                            MinLevel        = @MinLevel,
                            MaxLevel        = @MaxLevel,
                            SafeHeightLevel = @SafeHeight,
                            IsActive        = @IsActive,
                            UpdatedBy       = 1,
                            UpdatedDate     = GETDATE()
                        WHERE TankId = @TankId AND ISNULL(IsDelete,0) = 0", con, tx);
                    SetTankParams(upd, m);
                    upd.Parameters.AddWithValue("@TankId", m.TankId);
                    upd.ExecuteNonQuery();
                }

                tx.Commit();
                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { success = false, message = "Tank No or Name already exists." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Tanks/UpdateTank
        // Called exclusively by the Edit Modal.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult UpdateTank([FromBody] TankSaveModel m)
        {
            if (m == null || m.TankId == 0)
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE Tanks SET
                        TankNo          = @TankNo,
                        TankName        = @TankName,
                        TankDescription = @Desc,
                        SapTankNo       = @SapNo,
                        ProductId       = @ProdId,
                        TankType        = @TankType,
                        TankModeName    = @TankMode,
                        GaugeType       = @GaugeType,
                        Capacity        = @Capacity,
                        MinLevel        = @MinLevel,
                        MaxLevel        = @MaxLevel,
                        SafeHeightLevel = @SafeHeight,
                        IsActive        = @IsActive,
                        UpdatedBy       = 1,
                        UpdatedDate     = GETDATE()
                    WHERE TankId = @TankId AND ISNULL(IsDelete,0) = 0", con);
                SetTankParams(cmd, m);
                cmd.Parameters.AddWithValue("@TankId", m.TankId);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Tanks/DeleteTank
        // Soft delete.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult DeleteTank([FromBody] int id)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE Tanks SET
                        IsDelete    = 1,
                        IsActive    = 0,
                        DeletedBy   = 1,
                        DeletedDate = GETDATE()
                    WHERE TankId = @Id AND ISNULL(IsDelete,0) = 0", con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIVATE HELPER — shared param binding
        // ═══════════════════════════════════════════════════════════════
        private static void SetTankParams(SqlCommand cmd, TankSaveModel m)
        {
            cmd.Parameters.AddWithValue("@TankNo", (object?)m.TankNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TankName", m.TankName ?? "");
            cmd.Parameters.AddWithValue("@Desc", (object?)m.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SapNo", (object?)m.SapTankNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProdId", (object?)m.ProductId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TankType", (object?)m.TankType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TankMode", (object?)m.TankModeName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GaugeType", (object?)m.GaugeType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Capacity", (object?)m.Capacity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinLevel", (object?)m.MinLevel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxLevel", (object?)m.MaxLevel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SafeHeight", (object?)m.SafeHeightLevel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", m.IsActive);
        }
    }
}
