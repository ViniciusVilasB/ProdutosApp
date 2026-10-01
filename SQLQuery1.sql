-- Script de criação do banco e da tabela Produtos (SQL Server / LocalDB)
-- Execute no SSMS conectado em (localdb)\MSSQLLocalDB (ou no seu SQL Server).

IF DB_ID('LojaDB') IS NULL
    CREATE DATABASE LojaDB;
GO

USE LojaDB;
GO

IF OBJECT_ID('dbo.Produtos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Produtos (
        Id        INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Nome      NVARCHAR(100)  NOT NULL,
        Preco     DECIMAL(10,2)  NOT NULL CONSTRAINT CK_Produtos_Preco   CHECK (Preco >= 0),
        Estoque   INT            NOT NULL CONSTRAINT CK_Produtos_Estoque CHECK (Estoque >= 0),
        Categoria NVARCHAR(50)   NOT NULL
    );
END
GO

-- (Opcional) dados iniciais para testar rapidamente
-- INSERT INTO Produtos (Nome, Preco, Estoque, Categoria) VALUES
--   (N'Teclado Mecânico', 249.90, 15, N'Periféricos'),
--   (N'Mouse Gamer',      129.90, 30, N'Periféricos'),
--   (N'Monitor 24"',      899.00,  8, N'Monitores');
GO
