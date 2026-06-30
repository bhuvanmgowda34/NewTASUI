using System.Collections.Generic;

namespace NewTASUI.Models
{
    // ── BCU list row (table) ─────────────────────────────────────────────
    public class BCUListModel
    {
        public int BCUId { get; set; }
        public string BCUName { get; set; } = "";
        public int BCUNumber { get; set; }
        public string BayName { get; set; } = "";
        public string BCUType { get; set; } = "";
        public string PrimaryComPort { get; set; } = "";
        public int SlaveAddress { get; set; }
        public string SecondaryComPort { get; set; } = "";
        public int? RedundantSlaveAddress { get; set; }
        public bool IsCardReaderIntegrated { get; set; }
        public bool IsActive { get; set; }
        public string ArmType { get; set; } = "";
        public int? ArmNo { get; set; }
        public bool IsRIT { get; set; }
        public bool MFMStatus { get; set; }
    }

    // ── BCU detail (editor / form fill) ──────────────────────────────────
    public class BCUDetailModel
    {
        public int BCUId { get; set; }
        public string BCUName { get; set; } = "";
        public int BCUNumber { get; set; }
        public int? BCUTypeId { get; set; }
        public string BCUTypeName { get; set; } = "";
        public int? BayId { get; set; }
        public string BayName { get; set; } = "";
        public int SlaveAddress { get; set; }
        public int? RedundantSlaveAddress { get; set; }
        public int ComPortId { get; set; }
        public string PrimaryComPort { get; set; } = "";
        public int? RedundantComportId { get; set; }
        public string SecondaryComPort { get; set; } = "";
        public bool IsCardReaderIntegrated { get; set; }
        public bool IsActive { get; set; }
        public bool IsRIT { get; set; }
        public bool MFMStatus { get; set; }
        public int? ArmTypeId { get; set; }
        public string ArmType { get; set; } = "";
        public int? ArmNo { get; set; }
    }

    // ── Save / Update payload ────────────────────────────────────────────
    public class BCUSaveModel
    {
        public int? BCUId { get; set; }
        public string BCUName { get; set; } = "";
        public int BCUNumber { get; set; }
        public int? BCUTypeId { get; set; }
        public int? BayId { get; set; }
        public int SlaveAddress { get; set; }
        public int? RedundantSlaveAddress { get; set; }
        public int ComPortId { get; set; }
        public int? RedundantComportId { get; set; }
        public bool IsCardReaderIntegrated { get; set; }
        public bool IsActive { get; set; }
        public bool IsRIT { get; set; }
        public bool MFMStatus { get; set; }
        public int? ArmTypeId { get; set; }
        public int? ArmNo { get; set; }
    }

    // ── Dropdowns ────────────────────────────────────────────────────────
    public class BCUTypeDropdown
    {
        public int BCUTypeId { get; set; }
        public string BCUType { get; set; } = "";
    }

    public class ComPortDropdown
    {
        public int ComPortId { get; set; }
        public string ComportNumber { get; set; } = "";
    }

    public class ArmTypeDropdown
    {
        public int ArmTypeId { get; set; }
        public string ArmType { get; set; } = "";
    }

    // ── BCU Products ─────────────────────────────────────────────────────
    public class BCUProductListModel
    {
        public int BCUProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public bool IsAdditive { get; set; }
        public int BCUArmNo { get; set; }
        public int? BCURecipeNo { get; set; }
    }

    public class BCUProductSaveModel
    {
        public int BCUId { get; set; }
        public int ProductId { get; set; }
        public bool IsAdditive { get; set; }
        public int BCUArmNo { get; set; }
        public int? BCURecipeNo { get; set; }
    }

    public class BCUProductUpdateModel
    {
        public int BCUProductId { get; set; }
        public bool IsAdditive { get; set; }
        public int BCUArmNo { get; set; }
        public int? BCURecipeNo { get; set; }
    }
}
