using System.IdentityModel.Tokens.Jwt;
using UNI.API.Client;
using UNI.API.Contracts.Models;

namespace UNI.API.Tests;

[TestClass]
public class ClientTests
{
    //---------------------------------
    //
    // INFO - per fare test su oggetti di altri assembly non riferibili, copiare il codice sorgente in delle classi ricreate in questo progetto di test
    //
    //---------------------------------



    //private readonly UNIClient<PlantTest> client = new();

    //// test object that will be created, get, updated and deleted during the multiple tests run
    //private PlantTest testObject = new();
    //private PlantTest testObject2 = new();

    private readonly UNIClient<User> client = new();

    public async Task EnsureAuthentication()
    {
        if (UNIUser.Token == null)
            await Authenticate();
        else
        {
            JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIUser.Token.Value);
            if (jwtSecurityToken.ValidTo < DateTime.UtcNow)
                await Authenticate();
        }
    }

    [TestMethod]
    public async Task Authenticate()
    {
        UNIUser.Token = await client.Authenticate("a@a.a", "a");
        Assert.IsNotNull(UNIUser.Token);
    }

    [TestMethod]
    public async Task CreateCredentials()
    {
        await EnsureAuthentication();

        int insertedId = await client.CreateCredentials(new Credentials() { Username = "test", Password = "test" });
        Assert.IsTrue(insertedId > 0);
    }

    //[TestMethod]
    //public async Task CreateItem()
    //{
    //    await EnsureAuthentication();

    //    PlantTest newItem = new()
    //    {
    //        Alarms = 20,
    //        ConnectedDevice = 10,
    //        Customer = "Mago Merlino",
    //        Name = "Nuovo impianto test",
    //        Number = "centotrentasette",
    //        Status = "ideale",
    //        Temperature = "perfetta"
    //    };

    //    PlantTest? response = await client.CreateItem(newItem);

    //    if (response != null)
    //        testObject = response;

    //    Assert.IsNotNull(response);
    //}

    /// requires IsMainNumeration in field attribute
    //[TestMethod]
    //public async Task CreateRapidItem()
    //{
    //    await EnsureAuthentication();

    //    PlantTest newItem = new()
    //    {
    //        Alarms = 5,
    //        ConnectedDevice = 5,
    //        Customer = "Rapido",
    //        Name = "Nuovo impianto rapidtest",
    //        Number = "c",
    //        Status = "transitorio",
    //        Temperature = "n/a"
    //    };
    //    PlantTest? response = await client.CreateRapidItem(newItem);

    //    Assert.IsNotNull(response);
    //}

    //[TestMethod]
    //public async Task CreateItems()
    //{
    //    await EnsureAuthentication();

    //    List<PlantTest> newItems = new()
    //    {
    //        new()
    //        {
    //            Alarms = 2,
    //            ConnectedDevice = 1,
    //            Customer = "Listarello",
    //            Name = "Nuovo impianto test passato in lista",
    //            Number = "settantatre",
    //            Status = "celibe",
    //            Temperature = "tropicali"
    //        }
    //    };

    //    int response = await client.CreateItems(newItems);

    //    bool success = response > 199 && response < 300;

    //    if (success)
    //        testObject2 = newItems[0];

    //    Assert.IsTrue(success);
    //}

    //[TestMethod]
    //public async Task UpdateItems()
    //{
    //    await EnsureAuthentication();

    //    testObject.Alarms = 11;
    //    testObject.ConnectedDevice = 9;
    //    testObject.Customer = "Maga Magò lista";
    //    testObject.Name = "Impianto modificato test in lista";

    //    testObject2.Alarms = 22;
    //    testObject2.ConnectedDevice = 33;
    //    testObject2.Customer = "Listarello edit";
    //    testObject2.Name = "Impianto modificato test in lista";

    //    List<PlantTest> editedItems = new()
    //    {
    //        testObject,
    //        testObject2
    //    };

    //    int response = await client.UpdateItems(editedItems);

    //    Assert.IsTrue(response > 199 && response < 300);
    //}

    //[TestMethod]
    //public async Task UpdateItem()
    //{
    //    await EnsureAuthentication();

    //    testObject.Alarms = 11;
    //    testObject.ConnectedDevice = 9;
    //    testObject.Customer = "Maga Magò";
    //    testObject.Name = "Impianto modificato test";
    //    testObject.Number = "quarantadue";
    //    testObject.Status = "non più ideale";
    //    testObject.Temperature = "non più perfetto";

    //    int response = await client.UpdateItem(testObject);
    //    Assert.IsTrue(response > 199 && response < 300);
    //}

    //[TestMethod]
    //public async Task GetItems()
    //{
    //    await EnsureAuthentication();

    //    List<PlantTest>? response = await client.Get(new GetDataSetRequestDTO());
    //    Assert.IsNotNull(response);
    //    Assert.IsTrue(response.Any());
    //}

    //[TestMethod]
    //public async Task GetItemsSkipLists()
    //{
    //    await EnsureAuthentication();

    //    List<PlantTest>? response = await client.GetItemsSkipLists();
    //    Assert.IsNotNull(response);
    //    Assert.IsTrue(response.Any());
    //}

    //[TestMethod]
    //public async Task Get()
    //{
    //    await EnsureAuthentication();

    //    List<PlantTest>? response = await client.Get(new GetDataSetRequestDTO() { Id = testObject.ID });
    //    Assert.IsNotNull(response);
    //    Assert.IsTrue(response.Any());
    //}

    //[TestMethod]
    //public async Task GetDataSet()
    //{
    //    await EnsureAuthentication();


    //    UNI.Core.Library.GenericModels.ApiResponseModel<PlantTest>? response = await client.GetDataSet(new GetDataSetRequestDTO() { Id = testObject.ID });
    //    Assert.IsNotNull(response);
    //    Assert.IsTrue(response.ResponseBaseModels.Any());
    //}

    //[TestMethod]
    //public async Task GetItemsWhere()
    //{
    //    await EnsureAuthentication();

    //    var response = await client.GetItemsWhere(0, "test");
    //    Assert.IsNotNull(response);
    //    Assert.IsTrue(response.Any());
    //}

    //[TestMethod]
    //public async Task zz_DeleteItem()
    //{
    //    await EnsureAuthentication();

    //    var response = await client.DeleteItem(testObject);
    //    Assert.IsTrue(response == 0);
    //}
}