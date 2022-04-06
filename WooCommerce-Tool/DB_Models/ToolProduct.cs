using System;
using System.Collections.Generic;

namespace WooCommerce_Tool.DB_Models
{
    public partial class ToolProduct
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string? Name { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Category { get; set; }
        public string TotalProducts { get; set; } = null!;
        public string NnProducts { get; set; } = null!;
        public string TimeSeriesProducts { get; set; } = null!;
        public string RegresionProducts { get; set; } = null!;
        public string ProbabilityProducts { get; set; } = null!;
        public string ProbabilityCategory { get; set; } = null!;

        public virtual ToolLogin Shop { get; set; } = null!;
    }
}
