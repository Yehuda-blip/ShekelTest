# ShekelTestPart1

Part 1 of the assignment was to analyze this script:
```sql
CREATE DATABASE  [TestDB]
USE [TestDB]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customers](
	[CustomerID] [nchar](5) NOT NULL,
	[FirstName] [varchar](40) NOT NULL,
	[LastName] [varchar](30) NOT NULL,
	[City] [varchar](15) NOT NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Products](
	[ProductID] [int] NOT NULL,
	[ProductDesc] [varchar](40) NOT NULL,
	[InsertDate] [datetime] NOT NULL,
	[Price] [decimal](18, 0) NOT NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Orders](
	[OrderID] [int] NOT NULL,
	[CustomerID] [nchar](5) NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[PriceSum] [decimal](18, 0) NOT NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Order Details](
	[OrderID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[Quantity] [smallint] NOT NULL
) ON [PRIMARY]
GO
```
Here we create a database with `Customers`, `Products`, `Orders` and `[Order Detail]` table - a mini E-commerce type of situation. The assignment requested a diagram:
![Example Image](./readme-misc/diagram1.png)

We can infer that the `Order Details` row is meant to allow the `Orders` table to reference multiple `Product` rows.

There was also a request to write a query that will get the total quantity bought of each distinct product. Unfortunately I got a solved assignment link, so I decided this was a great time to refresh my .Net skills, and also prove that I can will manage working with a Database if I absolutely must.
To do that, I wrote a simple application to measure the turnaround times of two queries that would accomplish this task. So first, I must set up a database. I took some freedom with the given script so that I could add primary keys everywhere (which includes adding a column to the order details table) - it is generally a good idea to have primary keys except for some special situations. More importantly, the keys allowed me to easily work with Entity-Framework, the standard .Net ORM. We now have(Assuming the TestDB exists):
```sql
USE [TestDB]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customers](
	[CustomerID] [nchar](5) NOT NULL,
	[FirstName] [varchar](40) NOT NULL,
	[LastName] [varchar](30) NOT NULL,
	[City] [varchar](15) NOT NULL
	PRIMARY KEY (CustomerID)
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Products](
	[ProductID] [int] NOT NULL,
	[ProductDesc] [varchar](40) NOT NULL,
	[InsertDate] [datetime] NOT NULL,
	[Price] [decimal](18, 0) NOT NULL
	PRIMARY KEY (ProductID)
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Orders](
	[OrderID] [int] NOT NULL,
	[CustomerID] [nchar](5) NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[PriceSum] [decimal](18, 0) NOT NULL
	PRIMARY KEY (OrderID)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[OrderDetails](
	[OrderDetailsID] [int] NOT NULL,
	[OrderID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[Quantity] [smallint] NOT NULL
	PRIMARY KEY (OrderDetailsID)
) ON [PRIMARY]
GO
```

