module TwitterClone.DBAccess
open System
open FSharp.Data.Sql.Common
open FSharp.Data.Sql

let [<Literal>] DbVendor = Common.DatabaseProviderTypes.POSTGRESQL
let [<Literal>] ConnString = "Host=localhost;Database=twitter-giraffe;Username=postgres;Password=postgres"
let [<Literal>] ConnexStringName = "DefaultConnectionString"
let [<Literal>] ResPath = @"C:\Users\g16h0473\.nuget\packages\system.runtime.compilerservices.unsafe\4.5.2\lib\netcoreapp2.0"
let [<Literal>] IndivAmount = 100
let [<Literal>] UseOptTypes = true
let [<Literal>] Owner = "public"
type Sql = 
    SqlDataProvider<
        DbVendor,
        ConnString,
        "",
        ResPath,
        IndivAmount,
        UseOptTypes,
        Owner>

let dbctx = Sql.GetDataContext ()

let Tweets = dbctx.Public.Tweet
let Users = dbctx.Public.Users
let Followings = dbctx.Public.Following
let Retweets = dbctx.Public.Retweet