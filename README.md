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
