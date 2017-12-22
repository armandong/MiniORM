using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IDbProviderEvents
    {
        event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated; 
        event EventHandler<ConnectedEventArgs> Connected;
        event EventHandler<ExecutedEventArgs> Executed;
        event EventHandler<LoadDataCompletedEventArgs> LoadDataCompleted;
        event EventHandler<TransactionStartedEventArgs> TransactionStarted;
    }
}
