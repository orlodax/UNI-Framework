using UNI.API.Deploy;

ConfigInfo ci = new()
{
    Username = "pipeline",
    Password = "6r58WpE&SNDv.",
    Hosts = new List<string>()
    {
        "uni001.vegacloud.it",
        "uni002.vegacloud.it",
        "uni003.vegacloud.it"
    },
    DestinationPath = "/var/www/UNI.API.Gioia_Astra_test",
    Source = "C:\\Users\\orlod\\source\\repos\\UNI.API\\UNI.API.TestInstance\\bin\\Release\\net6.0\\publish",
    ServiceName = "astra_test.service"
};

DeployScripts.Deploy(ci);