

using System;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "productmovements")]
    public class ProductMovement : BaseModel
    {

        /// <summary>
        /// Class that records and shows all the outbound movements
        /// This class also keeps track of who made the movement using the AppUser object
        /// </summary>
        ///         
        [ValueInfo(IsReadOnly = true)]
        public string ProductMainCode { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductDescription { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string ProductMeasurementUnit { get; set; }


        [ValueInfo(SQLName = "idddt", IsDisplayProperty = true)]
        [RenderInfo(NewLine = true)]
        public int IdDDT { get; set; }


        [ValueInfo(SQLName = "date")]
        public DateTime Date { get; set; }


        [ValueInfo(SQLName = "quantity")]
        public double Quantity { get; set; }


        [ValueInfo(SQLName = "idslot_from")]
        [RenderInfo(NewLine = true)]
        public int IdProductSlotStart { get; set; }


        [ValueInfo(SQLName = "idslot_to")]
        public int IdProductSlotEnd { get; set; }


        [ValueInfo(SQLName = "description")]
        public string Description { get; set; }


        [ValueInfo(SQLName = "measurementunit")]
        public string MeasurementUnit { get; set; }


        [ValueInfo(SQLName = "idappuser")]
        public int IdAppUser { get; set; }


        [ValueInfo(SQLName = "idproduct")]
        public int IdProduct { get; set; }


        [ValueInfo(SQLName = "idrow")]
        public int IdRow { get; set; }


        [ValueInfo()]
        public Product Product { get; set; }


        [ValueInfo()]
        public AppUser AppUser { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public double PlatformQuantity { get; set; }

        public ProductMovement()
        {
            if (Product != null)
            {
                ProductDescription = Product.Description;
                ProductMeasurementUnit = Product.MeasurementUnit;
                ProductMainCode = Product.MainCode;

                if (Product?.PlatformQuantity != 0)
                    PlatformQuantity = Quantity / Product?.PlatformQuantity ?? 0;
            }
        }
    }
}
