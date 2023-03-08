using System;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "employees")]
    public class Employee : BaseModel
    {
        [ValueInfo(IsReadOnly = true, IsDisplayProperty = true)] public string DisplayName { get; set; }
        [ValueInfo(SQLName = "firstname")] public string Firstname { get; set; }
        [ValueInfo(SQLName = "surname")] public string Surname { get; set; }
        [ValueInfo(SQLName = "code")] public string Code { get; set; }
        [ValueInfo(SQLName = "borncity")] public string BornCity { get; set; }
        [ValueInfo(SQLName = "bornnation")] public string BornNation { get; set; }
        [ValueInfo(SQLName = "phonenumber")] public string PhoneNumber { get; set; }
        [ValueInfo(SQLName = "email")] public string Email { get; set; }
        [ValueInfo(SQLName = "documenttype")] public string DocumentType { get; set; }
        [ValueInfo(SQLName = "documentnumber")] public string DocumentNumber { get; set; }
        [ValueInfo(SQLName = "taxnumber")] public string TaxNumber { get; set; }
        [ValueInfo(SQLName = "iban")] public string Iban { get; set; }
        [ValueInfo(SQLName = "idvehicle", IsVisible = false)] public int IdVehicle { get; set; }
        [ValueInfo(SQLName = "dateofbirth")] public DateTime DateOfBirth { get; set; } = DateTime.Today;
        [ValueInfo(SQLName = "hiredate")] public DateTime HireDate { get; set; } = DateTime.Today;
        [ValueInfo(SQLName = "address")][RenderInfo(Group = "AddressGroup")] public string Address { get; set; }
        [ValueInfo(SQLName = "postalcode")][RenderInfo(Group = "AddressGroup")] public string PostalCode { get; set; }
        [ValueInfo(SQLName = "city")][RenderInfo(Group = "AddressGroup")] public string City { get; set; }
        [ValueInfo(SQLName = "state")][RenderInfo(Group = "AddressGroup")] public string State { get; set; }
        [ValueInfo(SQLName = "nation")][RenderInfo(Group = "AddressGroup")] public string Nation { get; set; }
        [ValueInfo(SQLName = "iduser")] public int IdUser { get; set; }
        [ValueInfo()] public Vehicle Vehicle { get; set; }

        public Employee()
        {
            DisplayName = Firstname + " " + Surname;
        }
    }
}