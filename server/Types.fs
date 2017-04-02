module Types

open FSharp.Data.Sql
open System.Collections.Generic
open System.Linq
open System

[<Measure>] type Bytes
[<Measure>] type MilliSeconds
[<Measure>] type Count
type Metric<'unit> =  {Name:string;Value:'unit}
type BytesAllocated =  Metric<int<Bytes>>
type ElapsedTime =  Metric<int<MilliSeconds>>
type Summation =  Metric<int<Count>>
type PerfUnits =
    | BytesAllocated  of BytesAllocated
    | ElapsedTime of ElapsedTime
    | Summation of Summation
type Scenario = {Name:string; Metrics:List<PerfUnits>}

[<AutoOpen>]
module Parsing =
    open System

    let private helper parseFunc =
        parseFunc >> function
        | (true, value) -> Some value
        | (false, _) -> None

    let parseInt = helper Int32.TryParse
        

let [<Literal>] connectionString = 
    "Server=tcp:perfstore.database.windows.net,1433;Initial Catalog=perfstore;Persist Security Info=False;User ID=roslyn;Password=originalcharacterdonotsteal1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

type sql = SqlDataProvider<
              ConnectionString = connectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER,
              ResolutionPath = "",
              IndividualsAmount = 1000,
              UseOptionTypes = true >

let perfData = sql.GetDataContext().Dbo

let convertValue metric =
    match metric with
        | (name:string, "ms", value:string) -> 
            let valueParsed = parseInt(value)
            match valueParsed with
                | Some intValue -> Some (BytesAllocated({Name=name; Value= intValue*1<Bytes> }))
                | _ -> None
        | (name:string, "Count", value:string) ->
            let valueParsed = parseInt(value)
            match valueParsed with
                | Some intValue -> Some (ElapsedTime({Name=name; Value= intValue*1<MilliSeconds> }))
                | _ -> None
        | (name:string, "Bytes", value:string) -> 
            let valueParsed = parseInt(value)
            match valueParsed with
                | Some intValue -> Some (Summation({Name=name; Value= intValue*1<Count> }))
                | _ -> None
        | _-> None

let metricsForScenario scenarioId =
    query {
        for metric in perfData.Metrics do
        where (metric.ScenarioId = scenarioId)
        select (convertValue (metric.MetricName, metric.MetricUnit, metric.MetricValue))
    }

let metricsForRun runId =
    query {
        for scenario in perfData.Scenarios do
        where (scenario.RunId = runId)
        let metrics = (metricsForScenario scenario.ScenarioId).Where(fun x -> x.  IsSome).Select(fun x -> x.Value).ToList()
        let scenarioType = {Name = scenario.ScenarioName; Metrics = metrics}
        select (scenarioType)
    }

