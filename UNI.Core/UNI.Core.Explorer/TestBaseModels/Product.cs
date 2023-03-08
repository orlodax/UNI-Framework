using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "products")]
    public class Product : BaseModel
    {
        /// <summary>
        /// Codice infinity
        /// </summary>
        [ValueInfo(SQLName = "maincode", IsDisplayProperty = true)]
        [RenderInfo()]
        public string MainCode { get; set; }


        /// <summary>
        /// Produttore
        /// </summary>
        [ValueInfo(SQLName = "producer")]
        [RenderInfo()]
        public string Producer { get; set; }


        /// <summary>
        /// Descrizione
        /// </summary>
        [ValueInfo(SQLName = "description")]
        [RenderInfo(NewLine = true)]
        public string Description { get; set; }


        /// <summary>
        /// Unità di misura
        /// </summary>
        [ValueInfo(SQLName = "measurementunit")]
        [RenderInfo()]
        public string MeasurementUnit { get; set; }


        /// <summary>
        /// Posizione
        /// </summary>
        [ValueInfo(SQLName = "location")]
        [RenderInfo(Group = "General")]
        public string Location { get; set; }


        /// <summary>
        /// Famiglia
        /// </summary>
        [ValueInfo(SQLName = "family")]
        [RenderInfo(Group = "General")]
        public string Family { get; set; }


        /// <summary>
        /// Categoria
        /// </summary>
        [ValueInfo(SQLName = "category")]
        [RenderInfo(Group = "General", NewLine = true)]
        public string Category { get; set; }


        /// <summary>
        /// Gruppo merceologico
        /// </summary>
        [ValueInfo(SQLName = "productgroup")]
        [RenderInfo(Group = "General")]
        public string ProductGroup { get; set; }


        /// <summary>
        /// Codice ean
        /// </summary>
        [ValueInfo(SQLName = "eancode")]
        [RenderInfo(Group = "Info")]
        public string EanCode { get; set; }


        /// <summary>
        /// Codice SSC
        /// </summary>
        [ValueInfo(SQLName = "ssccode")]
        [RenderInfo(Group = "Info")]
        public string SscCode { get; set; }


        /// <summary>
        ///  Codice Ean confezione
        /// </summary>
        [ValueInfo(SQLName = "packageeancode")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string PackageEanCode { get; set; }





        /// <summary>
        ///  Codice ean scatola
        /// </summary>
        [ValueInfo(SQLName = "cardboardeancode")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string CardboardEanCode { get; set; }




        /// <summary>
        /// Codice Ean pedana
        /// </summary>
        [ValueInfo(SQLName = "platformeancode")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string PlatformEanCode { get; set; }


        /// <summary>
        /// Quantità per pedane (In unità di misura base)
        /// </summary>
        [ValueInfo(SQLName = "platformquantity")]
        [RenderInfo(Group = "Info")]
        public double PlatformQuantity { get; set; }


        /// <summary>
        /// Prezzo lordo
        /// </summary>
        [ValueInfo(SQLName = "defaultgrossprice")]
        [RenderInfo(Group = "Price")]
        public double DefaultGrossPrice { get; set; }


        /// <summary>
        /// Prezzo di vendita
        /// </summary>
        [ValueInfo(SQLName = "defaultsellprice")]
        [RenderInfo(Group = "Price")]
        public double DefaultSellPrice { get; set; }


        /// <summary>
        /// Iva base
        /// </summary>
        [ValueInfo(SQLName = "defaultvat")]
        [RenderInfo(Group = "Price", NewLine = true)]
        public string DefaultVat { get; set; }


        /// <summary>
        /// Prezzo d'acquisto
        /// </summary>
        [ValueInfo(SQLName = "defaultpurchaseprice")]
        [RenderInfo(Group = "Price")]
        public double DefaultPurchasePrice { get; set; }

        //[ValueInfo(IsReadOnly = true)]
        //[SqlFieldInfo(Query = "SELECT SUM(quantity) FROM productmovements WHERE idproduct = {0};",
        //             PropertyWhereValues = "ID")]
        //public double Quantity { get; set; }

        [ValueInfo()]
        [RenderInfo(PageGroup = "Movimenti di Magazzino")]
        public List<ProductMovement> ProductMovements { get; set; }


        /// <summary>
        /// Codice settore
        /// </summary>
        [ValueInfo(SQLName = "sectorcode")]
        [RenderInfo()]
        public string SectorCode { get; set; }

        /// <summary>
        /// Quantità per confezione
        /// </summary>
        private double packageQuantity;
        [ValueInfo(SQLName = "packagequantity")]
        [RenderInfo(Group = "Info")]
        public double PackageQuantity 
        { 
            get 
            { 
                if (MeasurementUnit == "CF")
                    packageQuantity = 1;
                
                return packageQuantity;
            }
            set { packageQuantity = value; }

        }

        /// <summary>
        ///  Quantità per scatola
        /// </summary>
        /// 
        private double cardboardQuantity;
        [ValueInfo(SQLName = "cardboardquantity")]
        [RenderInfo(Group = "Info")]
        public double CardboardQuantity
        {
            get
            {
                if (MeasurementUnit == "SC")
                    cardboardQuantity = 1;

                return cardboardQuantity;
            }
            set { cardboardQuantity = value; }

        }
    }
}
