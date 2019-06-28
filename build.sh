#!/bin/sh
dotnet restore src/TwitterClone
dotnet build src/TwitterClone

dotnet restore tests/TwitterClone.Tests
dotnet build tests/TwitterClone.Tests
dotnet test tests/TwitterClone.Tests
