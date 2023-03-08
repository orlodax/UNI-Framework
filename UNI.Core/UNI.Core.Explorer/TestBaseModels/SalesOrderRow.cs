using System;
using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "salesorderrows")]
    public class SalesOrderRow : BaseModel
    {
        /// <summary>
        /// Line that keeps track, through references to other classes, of the products contained in each Sale Order
        /// </summary>

        [ValueInfo(SQLName = "idsalesorder", IsVisible = false)]
        public int IdSalesOrder { get; set; }


        [ValueInfo(SQLName = "idproduct", IsVisible = false)]
        public int IdProduct { get; set; }


        [ValueInfo(SQLName = "date")]
        public DateTime RegistrationDate { get; set; }


        [ValueInfo(SQLName = "quantity")]
        public double Quantity { get; set; }


        [ValueInfo(SQLName = "realquantity")]
        public double RealQuantity { get; set; }


        [ValueInfo(SQLName = "checked")]
        public bool Checked { get; set; }


        [ValueInfo(SQLName = "measurementunit")]
        public string MeasurementUnit { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public Product Product { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductProducer { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductDescription { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductSector { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public double PlatformQuantity { get; set; }

        public string SalesOrderNumber { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string CoordinataEstesa { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public double LoadedQuantity { get; set; }

        public SalesOrderRow()
        {
            if (Product != null)
            {
                ProductProducer = Product.Producer;
                ProductDescription = Product.Description;
                ProductSector = Product.SectorCode;

                if (Product?.PlatformQuantity != 0)
                    PlatformQuantity = Quantity / Product?.PlatformQuantity ?? 0;
            }
        }
    }
}
