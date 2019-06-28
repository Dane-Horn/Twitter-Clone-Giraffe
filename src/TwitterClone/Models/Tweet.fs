module TwitterClone.Models.Tweet

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

open TwitterClone.DBAccess
open System

[<CLIMutable>]
type Tweet =
    {
        Id: string
        Text : string
    }

let handlePostTweet (text: string) (next: HttpFunc) (ctx: HttpContext) =
        let row = Tweets.``Create(text)`` text
        let newId = (Guid.NewGuid ()).ToString () 
        row.Id <- newId
        row.UserId <- Some "0381e841-ce4f-422c-b4a4-e5a4643375c5"
        dbctx.SubmitUpdates ()
        json newId next ctx

let handleGetTweet (next: HttpFunc) (ctx: HttpContext) = 
    let tweets = 
        query {
            for tweet in Tweets do
            select tweet
        }
    let tweets = tweets |> Seq.toArray |> Array.map (fun i -> i.ColumnValues |> Map.ofSeq)
    json tweets next ctx
