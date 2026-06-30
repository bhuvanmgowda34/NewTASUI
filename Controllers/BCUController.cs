using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class BCUController : Controller
    {
        private readonly IConfiguration _cfg;
        public BCUController(IConfiguration cfg) { _cfg = cfg; }
        private string ConnStr() => _cfg.GetConnectionString("DefaultConnection")!;

        // ── Page ─────────────────────────────────────────────────────────
        public IActionResult Index() => View();

        // ═══════════════════════════════════════════════════════════════
        // BCU TAB ENDPOINTS
        // ═══════════════════════════════════════════════════════════════

        // ── GET /BCU/GetBCUs ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetBCUs()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  b.BCUId,
                        b.BCUName,
                        b.BCUNumber,
                        ISNULL(bay.BayName,'')          AS BayName,
                        ISNULL(bt.BCUType,'')           AS BCUType,
                        ISNULL(cp.ComportNumber,'')     AS PrimaryComPort,
                        b.SlaveAddress,
                        ISNULL(rcp.ComportNumber,'')    AS SecondaryComPort,
                        ISNULL(b.RedundantSlaveAddress,0) AS RedundantSlaveAddress,
                        ISNULL(b.IsCardReaderIntegrated,0) AS IsCardReaderIntegrated,
                        b.IsActive,
                        ISNULL(at.ArmType,'')           AS ArmType,
                        b.ArmNo,
                        ISNULL(b.IsRIT,0)               AS IsRIT,
                        ISNULL(b.MFMStatus,0)           AS MFMStatus
                FROM    BCUs b
                LEFT JOIN Bay           bay ON bay.BayId       = b.BayId
                LEFT JOIN BCUTypes      bt  ON bt.BCUTypeId    = b.BCUTypeId
                LEFT JOIN ComportDetails cp  ON cp.ComPortId   = b.ComPortId
                LEFT JOIN ComportDetails rcp ON rcp.ComPortId  = b.RedundantComportId
                LEFT JOIN ArmTypes      at  ON at.ArmTypeId    = b.ArmTypeId
                WHERE   b.IsDelete = 0
                ORDER BY b.BCUNumber", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    bcuId = r["BCUId"],
                    bcuName = r["BCUName"].ToString(),
                    bcuNumber = r["BCUNumber"],
                    bayName = r["BayName"].ToString(),
                    bcuType = r["BCUType"].ToString(),
                    primaryComPort = r["PrimaryComPort"].ToString(),
                    slaveAddress = r["SlaveAddress"],
                    secondaryComPort = r["SecondaryComPort"].ToString(),
                    redundantSlaveAddress = r["RedundantSlaveAddress"],
                    isCardReaderIntegrated = (bool)r["IsCardReaderIntegrated"],
                    isActive = (bool)r["IsActive"],
                    armType = r["ArmType"].ToString(),
                    armNo = r["ArmNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ArmNo"]),
                    isRIT = (bool)r["IsRIT"],
                    mfmStatus = (bool)r["MFMStatus"]
                });
            return Json(list);
        }

        // ── GET /BCU/GetBCUDetail/{id} ───────────────────────────────────
        [HttpGet]
        public IActionResult GetBCUDetail(int id)
        {
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  b.BCUId, b.BCUName, b.BCUNumber,
                        b.BCUTypeId, ISNULL(bt.BCUType,'') AS BCUTypeName,
                        b.BayId,    ISNULL(bay.BayName,'') AS BayName,
                        b.SlaveAddress, b.RedundantSlaveAddress,
                        b.ComPortId,  ISNULL(cp.ComportNumber,'')  AS PrimaryComPort,
                        b.RedundantComportId, ISNULL(rcp.ComportNumber,'') AS SecondaryComPort,
                        ISNULL(b.IsCardReaderIntegrated,0) AS IsCardReaderIntegrated,
                        b.IsActive,
                        ISNULL(b.IsRIT,0)     AS IsRIT,
                        ISNULL(b.MFMStatus,0) AS MFMStatus,
                        b.ArmTypeId, ISNULL(at.ArmType,'') AS ArmType,
                        b.ArmNo
                FROM    BCUs b
                LEFT JOIN Bay            bay ON bay.BayId      = b.BayId
                LEFT JOIN BCUTypes       bt  ON bt.BCUTypeId   = b.BCUTypeId
                LEFT JOIN ComportDetails cp  ON cp.ComPortId   = b.ComPortId
                LEFT JOIN ComportDetails rcp ON rcp.ComPortId  = b.RedundantComportId
                LEFT JOIN ArmTypes       at  ON at.ArmTypeId   = b.ArmTypeId
                WHERE   b.BCUId = @Id AND b.IsDelete = 0", con);
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return NotFound();
            return Json(new
            {
                bcuId = Convert.ToInt32(r["BCUId"]),
                bcuName = r["BCUName"].ToString(),
                bcuNumber = Convert.ToInt32(r["BCUNumber"]),
                bcuTypeId = r["BCUTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["BCUTypeId"]),
                bcuTypeName = r["BCUTypeName"].ToString(),
                bayId = r["BayId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["BayId"]),
                bayName = r["BayName"].ToString(),
                slaveAddress = Convert.ToInt32(r["SlaveAddress"]),
                redundantSlaveAddress = r["RedundantSlaveAddress"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["RedundantSlaveAddress"]),
                comPortId = Convert.ToInt32(r["ComPortId"]),
                primaryComPort = r["PrimaryComPort"].ToString(),
                redundantComportId = r["RedundantComportId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["RedundantComportId"]),
                secondaryComPort = r["SecondaryComPort"].ToString(),
                isCardReaderIntegrated = (bool)r["IsCardReaderIntegrated"],
                isActive = (bool)r["IsActive"],
                isRIT = (bool)r["IsRIT"],
                mfmStatus = (bool)r["MFMStatus"],
                armTypeId = r["ArmTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ArmTypeId"]),
                armType = r["ArmType"].ToString(),
                armNo = r["ArmNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ArmNo"])
            });
        }

        // ── GET /BCU/GetBCUTypes ─────────────────────────────────────────
        [HttpGet]
        public IActionResult GetBCUTypes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT BCUTypeId, RTRIM(BCUType) AS BCUType FROM BCUTypes WHERE ISNULL(IsDeleted,0)=0 ORDER BY BCUType", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["BCUTypeId"]), name = r["BCUType"].ToString() });
            return Json(list);
        }

        // ── GET /BCU/GetComPorts ─────────────────────────────────────────
        [HttpGet]
        public IActionResult GetComPorts()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT ComPortId, ComportNumber FROM ComportDetails WHERE ISNULL(IsDelete,0)=0 ORDER BY ComportNumber", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["ComPortId"]), name = r["ComportNumber"].ToString() });
            return Json(list);
        }

        // ── GET /BCU/GetArmTypes ─────────────────────────────────────────
        [HttpGet]
        public IActionResult GetArmTypes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT ArmTypeId, ArmType FROM ArmTypes ORDER BY ArmType", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["ArmTypeId"]), name = r["ArmType"].ToString() });
            return Json(list);
        }

        // ── GET /BCU/GetBays ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetBays()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT BayId, BayName FROM Bay WHERE IsDelete=0 ORDER BY BayName", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["BayId"]), name = r["BayName"].ToString() });
            return Json(list);
        }

        // ── POST /BCU/SaveBCU ────────────────────────────────────────────
        [HttpPost]
        public IActionResult SaveBCU([FromBody] BCUSaveModel m)
        {
            if (string.IsNullOrWhiteSpace(m.BCUName))
                return Json(new { success = false, message = "BCU Name is required." });
            if (m.BCUNumber < 1)
                return Json(new { success = false, message = "BCU Number is required." });
            if (m.ComPortId < 1)
                return Json(new { success = false, message = "Primary COM Port is required." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();

                bool isExisting = false;
                if (m.BCUId.HasValue && m.BCUId > 0)
                {
                    using var chk = new SqlCommand(
                        "SELECT COUNT(1) FROM BCUs WHERE BCUId=@Id AND IsDelete=0", con, tx);
                    chk.Parameters.AddWithValue("@Id", m.BCUId.Value);
                    isExisting = Convert.ToInt32(chk.ExecuteScalar()) > 0;
                }

                if (!isExisting)
                {
                    using var ins = new SqlCommand(@"
                        INSERT INTO BCUs
                            (BCUName, BCUNumber, BCUTypeId, BayId,
                             SlaveAddress, RedundantSlaveAddress,
                             ComPortId, RedundantComportId,
                             IsCardReaderIntegrated, IsActive, IsRIT, MFMStatus,
                             ArmTypeId, ArmNo,
                             IsDelete, CreatedBy, CreatedDate)
                        VALUES
                            (@Name, @Num, @TypeId, @BayId,
                             @Slave, @RedSlave,
                             @ComPort, @RedComPort,
                             @CardReader, @Active, @RIT, @MFM,
                             @ArmType, @ArmNo,
                             0, 1, GETDATE())", con, tx);
                    SetBCUParams(ins, m);
                    ins.ExecuteNonQuery();
                }
                else
                {
                    using var upd = new SqlCommand(@"
                        UPDATE BCUs SET
                            BCUName                = @Name,
                            BCUNumber              = @Num,
                            BCUTypeId              = @TypeId,
                            BayId                  = @BayId,
                            SlaveAddress           = @Slave,
                            RedundantSlaveAddress  = @RedSlave,
                            ComPortId              = @ComPort,
                            RedundantComportId     = @RedComPort,
                            IsCardReaderIntegrated = @CardReader,
                            IsActive               = @Active,
                            IsRIT                  = @RIT,
                            MFMStatus              = @MFM,
                            ArmTypeId              = @ArmType,
                            ArmNo                  = @ArmNo,
                            UpdatedBy              = 1,
                            UpdatedDate            = GETDATE()
                        WHERE BCUId = @BCUId AND IsDelete = 0", con, tx);
                    SetBCUParams(upd, m);
                    upd.Parameters.AddWithValue("@BCUId", m.BCUId!.Value);
                    upd.ExecuteNonQuery();
                }

                tx.Commit();
                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { success = false, message = "BCU Name already exists." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST /BCU/DeleteBCU ──────────────────────────────────────────
        [HttpPost]
        public IActionResult DeleteBCU([FromBody] int bcuId)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE BCUs SET IsDelete=1, IsActive=0,
                        DeletedBy=1, DeletedDate=GETDATE()
                    WHERE BCUId=@Id", con);
                cmd.Parameters.AddWithValue("@Id", bcuId);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ═══════════════════════════════════════════════════════════════
        // BCU PRODUCTS TAB ENDPOINTS
        // ═══════════════════════════════════════════════════════════════

        // ── GET /BCU/GetBCUList (dropdown for BCU Products tab) ──────────
        [HttpGet]
        public IActionResult GetBCUList()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT BCUId, BCUName FROM BCUs WHERE IsDelete=0 ORDER BY BCUName", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new { id = Convert.ToInt32(r["BCUId"]), name = r["BCUName"].ToString() });
            return Json(list);
        }

        // ── GET /BCU/GetAllProducts ──────────────────────────────────────
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
                list.Add(new { id = Convert.ToInt32(r["ProductId"]), name = r["ProductName"].ToString() });
            return Json(list);
        }

        // ── GET /BCU/GetBCUProducts/{bcuId} ─────────────────────────────
        [HttpGet]
        public IActionResult GetBCUProducts(int bcuId)
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  bp.BCUProductId, bp.ProductId,
                        p.ProductName,
                        ISNULL(bp.IsAdditive,0) AS IsAdditive,
                        ISNULL(bp.BCUArmNo,0)   AS BCUArmNo,
                        bp.BCURecipeNo
                FROM    BCUProducts bp
                JOIN    Products p ON p.ProductId = bp.ProductId
                WHERE   bp.BCUId = @BcuId AND bp.IsDelete = 0
                ORDER BY bp.BCURecipeNo", con);
            cmd.Parameters.AddWithValue("@BcuId", bcuId);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    bcuProductId = Convert.ToInt32(r["BCUProductId"]),
                    productId = Convert.ToInt32(r["ProductId"]),
                    productName = r["ProductName"].ToString(),
                    isAdditive = (bool)r["IsAdditive"],
                    bcuArmNo = Convert.ToInt32(r["BCUArmNo"]),
                    bcuRecipeNo = r["BCURecipeNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["BCURecipeNo"])
                });
            return Json(list);
        }

        // ── POST /BCU/AddBCUProduct ──────────────────────────────────────
        [HttpPost]
        public IActionResult AddBCUProduct([FromBody] BCUProductSaveModel m)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                // Check not already associated
                using var chk = new SqlCommand(
                    "SELECT COUNT(1) FROM BCUProducts WHERE BCUId=@BId AND ProductId=@PId AND IsDelete=0", con);
                chk.Parameters.AddWithValue("@BId", m.BCUId);
                chk.Parameters.AddWithValue("@PId", m.ProductId);
                con.Open();
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                    return Json(new { success = false, message = "Product already associated with this BCU." });

                using var ins = new SqlCommand(@"
                    INSERT INTO BCUProducts
                        (BCUId, ProductId, IsAdditive, BCUArmNo, BCURecipeNo,
                         IsDelete, CreatedBy, CreatedDate)
                    VALUES
                        (@BId, @PId, @Add, @Arm, @Recipe, 0, 1, GETDATE())", con);
                ins.Parameters.AddWithValue("@BId", m.BCUId);
                ins.Parameters.AddWithValue("@PId", m.ProductId);
                ins.Parameters.AddWithValue("@Add", m.IsAdditive);
                ins.Parameters.AddWithValue("@Arm", m.BCUArmNo);
                ins.Parameters.AddWithValue("@Recipe", (object?)m.BCURecipeNo ?? DBNull.Value);
                ins.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST /BCU/UpdateBCUProduct ───────────────────────────────────
        [HttpPost]
        public IActionResult UpdateBCUProduct([FromBody] BCUProductUpdateModel m)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE BCUProducts SET
                        IsAdditive  = @Add,
                        BCUArmNo    = @Arm,
                        BCURecipeNo = @Recipe,
                        UpdatedBy   = 1,
                        UpdatedDate = GETDATE()
                    WHERE BCUProductId = @Id AND IsDelete = 0", con);
                cmd.Parameters.AddWithValue("@Id", m.BCUProductId);
                cmd.Parameters.AddWithValue("@Add", m.IsAdditive);
                cmd.Parameters.AddWithValue("@Arm", m.BCUArmNo);
                cmd.Parameters.AddWithValue("@Recipe", (object?)m.BCURecipeNo ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── POST /BCU/RemoveBCUProduct ───────────────────────────────────
        [HttpPost]
        public IActionResult RemoveBCUProduct([FromBody] int bcuProductId)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE BCUProducts SET IsDelete=1, DeletedBy=1, DeletedDate=GETDATE()
                    WHERE BCUProductId=@Id", con);
                cmd.Parameters.AddWithValue("@Id", bcuProductId);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ── Helper ───────────────────────────────────────────────────────
        private static void SetBCUParams(SqlCommand cmd, BCUSaveModel m)
        {
            cmd.Parameters.AddWithValue("@Name", m.BCUName.Trim());
            cmd.Parameters.AddWithValue("@Num", m.BCUNumber);
            cmd.Parameters.AddWithValue("@TypeId", (object?)m.BCUTypeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BayId", (object?)m.BayId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Slave", m.SlaveAddress);
            cmd.Parameters.AddWithValue("@RedSlave", (object?)m.RedundantSlaveAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ComPort", m.ComPortId);
            cmd.Parameters.AddWithValue("@RedComPort", (object?)m.RedundantComportId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CardReader", m.IsCardReaderIntegrated);
            cmd.Parameters.AddWithValue("@Active", m.IsActive);
            cmd.Parameters.AddWithValue("@RIT", m.IsRIT);
            cmd.Parameters.AddWithValue("@MFM", m.MFMStatus);
            cmd.Parameters.AddWithValue("@ArmType", (object?)m.ArmTypeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ArmNo", (object?)m.ArmNo ?? DBNull.Value);
        }
    }
}
