// Insert sample agents with metrics as top-level fields
// Usage:
// mongosh "mongodb://127.0.0.1:27017" --file ./database/insert_agents_top_level.js

use("agentsInzynierka");

// Insert only if collection is empty to avoid duplicates
if (db.getCollection('agents').countDocuments() === 0) {
  print('agents collection is empty — inserting sample documents with top-level metric fields');
  db.agents.insertMany([
    {
      agentId: "agent-001",
      authToken: "abc123xyz789",
      serverAddress: "http://192.168.1.100:5000/api/metrics",
      collectionIntervalSeconds: 10,
      dataFormat: "json",
      cpuUsage: 35,
      cpuTemperature: 81,
      ramUsage: 71,
      gpuUsage: 61,
      gpuTemperature: 91,
      diskUsage: 31,
      fanSpeeds: 51
    },
    {
      agentId: "agent-002",
      authToken: "def456uvw123",
      serverAddress: "http://192.168.1.101:5000/api/metrics",
      collectionIntervalSeconds: 15,
      dataFormat: "json",
      cpuUsage: 36,
      cpuTemperature: 82,
      ramUsage: 72,
      gpuUsage: 62,
      gpuTemperature: 92,
      diskUsage: 32,
      fanSpeeds: 52
    },
    {
      agentId: "agent-003",
      authToken: "ghi789rst456",
      serverAddress: "http://192.168.1.102:5000/api/metrics",
      collectionIntervalSeconds: 20,
      dataFormat: "json",
      cpuUsage: 37,
      cpuTemperature: 83,
      ramUsage: 73,
      gpuUsage: 63,
      gpuTemperature: 93,
      diskUsage: 33,
      fanSpeeds: 53
    },
    {
      agentId: "agent-004",
      authToken: "jkl012mno345",
      serverAddress: "http://192.168.1.103:5000/api/metrics",
      collectionIntervalSeconds: 10,
      dataFormat: "json",
      cpuUsage: 38,
      cpuTemperature: 84,
      ramUsage: 74,
      gpuUsage: 64,
      gpuTemperature: 94,
      diskUsage: 34,
      fanSpeeds: 54
    },
    {
      agentId: "agent-005",
      authToken: "pqr678stu901",
      serverAddress: "http://192.168.1.104:5000/api/metrics",
      collectionIntervalSeconds: 30,
      dataFormat: "json",
      cpuUsage: 39,
      cpuTemperature: 85,
      ramUsage: 75,
      gpuUsage: 65,
      gpuTemperature: 95,
      diskUsage: 35,
      fanSpeeds: 55
    }
  ]);
} else {
  print('agents collection is not empty — skipping insert to avoid duplicates');
}
