using System;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "vehicles")]
    public class Vehicle : BaseModel
    {
        [ValueInfo(SQLName = "brand")] public string Brand { get; set; }
        [ValueInfo(SQLName = "model")] public string Model { get; set; }
        [ValueInfo(SQLName = "description")] public string Description { get; set; }
        [ValueInfo(SQLName = "type")] public string Type { get; set; }
        [ValueInfo(SQLName = "licenseplate", IsDisplayProperty = true)] public string LicensePlate { get; set; }
        [ValueInfo(SQLName = "euroclass")] public string EuroClass { get; set; }
        [ValueInfo(SQLName = "fueltype")] public string FuelType { get; set; }
        [ValueInfo(SQLName = "purchasedate")] public DateTime PurchaseDate { get; set; } = DateTime.Today;
        [ValueInfo(SQLName = "insurancecompany")][RenderInfo(Group = "InsuranceGroup")] public string InsuranceCompany { get; set; }
        [ValueInfo(SQLName = "insurancenumber")][RenderInfo(Group = "InsuranceGroup")] public string InsuranceNumber { get; set; }
        [ValueInfo(SQLName = "lastrevision")][RenderInfo(Group = "InsuranceGroup")] public DateTime LastRevision { get; set; }
        [ValueInfo(SQLName = "revisionedeadline")][RenderInfo(Group = "InsuranceGroup")] public DateTime RevisioneDeadline { get; set; }
        [ValueInfo(SQLName = "insurancedeadline")][RenderInfo(Group = "InsuranceGroup")] public DateTime InsuranceDeadline { get; set; }
        [ValueInfo(SQLName = "hassatellite")][RenderInfo(Group = "InsuranceGroup")] public bool HasSatellite { get; set; }
        [ValueInfo(SQLName = "haswastecer")] public bool HasWasteCer { get; set; }
        [ValueInfo(SQLName = "hasfireextinguisher")] public bool HasFireExtinguisher { get; set; }
        [ValueInfo(SQLName = "fireextinguisherserial")] public string FireExtinguisherSerial { get; set; }
    }
}
