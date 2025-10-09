using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

/// <summary>
/// 六边形流场导航 使用了job和rov 适用于 单个静态终点 大量单位导航
/// </summary>
public class HexFlowField : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridRadius = 10;
    public float hexSize = 1f;
    public Transform target;

    [Header("Agent Settings")]
    public int agentCount = 100;
    public float agentRadius = 0.5f;
    public float agentMaxSpeed = 5f;
    public float agentAvoidanceStrength = 2f;
    public float avoidanceRadius = 2f;
    public GameObject agentPrefab;

    [Header("Obstacle Settings")]
    public LayerMask obstacleLayer;
    public float obstacleCheckRadius = 0.4f;

    [Header("Visualization")]
    public bool showGrid = true;
    public bool showFlowVectors = true;
    public bool showCostField = true;
    public Color lowCostColor = Color.green;
    public Color highCostColor = Color.red;
    public float arrowScale = 0.5f;

    private Dictionary<Vector2Int, HexCell> grid = new Dictionary<Vector2Int, HexCell>();
    private Vector2Int targetHexCoord;
    private bool isFlowFieldGenerated = false;
    private List<Agent> agents = new List<Agent>();
    private NativeArray<AgentData> agentDataArray;
    private NativeArray<float3> flowDirections;
    private NativeArray<ObstacleData> obstacleDataArray;
    private JobHandle avoidanceJobHandle;

    // 六边形方向向量
    private static readonly Vector2Int[] directions = {
        new Vector2Int(1, 0),    // 东
        new Vector2Int(0, 1),    // 东北
        new Vector2Int(-1, 1),   // 西北
        new Vector2Int(-1, 0),   // 西
        new Vector2Int(0, -1),   // 西南
        new Vector2Int(1, -1)    // 东南
    };

    public class Agent
    {
        public Transform transform;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 desiredVelocity;
        public float radius;
        public float maxSpeed;
    }

    public struct AgentData
    {
        public float3 position;
        public float3 velocity;
        public float3 desiredVelocity;
        public float radius;
        public float maxSpeed;
        public float avoidanceStrength;
    }

    public struct ObstacleData
    {
        public float3 position;
        public float radius;
    }

    void Start()
    {
        GenerateHexGrid();
        GenerateFlowField();
        InitializeAgents();
        InitializeObstacleData();
    }

    void Update()
    {
        // 如果目标移动，重新计算流场
        Vector2Int currentTargetCoord = WorldToHexCoord(target.position);
        if (isFlowFieldGenerated && currentTargetCoord != targetHexCoord)
        {
            GenerateFlowField();
        }

        // 更新代理数据
        UpdateAgentData();

        // 调度避障作业
        ScheduleAvoidanceJob();

        // 完成作业并更新代理
        CompleteJobAndUpdateAgents();
    }

    void LateUpdate()
    {
        // 更新代理位置
        UpdateAgentTransforms();
    }

    void OnDestroy()
    {
        // 清理NativeArray
        if (agentDataArray.IsCreated) agentDataArray.Dispose();
        if (flowDirections.IsCreated) flowDirections.Dispose();
        if (obstacleDataArray.IsCreated) obstacleDataArray.Dispose();
    }

    void InitializeAgents()
    {
        // 创建代理
        for (int i = 0; i < agentCount; i++)
        {
            Vector3 randomPos = Vector3.zero;
            GameObject agentObj = Instantiate(agentPrefab, randomPos, Quaternion.identity);

            Agent agent = new Agent
            {
                transform = agentObj.transform,
                position = randomPos,
                velocity = Vector3.zero,
                radius = agentRadius,
                maxSpeed = agentMaxSpeed
            };

            agents.Add(agent);
        }

        // 初始化NativeArray
        agentDataArray = new NativeArray<AgentData>(agentCount, Allocator.Persistent);
    }

    void InitializeObstacleData()
    {
        // 检测场景中的障碍物
        Collider[] colliders = Physics.OverlapSphere(Vector3.zero, gridRadius * hexSize * 2, obstacleLayer);
        List<ObstacleData> obstacles = new List<ObstacleData>();

        foreach (Collider col in colliders)
        {
            // 简化处理：使用碰撞体包围球
            obstacles.Add(new ObstacleData
            {
                position = col.bounds.center,
                radius = col.bounds.extents.magnitude
            });
        }

        obstacleDataArray = new NativeArray<ObstacleData>(obstacles.ToArray(), Allocator.Persistent);
    }

    void UpdateAgentData()
    {
        // 更新代理的期望速度（从流场获取）
        for (int i = 0; i < agents.Count; i++)
        {
            Agent agent = agents[i];
            Vector3 flowDir = GetFlowDirection(agent.position);

            agents[i].desiredVelocity = flowDir * agentMaxSpeed;
            agents[i].position = agent.transform.position;

            // 更新NativeArray
            agentDataArray[i] = new AgentData
            {
                position = agent.position,
                velocity = agent.velocity,
                desiredVelocity = agents[i].desiredVelocity,
                radius = agent.radius,
                maxSpeed = agent.maxSpeed,
                avoidanceStrength = agentAvoidanceStrength
            };
        }
    }

    void ScheduleAvoidanceJob()
    {
        // 准备流场方向数据
        PrepareFlowDirections();

        // 创建并调度作业
        AvoidanceJob job = new AvoidanceJob
        {
            AgentData = agentDataArray,
            FlowDirections = flowDirections,
            Obstacles = obstacleDataArray,
            AvoidanceRadius = avoidanceRadius,
            DeltaTime = Time.deltaTime
        };

        avoidanceJobHandle = job.Schedule(agentCount, 32);
    }

    void CompleteJobAndUpdateAgents()
    {
        // 等待作业完成
        avoidanceJobHandle.Complete();

        // 更新代理数据
        for (int i = 0; i < agents.Count; i++)
        {
            AgentData data = agentDataArray[i];
            agents[i].velocity = data.velocity;
            agents[i].position += agents[i].velocity * Time.deltaTime;
        }
    }

    void UpdateAgentTransforms()
    {
        // 更新Unity物体位置
        foreach (Agent agent in agents)
        {
            if (agent.transform)
            {
                agent.transform.position = agent.position;

                // 更新朝向
                if (agent.velocity.sqrMagnitude > 0.01f)
                {
                    agent.transform.rotation = Quaternion.LookRotation(agent.velocity);
                }
            }
        }
    }

    void PrepareFlowDirections()
    {
        // 创建流场方向的NativeArray
        if (flowDirections.IsCreated) flowDirections.Dispose();
        flowDirections = new NativeArray<float3>(grid.Count, Allocator.TempJob);

        int index = 0;
        foreach (HexCell cell in grid.Values)
        {
            flowDirections[index] = cell.bestDirection;
            index++;
        }
    }

    [BurstCompile]
    struct AvoidanceJob : IJobParallelFor
    {
        public NativeArray<AgentData> AgentData;
        [ReadOnly] public NativeArray<float3> FlowDirections;
        [ReadOnly] public NativeArray<ObstacleData> Obstacles;
        public float AvoidanceRadius;
        public float DeltaTime;

        public void Execute(int index)
        {
            AgentData agent = AgentData[index];
            float3 avoidanceForce = float3.zero;

            // 动态代理避障
            for (int i = 0; i < AgentData.Length; i++)
            {
                if (i == index) continue;

                AgentData other = AgentData[i];
                float3 toOther = other.position - agent.position;
                float distance = math.length(toOther);

                if (distance < AvoidanceRadius && distance > 0.001f)
                {
                    float combinedRadius = agent.radius + other.radius;
                    float3 dir = math.normalize(toOther);
                    float forceMagnitude = agent.avoidanceStrength * math.max(0, (AvoidanceRadius - distance) / AvoidanceRadius);

                    avoidanceForce -= dir * forceMagnitude;
                }
            }

            // 静态障碍物避障
            for (int i = 0; i < Obstacles.Length; i++)
            {
                ObstacleData obstacle = Obstacles[i];
                float3 toObstacle = obstacle.position - agent.position;
                float distance = math.length(toObstacle);
                float minDistance = agent.radius + obstacle.radius;

                if (distance < AvoidanceRadius + minDistance && distance > 0.001f)
                {
                    float3 dir = math.normalize(toObstacle);
                    float forceMagnitude = agent.avoidanceStrength * 2 * math.max(0, (AvoidanceRadius + minDistance - distance) / AvoidanceRadius);

                    avoidanceForce -= dir * forceMagnitude;
                }
            }

            // 计算最终速度
            float3 desiredVelocity = agent.desiredVelocity;
            float3 newVelocity = desiredVelocity + avoidanceForce;
            float speed = math.length(newVelocity);

            if (speed > agent.maxSpeed)
            {
                newVelocity = math.normalize(newVelocity) * agent.maxSpeed;
            }

            // 应用流场方向约束
            float3 flowDir = GetFlowDirection(agent.position, FlowDirections);
            if (math.lengthsq(flowDir) > 0.1f)
            {
                newVelocity = math.lerp(newVelocity, flowDir * agent.maxSpeed, 0.7f);
            }

            // 更新代理数据
            agent.velocity = newVelocity;
            AgentData[index] = agent;
        }

        private float3 GetFlowDirection(float3 position, NativeArray<float3> flowDirections)
        {
            // 简化实现：在实际项目中应使用空间分区优化
            float closestDist = float.MaxValue;
            float3 closestDir = float3.zero;

            for (int i = 0; i < flowDirections.Length; i++)
            {
                float dist = math.distance(position, flowDirections[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestDir = flowDirections[i];
                }
            }

            return closestDir;
        }
    }

    void GenerateHexGrid()
    {
        grid.Clear();

        // 生成六边形网格
        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);

            for (int r = r1; r <= r2; r++)
            {
                Vector2Int coord = new Vector2Int(q, r);
                Vector3 position = HexToWorldPosition(coord);

                // 创建六边形单元
                grid[coord] = new HexCell(coord, position);

                // 检测障碍物
                if (Physics.CheckSphere(position, obstacleCheckRadius, obstacleLayer))
                {
                    grid[coord].isObstacle = true;
                    grid[coord].cost = int.MaxValue;
                }
            }
        }

        // 设置邻居关系
        foreach (var cell in grid.Values)
        {
            foreach (var dir in directions)
            {
                Vector2Int neighborCoord = cell.coord + dir;
                if (grid.TryGetValue(neighborCoord, out HexCell neighbor))
                {
                    cell.neighbors.Add(neighbor);
                }
            }
        }

        isFlowFieldGenerated = false;
    }

    void GenerateFlowField()
    {
        if (target == null) return;

        // 重置所有单元格
        foreach (var cell in grid.Values)
        {
            if (!cell.isObstacle)
            {
                cell.cost = int.MaxValue;
            }
            cell.bestDirection = Vector3.zero;
        }

        // 设置目标单元格
        targetHexCoord = WorldToHexCoord(target.position);
        if (!grid.TryGetValue(targetHexCoord, out HexCell targetCell))
        {
            Debug.LogWarning("Target is outside of grid!");
            return;
        }

        targetCell.cost = 0;

        // 使用广度优先搜索计算成本场
        Queue<HexCell> queue = new Queue<HexCell>();
        queue.Enqueue(targetCell);

        while (queue.Count > 0)
        {
            HexCell current = queue.Dequeue();

            foreach (HexCell neighbor in current.neighbors)
            {
                if (neighbor.isObstacle) continue;

                // 计算新成本（假设所有移动成本为1）
                int newCost = current.cost + 1;

                if (newCost < neighbor.cost)
                {
                    neighbor.cost = newCost;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 计算流场方向
        foreach (HexCell cell in grid.Values)
        {
            if (cell.cost == int.MaxValue || cell.isObstacle || cell == targetCell)
            {
                cell.bestDirection = Vector3.zero;
                continue;
            }

            HexCell bestNeighbor = null;
            int bestCost = cell.cost;

            // 寻找成本最低的邻居
            foreach (HexCell neighbor in cell.neighbors)
            {
                if (neighbor.cost < bestCost)
                {
                    bestCost = neighbor.cost;
                    bestNeighbor = neighbor;
                }
            }

            // 计算流动方向
            if (bestNeighbor != null)
            {
                cell.bestDirection = (bestNeighbor.position - cell.position).normalized;
            }
        }

        isFlowFieldGenerated = true;
    }

    // 世界坐标转六边形坐标
    public Vector2Int WorldToHexCoord(Vector3 worldPosition)
    {
        // 将世界坐标转换为平面坐标
        Vector2 point = new Vector2(worldPosition.x, worldPosition.z);

        // 六边形网格坐标转换算法
        float q = (Mathf.Sqrt(3) / 3 * point.x - 1f / 3 * point.y) / hexSize;
        float r = (2f / 3 * point.y) / hexSize;

        // 立方体坐标
        float x = q;
        float z = r;
        float y = -x - z;

        // 四舍五入到最近的整数坐标
        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);

        // 处理舍入误差
        float xDiff = Mathf.Abs(rx - x);
        float yDiff = Mathf.Abs(ry - y);
        float zDiff = Mathf.Abs(rz - z);

        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        else if (yDiff > zDiff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Vector2Int(rx, rz);
    }

    // 六边形坐标转世界坐标
    public Vector3 HexToWorldPosition(Vector2Int coord)
    {
        float x = hexSize * (Mathf.Sqrt(3) * coord.x + Mathf.Sqrt(3) / 2 * coord.y);
        float z = hexSize * (3f / 2 * coord.y);
        return new Vector3(x, 0, z);
    }

    // 获取移动方向（用于控制物体移动）
    public Vector3 GetFlowDirection(Vector3 worldPosition)
    {
        Vector2Int coord = WorldToHexCoord(worldPosition);
        if (grid.TryGetValue(coord, out HexCell cell))
        {
            return cell.bestDirection;
        }
        return Vector3.one * UnityEngine.Random.Range(-1f, 1f);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || grid.Count == 0) return;

        // 绘制网格
        if (showGrid)
        {
            Gizmos.color = Color.gray;
            foreach (var cell in grid.Values)
            {
                if (cell.isObstacle)
                {
                    Gizmos.color = Color.black;
                    DrawHexagon(cell.position, hexSize);
                    Gizmos.color = Color.gray;
                }
                else
                {
                    DrawHexagon(cell.position, hexSize);
                }
            }
        }

        // 绘制成本场
        if (showCostField && isFlowFieldGenerated)
        {
            int maxCost = grid.Values
                .Where(c => !c.isObstacle)
                .Max(c => c.cost == int.MaxValue ? 0 : c.cost);

            if (maxCost == 0) maxCost = 1; // 防止除以零

            foreach (var cell in grid.Values)
            {
                if (cell.cost == int.MaxValue || cell.isObstacle) continue;

                float t = Mathf.Clamp01((float)cell.cost / maxCost);
                Gizmos.color = Color.Lerp(lowCostColor, highCostColor, t);
                DrawHexagon(cell.position, hexSize * 0.9f);
            }
        }

        // 绘制流场方向
        if (showFlowVectors && isFlowFieldGenerated)
        {
            Gizmos.color = Color.blue;
            foreach (var cell in grid.Values)
            {
                if (cell.bestDirection != Vector3.zero && !cell.isObstacle)
                {
                    Vector3 endPoint = cell.position + cell.bestDirection * hexSize * arrowScale;
                    Gizmos.DrawLine(cell.position, endPoint);
                    DrawArrow(cell.position, cell.bestDirection, hexSize * arrowScale);
                }
            }
        }

        // 绘制目标点
        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(target.position, hexSize * 0.5f);
        }
    }

    void DrawHexagon(Vector3 center, float size)
    {
        for (int i = 0; i < 6; i++)
        {
            float angle = 60 * i - 30;
            Vector3 start = center + new Vector3(
                size * Mathf.Cos(angle * Mathf.Deg2Rad),
                0,
                size * Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector3 end = center + new Vector3(
                size * Mathf.Cos((angle + 60) * Mathf.Deg2Rad),
                0,
                size * Mathf.Sin((angle + 60) * Mathf.Deg2Rad)
            );

            Gizmos.DrawLine(start, end);
        }
    }

    void DrawArrow(Vector3 position, Vector3 direction, float length)
    {
        Vector3 tip = position + direction * length;

        // 绘制箭头主体
        Gizmos.DrawLine(position, tip);

        // 绘制箭头两侧
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200, 0) * Vector3.forward;

        Gizmos.DrawLine(tip, tip + right * length * 0.3f);
        Gizmos.DrawLine(tip, tip + left * length * 0.3f);
    }
}

