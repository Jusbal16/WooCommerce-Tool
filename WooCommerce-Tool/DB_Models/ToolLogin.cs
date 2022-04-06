using System;
using System.Collections.Generic;

namespace WooCommerce_Tool.DB_Models
{
    public partial class ToolLogin
    {
        public ToolLogin()
        {
            ToolOrders = new HashSet<ToolOrder>();
            ToolProducts = new HashSet<ToolProduct>();
        }

        public int Id { get; set; }
        public string? Url { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }

        public virtual ICollection<ToolOrder> ToolOrders { get; set; }
        public virtual ICollection<ToolProduct> ToolProducts { get; set; }
    }
}
