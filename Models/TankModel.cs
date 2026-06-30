using System;

namespace NewTASUI.Models
{
    // ── Tank list (table) ─────────────────────────────
    public class TankListModel
    {
        public int TankId { get; set; }
        public int? TankNo { get; set; }
        public string TankName { get; set; } = "";
        public string SapTankNo { get; set; } = "";
        public string Description { get; set; } = "";
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int? TankType { get; set; }
        public string TankTypeName { get; set; } = "";
        public int? TankModeName { get; set; }
        public string TankModeLabel { get; set; } = "";
        public int? GaugeType { get; set; }
        public string GaugeTypeName { get; set; } = "";
        public decimal? Capacity { get; set; }
        public decimal? MinLevel { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? SafeHeightLevel { get; set; }
        public bool IsActive { get; set; }
    }

    // ── Tank detail (modal / edit) ─────────────────────
    public class TankDetailModel
    {
        public int TankId { get; set; }
        public int? TankNo { get; set; }
        public string TankName { get; set; } = "";
        public string SapTankNo { get; set; } = "";
        public string Description { get; set; } = "";
        public int? ProductId { get; set; }
        public int? TankType { get; set; }
        public int? TankModeName { get; set; }
        public int? GaugeType { get; set; }
        public decimal? Capacity { get; set; }
        public decimal? MinLevel { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? SafeHeightLevel { get; set; }
        public bool IsActive { get; set; }
    }

    // ── Save / Update ──────────────────────────────────
    public class TankSaveModel
    {
        public int TankId { get; set; }
        public int? TankNo { get; set; }
        public string TankName { get; set; } = "";
        public string? Description { get; set; }
        public string? SapTankNo { get; set; }
        public int? ProductId { get; set; }
        public int? TankType { get; set; }
        public int? TankModeName { get; set; }
        public int? GaugeType { get; set; }
        public decimal? Capacity { get; set; }
        public decimal? MinLevel { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? SafeHeightLevel { get; set; }
        public bool IsActive { get; set; }
    }
}