package ai.scynet

import java.util.HashMap
import org.apache.ignite.Ignition
import org.apache.ignite.cluster.ClusterNode
import org.apache.ignite.compute.ComputeJob
import org.apache.ignite.compute.ComputeJobAdapter
import org.apache.ignite.compute.ComputeJobResult
import org.apache.ignite.compute.ComputeTaskAdapter
import java.util.Arrays
import org.apache.ignite.configuration.IgniteConfiguration
import org.apache.ignite.spi.deployment.uri.UriDeploymentSpi
import org.apache.ignite.spi.discovery.tcp.TcpDiscoverySpi
import org.apache.ignite.spi.discovery.tcp.ipfinder.vm.TcpDiscoveryVmIpFinder


class MapExampleCharacterCountTask : ComputeTaskAdapter<String, Int>() {
    override fun map(subgrid: MutableList<ClusterNode>?, arg: String?): MutableMap<out ComputeJob, ClusterNode> {
        val map = HashMap<ComputeJob, ClusterNode>()

        var it = subgrid!!.iterator()

        for (word in arg!!.split(" ".toRegex()).dropLastWhile { it.isEmpty() }.toTypedArray()) {
            // If we used all nodes, restart the iterator.
            if (!it.hasNext())
                it = subgrid.iterator()

            val node = it.next()

            map[object : ComputeJobAdapter() {
                override fun execute(): Any? {
                    println()
                    println(">>> Printing '$word' on this node from ignite job.")

                    // Return number of letters in the word.
                    return word.length
                }
            }] = node
        }

        return map
    }

    override fun reduce(results: MutableList<ComputeJobResult>?): Int? {
        var sum = 0

        for (res in results!!)
            sum += res.getData<Int>()

        return sum
    }


}

fun main(args: Array<String>) {

    val deploymentSpi = UriDeploymentSpi()
    deploymentSpi.setUriList(Arrays.asList<String>("file:///home/onrolll/Coding/kotlin-ignite-travisci/build/libs"))

    val ipFinder = TcpDiscoveryVmIpFinder()
    ipFinder.setAddresses(listOf("127.0.0.1:47500", "127.0.0.1:47501", "127.0.0.1:47502", "127.0.0.1:47503", "127.0.0.1:47504", "127.0.0.1:47505", "127.0.0.1:47506", "127.0.0.1:47507", "127.0.0.1:47508","127.0.0.1:47509"))

    val discoverySpi = TcpDiscoverySpi().setIpFinder(ipFinder)

    val cfg = IgniteConfiguration()
    cfg.setDeploymentSpi(deploymentSpi)
    cfg.setDiscoverySpi(discoverySpi)
    cfg.setPeerClassLoadingEnabled(true)

    Ignition.start(cfg).use { ignite -> val res = ignite.compute().execute<String, Any>(MapExampleCharacterCountTask::class.java.name, "Hello Ignite Enabled World!")
        println(">>> Total number of characters in the phrase is '$res'.")
        println(">>> Check all nodes for output (this node is also part of the cluster).")
    }
}


