#r "O365ETL.dll"

using System;
using System.Configuration;
using O365ETL;

public static void Run(TimerInfo pbiTimer, TraceWriter log)
{
	ConsoleWriter.GetInstance().Writer = log;

    string connstring = ConfigurationManager.ConnectionStrings["AuditDb"]?.ConnectionString;
    string schema = ConfigurationManager.ConnectionStrings["Schema"]?.ConnectionString;
    string clientSecret = ConfigurationManager.ConnectionStrings["ClientSecret"]?.ConnectionString;
    string tenant = ConfigurationManager.ConnectionStrings["Tenant"]?.ConnectionString;
    string clientId = ConfigurationManager.ConnectionStrings["ClientId"]?.ConnectionString;
    string productKey = ConfigurationManager.ConnectionStrings["ProductKey"]?.ConnectionString;
	
    var opsInstance = SQLClient.GetInstance(connstring, schema);
    opsInstance.Writer = log;
	
    const int daysToRetrieve = 1;

    for (int i = daysToRetrieve - 1; i >= 0; i--)
    {
        DateTime dateToProcess = DateTime.UtcNow.AddDays(-1 * i);
        try
        {
            log.Info($"Processing: {dateToProcess}");
            var result = Processor.Process(clientId, clientSecret, tenant, dateToProcess, connstring, schema, productKey).Result;
        }
        catch (Exception ex)
        {
            throw (ex);
        }
    }
    log.Info($"Move from Staging Start");
    opsInstance.CreateSP();
    opsInstance.RunStoredProc($"uspMoveStaging");
    log.Info($"Move from Staging Complete");
}
