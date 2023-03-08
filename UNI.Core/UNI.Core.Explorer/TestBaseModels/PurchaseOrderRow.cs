using System;
using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "purchaseorderrows")]
    public class PurchaseOrderRow : BaseModel
    {
        /// <summary>
        /// Line that keeps track, through references to other classes, of the products contained in each PurchaseOrder
        /// </summary>

        [ValueInfo(SQLName = "idpurchaseorder", IsVisible = false)]
        public int IdPurchaseOrder { get; set; }


        [ValueInfo(SQLName = "idproduct", IsVisible = false)]
        public int IdProduct { get; set; }


        [ValueInfo(SQLName = "quantity")]
        public double Quantity { get; set; }


        [ValueInfo(SQLName = "measurementunit")]
        public string MeasurementUnit { get; set; }


        [ValueInfo(SQLName = "date")]
        public DateTime RegistrationDate { get; set; }


        [ValueInfo()]
        public Product Product { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductProducer { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductDescription { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public double PlatformQuantity { get; set; }

        public PurchaseOrderRow()
        {
            if (Product != null)
            {

                ProductProducer = Product.Producer;
                ProductDescription = Product.Description;

                if (Product?.PlatformQuantity != 0)
                    PlatformQuantity = Quantity / Product?.PlatformQuantity ?? 0;
            }
        }
    }
}
