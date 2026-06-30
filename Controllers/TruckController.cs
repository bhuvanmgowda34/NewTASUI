using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;
using System.Data;

namespace NewTASUI.Controllers
{
    public class TruckController : Controller
    {
        private readonly string _conn;

        public TruckController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection is missing in appsettings.json");

            if (string.IsNullOrEmpty(_conn))
            {
                throw new Exception("DefaultConnection is missing");
            }
        }


        // ── PAGE ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Entry()
        {
            return View("~/Views/Workflow/Entry.cshtml");
        }

        // ── GET: Available Trucks ─────────────────────────────────────────────
        // Columns: No | Type | Key | Company Name | No Of Comp | IsPerman... |
        //          Printed Tare | Printed Gr... | Tanker Tare | Tanker Gross |
        //          CCOE Perm | Callibration... | License E... | Explosive Expiry
        [HttpGet]
        public IActionResult GetAvailableTrucks()
        {
            var list = new List<AvailableTruckModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
    SELECT  T.TruckId,
            T.RegistrationNumber,
            TT.Description          AS TruckType,
            T.TruckKeyId,
            TK.TruckKey,
            TK.IsPermanent,
            T.CompanyName,
            T.NoOfCompartments,
            T.IsPermanentKey,
            T.PrintedTareWeight,
            T.PrintedGrossWeight,
            T.TankerTareWeight,
            T.TankerGrossWeight,
            T.CCOEPermWeight,
            T.CallibrationDueDate,
            T.LicenseExpireDate,
            T.ExplosiveExpiryDate
    FROM    Trucks T
    INNER JOIN TruckType TT ON TT.Id = T.TruckTypeId
    LEFT JOIN  TruckKeyes TK ON TK.TruckKeyId = T.TruckKeyId
    WHERE   ISNULL(T.IsInside,  0) = 0
    AND     ISNULL(T.IsDelete,  0) = 0
    AND     ISNULL(T.IsActive,  1) = 1
    ORDER BY T.RegistrationNumber", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new AvailableTruckModel
                    {
                        TruckId = Convert.ToInt64(rdr["TruckId"]),
                        RegistrationNumber = rdr["RegistrationNumber"].ToString(),
                        TruckType = rdr["TruckType"]?.ToString(),
                        TruckKeyId = rdr["TruckKeyId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["TruckKeyId"]),
                        CompanyName = rdr["CompanyName"]?.ToString(),
                        NoOfCompartments = rdr["NoOfCompartments"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NoOfCompartments"]),
                        IsPermanentKey = rdr["IsPermanentKey"] != DBNull.Value && Convert.ToBoolean(rdr["IsPermanentKey"]),
                        PrintedTareWeight = rdr["PrintedTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedTareWeight"]),
                        PrintedGrossWeight = rdr["PrintedGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedGrossWeight"]),
                        TankerTareWeight = rdr["TankerTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerTareWeight"]),
                        TankerGrossWeight = rdr["TankerGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerGrossWeight"]),
                        CCOEPermWeight = rdr["CCOEPermWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CCOEPermWeight"]),
                        CallibrationDueDate = rdr["CallibrationDueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CallibrationDueDate"]),
                        LicenseExpireDate = rdr["LicenseExpireDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LicenseExpireDate"]),
                        ExplosiveExpiryDate = rdr["ExplosiveExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["ExplosiveExpiryDate"]),
                        TruckKey = rdr["TruckKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["TruckKey"]),
                        IsPermanent = rdr["IsPermanent"] != DBNull.Value && Convert.ToBoolean(rdr["IsPermanent"]),
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── GET: Today's Trucks ───────────────────────────────────────────────
        // Source: VW_TruckEntry
        // Columns: Sel | Date | Time | Type(entry) |
        //          No. | Type(truck) | Company... | No Of C... |
        //          Number | Is Permanent |
        //          CCOE Per... | Tanker T... | Tanker Gross |
        //          Callibration... | License Expi... |
        //          Status | Seq. No | Response
        [HttpGet]
        public IActionResult GetTodayTrucks()
        {
            var list = new List<TruckEntryViewModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT  TruckEntryId,
                            EntryDate,
                            Time,
                            EntryType,
                            TruckNumber,
                            TruckType,
                            CompanyName,
                            NoOfCompartments,
                            TruckKey,
                            IsPermanentKey,
                            CCOEPermWeight,
                            TankerTareWeight,
                            TankerGrossWeight,
                            CallibrationDueDate,
                            LicenseExpireDate,
                            TTStatus,
                            SequenceNumber,
                            PROCESSING_RESPONSE,
                            LicenseExpiryInfo
                    FROM    VW_TruckEntry
                    ORDER BY TruckEntryId DESC", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new TruckEntryViewModel
                    {
                        TruckEntryId = Convert.ToInt64(rdr["TruckEntryId"]),
                        EntryDate = rdr["EntryDate"]?.ToString(),
                        Time = rdr["Time"]?.ToString(),
                        EntryType = rdr["EntryType"]?.ToString(),
                        TruckNumber = rdr["TruckNumber"]?.ToString(),
                        TruckType = rdr["TruckType"]?.ToString(),
                        CompanyName = rdr["CompanyName"]?.ToString(),
                        NoOfCompartments = rdr["NoOfCompartments"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["NoOfCompartments"]),
                        TruckKey = rdr["TruckKey"]?.ToString(),
                        IsPermanentKey = rdr["IsPermanentKey"] != DBNull.Value && Convert.ToBoolean(rdr["IsPermanentKey"]),
                        CCOEPermWeight = rdr["CCOEPermWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CCOEPermWeight"]),
                        TankerTareWeight = rdr["TankerTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerTareWeight"]),
                        TankerGrossWeight = rdr["TankerGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerGrossWeight"]),
                        CallibrationDueDate = rdr["CallibrationDueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CallibrationDueDate"]),
                        LicenseExpireDate = rdr["LicenseExpireDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LicenseExpireDate"]),
                        TTStatus = rdr["TTStatus"]?.ToString(),
                        SequenceNumber = rdr["SequenceNumber"] == DBNull.Value ? (long?)null : Convert.ToInt64(rdr["SequenceNumber"]),
                        ProcessingResponse = rdr["PROCESSING_RESPONSE"]?.ToString(),
                        LicenseExpiryInfo = rdr["LicenseExpiryInfo"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── GET: SOD Check ────────────────────────────────────────────────────
        // Checks TASDay table for a row with EndDateTime IS NULL
        [HttpGet]
        public IActionResult CheckSOD()
        {
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT TOP 1 TASDayId
                    FROM   TASDay
                    WHERE  EndDateTime IS NULL", con);

                con.Open();
                var val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value)
                    return Json(new { sodStarted = false, tasDayId = (int?)null });

                return Json(new { sodStarted = true, tasDayId = Convert.ToInt32(val) });
            }
            catch (Exception ex)
            {
                return Json(new { sodStarted = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetEntryReasons()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT  TruckCancellationReasonId,
                            ReasonId,
                            ReasonDescription
                    FROM    TruckCancellationReason
                    WHERE   ISNULL(ReasonType, 0) = 21
                    ORDER BY ReasonDescription", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new
                    {
                        truckCancellationReasonId = Convert.ToInt64(rdr["TruckCancellationReasonId"]),
                        reasonId = rdr["ReasonId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ReasonId"]),
                        reasonDescription = rdr["ReasonDescription"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }

            return Json(list);
        }

        // ── POST: Manual Entry ────────────────────────────────────────────────
        // Full transactional flow:
        //   1. Re-verify SOD inside transaction
        //   2. Guard: ensure truck is still IsInside=0
        //   3. INSERT TruckEntry
        //   4. UPDATE Trucks SET IsInside=1
        [HttpPost]
        public IActionResult PostManualEntry([FromBody] ManualEntryRequest req)
        {
            if (req == null || req.TruckId <= 0)
                return Json(new ApiResult { Success = false, Message = "Invalid request." });

            if (string.IsNullOrWhiteSpace(req.Reason))
                return Json(new ApiResult { Success = false, Message = "Reason is required." });

            try
            {
                using var con = new SqlConnection(_conn);
                con.Open();
                using var trx = con.BeginTransaction();

                // Step 1: Re-verify SOD inside the transaction
                using var sodCmd = new SqlCommand(@"
                    SELECT TOP 1 TASDayId
                    FROM   TASDay
                    WHERE  EndDateTime IS NULL", con, trx);

                var sodVal = sodCmd.ExecuteScalar();
                if (sodVal == null || sodVal == DBNull.Value)
                {
                    trx.Rollback();
                    return Json(new ApiResult { Success = false, Message = "NO_SOD" });
                }
                int tasDayId = Convert.ToInt32(sodVal);

                // Step 2: Guard — truck must still be outside
                using var guardCmd = new SqlCommand(@"
                    SELECT ISNULL(IsInside, 0)
                    FROM   Trucks
                    WHERE  TruckId = @TruckId", con, trx);
                guardCmd.Parameters.AddWithValue("@TruckId", req.TruckId);
                var isInsideVal = guardCmd.ExecuteScalar();
                if (isInsideVal != null && Convert.ToBoolean(isInsideVal))
                {
                    trx.Rollback();
                    return Json(new ApiResult { Success = false, Message = "Truck is already inside the terminal." });
                }

                // Step 3: INSERT TruckEntry
                using var insCmd = new SqlCommand(@"
                    INSERT INTO TruckEntry
                        (TruckId, TASDayId, DateAndTime, IsManualEntry,
                         CreatedBy, CreatedDate, IsExited, IsSapPosted,
                         IsManualPosting, IsExitAllow)
                    VALUES
                        (@TruckId, @TASDayId, GETDATE(), 1,
                         -1, GETDATE(), 0, 0, 0, 0)", con, trx);

                insCmd.Parameters.AddWithValue("@TruckId", req.TruckId);
                insCmd.Parameters.AddWithValue("@TASDayId", tasDayId);
                insCmd.ExecuteNonQuery();

                // Step 4: UPDATE Trucks SET IsInside=1
                using var updCmd = new SqlCommand(@"
                    UPDATE Trucks
                    SET    IsInside    = 1,
                           UpdatedBy   = -1,
                           UpdatedDate = GETDATE()
                    WHERE  TruckId = @TruckId", con, trx);
                updCmd.Parameters.AddWithValue("@TruckId", req.TruckId);
                updCmd.ExecuteNonQuery();

                trx.Commit();
                return Json(new ApiResult { Success = true, Message = "Truck entry recorded successfully." });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }
    }
}
