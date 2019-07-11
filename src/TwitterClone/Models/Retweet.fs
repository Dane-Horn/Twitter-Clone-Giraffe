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