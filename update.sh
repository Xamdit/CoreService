#!/bin/bash

# Scaffold SQL Server databases

#npx prisma format
#npx prisma db push --schema prisma/schema.prisma;

#dotnet add package Microsoft.EntityFrameworkCore
#dotnet add package Microsoft.EntityFrameworkCore.Design
#dotnet add package MySql.EntityFrameworkCore

dotnet ef dbcontext scaffold "Server=localhost;User=root;Password=password;Database=crm;Convert Zero Datetime=True;TreatTinyAsBoolean=true" MySql.EntityFrameworkCore -c MyContext -o ./Entities -f
#npx prisma db pull --schema prisma/xaxino.prisma

echo "Done"
