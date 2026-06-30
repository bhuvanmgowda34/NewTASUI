namespace NewTASUI.Models
{
    // ── Request: POST /TruckRegistration/PostRegistration ────────────────────────
    public class RegistrationPostRequest
    {
        public long   TruckAttendanceId { get; set; }
        public long   TruckId           { get; set; }
        public int    OperationTypeId   { get; set; }   // 1=Loading, 2=Unloading
        public string? OilCompany       { get; set; }
        public string? PlantCode        { get; set; }
        public string? PlantDesc        { get; set; }
        public string? TransporterCode  { get; set; }
        public string? TransporterDesc  { get; set; }
        public string? DestinationCode  { get; set; }
        public string? DestinationDesc  { get; set; }
        public string? LocationCode     { get; set; }
        public string? LocationDesc     { get; set; }
        public string? CustomerCode     { get; set; }
        public string? CustomerDesc     { get; set; }
        public string? ShipmentNo       { get; set; }
        public string? Reason           { get; set; }

        public List<CompartmentItem> Compartments { get; set; } = new();
    }

    public class CompartmentItem
    {
        public int  CompartmentNo { get; set; }
        public int  ProductId     { get; set; }
        public int  Quantity      { get; set; }
    }

    // ── ApiResult extended with RegistrationNumber ───────────────────────────────
    // NOTE: if ApiResult already exists in your project, just ADD RegistrationNumber
    // to the existing class instead of defining a new one here.
    // If ApiResult does NOT exist yet, use this full definition:
    //
    //public class ApiResult
    //{
    //    public bool Success { get; set; }
    //    public string? Message { get; set; }
    //    public string? RegistrationNumber { get; set; }
    // }
    


}
