using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WooCommerce_Tool.DB_Models
{
    public partial class ToolOrder
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string? Name { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? TimeOfTheDay { get; set; }
        public string? TimeOfTheMonth { get; set; }
        public string? TotalOrder { get; set; }
        public string? NnOrder { get; set; }
        public string? TimeSeriesOrder { get; set; }
        public string? RegresionOrder { get; set; }
        public string? ProbabilityTimeOfTheDay { get; set; }
        public string? ProbabilityTimeOfTheMonth { get; set; }

        public virtual ToolLogin Shop { get; set; } = null!;
    }
}
