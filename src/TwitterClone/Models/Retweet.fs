module TwitterClone.Models.Retweet

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

open System.Security.Claims
open TwitterClone.Auth
open TwitterClone.DBAccess
open TwitterClone.DataModels
open System

let handlePostRetweet (tweetId: string) (next: HttpFunc) (ctx: HttpContext) = 
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
    let retweet = Retweets.Create ()
    let newId = (Guid.NewGuid ()).ToString ()
    retweet.Id <- newId
    retweet.User <- userId
    retweet.TweetId <- tweetId
    try
        dbctx.SubmitUpdates ()
    with e ->
        dbctx.ClearUpdates () |> ignore
    let result = {Id = newId; UserId = userId; TweetId = tweetId}
    json result next ctx

let handleDeleteRetweet (id: string) (next: HttpFunc) (ctx: HttpContext) =
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
    let retweetMaybe = 
        query {
            for retweet in Retweets do
            where (retweet.Id = id)
            select retweet
            take 1
        }
        |> Seq.toArray
        |> Array.tryHead
    match retweetMaybe with
    | None -> text "Retweet does not exist" next ctx
    | Some retweet -> 
        match retweet.User = userId with
        | false -> text "Unauthorized" next ctx
        | true ->
            try 
                retweet.Delete ()
                dbctx.SubmitUpdates ()
                text "Retweet deleted" next ctx
            with (e) ->
                dbctx.ClearUpdates () |> ignore
                text "DB error occurred" next ctx