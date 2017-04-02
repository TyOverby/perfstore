namespace PerfStore

open Suave
open Option
open Suave.Successful
open Suave.Web
open Suave.Filters
open Suave.Operators
open Suave.Writers
open FSharp.Data
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils
open System
open Types

module Webbernet = 

    type UploadSchema = JsonProvider<"../json_schemas/benchview_upload.json", true>

    let staticFiles =
        let dir = match Seq.tryItem 1 (Environment.GetCommandLineArgs()) with
                    | Some(x) -> x 
                    | None -> Environment.CurrentDirectory
        dir |> IO.Path.GetFullPath


    let processUpload (x: HttpContext) =
        let json = match x.request.form.Head with
                    | (k, Some(v)) -> k // TODO do something with this
                    | (k, None) -> k
        let object =
            try 
                let uploadData = UploadSchema.Parse(json);
                printfn "%s" (uploadData.ToString())
                let onlyRun = (Seq.first uploadData.Runs).Value
                let scenarios = [
                    for test in onlyRun.Tests -> 
                        {
                            Name = test.Name;
                            Metrics = []
                        }
                ]
                uploadData 
            with e -> failwith (e.ToString())
            
        

        OK (object.ToString()) x

        
       

    let app =
        POST >=> choose [
             path "/upload" >=> processUpload
        ]

    let start = fun() -> startWebServer defaultConfig app