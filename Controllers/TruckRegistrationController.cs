using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class TruckRegistrationController : Controller
    {
        private readonly string _conn;

        public TruckRegistrationController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection is missing in appsettings.json");
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View("~/Views/Workflow/Registration.cshtml");
        }

        // ── Pending Registrations ─────────────────────────────────────────────────
        // Trucks that have attendance (VW_CURRENTATTENDANCE) but no TruckRegistration yet
        [HttpGet]
        public IActionResult GetPendingRegistration()
        {
            var list = new List<TruckAttendanceModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT  CA.TruckAttendanceId,
                            CA.AttendanceDate        AS EntryDate,
                            CA.Time,
                            CA.EntryType,
                            CA.TruckNumber,
                            CA.TruckType,
                            CA.TruckKey,
                            CA.IsPermanentKey,
                            CA.CompanyName,
                            CA.NoOfCompartments,
                            CA.PrintedTareWeight,
                            CA.PrintedGrossWeight,
                            CA.TankerTareWeight,
                            CA.TankerGrossWeight,
                            CA.CCOEPermWeight,
                            CA.CallibrationDueDate,
                            CA.LicenseExpireDate,
                            CA.TTStatus,
                            CA.SequenceNumber,
                            CA.PROCESSING_RESPONSE,
                            CA.LicenseExpiryInfo,
                            TA.TruckId
                    FROM    VW_CURRENTATTENDANCE CA
                    INNER JOIN TruckAttendance TA ON TA.TruckAttendanceId = CA.TruckAttendanceId
                    WHERE   CA.TruckAttendanceId NOT IN (
                                SELECT ISNULL(TruckAttendanceId, 0)
                                FROM   TruckRegistrations
                                WHERE  ISNULL(IsDelete, 0) = 0
                            )
                    ORDER BY CA.TruckAttendanceId DESC", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new TruckAttendanceModel
                    {
                        TruckEntryId = Convert.ToInt64(rdr["TruckAttendanceId"]),
                        EntryDate = rdr["EntryDate"]?.ToString(),
                        Time = rdr["Time"]?.ToString(),
                        EntryType = rdr["EntryType"]?.ToString(),
                        TruckNumber = rdr["TruckNumber"]?.ToString(),
                        TruckType = rdr["TruckType"]?.ToString(),
                        TruckKey = rdr["TruckKey"]?.ToString(),
                        IsPermanentKey = rdr["IsPermanentKey"] != DBNull.Value && Convert.ToBoolean(rdr["IsPermanentKey"]),
                        CompanyName = rdr["CompanyName"]?.ToString(),
                        NoOfCompartments = rdr["NoOfCompartments"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["NoOfCompartments"]),
                        PrintedTareWeight = rdr["PrintedTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedTareWeight"]),
                        PrintedGrossWeight = rdr["PrintedGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["PrintedGrossWeight"]),
                        TankerTareWeight = rdr["TankerTareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerTareWeight"]),
                        TankerGrossWeight = rdr["TankerGrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TankerGrossWeight"]),
                        CCOEPermWeight = rdr["CCOEPermWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CCOEPermWeight"]),
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

        // ── Next Registration Number ──────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetNextRegistrationNumber()
        {
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT ISNULL(MAX(CAST(RegistrationNumber AS BIGINT)), 20000) + 1
                    FROM   TruckRegistrations
                    WHERE  ISNULL(IsDelete, 0) = 0
                      AND  TRY_CAST(RegistrationNumber AS BIGINT) IS NOT NULL", con);
                con.Open();
                var val = cmd.ExecuteScalar();
                long next = (val == null || val == DBNull.Value) ? 20001 : Convert.ToInt64(val);
                return Json(new { nextNumber = next.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { nextNumber = "20001", error = ex.Message });
            }
        }

        // ── Oil Companies ─────────────────────────────────────────────────────────
        // First 4 chars of CompanyName as the code
        [HttpGet]
        public IActionResult GetOilCompanies()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT DISTINCT LEFT(LTRIM(RTRIM(CompanyName)), 4) AS Code
                    FROM   CompanyInfo
                    WHERE  ISNULL(CompanyName, '') <> ''
                    ORDER BY Code", con);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    list.Add(new { code = rdr["Code"]?.ToString() ?? "", desc = "" });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── Plant / loading lookup codes ─────────────────────────────────────────
        // PlantCode comes from Configuration; description follows CompanyInfo code.
        private List<object> GetPlantCompanyLookups()
        {
            var list = new List<object>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT DISTINCT
                       LTRIM(RTRIM(C.PlantCode)) AS Code,
                       ISNULL(NULLIF(LEFT(LTRIM(RTRIM(CI.CompanyName)), 4), ''), LTRIM(RTRIM(C.PlantCode))) AS [Desc]
                FROM   Configuration C
                OUTER APPLY (
                    SELECT TOP 1 CompanyName
                    FROM   CompanyInfo
                    WHERE  ISNULL(CompanyName, '') <> ''
                    ORDER BY CompanyId
                ) CI
                WHERE  ISNULL(C.IsDelete, 0) = 0
                  AND  ISNULL(C.PlantCode, '') <> ''
                ORDER BY Code", con);

            con.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new { code = rdr["Code"]?.ToString() ?? "", desc = rdr["Desc"]?.ToString() ?? "" });

            return list;
        }

        [HttpGet]
        public IActionResult GetPlantCodes()
        {
            try
            {
                return Json(GetPlantCompanyLookups());
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetTruckCompartments(long truckId)
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT CompartmentNo, Capacity
                    FROM   TruckCompartmentDetails
                    WHERE  TruckId = @TruckId
                      AND  ISNULL(IsDelete, 0) = 0
                    ORDER BY CompartmentNo", con);
                cmd.Parameters.AddWithValue("@TruckId", truckId);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new
                    {
                        compartmentNo = Convert.ToInt32(rdr["CompartmentNo"]),
                        capacity = rdr["Capacity"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Capacity"])
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── Transporter Codes ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetTransporterCodes()
        {
            try
            {
                return Json(GetPlantCompanyLookups());
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        // ── Destination Codes ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetDestinationCodes()
        {
            try
            {
                return Json(GetPlantCompanyLookups());
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        // ── Location Codes ────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetLocationCodes()
        {
            try
            {
                return Json(GetPlantCompanyLookups());
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        // ── Customer Codes ────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetCustomerCodes()
        {
            try
            {
                return Json(GetPlantCompanyLookups());
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }

        // ── Products ──────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetProducts()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT ProductId, ProductCode, ProductName
                    FROM   Products
                    WHERE  ISNULL(IsDelete,0)=0
                    ORDER BY ProductCode", con);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    list.Add(new
                    {
                        productId = Convert.ToInt32(rdr["ProductId"]),
                        productCode = rdr["ProductCode"]?.ToString() ?? "",
                        productName = rdr["ProductName"]?.ToString() ?? ""
                    });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── Registration Reasons (ReasonType = 12) ────────────────────────────────
        [HttpGet]
        public IActionResult GetRegistrationReasons()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT TruckCancellationReasonId, ReasonDescription
                    FROM   TruckCancellationReason
                    WHERE  ISNULL(ReasonType, 0) = 12
                    ORDER BY ReasonDescription", con);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    list.Add(new { reasonDescription = rdr["ReasonDescription"]?.ToString() ?? "" });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── Post Registration ─────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult PostRegistration([FromBody] RegistrationPostRequest req)
        {
            if (req == null || req.TruckAttendanceId <= 0 || req.TruckId <= 0)
                return Json(new ApiResult { Success = false, Message = "Invalid request." });

            if (req.Compartments == null || req.Compartments.Count == 0)
                return Json(new ApiResult { Success = false, Message = "No compartment data provided." });

            try
            {
                using var con = new SqlConnection(_conn);
                con.Open();
                using var trx = con.BeginTransaction();

                // SOD check
                using var sodCmd = new SqlCommand(
                    "SELECT TOP 1 TASDayId FROM TASDay WHERE EndDateTime IS NULL", con, trx);
                var sodVal = sodCmd.ExecuteScalar();
                if (sodVal == null || sodVal == DBNull.Value)
                {
                    trx.Rollback();
                    return Json(new ApiResult { Success = false, Message = "NO_SOD" });
                }
                int tasDayId = Convert.ToInt32(sodVal);

                // Compute next registration number inside transaction (serialised)
                using var numCmd = new SqlCommand(@"
                    SELECT ISNULL(MAX(CAST(RegistrationNumber AS BIGINT)), 20000) + 1
                    FROM   TruckRegistrations
                    WHERE  ISNULL(IsDelete,0)=0
                      AND  TRY_CAST(RegistrationNumber AS BIGINT) IS NOT NULL", con, trx);
                var numVal = numCmd.ExecuteScalar();
                long regNum = (numVal == null || numVal == DBNull.Value) ? 20001 : Convert.ToInt64(numVal);
                string registrationNumber = regNum.ToString();

                // Guard: already registered for this attendance
                using var guardCmd = new SqlCommand(@"
                    SELECT COUNT(1) FROM TruckRegistrations
                    WHERE TruckAttendanceId = @AttId AND ISNULL(IsDelete,0)=0", con, trx);
                guardCmd.Parameters.AddWithValue("@AttId", req.TruckAttendanceId);
                int existing = Convert.ToInt32(guardCmd.ExecuteScalar());
                if (existing > 0)
                {
                    trx.Rollback();
                    return Json(new ApiResult { Success = false, Message = "This truck already has an active registration." });
                }

                // INSERT TruckRegistrations
                using var regCmd = new SqlCommand(@"
                    INSERT INTO TruckRegistrations
                        (OperationTypeId, TruckAttendanceId, TASDayId, IsManualEntry,
                         NoOfCompartments, RegistrationNumber, RegisteredBy, RegisteredDateTime,
                         OilCompany, PlantCode, PlantDesc, TransporterCode, TransporterDesc,
                         DestinationCode, DestinationDesc, LocationCode, LocationDesc,
                         CustomerCode, CustomerDesc, ShipmentNo,
                         TruckMovStatusId, IsVisible, IsCancelled,
                         IsWeighingRequired, IsKeyAssigned, IsBayAllocated, IsAuthorized,
                         IsRequiresReauthorization, IsReauthorized, IsRejected,
                         IsDelete, CreatedBy, CreatedDate)
                    VALUES
                        (@OperationTypeId, @TruckAttendanceId, @TASDayId, 1,
                         @NoOfCompartments, @RegistrationNumber, '-1', GETDATE(),
                         @OilCompany, @PlantCode, @PlantDesc, @TransporterCode, @TransporterDesc,
                         @DestinationCode, @DestinationDesc, @LocationCode, @LocationDesc,
                         @CustomerCode, @CustomerDesc, @ShipmentNo,
                         3, 1, 0,
                         0, 0, 0, 0,
                         0, 0, 0,
                         0, -1, GETDATE());

                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", con, trx);

                regCmd.Parameters.AddWithValue("@OperationTypeId", req.OperationTypeId);
                regCmd.Parameters.AddWithValue("@TruckAttendanceId", req.TruckAttendanceId);
                regCmd.Parameters.AddWithValue("@TASDayId", tasDayId);
                regCmd.Parameters.AddWithValue("@NoOfCompartments", req.Compartments.Count);
                regCmd.Parameters.AddWithValue("@RegistrationNumber", registrationNumber);
                regCmd.Parameters.AddWithValue("@OilCompany", (object?)req.OilCompany ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@PlantCode", (object?)req.PlantCode ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@PlantDesc", (object?)req.PlantDesc ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@TransporterCode", (object?)req.TransporterCode ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@TransporterDesc", (object?)req.TransporterDesc ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@DestinationCode", (object?)req.DestinationCode ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@DestinationDesc", (object?)req.DestinationDesc ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@LocationCode", (object?)req.LocationCode ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@LocationDesc", (object?)req.LocationDesc ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@CustomerCode", (object?)req.CustomerCode ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@CustomerDesc", (object?)req.CustomerDesc ?? DBNull.Value);
                regCmd.Parameters.AddWithValue("@ShipmentNo", (object?)req.ShipmentNo ?? DBNull.Value);

                long truckRegId = Convert.ToInt64(regCmd.ExecuteScalar());

                // INSERT TruckRegistrationDetails per compartment.
                // Some database deployments create TruckLoadingDetails from a trigger on
                // TruckRegistrationDetails, so keep this insert idempotent.
                foreach (var comp in req.Compartments)
                {
                    using var detCmd = new SqlCommand(@"
                        INSERT INTO TruckRegistrationDetails
                            (TruckRegistrationId, CompartmentNo, ProductId,
                             CompartmentCapacity, Quantity, BookedQuanitty, BayId,
                             LoadedQuantity, IsLoadingError, IsLoadingComplete,
                             IsLoadingAbborted, IsCardAuthorized, IsValidated, IsDelete,
                             CreatedBy, CreatedDate)
                        VALUES
                            (@TruckRegId, @CompNo, @ProductId,
                             0, 0, @BookedQty, 0,
                             0, 0, 0,
                             0, 0, 0, 0,
                             -1, GETDATE());

                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", con, trx);

                    detCmd.Parameters.AddWithValue("@TruckRegId", truckRegId);
                    detCmd.Parameters.AddWithValue("@CompNo", comp.CompartmentNo);
                    detCmd.Parameters.AddWithValue("@ProductId", comp.ProductId);
                    detCmd.Parameters.AddWithValue("@BookedQty", comp.Quantity);

                    long detailId = Convert.ToInt64(detCmd.ExecuteScalar());

                    using var ldCmd = new SqlCommand(@"
                        IF NOT EXISTS (
                            SELECT 1
                            FROM TruckLoadingDetails
                            WHERE TruckRegDetailId = @DetailId
                        )
                        BEGIN
                            INSERT INTO TruckLoadingDetails
                                (TruckRegDetailId, CreatedBy, CreatedDate)
                            VALUES
                                (@DetailId, -1, GETDATE())
                        END", con, trx);
                    ldCmd.Parameters.AddWithValue("@DetailId", detailId);
                    ldCmd.ExecuteNonQuery();
                }

                trx.Commit();
                return Json(new ApiResult { Success = true, Message = "Registered", RegistrationNumber = registrationNumber });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
        }
    }
}
