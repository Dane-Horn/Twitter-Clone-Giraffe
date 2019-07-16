module TwitterClone.Models.Tweet

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open System.Security.Claims
open TwitterClone.DataModels
open TwitterClone.DBAccess
open System

let handlePostTweet (references: string Option) (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! newTweet = ctx.BindJsonAsync<Tweet> () 
        let row = Tweets.``Create(text)`` newTweet.Text
        let newId = (Guid.NewGuid ()).ToString () 
        let userId = ctx.User.FindFirst ClaimTypes.NameIdentifier
        row.Id <- newId
        row.UserId <- Some userId.Value
        row.References <- references
        try 
            dbctx.SubmitUpdates ()
            let tweetId = Map.empty.Add("id", row.Id)
            return! json tweetId next ctx
        with e ->
            dbctx.ClearUpdates () |> ignore
            let response = Map.empty.Add("message", "invalid data sent")
            return! json response next ctx
    }    
    
let handleGetTweet (next: HttpFunc) (ctx: HttpContext) = 
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier 
    let tweets = 
        query {
            for tweet in Tweets do
            where (tweet.UserId = Some userId && tweet.References = None)
            select tweet
        }
    let tweets = 
        tweets 
        |> Seq.toArray 
        |> Array.map (fun tweet -> {
            Id = tweet.Id; 
            Text = tweet.Text; 
            CreatedAt = tweet.CreatedAt;
            Replies = 
                tweet.``public.tweet by id`` 
                |> Seq.toArray
                |> Array.map (fun tweet -> {
                    Id = tweet.Id; 
                    Text = tweet.Text; 
                    CreatedAt = tweet.CreatedAt; 
                    Replies = 
                        tweet.``public.tweet by id``
                        |> Seq.toArray
                        |> Array.map (fun tweet -> {
                            Id = tweet.Id;
                            Text = tweet.Text;
                            CreatedAt = tweet.CreatedAt;
                            Replies = [||]
                        }
                        )
                }
                )
            }
            )
    let response = Map.empty.Add("tweets", tweets)
    json response next ctx

let handleDeleteTweet (id: string) (next: HttpFunc) (ctx: HttpContext) =
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
    let tweetMaybe = 
        query {
            for tweet in Tweets do
            where (tweet.Id = id)
            select tweet
            take 1
        }
        |> Seq.toArray
        |> Array.tryHead
    match tweetMaybe with
    | None -> text "Tweet does not exist" next ctx
    | Some tweet -> 
        match tweet.UserId = Some userId with
        | false -> text "Unauthorized" next ctx
        | true ->
            try 
                ctx.SetStatusCode 201
                tweet.Delete ()
                dbctx.SubmitUpdates ()
                text "Tweet deleted" next ctx
            with (e) ->
                dbctx.ClearUpdates () |> ignore
                ctx.SetStatusCode 400
                let response = Map.empty.Add ("message", "invalid data sent")
                json response next ctx