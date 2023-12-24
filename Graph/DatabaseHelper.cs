using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace Graph.Int
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void CreateDatabase()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var createDatabaseQuery = "CREATE DATABASE Okulovsky100";

                connection.Execute(createDatabaseQuery);
            }
        }

        public void CreateTable()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var createTableQuery = @"
                CREATE TABLE BuilderImplementationDDD (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    TypeImplementation INT,
                    Name NVARCHAR(100),
                    Author NVARCHAR(100) DEFAULT 'YuriOkulovsky',
                    Description NVARCHAR(255),
                    Code NVARCHAR(MAX)
                )";

                connection.Execute(createTableQuery);
            }
        }

        public void InsertBuilderImplementation(BuilderImplementation builderImplementation)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO BuilderImplementationDDD (TypeImplementation, Name, Author, Description, Code)
                VALUES (@TypeImplementation, @Name, @Author, @Description, @Code)";

                connection.Execute(insertQuery, builderImplementation);
            }
        }

        public List<BuilderImplementation> GetAllBuilderImplementations()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var selectQuery = "SELECT * FROM BuilderImplementationDDD";

                return connection.Query<BuilderImplementation>(selectQuery).ToList();
            }
        }

        public BuilderImplementation GetBuilderImplementationByName(string implementationName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var selectQuery = "SELECT * FROM BuilderImplementationDDD WHERE Name = @ImplementationName";

                return connection.QueryFirstOrDefault<BuilderImplementation>(selectQuery, new { ImplementationName = implementationName });
            }
        }

        public List<BuilderImplementation> GetBuilderImplementationsByName(string implementationName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var selectQuery = "SELECT * FROM BuilderImplementationDDD WHERE Name LIKE '%' + @ImplementationName + '%'";

                return connection.Query<BuilderImplementation>(selectQuery, new { ImplementationName = implementationName }).ToList();
            }
        }

        public List<BuilderImplementation> GetBuilderImplementationsByAuthorName(string authorName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var selectQuery = "SELECT * FROM BuilderImplementationDDD WHERE Author LIKE '%' + @AuthorName + '%'";

                return connection.Query<BuilderImplementation>(selectQuery, new { AuthorName = authorName }).ToList();
            }
        }
    }
}
