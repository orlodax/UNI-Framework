namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "suppliers")]
    public class Supplier : BaseModel
    {
        #region DB Fields Properties
        [ValueInfo(SQLName = "code")] public string Code { get; set; }
        [ValueInfo(SQLName = "denomination", IsDisplayProperty = true)] public string Denomination { get; set; }
        [ValueInfo(SQLName = "firstname")] public string FirstName { get; set; }
        [ValueInfo(SQLName = "lastname")] public string LastName { get; set; }
        [ValueInfo(SQLName = "type")] public string Type { get; set; }
        [ValueInfo(SQLName = "description")] public string Description { get; set; }
        [ValueInfo(SQLName = "address")] public string Address { get; set; }
        [ValueInfo(SQLName = "city")] public string City { get; set; }
        [ValueInfo(SQLName = "province")] public string Province { get; set; }
        [ValueInfo(SQLName = "postalCode")] public string PostalCode { get; set; }
        [ValueInfo(SQLName = "region")] public string Region { get; set; }
        [ValueInfo(SQLName = "nation")] public string Nation { get; set; }
        [ValueInfo(SQLName = "landline")] public string Landline { get; set; }
        [ValueInfo(SQLName = "mobile")] public string Mobile { get; set; }
        [ValueInfo(SQLName = "email")] public string Email { get; set; }
        [ValueInfo(SQLName = "fax")] public string Fax { get; set; }
        [ValueInfo(SQLName = "taxNumber")] public string TaxNumber { get; set; }
        [ValueInfo(SQLName = "vatNumber")] public string VatNumber { get; set; }
        [ValueInfo(SQLName = "atecocode")] public string AtecoCode { get; set; }
        [ValueInfo(SQLName = "sdicode")] public string SDICode { get; set; }
        [ValueInfo(SQLName = "companyregistrynumber")] public string CompanyRegistryNumber { get; set; }
        [ValueInfo(SQLName = "reacode")] public string ReaCode { get; set; }
        [ValueInfo(SQLName = "lat")] public double Lat { get; set; }
        [ValueInfo(SQLName = "lon")] public double Lon { get; set; }
        [ValueInfo(SQLName = "iban")] public string IBAN { get; set; }
        [ValueInfo(SQLName = "website")] public string Website { get; set; }
        [ValueInfo(SQLName = "legalMail")] public string LegalMail { get; set; }
        [ValueInfo(SQLName = "notes")] public string Notes { get; set; }
        #endregion
    }
}
