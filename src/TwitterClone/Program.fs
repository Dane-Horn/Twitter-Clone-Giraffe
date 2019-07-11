module TwitterClone.App

open System
open System.Text
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication.JwtBearer
open Giraffe
open Microsoft.IdentityModel.Tokens

open TwitterClone.HttpHandlers
open TwitterClone.Models.Tweet
open TwitterClone.Models.User
open TwitterClone.Auth
// ---------------------------------
// Web app
// ---------------------------------


let webApp =
    choose [
        subRoute "/api"
            (choose [
                GET >=> choose [
                    route "/hello" >=> handleGetHello
                    route "/create-tweet" >=> handlePostTweet
                    route "/get-tweets" >=> authorize >=> handleGetTweet
                    route "/posts/own" >=> authorize >=> handleGetTweet
                ]
                POST >=> choose [
                    route "/token" >=> handleLogin
                ]
                subRoute "/user" (
                    choose [
                        route "/login" >=> handleLogin
                        route "/me" >=> authorize >=> handleMe
                        route "/register" >=> handleRegisterUser
                    ]
                )
                subRoute "/tweet" (
                    choose [
                        POST >=> choose [
                            route "" >=> authorize >=> handlePostTweet
                        ]
                    ]
                )
            ])
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder//.WithOrigins("http://localhost:8080")
        // .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseAuthentication()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun options ->
            options.TokenValidationParameters <- TokenValidationParameters (
                ValidateActor = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "jwtwebapp.net",
                ValidAudience = "jwtwebapp.net",
                IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsecretsecretsecret"))
            )
        ) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
        .AddConsole()
        .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0