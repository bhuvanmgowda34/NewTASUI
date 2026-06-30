using System;
using System.Collections.Generic;

namespace NewTASUI.Models
{
    public class TruckListModel
    {
        public long TruckId { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string TruckTypeName { get; set; } = "";
        public int NoOfCompartments { get; set; }
        public string LockNo { get; set; } = "";
        public int? TruckKey { get; set; }
        public bool IsPermanentKey { get; set; }
        public bool IsInside { get; set; }
        public bool IsBioMetric { get; set; }
        public bool IsActive { get; set; }
    }

    public class TruckDetailModel
    {
        public long TruckId { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string TruckTypeId { get; set; } = "";
        public string TruckTypeName { get; set; } = "";
        public int? TruckKeyId { get; set; }
        public int? TruckKey { get; set; }
        public bool IsPermanentKey { get; set; }
        public string CompanyName { get; set; } = "";
        public int NoOfCompartments { get; set; }
        public string LockNo { get; set; } = "";
        public string? CallibrationDueDate { get; set; }
        public string? LicenseExpireDate { get; set; }
        public string? ExplosiveExpiryDate { get; set; }
        public bool IsBioMetric { get; set; }
        public bool IsActive { get; set; }
        public List<CompartmentModel> Compartments { get; set; } = new();
    }

    public class CompartmentModel
    {
        public int CompartmentNo { get; set; }
        public int Capacity { get; set; }
    }

    public class TruckTypeDropdown
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class TruckKeyDropdown
    {
        public int TruckKeyId { get; set; }
        public int TruckKey { get; set; }
    }

    public class TruckSaveModel
    {
        public long? TruckId { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string TruckTypeId { get; set; } = "";
        public int? TruckKeyId { get; set; }
        public bool IsPermanentKey { get; set; }
        public string CompanyName { get; set; } = "";
        public int NoOfCompartments { get; set; }
        public string LockNo { get; set; } = "";
        public string? CallibrationDueDate { get; set; }
        public string? LicenseExpireDate { get; set; }
        public string? ExplosiveExpiryDate { get; set; }
        public bool IsBioMetric { get; set; }
        public List<CompartmentModel> Compartments { get; set; } = new();
    }
}
