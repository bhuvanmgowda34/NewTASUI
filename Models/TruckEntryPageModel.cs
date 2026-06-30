using System;
using System.Collections.Generic;

namespace NewTASUI.Models
{ 
    public class AvailableTruckModel
{
    public long TruckId { get; set; }
    public string? RegistrationNumber { get; set; }   // Col: No
    public string? TruckType { get; set; }   // Col: Type  (from TruckType.Description)
    public int? TruckKeyId { get; set; }   // Col: Key
    public string? CompanyName { get; set; }   // Col: Company Name
    public int NoOfCompartments { get; set; }   // Col: No Of Comp
    public bool IsPermanentKey { get; set; }   // Col: IsPerman... (checkbox)
    public decimal? PrintedTareWeight { get; set; }   // Col: Printed Tare
    public decimal? PrintedGrossWeight { get; set; }   // Col: Printed Gr...
    public decimal? TankerTareWeight { get; set; }   // Col: Tanker Tare
    public decimal? TankerGrossWeight { get; set; }   // Col: Tanker Gross
    public decimal? CCOEPermWeight { get; set; }   // Col: CCOE Perm
    public DateTime? CallibrationDueDate { get; set; }   // Col: Callibration...
    public DateTime? LicenseExpireDate { get; set; }   // Col: License E...
    public DateTime? ExplosiveExpiryDate { get; set; }   // Col: Explosive Expiry
                                                         // Action column is empty / reserved
        public int? TruckKey { get; set; }        // Col: Key (actual key number)
        public bool IsPermanent { get; set; }     // Col: IsPermanent (from TruckKeyes)
    }

// ── Today's Trucks (RIGHT TABLE) ──────────────────────────────────────────
// Source: VW_TruckEntry (already joined to Trucks, TruckType, TruckKeyes)
public class TruckEntryViewModel
{
    public long TruckEntryId { get; set; }
    // Entry group
    public string? EntryDate { get; set; }   // Col: Date
    public string? Time { get; set; }   // Col: Time
    public string? EntryType { get; set; }   // Col: Type  (M / A)
                                             // Truck group
    public string? TruckNumber { get; set; }   // Col: No.
    public string? TruckType { get; set; }   // Col: Type  (truck type)
    public string? CompanyName { get; set; }   // Col: Company...
    public int? NoOfCompartments { get; set; }   // Col: No Of C...
                                                 // Key group
    public string? TruckKey { get; set; }   // Col: Number  (TruckKey value)
    public bool IsPermanentKey { get; set; }   // Col: Is Permanent (checkbox)
                                               // Weight group
    public decimal? CCOEPermWeight { get; set; }   // Col: CCOE Per...
    public decimal? TankerTareWeight { get; set; }   // Col: Tanker T...
    public decimal? TankerGrossWeight { get; set; }   // Col: Tanker Gross
                                                      // Date group
    public DateTime? CallibrationDueDate { get; set; }   // Col: Callibration...
    public DateTime? LicenseExpireDate { get; set; }   // Col: License Expi...
                                                       // ERP Posting group
    public string? TTStatus { get; set; }   // Col: Status  (Y / N)
    public long? SequenceNumber { get; set; }   // Col: Seq. No
    public string? ProcessingResponse { get; set; }   // Col: Response  (SAPResponse)
    public string? LicenseExpiryInfo { get; set; }   // supplementary — not a column but useful
}

// ── Manual Entry POST payload ─────────────────────────────────────────────
public class ManualEntryRequest
{
    public long TruckId { get; set; }
    public string? Reason { get; set; }   // System Issue / Manual Override / Emergency Entry
}

// ── Generic API response ──────────────────────────────────────────────────
public class ApiResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? RegistrationNumber { get; set; }

    }
}
