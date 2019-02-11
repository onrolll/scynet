using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scynet.GrainInterfaces
{
    /// <summary>
    /// A component
    /// </summary>
    public interface IComponent : Orleans.IGrainWithGuidKey
    {
        /// <summary>
        /// Initialize the component
        /// </summary>
        Task Initialize(String endpoint, ISet<String> runnerTypes);

        /// <summary>
        /// Mark the component as disconnected
        /// </summary>
        Task Disconnect();

        /// <summary>
        /// Register an input for the component
        /// </summary>
        Task RegisterInput(Guid agentId);
    }
}
