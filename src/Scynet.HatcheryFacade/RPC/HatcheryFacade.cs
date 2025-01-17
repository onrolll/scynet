﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Orleans;
using Scynet.GrainInterfaces;
using Scynet.GrainInterfaces.Agent;
using Scynet.GrainInterfaces.Component;
using Scynet.GrainInterfaces.Registry;

namespace Scynet.HatcheryFacade.RPC
{
    public class HatcheryFacade : Hatchery.HatcheryBase
    {
        private readonly ILogger<HatcheryFacade> Logger;
        private readonly IClusterClient ClusterClient;

        public HatcheryFacade(ILogger<HatcheryFacade> logger, IClusterClient clusterClient)
        {
            Logger = logger;
            ClusterClient = clusterClient;
        }

        public override async Task<ComponentRegisterResponse> RegisterComponent(ComponentRegisterRequest request, ServerCallContext context)
        {
            var id = Guid.Parse(request.Uuid);

            var component = ClusterClient.GetGrain<IComponent>(id);
            await component.Initialize(request.Address, new HashSet<String>(request.RunnerType));

            return new ComponentRegisterResponse();
        }

        public override async Task<AgentRegisterResponse> RegisterAgent(AgentRegisterRequest request, ServerCallContext context)
        {
            var id = Guid.Parse(request.Agent.Uuid);

            var agent = ClusterClient.GetGrain<IComponentAgent>(id);

            var data = request.Agent.EggData.ToByteArray();
            var inputs = request.Agent.Inputs.Select(
                x => ClusterClient.GetGrain<IAgent>(Guid.Parse(x))
            ).ToList();

            await agent.Initialize(new AgentInfo
            {
                ComponentId = Guid.Parse(request.Agent.ComponentId),
                OutputShapes = request.Agent.Outputs.Select(o => o.Dimension.ToList()).ToList(),
                Frequency = request.Agent.Frequency,
                RunnerType = request.Agent.ComponentType,
                Agent = agent
            }, inputs, data);

            return new AgentRegisterResponse();
        }

        public override async Task<Void> UnregisterComponent(ComponentUnregisterRequest request, ServerCallContext context)
        {
            var id = Guid.Parse(request.Uuid);

            var component = ClusterClient.GetGrain<IComponent>(id);
            await component.Disconnect();

            var registry = ClusterClient.GetGrain<IRegistry<Guid, AgentInfo>>(0);
            var agents = await registry.Query(list =>
                from kv in list
                where kv.Value.ComponentId == id
                select kv.Key);

            await Task.WhenAll(agents.Select(agent => ClusterClient.GetGrain<IAgent>(agent).ReleaseAll()));

            return new Void();
        }

        public override async Task<Void> AgentStopped(AgentStoppedEvent request, ServerCallContext context)
        {
            var id = Guid.Parse(request.Agent.Uuid);

            var agent = ClusterClient.GetGrain<IComponentAgent>(id);
            await agent.ReleaseAll();

            return new Void();
        }
    }
}
