module TwitterClone.Models.Tweet

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open System.Security.Claims
open TwitterClone.DataModels
open TwitterClone.DBAccess
open System

let handlePostTweet (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! newTweet = ctx.BindJsonAsync<Tweet> () 
        let row = Tweets.``Create(text)`` newTweet.Text
        let newId = (Guid.NewGuid ()).ToString () 
        let userId = ctx.User.FindFirst ClaimTypes.NameIdentifier
        row.Id <- newId
        row.UserId <- Some userId.Value
        dbctx.SubmitUpdates ()
        let tweetId = Map.empty.Add("id", row.Id)
        return! json tweetId next ctx
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
