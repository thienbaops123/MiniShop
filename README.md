\# MiniShop



MiniShop is a simple ASP.NET Core MVC web application for managing products and orders.

It uses Entity Framework Core with SQLite.



\## Tech stack

\- ASP.NET Core MVC

\- Entity Framework Core

\- SQLite

\- Bootstrap



\## Prerequisites

\- .NET SDK 8 (or the version required by the project)



Check installed version:

```bash

dotnet --version

```



\## Setup \& Run

1\. Restore packages:

```bash

dotnet restore

```



2\. Apply database migrations (recommended):

```bash

dotnet ef database update

```



3\. Run the application:

```bash

dotnet run

```



Then open the URL shown in the terminal (usually `https://localhost:xxxx` or `http://localhost:xxxx`).



\## Admin

If the project seeds an admin account, use the default credentials defined in your seed code (see `Program.cs` / seeding logic).

You can change the admin password in the seed or create a new admin user in the database.



\## Notes

\- Do \*\*not\*\* commit SQLite database files (`\*.db`, `\*.db-wal`, `\*.db-shm`) to Git.

\- If you get `SQLite Error 5: database is locked`, close any tool opening the DB (DB Browser/SQLiteStudio) and restart the app.



\## Contributing

1\. Fork the repo

2\. Create a feature branch:

```bash

git checkout -b feature/my-change

```

3\. Commit your changes:

```bash

git commit -m "Describe change"

```

4\. Push and open a Pull Request

```bash

git push -u origin feature/my-change

```

