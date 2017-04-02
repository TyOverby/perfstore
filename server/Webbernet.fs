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

    let app = choose [
            //path "/websocket" >=> handShake echo
        GET >=> choose [
            Files.browse staticFiles
        ]
    ]

    let start = fun() -> startWebServer defaultConfig app