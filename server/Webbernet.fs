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

    let staticFiles =
        let dir = match Seq.tryItem 1 (Environment.GetCommandLineArgs()) with
                    | Some(x) -> x 
                    | None -> Environment.CurrentDirectory
        dir |> IO.Path.GetFullPath


    let foo (x: HttpContext) =
        match x.request.form.Head with
            | (k, Some(v)) -> OK k x
            | (k, None) -> OK k x

    let app =
        POST >=> choose [
             path "/upload" >=> foo
        ]

    let start = fun() -> startWebServer defaultConfig app