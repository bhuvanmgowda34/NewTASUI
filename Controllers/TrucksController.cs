using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;
using System.Data;

namespace NewTASUI.Controllers
{
    public class TrucksController : Controller
    {
        private readonly IConfiguration _cfg;
        public TrucksController(IConfiguration cfg) { _cfg = cfg; }
        private string ConnStr() => _cfg.GetConnectionString("DefaultConnection")!;

        // ── GET /Trucks/Index ─────────────────────────────────────────────
        public IActionResult Index() => View("~/Views/Masters/Trucks.cshtml");

        // ── GET /Trucks/GetTrucks — table list ───────────────────────────
        [HttpGet]
        public IActionResult GetTrucks()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  t.TruckId,
                        t.RegistrationNumber,
                        ISNULL(tt.Description, '') AS TruckTypeName,
                        t.NoOfCompartments,
                        ISNULL(t.DriverName, '')       AS LockNo,
                        tk.TruckKey,
                        t.IsPermanentKey,
                        ISNULL(t.IsInside, 0)      AS IsInside,
                        ISNULL(t.IsBioMetric, 0)   AS IsBioMetric,
                        ISNULL(t.IsActive, 1)      AS IsActive
                FROM    Trucks t
                LEFT JOIN TruckType   tt ON tt.Id          = t.TruckTypeId
                LEFT JOIN TruckKeyes  tk ON tk.TruckKeyId  = t.TruckKeyId
                WHERE   t.IsDelete = 0
                ORDER BY t.RegistrationNumber", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new
                {
                    truckId = r["TruckId"],
                    registrationNumber = r["RegistrationNumber"].ToString(),
                    truckTypeName = r["TruckTypeName"].ToString(),
                    noOfCompartments = r["NoOfCompartments"],
                    lockNo = r["LockNo"].ToString(),
                    truckKey = r["TruckKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["TruckKey"]),
                    isPermanentKey = (bool)r["IsPermanentKey"],
                    isInside = r["IsInside"] != DBNull.Value && (bool)r["IsInside"],
                    isBioMetric = r["IsBioMetric"] != DBNull.Value && Convert.ToInt32(r["IsBioMetric"]) == 1,
                    isActive = r["IsActive"] != DBNull.Value && (bool)r["IsActive"]
                });
            }
            return Json(list);
        }

        // ── GET /Trucks/GetTruckDetail/{id} — form fill on row click ─────
        [HttpGet]
        public IActionResult GetTruckDetail(long id)
        {
            TruckDetailModel? detail = null;

            using var con = new SqlConnection(ConnStr());
            con.Open();

            // Main truck row
            using (var cmd = new SqlCommand(@"
                SELECT  t.TruckId,
                        t.RegistrationNumber,
                        t.TruckTypeId,
                        ISNULL(tt.Description,'')  AS TruckTypeName,
                        t.TruckKeyId,
                        tk.TruckKey,
                        t.IsPermanentKey,
                        ISNULL(t.CompanyName,'')   AS CompanyName,
                        t.NoOfCompartments,
                        ISNULL(t.DriverName,'')        AS LockNo,
                        t.CallibrationDueDate,
                        t.LicenseExpireDate,
                        t.ExplosiveExpiryDate,
                        ISNULL(t.IsBioMetric,0)    AS IsBioMetric,
                        ISNULL(t.IsActive,1)       AS IsActive
                FROM    Trucks t
                LEFT JOIN TruckType  tt ON tt.Id         = t.TruckTypeId
                LEFT JOIN TruckKeyes tk ON tk.TruckKeyId = t.TruckKeyId
                WHERE   t.TruckId = @Id AND t.IsDelete = 0", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    detail = new TruckDetailModel
                    {
                        TruckId = Convert.ToInt64(r["TruckId"]),
                        RegistrationNumber = r["RegistrationNumber"].ToString()!,
                        TruckTypeId = r["TruckTypeId"].ToString()!,
                        TruckTypeName = r["TruckTypeName"].ToString()!,
                        TruckKeyId = r["TruckKeyId"] == DBNull.Value ? null : Convert.ToInt32(r["TruckKeyId"]),
                        TruckKey = r["TruckKey"] == DBNull.Value ? null : Convert.ToInt32(r["TruckKey"]),
                        IsPermanentKey = (bool)r["IsPermanentKey"],
                        CompanyName = r["CompanyName"].ToString()!,
                        NoOfCompartments = Convert.ToInt32(r["NoOfCompartments"]),
                        LockNo = r["LockNo"].ToString()!,
                        CallibrationDueDate = r["CallibrationDueDate"] == DBNull.Value ? null :
                            Convert.ToDateTime(r["CallibrationDueDate"]).ToString("yyyy-MM-ddTHH:mm"),
                        LicenseExpireDate = r["LicenseExpireDate"] == DBNull.Value ? null :
                            Convert.ToDateTime(r["LicenseExpireDate"]).ToString("yyyy-MM-ddTHH:mm"),
                        ExplosiveExpiryDate = r["ExplosiveExpiryDate"] == DBNull.Value ? null :
                            Convert.ToDateTime(r["ExplosiveExpiryDate"]).ToString("yyyy-MM-ddTHH:mm"),
                        IsBioMetric = Convert.ToInt32(r["IsBioMetric"]) == 1,
                        IsActive = r["IsActive"] != DBNull.Value && (bool)r["IsActive"]
                    };
                }
            }

            if (detail == null) return NotFound();

            // Compartment details
            using (var cmd = new SqlCommand(@"
                SELECT CompartmentNo, Capacity
                FROM   TruckCompartmentDetails
                WHERE  TruckId = @Id AND IsDelete = 0
                ORDER BY CompartmentNo", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    detail.Compartments.Add(new CompartmentModel
                    {
                        CompartmentNo = Convert.ToInt32(r["CompartmentNo"]),
                        Capacity = Convert.ToInt32(r["Capacity"])
                    });
                }
            }

            return Json(detail);
        }

        // ── GET /Trucks/GetTruckTypes ─────────────────────────────────────
        [HttpGet]
        public IActionResult GetTruckTypes()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT Id, Name, Description FROM TruckType ORDER BY Description", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    id = r["Id"].ToString(),
                    name = r["Name"].ToString(),
                    description = r["Description"].ToString()
                });
            return Json(list);
        }

        // ── GET /Trucks/GetTruckKeys — only unassigned keys (VW_TRUCKKEYS) ─
        [HttpGet]
        public IActionResult GetTruckKeys()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                "SELECT TruckKeyId, TruckKey FROM VW_TRUCKKEYS ORDER BY TruckKey", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    truckKeyId = Convert.ToInt32(r["TruckKeyId"]),
                    truckKey = Convert.ToInt32(r["TruckKey"])
                });
            return Json(list);
        }

        // ── POST /Trucks/SaveTruck — Insert or Update ─────────────────────
        [HttpPost]
        public IActionResult SaveTruck([FromBody] TruckSaveModel m)
        {
            if (string.IsNullOrWhiteSpace(m.RegistrationNumber))
                return Json(new { success = false, message = "Registration Number is required." });
            if (string.IsNullOrWhiteSpace(m.TruckTypeId))
                return Json(new { success = false, message = "Truck Type is required." });
            if (m.NoOfCompartments < 1)
                return Json(new { success = false, message = "No. of Compartments must be at least 1." });
            if (m.IsPermanentKey && m.TruckKeyId == null)
                return Json(new { success = false, message = "Truck Key is required when Is Permanent is checked." });

            DateTime? calibDate = ParseDate(m.CallibrationDueDate);
            DateTime? licenseDate = ParseDate(m.LicenseExpireDate);
            DateTime? explosiveDate = ParseDate(m.ExplosiveExpiryDate);

            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();

                long truckId;

                // Safety check: if TruckId provided, verify it exists in DB
                // This prevents duplicate inserts if JS sends wrong editingId
                bool isExisting = false;
                if (m.TruckId.HasValue && m.TruckId > 0)
                {
                    using var existChk = new SqlCommand(
                        "SELECT COUNT(1) FROM Trucks WHERE TruckId=@Id AND IsDelete=0", con, tx);
                    existChk.Parameters.AddWithValue("@Id", m.TruckId.Value);
                    isExisting = Convert.ToInt32(existChk.ExecuteScalar()) > 0;
                }

                if (!isExisting)
                {
                    // ── INSERT ────────────────────────────────────────────
                    using var ins = new SqlCommand(@"
                        INSERT INTO Trucks
                            (RegistrationNumber, TruckTypeId, TruckKeyId, IsPermanentKey,
                             CompanyName, NoOfCompartments, DriverName,
                             CallibrationDueDate, LicenseExpireDate, ExplosiveExpiryDate,
                             IsBioMetric, IsActive, IsDelete, IsInside,
                             CreatedBy, CreatedDate)
                        OUTPUT INSERTED.TruckId
                        VALUES
                            (@RegNo, @TypeId, @KeyId, @IsPerm,
                             @Company, @NumComp, @LockNo,
                             @CalibDate, @LicDate, @ExpDate,
                             @BioMetric, 1, 0, 0,
                             1, GETDATE())", con, tx);

                    ins.Parameters.AddWithValue("@RegNo", m.RegistrationNumber.Trim());
                    ins.Parameters.AddWithValue("@TypeId", m.TruckTypeId);
                    ins.Parameters.AddWithValue("@KeyId", (object?)m.TruckKeyId ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@IsPerm", m.IsPermanentKey);
                    ins.Parameters.AddWithValue("@Company", m.CompanyName?.Trim() ?? "");
                    ins.Parameters.AddWithValue("@NumComp", m.NoOfCompartments);
                    ins.Parameters.AddWithValue("@LockNo", m.LockNo?.Trim() ?? "");
                    ins.Parameters.AddWithValue("@CalibDate", (object?)calibDate ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@LicDate", (object?)licenseDate ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@ExpDate", (object?)explosiveDate ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@BioMetric", m.IsBioMetric ? 1 : 0);

                    truckId = Convert.ToInt64(ins.ExecuteScalar());

                    //  INSERT compartments (ONLY FOR NEW TRUCK)
                    foreach (var comp in m.Compartments)
                    {
                        using var ic = new SqlCommand(@"
        INSERT INTO TruckCompartmentDetails
        (CompartmentNo, Capacity, TruckId, IsDelete, CreatedBy, CreatedDate)
        VALUES (@No, @Cap, @TruckId, 0, 1, GETDATE())", con, tx);

                        ic.Parameters.AddWithValue("@No", comp.CompartmentNo);
                        ic.Parameters.AddWithValue("@Cap", comp.Capacity);
                        ic.Parameters.AddWithValue("@TruckId", truckId);
                        ic.ExecuteNonQuery();
                    }
                    // Mark key as assigned + set IsPermanent
                    if (m.TruckKeyId.HasValue)
                        MarkKeyAssigned(con, tx, m.TruckKeyId.Value, true, m.IsPermanentKey);
                }
                else
                {
                    // ── UPDATE ────────────────────────────────────────────
                    truckId = m.TruckId!.Value;

                    // Get old key to unassign if changed
                    int? oldKeyId = null;
                    using (var chk = new SqlCommand(
                        "SELECT TruckKeyId FROM Trucks WHERE TruckId=@Id", con, tx))
                    {
                        chk.Parameters.AddWithValue("@Id", truckId);
                        var val = chk.ExecuteScalar();
                        if (val != null && val != DBNull.Value)
                            oldKeyId = Convert.ToInt32(val);
                    }

                    using var upd = new SqlCommand(@"
                        UPDATE Trucks SET
                            TruckTypeId          = @TypeId,
                            TruckKeyId           = @KeyId,
                            IsPermanentKey       = @IsPerm,
                            CompanyName          = @Company,
                            NoOfCompartments     = @NumComp,
                            DriverName               = @LockNo,
                            CallibrationDueDate  = @CalibDate,
                            LicenseExpireDate    = @LicDate,
                            ExplosiveExpiryDate  = @ExpDate,
                            IsBioMetric          = @BioMetric,
                            UpdatedBy            = 1,
                            UpdatedDate          = GETDATE()
                        WHERE TruckId = @Id AND IsDelete = 0", con, tx);

                    upd.Parameters.AddWithValue("@TypeId", m.TruckTypeId);
                    upd.Parameters.AddWithValue("@KeyId", (object?)m.TruckKeyId ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@IsPerm", m.IsPermanentKey);
                    upd.Parameters.AddWithValue("@Company", m.CompanyName?.Trim() ?? "");
                    upd.Parameters.AddWithValue("@NumComp", m.NoOfCompartments);
                    upd.Parameters.AddWithValue("@LockNo", m.LockNo?.Trim() ?? "");
                    upd.Parameters.AddWithValue("@CalibDate", (object?)calibDate ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@LicDate", (object?)licenseDate ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@ExpDate", (object?)explosiveDate ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@BioMetric", m.IsBioMetric ? 1 : 0);
                    upd.Parameters.AddWithValue("@Id", truckId);
                    upd.ExecuteNonQuery();

                    // Re-assign keys if changed
                    if (oldKeyId.HasValue && oldKeyId != m.TruckKeyId)
                        MarkKeyAssigned(con, tx, oldKeyId.Value, false, false);
                    if (m.TruckKeyId.HasValue)
                        MarkKeyAssigned(con, tx, m.TruckKeyId.Value, true, m.IsPermanentKey);

                    // VALIDATION (ADD THIS HERE)
                    if (m.Compartments.Count != m.NoOfCompartments)
                    {
                        return Json(new { success = false, message = "Compartment count mismatch." });
                    }

                    // Get existing compartments
                    var existing = new Dictionary<int, int>();



                    using (var cmd = new SqlCommand(@"
    SELECT CompartmentNo, Capacity 
    FROM TruckCompartmentDetails 
    WHERE TruckId=@Id AND IsDelete=0", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", truckId);
                        using var r = cmd.ExecuteReader();
                        while (r.Read())
                        {
                            existing[Convert.ToInt32(r["CompartmentNo"])] =
                                Convert.ToInt32(r["Capacity"]);
                        }
                    }

                    //  UPDATE / INSERT / DELETE LOGIC

                    // 1. UPDATE or INSERT
                    foreach (var comp in m.Compartments)
                    {
                        if (existing.ContainsKey(comp.CompartmentNo))
                        {
                            // UPDATE
                            using var updComp = new SqlCommand(@"
            UPDATE TruckCompartmentDetails
            SET Capacity=@Cap
            WHERE TruckId=@TruckId AND CompartmentNo=@No AND IsDelete=0",
                                con, tx);

                            updComp.Parameters.AddWithValue("@Cap", comp.Capacity);
                            updComp.Parameters.AddWithValue("@TruckId", truckId);
                            updComp.Parameters.AddWithValue("@No", comp.CompartmentNo);
                            updComp.ExecuteNonQuery();
                        }
                        else
                        {
                            // INSERT NEW
                            using var ins = new SqlCommand(@"
            INSERT INTO TruckCompartmentDetails
            (CompartmentNo, Capacity, TruckId, IsDelete, CreatedBy, CreatedDate)
            VALUES (@No, @Cap, @TruckId, 0, 1, GETDATE())", con, tx);

                            ins.Parameters.AddWithValue("@No", comp.CompartmentNo);
                            ins.Parameters.AddWithValue("@Cap", comp.Capacity);
                            ins.Parameters.AddWithValue("@TruckId", truckId);
                            ins.ExecuteNonQuery();
                        }
                    }

                    // 2. DELETE REMOVED
                    foreach (var oldComp in existing.Keys)
                    {
                        if (!m.Compartments.Any(c => c.CompartmentNo == oldComp))
                        {
                            using var del = new SqlCommand(@"
            UPDATE TruckCompartmentDetails
            SET IsDelete=1, DeletedBy=1, DeletedDate=GETDATE()
            WHERE TruckId=@TruckId AND CompartmentNo=@No AND IsDelete=0",
                                con, tx);

                            del.Parameters.AddWithValue("@TruckId", truckId);
                            del.Parameters.AddWithValue("@No", oldComp);
                            del.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { success = false, message = "Registration Number already exists." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST /Trucks/DeleteTruck ──────────────────────────────────────
        [HttpPost]
        public IActionResult DeleteTruck([FromBody] long truckId)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();

                // Get key to unassign
                int? keyId = null;
                using (var chk = new SqlCommand(
                    "SELECT TruckKeyId FROM Trucks WHERE TruckId=@Id", con, tx))
                {
                    chk.Parameters.AddWithValue("@Id", truckId);
                    var val = chk.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                        keyId = Convert.ToInt32(val);
                }

                using var cmd = new SqlCommand(@"
                    UPDATE Trucks SET
                        IsDelete=1, IsActive=0,
                        DeletedBy=1, DeletedDate=GETDATE()
                    WHERE TruckId=@Id", con, tx);
                cmd.Parameters.AddWithValue("@Id", truckId);
                cmd.ExecuteNonQuery();

                // Soft delete compartments
                using var dc = new SqlCommand(@"
                    UPDATE TruckCompartmentDetails SET
                        IsDelete=1, DeletedBy=1, DeletedDate=GETDATE()
                    WHERE TruckId=@Id AND IsDelete=0", con, tx);
                dc.Parameters.AddWithValue("@Id", truckId);
                dc.ExecuteNonQuery();

                // Free the key
                if (keyId.HasValue)
                    MarkKeyAssigned(con, tx, keyId.Value, false);

                tx.Commit();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Helper: mark TruckKey assigned/unassigned ─────────────────────
        private static void MarkKeyAssigned(SqlConnection con, SqlTransaction tx, int keyId, bool assigned, bool isPermanent = false)
        {
            using var cmd = new SqlCommand(
                "UPDATE TruckKeyes SET IsAssigned=@A, IsPermanent=@P WHERE TruckKeyId=@Id", con, tx);
            cmd.Parameters.AddWithValue("@A", assigned);
            cmd.Parameters.AddWithValue("@P", isPermanent);
            cmd.Parameters.AddWithValue("@Id", keyId);
            cmd.ExecuteNonQuery();
        }

        private static DateTime? ParseDate(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : DateTime.Parse(s);
    }
}
