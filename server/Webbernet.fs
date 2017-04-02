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

module Webbernet = 
    open System

    type UploadSchema = JsonProvider<"../json_schemas/benchview_upload.json", true>

    let staticFiles =
        let dir = match Seq.tryItem 1 (Environment.GetCommandLineArgs()) with
                    | Some(x) -> x 
                    | None -> Environment.CurrentDirectory
        dir |> IO.Path.GetFullPath


    let foo (x: HttpContext) =
        let json = match x.request.form.Head with
                    | (k, Some(v)) -> k // TODO do something with this
                    | (k, None) -> k
        let object =
            try 
                let uploadData = UploadSchema.Parse(json);
                printfn "%s" (uploadData.ToString())
                uploadData 
            with e -> failwith (e.ToString())
            
        

        OK (object.ToString()) x

        
       

    let app =
        POST >=> choose [
             path "/upload" >=> foo
        ]

    let start = fun() -> startWebServer defaultConfig app