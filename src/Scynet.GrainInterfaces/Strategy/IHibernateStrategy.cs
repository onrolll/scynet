using System;
using System.Threading.Tasks;

namespace Scynet.GrainInterfaces.Strategy
{
    /// <summary>
    /// A Strategy which decides when to hibernate agents
    /// </summary>
    public interface IHibernateStrategy : IStrategy, Orleans.IGrainWithGuidKey
    {
        /// <summary>
        /// Change the frequency of running the hibernation strategy
        /// </summary>
        Task SetUpdateFrequency(TimeSpan updateFrequency);
    }
}
