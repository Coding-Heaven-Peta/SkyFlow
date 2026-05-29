Part 1: Database Setup

1. Open Visual Studio 2022
2. Go to View → SQL Server Object Explorer
3. Expand SQL Server → (localdb)\MSSQLLocalDB
4. Right-click (localdb)\MSSQLLocalDB** → select New Query
5. In the query window, go to **File → Open → File
6. Navigate to the project folder and open: SkyFlow\Data\SkyFlow_Database.sql
7. Press Ctrl + Shift + E to execute
8. If a Connect window appears, enter the following and click Connect:
   - Server Name: `(localdb)\MSSQLLocalDB`
   - Authentication: `Windows Authentication`
   - Leave everything else as is !!!!
9. Wait for the messages panel at the bottom to show: SkyFLOWDB setup and seed complete.


Part: 2 Running tje application

1. In Visual Studio, go to File → Open → Project/Solution
2. Navigate to the extracted folder and open: SkyFlow\SkyFlow.csproj
3. Press Ctrl + Shift + B to build

Press Ctrl + F5 (this keeps the console window open)

Login Credentials:

Username	Password	Role
admin		admin123	Administrator
agent01		agent123	Gate Agent
agent02		agent123	Gate Agent


N.B. Passwords are case sensitive and you only have 3 attempts.