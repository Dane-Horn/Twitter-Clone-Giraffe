module TwitterClone.DBAccess
open System
open FSharp.Data.Sql.Common
open FSharp.Data.Sql

let [<Literal>] DbVendor = Common.DatabaseProviderTypes.POSTGRESQL
let [<Literal>] ConnString = "Host=localhost;Database=twitterCloneGiraffe;Username=postgres;Password=postgres"
let [<Literal>] ConnexStringName = "DefaultConnectionString"
let [<Literal>] ResPath = @"C:\Users\Dane\.nuget\packages\system.runtime.compilerservices.unsafe\4.5.2\lib\netcoreapp2.0"
// let [<Literal>] ResPath = @"C:\Users\Dane\.nuget\packages\npgsql\4.0.7\lib\netstandard2.0\Npgsql.dll"
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
let Users = dbctx.Public.User
