using System;
using System.Threading.Tasks;
using Npgsql;

namespace Rebus.PostgreSql
{
    /// <summary>
    /// Wraps an opened <see cref="NpgsqlConnection"/> and makes it easier to work with it
    /// </summary>
    public class PostgresConnection : IDisposable
    {
        readonly NpgsqlConnection _currentConnection;
        NpgsqlTransaction _currentTransaction;

        bool _completed;
        bool _disposed;

        /// <summary>
        /// Constructs the wrapper with the given connection and transaction
        /// </summary>
        public PostgresConnection(NpgsqlConnection currentConnection, NpgsqlTransaction currentTransaction)
        {
            if (currentConnection == null) throw new ArgumentNullException(nameof(currentConnection));
            if (currentTransaction == null) throw new ArgumentNullException(nameof(currentTransaction));
            _currentConnection = currentConnection;
            _currentTransaction = currentTransaction;
        }

        /// <summary>
        /// Creates a new command, enlisting it in the current transaction
        /// </summary>
        public NpgsqlCommand CreateCommand()
        {
            var command = _currentConnection.CreateCommand();
            command.Transaction = _currentTransaction;
            return command;
        }

        /// <summary>
        /// Completes the transaction
        /// </summary>

        public void Complete()
        {
            if (_currentTransaction != null)
            {
                using (_currentTransaction)
                {
                    _currentTransaction.Commit();
                    _currentTransaction = null;
                }
            }
        }
        
        

        /// <summary>
        /// Rolls back the transaction if it hasn't been completed
        /// </summary>
        public void Dispose()
        {
            if (_currentTransaction != null) return;
            if (_disposed) return;

            try
            {
                try
                {
                    if (_currentTransaction != null)
                    {
                        using (_currentTransaction)
                        {
                            try
                            {
                                _currentTransaction.Rollback();
                            }
                            catch { }
                            _currentTransaction = null;
                        }
                    }
                }
                finally
                {
                    _currentConnection.Dispose();
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}