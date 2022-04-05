#!/bin/bash

rm -rf Model
dotnet ef dbcontext scaffold -o Model "Server=localhost;Database=test;User=SA;Password=Abcd5678;Connect Timeout=60;ConnectRetryCount=0;Encrypt=false" Microsoft.EntityFrameworkCore.SqlServer
