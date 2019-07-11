module TwitterClone.Models.User

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

open System.Security.Claims
open TwitterClone.Auth
open TwitterClone.DBAccess
open TwitterClone.DataModels
open System



let handleRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
        let row = Users.Create ()
        let! newUser = ctx.BindJsonAsync<User> ()
        let newId = (Guid.NewGuid ()).ToString ()
        row.Id <- newId
        row.Email <- newUser.Email
        row.Password <- newUser.Password
        row.Username <- newUser.Username
        try 
            dbctx.SubmitUpdates ()    
        with (e) ->
            dbctx.ClearUpdates () |> ignore
        let result = {Id = newId; Email = newUser.Email; Username = newUser.Username}
        let result = Map.empty.Add ("user", result)
        return! json result next ctx
    }
let handleMe = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let id = ctx.User.FindFirst ClaimTypes.NameIdentifier
        let userMaybe= 
            query {
                for user in Users do
                where (user.Id = id.Value)
                select (Some {Id = user.Id; Email=user.Email; Username=user.Username})
                exactlyOneOrDefault
            }
        match userMaybe with
        | Some user ->
            let user = Map.empty.Add("user", user)
            json user next ctx
        | None ->
            text "User does not exist" next ctx
let handleLogin = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindJsonAsync<LoginModel> ()
            let email = model.Email
            let password = model.Password
            let idMaybe = query {
                for user in Users do
                    where (user.Email = email && user.Password = password)
                    select (Some (user.Id))
                    exactlyOneOrDefault
                }
            match idMaybe with
            | Some id -> 
                let token = Map.empty.Add("token", generateToken id)
                return! json token next ctx
            | None ->
                return! text "User does not exist" next ctx
        }
let handleFollow (followingId: string) (next: HttpFunc) (ctx: HttpContext) =
    let userToFollow = query {
        for user in Users do
        where(user.Id = followingId)
        select (Some user)
        exactlyOneOrDefault
    }
    match userToFollow with
    | None -> 
        text "User does not exist" next ctx
    | Some user ->
        let newFollow = Followings.Create ()
        let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
        newFollow.UserId <- userId
        newFollow.FollowingId <- followingId
        try
            dbctx.SubmitUpdates ()
        with e -> 
            dbctx.ClearUpdates () |> ignore
        let user = {Id = user.Id; Email = user.Email; Username = user.Username}
        json user next ctx

let handleGetFollowing (next: HttpFunc) (ctx: HttpContext) =
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
    let users = query {
        for user in Users do
            where (user.Id = userId)
            for following in user.``public.following by id`` do
                for followedUser in following.``public.users by id_1`` do
                    select (followedUser)
    }
    let users = 
        users
        |> Seq.toArray
        |> Array.map (fun (user) -> 
            {Id = user.Id; Email = user.Email; Username = user.Username})
    json users next ctx

let getFollowingTweets (userId: string) = 
    query {
        for user in Users do
            where (user.Id = userId)
            for following in user.``public.following by id`` do
                for followedUser in following.``public.users by id_1`` do
                    for tweet in followedUser.``public.tweet by id`` do
                    select (tweet)}
    |> Seq.toArray 
    |> Array.map (fun tweet -> {Id = tweet.Id; Text = tweet.Text; CreatedAt = tweet.CreatedAt; Replies = [||]})

let getFollowingRetweets (userId: string) =
    query {
        for user in Users do
            where (user.Id = userId)
            for following in user.``public.following by id`` do
                for followedUser in following.``public.users by id_1`` do
                    for retweet in followedUser.``public.retweet by id`` do
                    select (retweet)}
    |> Seq.toArray  
    |> Array.map (fun retweet -> {Id = retweet.Id; UserId = retweet.User; TweetId = retweet.TweetId})

let handleGetFeed (next: HttpFunc) (ctx: HttpContext) =
    let userId = ctx.User.FindFirstValue ClaimTypes.NameIdentifier
    let tweets = getFollowingTweets userId
    let retweets = getFollowingRetweets userId
    let posts = {Tweets = tweets; Retweets = retweets}
    json posts next ctx