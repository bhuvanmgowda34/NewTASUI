using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class SODEODController : Controller
    {
        private readonly IConfiguration _cfg;
        public SODEODController(IConfiguration cfg) { _cfg = cfg; }
        private string ConnStr() => _cfg.GetConnectionString("DefaultConnection")!;

        // GET: /SODEOD/SODEOD
        public IActionResult SODEOD()
        {
            return View("~/Views/Workflow/SODEOD.cshtml");
        }

        // GET: /SODEOD/GetSODEODStatus
        [HttpGet]
        public IActionResult GetSODEODStatus()
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();

                var model = BuildStatus(con);
                return Json(model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /SODEOD/CanProceedForOperations
        [HttpGet]
        public IActionResult CanProceedForOperations()
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();

                var gate = GetCurrentGate(con);
                return Json(new
                {
                    success = true,
                    canProceed = gate.isSODDone && !gate.isEODDone,
                    isSODDone = gate.isSODDone,
                    isEODDone = gate.isEODDone,
                    currentDayId = gate.dayId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, canProceed = false });
            }
        }

        // POST: /SODEOD/StartDay
        [HttpPost]
        public IActionResult StartDay()
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();
                AcquireWorkflowLock(con, tx);

                var openDayId = GetOpenDayId(con, tx);
                if (openDayId.HasValue)
                {
                    tx.Rollback();
                    return Json(new { success = false, message = "Start of Day already done. Please complete End of Day first." });
                }

                int dayId;
                using (var insDay = new SqlCommand(@"
                    INSERT INTO TASDay
                        (StartDateTime, EndDateTime, IsDelete, CreatedBy, CreatedDate)
                    VALUES
                        (GETDATE(), NULL, 0, 1, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS int);", con, tx))
                {
                    dayId = Convert.ToInt32(insDay.ExecuteScalar());
                }

                using (var insSodEod = new SqlCommand(@"
                    IF EXISTS (SELECT 1 FROM TASSODEOD)
                    BEGIN
                        UPDATE TOP (1) TASSODEOD
                        SET TASDayId = @DayId,
                            IsSOD = 1,
                            IsEOD = 0,
                            UpdatedBy = 1,
                            UpdatedDate = GETDATE()
                    END
                    ELSE
                    BEGIN
                        INSERT INTO TASSODEOD
                            (TASDayId, IsSOD, IsEOD, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
                        VALUES
                            (@DayId, 1, 0, 1, GETDATE(), 1, GETDATE())
                    END", con, tx))
                {
                    insSodEod.Parameters.AddWithValue("@DayId", dayId);
                    insSodEod.ExecuteNonQuery();
                }

                tx.Commit();
                return Json(new { success = true, message = "Start of Day completed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /SODEOD/EndDay
        [HttpPost]
        public IActionResult EndDay()
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();
                AcquireWorkflowLock(con, tx);

                var openDayId = GetOpenDayId(con, tx);
                if (!openDayId.HasValue)
                {
                    tx.Rollback();
                    return Json(new { success = false, message = "No active day found. Please complete Start of Day first." });
                }

                var gate = GetCurrentGate(con, tx, openDayId.Value);
                if (!gate.isSODDone)
                {
                    tx.Rollback();
                    return Json(new { success = false, message = "Start of Day is not completed. End of Day is not allowed." });
                }

                if (gate.isEODDone)
                {
                    tx.Rollback();
                    return Json(new { success = false, message = "End of Day is already completed." });
                }

                using (var truckChk = new SqlCommand(@"
                    SELECT COUNT(1)
                    FROM Trucks
                    WHERE ISNULL(IsInside, 0) = 1
                      AND ISNULL(IsDelete, 0) = 0", con, tx))
                {
                    var insideCount = Convert.ToInt32(truckChk.ExecuteScalar());
                    if (insideCount > 0)
                    {
                        tx.Rollback();
                        return Json(new { success = false, message = "Please make sure all the trucks are exited before making EOD", code = "TRUCKS_INSIDE" });
                    }
                }

                using (var updDay = new SqlCommand(@"
                    UPDATE TASDay
                    SET EndDateTime = GETDATE(),
                        UpdatedBy = 1,
                        UpdatedDate = GETDATE()
                    WHERE TASDayId = @DayId AND IsDelete = 0", con, tx))
                {
                    updDay.Parameters.AddWithValue("@DayId", openDayId.Value);
                    updDay.ExecuteNonQuery();
                }

                using (var upd = new SqlCommand(@"
                    UPDATE TOP (1) TASSODEOD
                    SET TASDayId = @DayId,
                        IsSOD = 1,
                        IsEOD = 1,
                        UpdatedBy = 1,
                        UpdatedDate = GETDATE()", con, tx))
                {
                    upd.Parameters.AddWithValue("@DayId", openDayId.Value);
                    upd.ExecuteNonQuery();
                }

                tx.Commit();
                return Json(new { success = true, message = "End of Day completed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private SODEODStatusResponseModel BuildStatus(SqlConnection con)
        {
            var gate = GetCurrentGate(con);

            var model = new SODEODStatusResponseModel
            {
                CurrentDayId = gate.dayId,
                ServerTime = DateTime.Now,
                ExpectedEODTime = "23:59",
                CurrentSODDateTime = gate.startDateTime,
                CurrentEODDateTime = gate.endDateTime,
                IsSODDone = gate.isSODDone,
                IsEODDone = gate.isEODDone,
                CanStartDay = !gate.dayId.HasValue || gate.isEODDone,
                CanEndDay = gate.dayId.HasValue && gate.isSODDone && !gate.isEODDone,
                CanProceedForWorkflow = gate.isSODDone && !gate.isEODDone,
                History = GetHistory(con)
            };

            return model;
        }

        private static List<SODEODHistoryRowModel> GetHistory(SqlConnection con)
        {
            var list = new List<SODEODHistoryRowModel>();
            using var cmd = new SqlCommand(@"
                SELECT
                    d.TASDayId,
                    d.StartDateTime,
                    d.EndDateTime,
                    ISNULL(se.IsSOD, 0) AS IsSOD,
                    ISNULL(se.IsEOD, 0) AS IsEOD
                FROM TASDay d
                OUTER APPLY (
                    SELECT TOP 1 x.IsSOD, x.IsEOD
                    FROM TASSODEOD x
                    WHERE x.TASDayId = d.TASDayId
                    ORDER BY ISNULL(x.UpdatedDate, x.CreatedDate) DESC, x.TASSODEODId DESC
                ) se
                WHERE d.IsDelete = 0
                ORDER BY d.StartDateTime DESC", con);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var sod = Convert.ToDateTime(r["StartDateTime"]);
                var eod = r["EndDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDateTime"]);
                list.Add(new SODEODHistoryRowModel
                {
                    TASDayId = Convert.ToInt32(r["TASDayId"]),
                    DayDate = sod.ToString("dd/MM/yyyy"),
                    StartDateTime = sod,
                    EndDateTime = eod,
                    IsSOD = Convert.ToBoolean(r["IsSOD"]),
                    IsEOD = Convert.ToBoolean(r["IsEOD"])
                });
            }

            return list;
        }

        private static int? GetOpenDayId(SqlConnection con, SqlTransaction tx)
        {
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 TASDayId
                FROM TASDay
                WHERE IsDelete = 0
                  AND EndDateTime IS NULL
                ORDER BY StartDateTime DESC", con, tx);
            var v = cmd.ExecuteScalar();
            return v == null || v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        private static void AcquireWorkflowLock(SqlConnection con, SqlTransaction tx)
        {
            using var cmd = new SqlCommand(@"
                DECLARE @Result INT;
                EXEC @Result = sp_getapplock
                    @Resource = 'SODEOD_WORKFLOW_LOCK',
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 10000;
                SELECT @Result;", con, tx);

            var result = Convert.ToInt32(cmd.ExecuteScalar());
            if (result < 0)
                throw new Exception("Could not acquire SOD/EOD lock. Please try again.");
        }

        private static (int? dayId, DateTime? startDateTime, DateTime? endDateTime, bool isSODDone, bool isEODDone) GetCurrentGate(SqlConnection con)
        {
            using var cmd = new SqlCommand(@"
                SELECT TOP 1
                    d.TASDayId,
                    d.StartDateTime,
                    d.EndDateTime,
                    ISNULL(se.IsSOD, 0) AS IsSOD,
                    ISNULL(se.IsEOD, 0) AS IsEOD
                FROM TASDay d
                LEFT JOIN TASSODEOD se ON se.TASDayId = d.TASDayId
                WHERE d.IsDelete = 0
                ORDER BY d.StartDateTime DESC", con);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                return (null, null, null, false, false);

            return (
                Convert.ToInt32(r["TASDayId"]),
                Convert.ToDateTime(r["StartDateTime"]),
                r["EndDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDateTime"]),
                Convert.ToBoolean(r["IsSOD"]),
                Convert.ToBoolean(r["IsEOD"])
            );
        }

        private static (int? dayId, DateTime? startDateTime, DateTime? endDateTime, bool isSODDone, bool isEODDone) GetCurrentGate(SqlConnection con, SqlTransaction tx, int dayId)
        {
            using var cmd = new SqlCommand(@"
                SELECT TOP 1
                    d.TASDayId,
                    d.StartDateTime,
                    d.EndDateTime,
                    ISNULL(se.IsSOD, 0) AS IsSOD,
                    ISNULL(se.IsEOD, 0) AS IsEOD
                FROM TASDay d
                LEFT JOIN TASSODEOD se ON se.TASDayId = d.TASDayId
                WHERE d.IsDelete = 0
                  AND d.TASDayId = @DayId
                ORDER BY d.StartDateTime DESC", con, tx);
            cmd.Parameters.AddWithValue("@DayId", dayId);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                return (null, null, null, false, false);

            return (
                Convert.ToInt32(r["TASDayId"]),
                Convert.ToDateTime(r["StartDateTime"]),
                r["EndDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDateTime"]),
                Convert.ToBoolean(r["IsSOD"]),
                Convert.ToBoolean(r["IsEOD"])
            );
        }
    }
}
