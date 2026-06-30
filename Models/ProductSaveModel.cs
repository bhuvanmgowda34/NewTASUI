namespace NewTASUI.Models
{
    // ── Products save / update payload ───────────────────────────────────
    public class ProductSaveModel
    {
        public int ProductId { get; set; }   // 0 = INSERT, >0 = UPDATE
        public string ProductCode { get; set; } = "";
        public string BcuProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string? Description { get; set; }
        public string? ColorValue { get; set; }
        public bool IsWeighingRequired { get; set; }
        public bool IsActive { get; set; }
        public decimal? PropA { get; set; }
        public decimal? PropB { get; set; }
        public int? GroupId { get; set; }
        public int? BlendGroupId { get; set; }
        public decimal? DensityMin { get; set; }
        public decimal? DensityMax { get; set; }
        public decimal? DensityTol { get; set; }
        public decimal? TempMin { get; set; }
        public decimal? TempMax { get; set; }
        public decimal? TempTol { get; set; }
    }
}
