using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NfImpacto.Envio.Domain.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Configurations.Factories
{
    public interface IDatabaseFactory
    {
        IDbTransaction BeginTransaction();
        IDbConnection Connection();
        Task OpenConnectionAsync();
        void CommitTransaction();
        void RollbackTransaction();
        void CloseConnection();
        void DisposeConnection();
    }

    public class DatabaseFactory : IDatabaseFactory
    {
        private SqlConnection _connection;
        private readonly DatabaseOptions _database;
        private SqlTransaction _transaction;
        private bool _isTransactionOpen;
        private readonly string _connectionString;
        private bool _disposed = false;

        public DatabaseFactory(IOptions<DatabaseOptions> database)
        {
            _database = database.Value ?? throw new ArgumentNullException(nameof(database));

            _connectionString = _database.ConnectionString;
            _connection = new SqlConnection(_connectionString);
        }

        public IDbTransaction BeginTransaction()
        {
            if (_transaction == null)
            {
                if (_connection.State != ConnectionState.Open)
                    throw new Exception("A conexão com o banco não esta aberta.");

                _transaction = _connection.BeginTransaction();
            }

            _isTransactionOpen = true;

            return _transaction;
        }

        public IDbConnection Connection()
        {
            return _connection;
        }

        public async Task OpenConnectionAsync()
        {
            if (_disposed)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
                _disposed = false;
            }

            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
                _transaction = null;
            }
        }

        public void CommitTransaction()
        {
            if (_isTransactionOpen)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }

            _isTransactionOpen = false;
        }

        public void RollbackTransaction()
        {
            if (_isTransactionOpen)
                _transaction.Rollback();
        }

        public void CloseConnection()
        {
            _connection.Close();
        }

        public void DisposeConnection()
        {
            _connection.Dispose();
        }
    }
}
