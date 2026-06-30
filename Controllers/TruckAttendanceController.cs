using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;
using System.Data;

namespace NewTASUI.Controllers
{
    public class TruckAttendanceController : Controller
    {
        private readonly string _conn;

        public TruckAttendanceController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection is missing in appsettings.json");
        }

        [HttpGet]
        public IActionResult Attendance()
        {
            return View("~/Views/Workflow/Attendance.cshtml");
        }

        [HttpGet]
        public IActionResult GetPendingAttendance()
        {
            var list = new List<TruckAttendanceModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT  V.TruckEntryId,
                            V.EntryDate,
                            V.Time,
                            V.EntryType,
                            V.TruckNumber,
                            V.TruckType,
                            V.CompanyName,
                            V.NoOfCompartments,
                            V.TruckKey,
                            V.IsPermanentKey,
                            V.CCOEPermWeight,
                            V.TankerTareWeight,
                            V.TankerGrossWeight,
                            V.CallibrationDueDate,
                            V.LicenseExpireDate,
                            V.TTStatus,
                            V.SequenceNumber,
                            V.PROCESSING_RESPONSE,
                            V.LicenseExpiryInfo,
                            TE.TruckId
                    FROM    VW_TruckEntry V
                    INNER JOIN TruckEntry TE ON TE.TruckEntryId = V.TruckEntryId
                    WHERE V.TruckEntryId NOT IN (SELECT ISNULL(TruckEntryId,0) FROM TruckAttendance WHERE ISNULL(IsDelete,0)=0)
                    ORDER BY V.TruckEntryId DESC", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new TruckAttendanceModel
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
                        LicenseExpiryInfo = rdr["LicenseExpiryInfo"]?.ToString(),
                        TruckId = Convert.ToInt64(rdr["TruckId"])
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetCompletedAttendance()
        {
            var list = new List<TruckAttendanceModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT TTStatus, IsSapPosted, IsManualPosting, EntryType, TruckAttendanceId, AttendanceDate, Time, TruckId, TruckKey, IsPermanentKey, TruckNumber, TruckType, CompanyName, NoOfCompartments, CCOEPermWeight, CallibrationDueDate, LicenseExpireDate, TankerGrossWeight, TankerTareWeight, PrintedTareWeight, PrintedGrossWeight, PROCESSING_RESPONSE, SequenceNumber, LicenseExpiryInfo
                    FROM VW_CURRENTATTENDANCE
                    ORDER BY TruckAttendanceId DESC", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new TruckAttendanceModel
                    {
                        TruckEntryId = Convert.ToInt64(rdr["TruckAttendanceId"]),
                        EntryDate = rdr["AttendanceDate"]?.ToString(),
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
                        PrintedTareWeight = rdr["PrintedTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedTareWeight"]),
                        PrintedGrossWeight = rdr["PrintedGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedGrossWeight"]),
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

        [HttpGet]
        public IActionResult GetAttendanceReasons()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT  TruckCancellationReasonId, ReasonId, ReasonDescription
                    FROM    TruckCancellationReason
                    WHERE   ISNULL(ReasonType, 0) = 8
                    ORDER BY ReasonDescription", con);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while(rdr.Read()) {
                    list.Add(new { reasonDescription = rdr["ReasonDescription"]?.ToString() ?? "" });
                }
            } 
            catch(Exception) { return Json(new ApiResult
            {
                Success = false,
                Message = "Something went wrong",
            }); }
            return Json(list);
        }

        [HttpGet]
        public IActionResult CheckSOD()
        {
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT TOP 1 TASDayId FROM TASDay WHERE EndDateTime IS NULL", con);
                con.Open();
                var val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value) return Json(new { sodStarted = false });
                return Json(new { sodStarted = true, tasDayId = Convert.ToInt32(val) });
            } catch(Exception ex) { return Json(new { sodStarted = false, error = ex.Message }); }
        }

        [HttpPost]
        public IActionResult PostAttendance([FromBody] AttendancePostRequest req)
        {
            if (req == null || req.TruckId <= 0 || req.TruckEntryId <= 0) 
                return Json(new ApiResult { Success = false, Message = "Invalid request." });
            try {
                using var con = new SqlConnection(_conn); con.Open();
                using var trx = con.BeginTransaction();
                
                using var sodCmd = new SqlCommand(@"SELECT TOP 1 TASDayId FROM TASDay WHERE EndDateTime IS NULL", con, trx);
                var sodVal = sodCmd.ExecuteScalar();
                if (sodVal == null || sodVal == DBNull.Value) { trx.Rollback(); return Json(new ApiResult { Success = false, Message = "NO_SOD" }); }
                int tasDayId = Convert.ToInt32(sodVal);

                using var insCmd = new SqlCommand(@"
                    INSERT INTO TruckAttendance (TruckEntryId, TruckId, TruckType, TASDayId, DateAndTime, IsManualEntry, IsCarryForward, PreviousAttendanceId, IsDelete, CreatedBy, CreatedDate)
                    SELECT @TruckEntryId, @TruckId, CAST(TruckTypeId AS nvarchar(50)), @TASDayId, GETDATE(), 1, 0, 0, 0, -1, GETDATE() FROM Trucks WHERE TruckId = @TruckId", con, trx);
                
                insCmd.Parameters.AddWithValue("@TruckEntryId", req.TruckEntryId);
                insCmd.Parameters.AddWithValue("@TruckId", req.TruckId);
                insCmd.Parameters.AddWithValue("@TASDayId", tasDayId);
                insCmd.ExecuteNonQuery();
                trx.Commit();
                return Json(new ApiResult { Success = true, Message = "Attendance Updated successfully" });
            } catch(Exception ex) { return Json(new ApiResult { Success = false, Message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult DeleteAttendance([FromBody] DeleteAttendanceRequest req) {
            if (req == null || req.Id <= 0) return Json(new ApiResult { Success = false, Message = "Invalid input" });
            try {
                using var con = new SqlConnection(_conn); con.Open();
                using var cmd = new SqlCommand(@"UPDATE TruckAttendance SET IsDelete = 1, DeletedBy = -1, DeletedDate = GETDATE() WHERE TruckAttendanceId = @Id", con);
                cmd.Parameters.AddWithValue("@Id", req.Id);
                cmd.ExecuteNonQuery();
                return Json(new ApiResult { Success = true, Message = "Attendance deleted" });
            } catch(Exception ex) { return Json(new ApiResult{Success = false, Message = ex.Message}); }
        }
    }
}
