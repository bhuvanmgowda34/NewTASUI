using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewTASUI.Models;

namespace NewTASUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IConfiguration _cfg;
        public ProductsController(IConfiguration cfg) { _cfg = cfg; }
        private string ConnStr() => _cfg.GetConnectionString("DefaultConnection")!;

        // ── Page ─────────────────────────────────────────────────────────
        public IActionResult Index() => View("~/Views/Masters/Products.cshtml");

        // ═══════════════════════════════════════════════════════════════
        // GET /Products/GetProductGroups
        // Returns [{ id, name }] — used by both Group Name and
        // Blended Group Name dropdowns (same table).
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetProductGroups()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(
                @"SELECT ProductGroupId, RTRIM(ProductGroupName) AS ProductGroupName
                  FROM   ProuctGroup
                  WHERE  ProductGroupId > 0
                  ORDER BY GroupOrder", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    id   = Convert.ToInt32(r["ProductGroupId"]),
                    name = r["ProductGroupName"].ToString()
                });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Products/GetProducts
        // Returns all non-deleted products with group name lookups.
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetProducts()
        {
            var list = new List<object>();
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  p.ProductId,
                        p.ProductCode,
                        p.BCUProductCode,
                        p.ProductName,
                        p.Description,
                        p.ColorValue,
                        ISNULL(p.IsWeighingRequired, 0) AS IsWeighingRequired,
                        ISNULL(p.IsActive, 0)           AS IsActive,
                        p.PropotionA,
                        p.PropotionB,
                        p.GroupId,
                        p.BlendGroupId,
                        ISNULL(g.ProductGroupName, '')  AS GroupName,
                        ISNULL(bg.ProductGroupName, '') AS BlendGroupName,
                        p.DensityMin,
                        p.DensityMax,
                        p.DensityTolerance,
                        p.TempMin,
                        p.TempMax,
                        p.TempTolerance
                FROM    Products p
                LEFT JOIN ProuctGroup g  ON g.ProductGroupId  = p.GroupId
                LEFT JOIN ProuctGroup bg ON bg.ProductGroupId = p.BlendGroupId
                WHERE   ISNULL(p.IsDelete, 0) = 0
                ORDER BY p.ProductName", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new
                {
                    productId          = Convert.ToInt32(r["ProductId"]),
                    productCode        = r["ProductCode"].ToString(),
                    bcuProductCode     = r["BCUProductCode"].ToString(),
                    productName        = r["ProductName"].ToString(),
                    description        = r["Description"]  == DBNull.Value ? null : r["Description"].ToString(),
                    colorValue         = r["ColorValue"]   == DBNull.Value ? null : r["ColorValue"].ToString(),
                    isWeighingRequired = (bool)r["IsWeighingRequired"],
                    isActive           = (bool)r["IsActive"],
                    propA              = r["PropotionA"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["PropotionA"]),
                    propB              = r["PropotionB"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["PropotionB"]),
                    groupId            = r["GroupId"]      == DBNull.Value ? (int?)null    : Convert.ToInt32(r["GroupId"]),
                    blendGroupId       = r["BlendGroupId"] == DBNull.Value ? (int?)null    : Convert.ToInt32(r["BlendGroupId"]),
                    groupName          = r["GroupName"].ToString(),
                    blendGroupName     = r["BlendGroupName"].ToString(),
                    densityMin         = r["DensityMin"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityMin"]),
                    densityMax         = r["DensityMax"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityMax"]),
                    densityTol         = r["DensityTolerance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityTolerance"]),
                    tempMin            = r["TempMin"]      == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempMin"]),
                    tempMax            = r["TempMax"]      == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempMax"]),
                    tempTol            = r["TempTolerance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempTolerance"])
                });
            return Json(list);
        }

        // ═══════════════════════════════════════════════════════════════
        // GET /Products/GetProductDetail/{id}
        // Returns a single product (used by row-click and edit modal).
        // ═══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult GetProductDetail(int id)
        {
            using var con = new SqlConnection(ConnStr());
            using var cmd = new SqlCommand(@"
                SELECT  p.ProductId,
                        p.ProductCode,
                        p.BCUProductCode,
                        p.ProductName,
                        p.Description,
                        p.ColorValue,
                        ISNULL(p.IsWeighingRequired, 0) AS IsWeighingRequired,
                        ISNULL(p.IsActive, 0)           AS IsActive,
                        p.PropotionA,
                        p.PropotionB,
                        p.GroupId,
                        p.BlendGroupId,
                        ISNULL(g.ProductGroupName, '')  AS GroupName,
                        ISNULL(bg.ProductGroupName, '') AS BlendGroupName,
                        p.DensityMin,
                        p.DensityMax,
                        p.DensityTolerance,
                        p.TempMin,
                        p.TempMax,
                        p.TempTolerance
                FROM    Products p
                LEFT JOIN ProuctGroup g  ON g.ProductGroupId  = p.GroupId
                LEFT JOIN ProuctGroup bg ON bg.ProductGroupId = p.BlendGroupId
                WHERE   p.ProductId = @Id AND ISNULL(p.IsDelete, 0) = 0", con);
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return NotFound();
            return Json(new
            {
                productId          = Convert.ToInt32(r["ProductId"]),
                productCode        = r["ProductCode"].ToString(),
                bcuProductCode     = r["BCUProductCode"].ToString(),
                productName        = r["ProductName"].ToString(),
                description        = r["Description"]  == DBNull.Value ? null : r["Description"].ToString(),
                colorValue         = r["ColorValue"]   == DBNull.Value ? null : r["ColorValue"].ToString(),
                isWeighingRequired = (bool)r["IsWeighingRequired"],
                isActive           = (bool)r["IsActive"],
                propA              = r["PropotionA"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["PropotionA"]),
                propB              = r["PropotionB"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["PropotionB"]),
                groupId            = r["GroupId"]      == DBNull.Value ? (int?)null    : Convert.ToInt32(r["GroupId"]),
                blendGroupId       = r["BlendGroupId"] == DBNull.Value ? (int?)null    : Convert.ToInt32(r["BlendGroupId"]),
                groupName          = r["GroupName"].ToString(),
                blendGroupName     = r["BlendGroupName"].ToString(),
                densityMin         = r["DensityMin"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityMin"]),
                densityMax         = r["DensityMax"]   == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityMax"]),
                densityTol         = r["DensityTolerance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["DensityTolerance"]),
                tempMin            = r["TempMin"]      == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempMin"]),
                tempMax            = r["TempMax"]      == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempMax"]),
                tempTol            = r["TempTolerance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TempTolerance"])
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Products/SaveProduct
        // INSERT (productId == 0) or UPDATE via the main form.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult SaveProduct([FromBody] ProductSaveModel m)
        {
            if (m == null)
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                con.Open();
                using var tx = con.BeginTransaction();

                bool isExisting = false;
                if (m.ProductId > 0)
                {
                    using var chk = new SqlCommand(
                        "SELECT COUNT(1) FROM Products WHERE ProductId=@Id AND ISNULL(IsDelete,0)=0",
                        con, tx);
                    chk.Parameters.AddWithValue("@Id", m.ProductId);
                    isExisting = Convert.ToInt32(chk.ExecuteScalar()) > 0;
                }

                if (!isExisting)
                {
                    // ── INSERT ──────────────────────────────────────────
                    using var ins = new SqlCommand(@"
                        INSERT INTO Products
                            (ProductCode, BCUProductCode, ProductName, Description,
                             ColorValue, IsWeighingRequired, IsActive,
                             PropotionA, PropotionB,
                             GroupId, BlendGroupId,
                             DensityMin, DensityMax, DensityTolerance,
                             TempMin, TempMax, TempTolerance,
                             IsDelete, CreatedBy, CreatedDate)
                        VALUES
                            (@Code, @BcuCode, @Name, @Desc,
                             @Color, @Weighing, @Active,
                             @PropA, @PropB,
                             @GroupId, @BlendGroupId,
                             @DenMin, @DenMax, @DenTol,
                             @TmpMin, @TmpMax, @TmpTol,
                             0, 1, GETDATE())", con, tx);
                    SetProductParams(ins, m);
                    ins.ExecuteNonQuery();
                }
                else
                {
                    // ── UPDATE ───────────────────────────────────────────
                    using var upd = new SqlCommand(@"
                        UPDATE Products SET
                            ProductCode        = @Code,
                            BCUProductCode     = @BcuCode,
                            ProductName        = @Name,
                            Description        = @Desc,
                            ColorValue         = @Color,
                            IsWeighingRequired = @Weighing,
                            IsActive           = @Active,
                            PropotionA         = @PropA,
                            PropotionB         = @PropB,
                            GroupId            = @GroupId,
                            BlendGroupId       = @BlendGroupId,
                            DensityMin         = @DenMin,
                            DensityMax         = @DenMax,
                            DensityTolerance   = @DenTol,
                            TempMin            = @TmpMin,
                            TempMax            = @TmpMax,
                            TempTolerance      = @TmpTol,
                            UpdatedBy          = 1,
                            UpdatedDate        = GETDATE()
                        WHERE ProductId = @ProductId AND ISNULL(IsDelete,0) = 0", con, tx);
                    SetProductParams(upd, m);
                    upd.Parameters.AddWithValue("@ProductId", m.ProductId);
                    upd.ExecuteNonQuery();
                }

                tx.Commit();
                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { success = false, message = "Product Code already exists." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Products/UpdateProduct
        // Called exclusively by the Edit Modal "Update" button.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult UpdateProduct([FromBody] ProductSaveModel m)
        {
            if (m == null || m.ProductId == 0)
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE Products SET
                        ProductCode        = @Code,
                        BCUProductCode     = @BcuCode,
                        ProductName        = @Name,
                        Description        = @Desc,
                        ColorValue         = @Color,
                        IsWeighingRequired = @Weighing,
                        IsActive           = @Active,
                        PropotionA         = @PropA,
                        PropotionB         = @PropB,
                        GroupId            = @GroupId,
                        BlendGroupId       = @BlendGroupId,
                        DensityMin         = @DenMin,
                        DensityMax         = @DenMax,
                        DensityTolerance   = @DenTol,
                        TempMin            = @TmpMin,
                        TempMax            = @TmpMax,
                        TempTolerance      = @TmpTol,
                        UpdatedBy          = 1,
                        UpdatedDate        = GETDATE()
                    WHERE ProductId = @ProductId AND ISNULL(IsDelete,0) = 0", con);
                SetProductParams(cmd, m);
                cmd.Parameters.AddWithValue("@ProductId", m.ProductId);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // POST /Products/DeleteProduct
        // Soft delete — sets IsDelete = 1, records who/when.
        // ═══════════════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult DeleteProduct([FromBody] int id)
        {
            try
            {
                using var con = new SqlConnection(ConnStr());
                using var cmd = new SqlCommand(@"
                    UPDATE Products SET
                        IsDelete    = 1,
                        IsActive    = 0,
                        DeletedBy   = 1,
                        DeletedDate = GETDATE()
                    WHERE ProductId = @Id AND ISNULL(IsDelete,0) = 0", con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIVATE HELPER — shared parameter binding for INSERT / UPDATE
        // ═══════════════════════════════════════════════════════════════
        private static void SetProductParams(SqlCommand cmd, ProductSaveModel m)
        {
            cmd.Parameters.AddWithValue("@Code",       m.ProductCode    ?? "");
            cmd.Parameters.AddWithValue("@BcuCode",    m.BcuProductCode ?? "");
            cmd.Parameters.AddWithValue("@Name",       m.ProductName    ?? "");
            cmd.Parameters.AddWithValue("@Desc",       (object?)m.Description  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Color",      (object?)m.ColorValue   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Weighing",   m.IsWeighingRequired);
            cmd.Parameters.AddWithValue("@Active",     m.IsActive);
            cmd.Parameters.AddWithValue("@PropA",      (object?)m.PropA        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PropB",      (object?)m.PropB        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GroupId",    (object?)m.GroupId      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BlendGroupId",(object?)m.BlendGroupId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DenMin",     (object?)m.DensityMin   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DenMax",     (object?)m.DensityMax   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DenTol",     (object?)m.DensityTol   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TmpMin",     (object?)m.TempMin      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TmpMax",     (object?)m.TempMax      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TmpTol",     (object?)m.TempTol      ?? DBNull.Value);
        }
    }
}
