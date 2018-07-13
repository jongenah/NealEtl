#r "O365ETL.dll"

using System;

public static void Run(TimerInfo pbiTimer, TraceWriter log)
{
	O365ETL.ConsoleWriter.GetInstance().Writer = log;

    string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["AuditDb"]?.ConnectionString;
    string schema = System.Configuration.ConfigurationManager.ConnectionStrings["Schema"]?.ConnectionString;
    string clientSecret = System.Configuration.ConfigurationManager.ConnectionStrings["ClientSecret"]?.ConnectionString;
    string tenant = System.Configuration.ConfigurationManager.ConnectionStrings["Tenant"]?.ConnectionString;
    string clientId = System.Configuration.ConfigurationManager.ConnectionStrings["ClientId"]?.ConnectionString;
    string productKey = System.Configuration.ConfigurationManager.ConnectionStrings["ProductKey"]?.ConnectionString;
	
    var opsInstance = O365ETL.SQLClient.GetInstance(connstring, schema);
    opsInstance.Writer = log;
	
    const int daysToRetrieve = 2;

    for (int i = 0; i < daysToRetrieve; i++)
    {
        DateTime dateToProcess = DateTime.UtcNow.AddDays(-1 * i);
        try
        {
            var result = O365ETL.Processor.Process(clientId, clientSecret, tenant, dateToProcess, connstring, schema, productKey).Result;
        }
        catch (Exception ex)
        {
            throw (ex);
        }
    }
    opsInstance.CreateSP();
    opsInstance.RunStoredProc(schema + ".uspMoveStaging");
}
