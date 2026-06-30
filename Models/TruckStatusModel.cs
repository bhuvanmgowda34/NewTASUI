namespace NewTASUI.Models
{
    public class TruckStatusModel
    {
        public long TruckRegistrationId { get; set; }
        public string? TruckType { get; set; }
        public int? TruckTypeId { get; set; }
        public int? OperationTypeId { get; set; }
        public long? FANId { get; set; }
        public long? TruckAttendanceId { get; set; }
        public long? TruckId { get; set; }
        public string? FANNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? RegNo { get; set; }
        public string? EntryType { get; set; }
        public string? TruckKey { get; set; }
        public string? AuthorizedBy { get; set; }
        public DateTime? AuthorizedDateTime { get; set; }
        public string? SealNumber { get; set; }
        public int? TASDayId { get; set; }
        public string? OilCompany { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationDesc { get; set; }
        public string? TransporterCode { get; set; }
        public string? TransporterDesc { get; set; }
        public string? DestinationCode { get; set; }
        public string? DestinationDesc { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerDesc { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NETWeight { get; set; }
        public bool IsWeighingRequired { get; set; }
        public string? ShipmentNo { get; set; }
        public string? DocumentNo { get; set; }
        public string? AllocatedBay { get; set; }
        public string? Remarks { get; set; }
        public string? ApprovedBy { get; set; }
        public string? SAPCreatedBy { get; set; }
        public string? ValuationType { get; set; }
        public int? TruckMovStatusId { get; set; }
        public bool IsExpired { get; set; }
        public string? StatusName { get; set; }
        public string? Products { get; set; }
        public int? NoOfCompartments { get; set; }
    }
}
