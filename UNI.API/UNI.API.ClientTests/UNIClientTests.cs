using Microsoft.VisualStudio.TestTools.UnitTesting;
using UNI.API.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Gioiaspa.Warehouse.Library;
using UNI.Core.Library;
using UNI.API.Contracts.RequestsDTO;
using System.Threading.Tasks;

namespace UNI.API.Client.Tests
{
    [TestClass()]
    public class UNIClientTests
    {
        [TestMethod()]
        public async Task UNIClientTest()
        {
            UNIClient<SalesOrderRow> uNIClient = new UNIClient<SalesOrderRow>();

            FilterExpression mainContainerFilterExpression = new FilterExpression();



            FilterExpression SalesOrderFilterExpression = new FilterExpression() { PropertyName = string.Empty, ComparisonType = "OR" };



            List<SalesOrderRow> rows = new List<SalesOrderRow>();
            List<FilterExpression> filterExpressionsRows = new List<FilterExpression>();
            SalesOrderFilterExpression.FilterExpressions.Add(new FilterExpression()
            {
                PropertyName = "IdSalesOrder",
                ComparisonType = "OR",
                PropertyValue = "",
            });
            SalesOrderFilterExpression.FilterExpressions.Add(new FilterExpression()
            {
                PropertyName = "IdSalesOrder",
                ComparisonType = "OR",
                PropertyValue = "",
            });
            mainContainerFilterExpression.FilterExpressions.Add(SalesOrderFilterExpression);


            FilterExpression sectorFilterExpression = new FilterExpression() { PropertyName = nameof(SalesOrderRow.ProductSector), PropertyValue = "P9" };


            filterExpressionsRows.Add(mainContainerFilterExpression);
            filterExpressionsRows.Add(sectorFilterExpression);
            var requestDto = new GetDataSetRequestDTO
            {
                FilterExpressions = filterExpressionsRows
            };
            rows = await uNIClient.Get(requestDto);
        }
    }
}