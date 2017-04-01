open Suave
open FSharp.Data.Sql
open System.Collections.Generic
open System.Linq

type Bytes = int
type MilliSeconds = int
type Count = int
type Metric<'unit> =  {Name:string;Unit:'unit}
type Scenario = {Name:string; Metrics:List<Metric<obj>>}
type BytesAllocated =  Metric<Bytes>
type ElapsedTime =  Metric<MilliSeconds>
type Summation =  Metric<Count>
        

let [<Literal>] connectionString = 
    "Server=tcp:perfstore.database.windows.net,1433;Initial Catalog=perfstore;Persist Security Info=False;User ID=****;Password=***;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

type sql = SqlDataProvider<
              ConnectionString = connectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER,
              ResolutionPath = "",
              IndividualsAmount = 1000,
              UseOptionTypes = true >

let perfData = sql.GetDataContext().Dbo

let metricsForRun runId =
    query {
        for run in perfData.Runs do
        for metric in perfData.Metrics do
        for scenario in perfData.Scenarios do
        where (scenario.RunId = runId)
        where (metric.ScenarioId = scenario.ScenarioId)
        select (metric.MetricName, metric.MetricUnit, metric.MetricValue)
    }

let metricValueConverter metric =
    match metric with
        | (name, unit, value) -> sprintf "name: %A unit: %A value: %A" name unit value

[<EntryPoint>]
let main argv = 
    let result = metricsForRun(0).AsEnumerable().First()
    //startWebServer defaultConfig (Successful.OK "Hello World!")
    0 // return an integer exit code
