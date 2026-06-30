using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class TruckStatusController : Controller
    {
        private readonly string _conn;

        public TruckStatusController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection missing");
        }

        [HttpGet]
        public IActionResult Status()
        {
            return View("~/Views/Workflow/TruckStatus.cshtml");
        }

        // ── Statuses (IsForTracking = 1) ──────────────────────────────────────
        [HttpGet]
        public IActionResult GetStatuses()
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT TruckMovStatusId, StatusName
                    FROM   TruckMoveStatus
                    WHERE  ISNULL(IsForTracking, 0) = 1
                      AND  ISNULL(IsDelete, 0) = 0
                    ORDER BY TruckMovStatusId", con);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    list.Add(new
                    {
                        truckMovStatusId = Convert.ToInt32(rdr["TruckMovStatusId"]),
                        statusName = rdr["StatusName"]?.ToString() ?? ""
                    });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }

        // ── Registered Trucks from VW_RegisteredTrucks_Current ───────────────
        [HttpGet]
        public IActionResult GetRegisteredTrucks()
        {
            var list = new List<TruckStatusModel>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
                    SELECT
                        TruckRegistrationId,
                        TruckType,
                        TruckTypeId,
                        OperationTypeId,
                        FANId,
                        TruckAttendanceId,
                        TruckId,
                        FANNumber,
                        RegistrationNumber,
                        RegNo,
                        EntryType,
                        TruckKey,
                        AuthorizedBy,
                        AuthorizedDateTime,
                        SealNumber,
                        TASDayId,
                        OilCompany,
                        LocationCode,
                        LocationDesc,
                        TransporterCode,
                        TransporterDesc,
                        DestinationCode,
                        DestinationDesc,
                        CustomerCode,
                        CustomerDesc,
                        TareWeight,
                        GrossWeight,
                        NETWeight,
                        IsWeighingRequired,
                        ShipmentNo,
                        DocumentNo,
                        AllocatedBay,
                        Remarks,
                        ApprovedBy,
                        SAPCreatedBy,
                        VALUATION_TYPE,
                        TruckMovStatusId,
                        IsExpired,
                        StatusName,
                        Products
                    FROM VW_RegisteredTrucks_Current
                    ORDER BY TruckRegistrationId DESC", con);

                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new TruckStatusModel
                    {
                        TruckRegistrationId = Convert.ToInt64(rdr["TruckRegistrationId"]),
                        TruckType = rdr["TruckType"]?.ToString(),
                        TruckTypeId = rdr["TruckTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["TruckTypeId"]),
                        OperationTypeId = rdr["OperationTypeId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["OperationTypeId"]),
                        FANId = rdr["FANId"] == DBNull.Value ? (long?)null : Convert.ToInt64(rdr["FANId"]),
                        TruckAttendanceId = rdr["TruckAttendanceId"] == DBNull.Value ? (long?)null : Convert.ToInt64(rdr["TruckAttendanceId"]),
                        TruckId = rdr["TruckId"] == DBNull.Value ? (long?)null : Convert.ToInt64(rdr["TruckId"]),
                        FANNumber = rdr["FANNumber"]?.ToString(),
                        RegistrationNumber = rdr["RegistrationNumber"]?.ToString(),
                        RegNo = rdr["RegNo"]?.ToString(),
                        EntryType = rdr["EntryType"]?.ToString(),
                        TruckKey = rdr["TruckKey"]?.ToString(),
                        AuthorizedBy = rdr["AuthorizedBy"]?.ToString(),
                        AuthorizedDateTime = rdr["AuthorizedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["AuthorizedDateTime"]),
                        SealNumber = rdr["SealNumber"]?.ToString(),
                        TASDayId = rdr["TASDayId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["TASDayId"]),
                        OilCompany = rdr["OilCompany"]?.ToString(),
                        LocationCode = rdr["LocationCode"]?.ToString(),
                        LocationDesc = rdr["LocationDesc"]?.ToString(),
                        TransporterCode = rdr["TransporterCode"]?.ToString(),
                        TransporterDesc = rdr["TransporterDesc"]?.ToString(),
                        DestinationCode = rdr["DestinationCode"]?.ToString(),
                        DestinationDesc = rdr["DestinationDesc"]?.ToString(),
                        CustomerCode = rdr["CustomerCode"]?.ToString(),
                        CustomerDesc = rdr["CustomerDesc"]?.ToString(),
                        TareWeight = rdr["TareWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["TareWeight"]),
                        GrossWeight = rdr["GrossWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["GrossWeight"]),
                        NETWeight = rdr["NETWeight"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NETWeight"]),
                        IsWeighingRequired = rdr["IsWeighingRequired"] != DBNull.Value && Convert.ToBoolean(rdr["IsWeighingRequired"]),
                        ShipmentNo = rdr["ShipmentNo"]?.ToString(),
                        DocumentNo = rdr["DocumentNo"]?.ToString(),
                        AllocatedBay = rdr["AllocatedBay"]?.ToString(),
                        Remarks = rdr["Remarks"]?.ToString(),
                        ApprovedBy = rdr["ApprovedBy"]?.ToString(),
                        SAPCreatedBy = rdr["SAPCreatedBy"]?.ToString(),
                        ValuationType = rdr["VALUATION_TYPE"]?.ToString(),
                        TruckMovStatusId = rdr["TruckMovStatusId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["TruckMovStatusId"]),
                        IsExpired = rdr["IsExpired"] != DBNull.Value && Convert.ToBoolean(rdr["IsExpired"]),
                        StatusName = rdr["StatusName"]?.ToString(),
                        Products = rdr["Products"]?.ToString(),
                        NoOfCompartments = null  // not in view directly
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
        public IActionResult GetLoadingDetails(long truckRegistrationId)
        {
            var list = new List<object>();
            try
            {
                using var con = new SqlConnection(_conn);
                using var cmd = new SqlCommand(@"
            SELECT
                TRD.TruckRegDetailId,
                TRD.CompartmentNo,
                B.BayName,
                P.ProductCode,
                P.ProductName,
                TRD.BookedQuanitty,
                TRD.LoadedQuantity,
                TRD.LoadingStartDateTime,
                TRD.LoadingEndDateTime,
                TRD.IsLoadingComplete,
                TRD.IsCardAuthorized,
                TRD.ManualCardAuthBy,
                TRD.ManualCardAuthDateTime,
                TRD.LockNumber,
                TRD.UOM,
                TRD.TankName
            FROM TruckRegistrationDetails TRD
            INNER JOIN Products P ON P.ProductId = TRD.ProductId
            LEFT JOIN Bay B ON B.BayId = TRD.BayId
            WHERE TRD.TruckRegistrationId = @Id
              AND ISNULL(TRD.IsDelete, 0) = 0
            ORDER BY TRD.CompartmentNo", con);
                cmd.Parameters.AddWithValue("@Id", truckRegistrationId);
                con.Open();
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new
                    {
                        truckRegDetailId = Convert.ToInt64(rdr["TruckRegDetailId"]),
                        compartmentNo = rdr["CompartmentNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["CompartmentNo"]),
                        bayName = rdr["BayName"]?.ToString(),
                        productCode = rdr["ProductCode"]?.ToString(),
                        productName = rdr["ProductName"]?.ToString(),
                        bookedQuantity = rdr["BookedQuanitty"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["BookedQuanitty"]),
                        loadedQuantity = rdr["LoadedQuantity"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["LoadedQuantity"]),
                        loadingStartDateTime = rdr["LoadingStartDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LoadingStartDateTime"]),
                        loadingEndDateTime = rdr["LoadingEndDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["LoadingEndDateTime"]),
                        isLoadingComplete = rdr["IsLoadingComplete"] != DBNull.Value && Convert.ToBoolean(rdr["IsLoadingComplete"]),
                        isCardAuthorized = rdr["IsCardAuthorized"] != DBNull.Value && Convert.ToBoolean(rdr["IsCardAuthorized"]),
                        manualCardAuthBy = rdr["ManualCardAuthBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ManualCardAuthBy"]),
                        manualCardAuthDateTime = rdr["ManualCardAuthDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["ManualCardAuthDateTime"]),
                        lockNumber = rdr["LockNumber"]?.ToString(),
                        uom = rdr["UOM"]?.ToString(),
                        tankName = rdr["TankName"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResult { Success = false, Message = ex.Message });
            }
            return Json(list);
        }
    }
}
