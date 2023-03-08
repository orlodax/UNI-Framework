using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "customers")]
    public class BaseCustomer : BaseModel
    {
        [ValueInfo(SQLName = "code")] public string Code { get; set; }
        [ValueInfo(SQLName = "denomination", IsDisplayProperty = true)] public string Denomination { get; set; }
        [ValueInfo(SQLName = "firstname")] public string FirstName { get; set; }
        [ValueInfo(SQLName = "lastname")] public string LastName { get; set; }
        [ValueInfo(SQLName = "type")] public string Type { get; set; }
        [ValueInfo(SQLName = "description")] public string Description { get; set; }
        [ValueInfo(SQLName = "taxnumber")] public string TaxNumber { get; set; }
        [ValueInfo(SQLName = "vatnumber")] public string VatNumber { get; set; }
        [ValueInfo(SQLName = "atecocode")] public string AtecoCode { get; set; }
        [ValueInfo(SQLName = "sdicode")] public string SDICode { get; set; }
        [ValueInfo(SQLName = "companyregistrynumber")] public string CompanyRegistryNumber { get; set; }
        [ValueInfo(SQLName = "reacode")] public string ReaCode { get; set; }
        [ValueInfo(SQLName = "lat")] public double Lat { get; set; }
        [ValueInfo(SQLName = "lon")] public double Lon { get; set; }
        [ValueInfo(SQLName = "iban")] public string IBAN { get; set; }
        [ValueInfo(SQLName = "address")][RenderInfo(Group = "AddressGroup")] public string Address { get; set; }
        [ValueInfo(SQLName = "city")][RenderInfo(Group = "AddressGroup")] public string City { get; set; }
        [ValueInfo(SQLName = "postalcode")][RenderInfo(Group = "AddressGroup")] public string PostalCode { get; set; }
        [ValueInfo(SQLName = "state")][RenderInfo(Group = "AddressGroup")] public string State { get; set; }
        [ValueInfo(SQLName = "nation")][RenderInfo(Group = "AddressGroup")] public string Nation { get; set; }
        [ValueInfo(SQLName = "landline")][RenderInfo(Group = "ContactsGroup")] public string Landline { get; set; }
        [ValueInfo(SQLName = "mobile")][RenderInfo(Group = "ContactsGroup")] public string Mobile { get; set; }
        [ValueInfo(SQLName = "email")][RenderInfo(Group = "ContactsGroup")] public string Email { get; set; }
        [ValueInfo(SQLName = "fax")][RenderInfo(Group = "ContactsGroup")] public string Fax { get; set; }
        [ValueInfo(SQLName = "website")][RenderInfo(Group = "ContactsGroup")] public string Website { get; set; }
        [ValueInfo(SQLName = "legalmail")][RenderInfo(Group = "ContactsGroup")] public string LegalMail { get; set; }
        [ValueInfo(SQLName = "notes")] public string Notes { get; set; }
    }
}