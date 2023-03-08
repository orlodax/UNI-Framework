using System;

namespace UNI.Core.Library
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValueInfo : Attribute
    {
        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string SQLName { get; set; }

        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string ManyToManySQLName { get; set; }

        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string LinkTableSQLName { get; set; }

        /// <summary>
        /// Map the column name of the reference in the table
        /// </summary>
        public string ColumnReference1Name { get; set; }

        /// <summary>
        /// Map the column name of the reference in the table
        /// </summary>
        public string ColumnReference2Name { get; set; }

        /// <summary>
        /// For one to one different behavour to generate showboxOnetoOne
        /// </summary>
        public bool IsOneToOne { get; set; } = false;

        /// <summary>
        /// Determine if the element/field is rendered in views
        /// </summary>
        public bool IsVisible { get; set; } = true;   
        
        /// <summary>
        /// Exclude field from insert/update queries
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// For objects which class name/class attribute do not correspond to a table
        /// </summary>
        public string WriteTable { get; set; }

        /// <summary>
        /// It is inserted/updated in queries but user cannot choose
        /// </summary>
        public bool IsUserReadOnly { get; set; } = false;

        #region Dependency logic

        /// <summary>
        /// Marks the property as the one to be displayed in ShowBoxes, ComoboBoxes, etc
        /// </summary>
        public bool IsDisplayProperty { get; set; }

        /// <summary>
        /// Marks the property as main numeration of a table, used for rapid creation
        /// </summary>
        public bool IsMainNumeration { get; set; } = false;

        /// <summary>
        /// Marks the property as default data filter
        /// </summary>
        public bool IsDefaultDataFilter { get; set; } = false;

        /// <summary>
        /// Set a dependency to a parent's property, used in new item logic
        /// </summary>
        public string ParentPropertyDependendency { get; set; } 

        #endregion
    }
}