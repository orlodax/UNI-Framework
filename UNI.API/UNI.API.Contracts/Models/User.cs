using System;
using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.AttributesMetadata;

namespace UNI.API.Contracts.Models
{
    [ClassInfo(SQLName = "usersview")]
    public class User : BaseModel
    {
        [ValueInfo(SQLName = "firstname")]
        public string FirstName { get; set; }

        [ValueInfo(SQLName = "lastname")]
        public string LastName { get; set; }

        [ValueInfo(SQLName = "dateofbirth")]
        public DateTime DateOfBirth { get; set; }

        // if not readonly, this would be written to table 'credentials' specifing WriteTable in the ValueInfo attribute
        [ValueInfo(SQLName = "email", IsReadOnly = true)]
        public string Email { get; set; }

        [ValueInfo(SQLName = "phonenumber")]
        public string PhoneNumber { get; set; }

        [ValueInfo(SQLName = "address")]
        public string Address { get; set; }

        [ValueInfo(SQLName = "avatar")]
        public byte[] Avatar { get; set; }

        [ValueInfo(ManyToManySQLName = "viewrolesofusers", LinkTableSQLName = "userroles")]
        public IEnumerable<Role> Roles { get; set; }

        public string FullName { get => $"{FirstName} {LastName}"; }

        public object ProfilePicture { get; set; } // return transform byte[] Avatar into whatever


        public User()
        {
            InitAttributes();
        }

        #region Abstracts override

        public override BaseModel InitAttributes()
        {
            AddClassAttributes();
            AddDataAttributes();
            AddGraphicAttributes();
            return this;
        }

        protected void AddClassAttributes()
        {
            AddClassAttribute(new ClassAttributes
            {
                SQLName = "usersview",
                MasterTable = "users",
                BaseModelType = EnBaseModelTypes.ViewOnlyBaseModel
            });
        }

        protected void AddDataAttributes()
        {
            AddDataAttribute(nameof(FirstName), new DataAttributes()
            {
                SQLName = "firstname",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(LastName), new DataAttributes()
            {
                SQLName = "lastname",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(DateOfBirth), new DataAttributes()
            {
                SQLName = "dateofbirth",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(PhoneNumber), new DataAttributes()
            {
                SQLName = "phonenumber",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(Address), new DataAttributes()
            {
                SQLName = "address",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(Avatar), new DataAttributes()
            {
                SQLName = "avatar",
                WriteTable = "users"
            });
            AddDataAttribute(nameof(Roles), new DataAttributes()
            {
                ManyToManySQLName = "viewrolesofusers",
                LinkTableSQLName = "userroles"
            });
            AddDataAttribute(nameof(Email), new DataAttributes()
            {
                SQLName = "email",
                IsReadOnly = true
            });
        }

        protected void AddGraphicAttributes()
        {
            //AddGraphicAttribute(nameof(Email), new GraphicAttributes()
            //{
            //    PageGroup = "",
            //    Converter = typeof(object)
            //});
        }
        #endregion
    }
}
