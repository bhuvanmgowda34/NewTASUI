using System;

namespace NewTASUI.Models
{
    // Represents one row in the Bay table
    public class BayModel
    {
        public int BayId { get; set; }
        public string BayName { get; set; } = "";
        public string BayDesc { get; set; } = "";
        public int? BayTypeId { get; set; }
        public string BayType { get; set; } = "";   // joined from BayType table
        public bool IsActive { get; set; }
    }

    // Represents one row in the BayType lookup table
    public class BayTypeModel
    {
        public int BayTypeId { get; set; }
        public string BayTypeName { get; set; } = "";
        public string BayTypeDesc { get; set; } = "";
    }

    // Payload the browser sends when saving (new or edit)
    public class BaySaveModel
    {
        public int? BayId { get; set; }   // null = new record
        public string BayName { get; set; } = "";
        public string BayDesc { get; set; } = "";
        public int BayTypeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class BayMaterialListModel
    {
        public int BCUProductId { get; set; }
        public int BCUId { get; set; }
        public string BCUName { get; set; } = "";
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public bool IsAdditive { get; set; }
        public int BCUArmNo { get; set; }
        public int? BCURecipeNo { get; set; }
    }
}
