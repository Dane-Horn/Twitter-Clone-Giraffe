namespace TwitterClone.DataModels
open System

//#region User Models
[<CLIMutable>]
type User = {
    Email: string
    Password: string
    Username: string
}

type UserResponse = {
    Id: string
    Username: string
}

type LoginModel = {
    Email: string
    Password: string
}
//#endregion
//#region  Tweet Models
[<CLIMutable>]
type Tweet = {
    Id: string
    Text : string
}

type TweetResponse = {
    Id: string
    Text: string
    CreatedAt: DateTime
    Replies: TweetResponse array
}

type RetweetResponse = {
    Id: string
    UserId: string
    TweetId: string
}

type PostsResponse = {
    Tweets: TweetResponse array
    Retweets: RetweetResponse array
}
//#endregion