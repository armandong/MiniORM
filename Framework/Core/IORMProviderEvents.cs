using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IORMProviderEvents
    {
        event EventHandler<InsertCompletedEventArgs> InsertCompleted;
        event EventHandler<DeleteCompletedEventArgs> DeleteCompleted;
        event EventHandler<UpdateCompletedEventArgs> UpdateCompleted;
    }
}
