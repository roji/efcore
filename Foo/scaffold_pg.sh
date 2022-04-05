#!/bin/bash

rm -rf Model
dotnet ef dbcontext scaffold -o Model "Host=localhost;Username=test;Password=test" Npgsql.EntityFrameworkCore.PostgreSQL
