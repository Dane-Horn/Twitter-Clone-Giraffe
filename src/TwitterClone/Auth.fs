module TwitterClone.Auth

open System
open System.Text
open Microsoft
open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Security.Claims
open System.IdentityModel.Tokens.Jwt
let (authorize: HttpFunc -> AspNetCore.Http.HttpContext -> HttpFuncResult) = 
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let generateToken email =
    let claims = [| 
        Claim (JwtRegisteredClaimNames.Sub, email);
        Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
    let expires = Nullable (DateTime.UtcNow.AddHours (1.0))
    let notBefore = Nullable (DateTime.UtcNow) 
    let securityKey = SymmetricSecurityKey (Encoding.UTF8.GetBytes("secretsecretsecretsecret"))
    let signingCredentials = SigningCredentials (key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)

    let token = 
        JwtSecurityToken (
            issuer = "jwtwebapp.net",
            audience = "jwtwebapp.net",
            claims = claims,
            expires = expires,
            notBefore = notBefore,
            signingCredentials = signingCredentials
        )
    JwtSecurityTokenHandler().WriteToken(token)