using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.SceneManagement;


public class PlayAgent : Agent
{
    [Header("이동")]
    public Vector3 startPos;
    public Vector3 nextPos;    // 다음 이동하는 위치

    // w-0, a-1, s-2, d-3
    public int curDir = 0;      // 현재 이동하는 방향 (이동하는동안 고정)
    public int inputDir = 0;     // 다음에 이동할 방향 (입력)
    private int[] curDirection = { 0, 0, 0, 0 };

    public float moveTime = 0;
    public float moveSpeed = 1;

    [Space(20f)]
    [Header("지렁이")]
    public Transform head;
    public List<Transform> tails;
    public int startTailCount = 3;

    [Space(20f)]
    [Header("프리팹")]
    public GameObject p_tail;

    [Space(20f)]
    [Header("레이어")]
    public LayerMask l_item;
    public LayerMask l_tail;

    [Space(20f)]
    [Header("기타")]
    public float distance;

    void Move()
    {
        if (moveTime < 0.5f) { return; }
        moveTime = 0;

        distance = Vector3.Distance(head.position, item.position);

        // int curMax = 0;
        // for (int i = 0; i < curDirection.Length; i++)
        // {
        //     if (curDirection[i] > curMax)
        //     {
        //         curMax = curDirection[i];
        //         inputDir = i;
        //     }
        //     curDirection[i] = 0;
        // }

        if (curDir % 2 != inputDir % 2)  // 뒤로 이동 불가
            curDir = inputDir;
        else
        {
            if (curDir == inputDir) AddReward(-0.01f);
        }
        switch (curDir)
        {
            case 0: nextPos += Vector3.forward; break;
            case 1: nextPos += Vector3.left; break;
            case 2: nextPos += Vector3.back; break;
            case 3: nextPos += Vector3.right; break;
        }

        // 꼬리 이동
        for (int i = tails.Count - 1; i > 0; i--)
        {
            tails[i].position = tails[i - 1].position;
        }
        tails[0].position = head.position;

        // 머리 이동
        head.position = nextPos;

        if (distance > Vector3.Distance(head.position, item.position))
        {
            AddReward(0.0001f);
        }
        else
        {
            AddReward(-0.001f);
        }

        TryEatItem(nextPos);

        CheckCollideTail(nextPos);
    }

    void TryEatItem(Vector3 _pos)
    {
        Vector3 pos = new Vector3(_pos.x, agentStartPos.y + 1, _pos.z);
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit _item, 1f, l_item))
        {
            EatItem(_item.transform.gameObject);
        }
    }

    void EatItem(GameObject _item)
    {
        ChangeItemPos();
        AddTail();
        moveSpeed += 0.1f;

        AddReward(1f);
        print(GetCumulativeReward());
    }

    void CheckCollideTail(Vector3 _pos)
    {
        Vector3 pos = new Vector3(_pos.x, agentStartPos.y + 1, _pos.z);
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit _tail, 1f, l_tail))
        {
            SetReward(-1f);

            Die();
        }
    }

    void Die()
    {
        print(GetCumulativeReward());
        EndEpisode();
    }


    void ResetGame()
    {
        for (int i = 0; i < 3; i++)
        {
            tails[i].transform.position = startPos;
        }
        for (int i = tails.Count - 1; i >= 3; i--)
        {
            Destroy(tails[3].gameObject);
            tails.RemoveAt(3);
        }

        head.position = startPos;
        nextPos = startPos;
        curDir = 0;

        moveTime = 0;
        moveSpeed = 1;

        ChangeItemPos();
    }

    void AddTail()
    {
        GameObject _tail = Instantiate(p_tail, transform);
        _tail.transform.position = tails[^1].position;
        tails.Add(_tail.transform);
    }


    #region ML-Agents

    // 처음 실행 시 호출
    public override void Initialize()
    {
        t_walls = transform.parent.GetChild(1).transform;
        t_floors = transform.parent.GetChild(2).transform;
        agentStartPos = transform.GetChild(0).position;

        startPos = new Vector3(5, 0.25f, 5) + agentStartPos;
        nextPos = new Vector3(5, 0.25f, 5) + agentStartPos;    // 다음 이동하는 위치

        MakeItem();
        ResetGame();
        InitFloor();
    }

    public override void OnEpisodeBegin()
    {
        ResetGame();
    }

    // 모든 단계에서 에이전트 동작을 지정
    // 개발자가 구현해야 하는 부분
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        moveTime += Time.deltaTime * moveSpeed;

        var action = actionBuffers.DiscreteActions[0];

        inputDir = action;
        // CheckDirection(action);
        Move();
    }

    private void CheckDirection(int direction)
    {
        curDirection[direction]++;
        Move();
    }

    // agent이 여기의 정보를 통해 목표를 찾아감
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(head.position.x / 10f);
        sensor.AddObservation(head.position.z / 10f);
        // sensor.AddObservation(tails[^1].position.x / 10f);
        // sensor.AddObservation(tails[^1].position.z / 10f);
        sensor.AddObservation(tails.Count/100);
        // sensor.AddObservation(floorSize / 30f);

        sensor.AddObservation((head.position.x - item.position.x) / 10f);
        sensor.AddObservation((head.position.z - item.position.z) / 10f);
        // sensor.AddObservation(item.position.x / 10f);
        // sensor.AddObservation(item.position.z / 10f);
        sensor.AddObservation(moveTime * 2);
        sensor.AddObservation(curDir / 3);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        moveTime += Time.deltaTime * moveSpeed;

        var action = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 0;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            action[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            action[0] = 3;
        }
        inputDir = action[0];

        Move();
    }

    #endregion


    #region GameManager
    [Header("프리팹")]
    public GameObject p_floor;
    public GameObject p_wall;
    public GameObject p_item;

    [Header("메추리알")]
    public Material m_wall;
    public Material m_floor1;
    public Material m_floor2;

    [Space(20f)]
    [Header("트랜스폼")]
    public Transform t_floors;
    public Transform t_walls;
    public Vector3 agentStartPos;

    [Space(20f)]
    [Header("게임 설정")]
    public int floorSize;

    [Space(20f)]
    [Header("게임 오브젝트")]
    public Transform item;


    void InitFloor()
    {
        // 바닥이랑 벽 파괴
        int numOfChild = t_floors.childCount;
        for (int i = 0; i < numOfChild; i++)
            Destroy(t_floors.GetChild(i).gameObject);
        numOfChild = t_walls.childCount;
        for (int i = 0; i < numOfChild; i++)
            Destroy(t_walls.GetChild(i).gameObject);

        // 범위만큼 새로 바닥&벽 만들기
        for (int i = -1; i <= floorSize; i++)
        {
            for (int j = -1; j <= floorSize; j++)
            {
                if (i == -1 | i == floorSize | j == -1 | j == floorSize)
                {
                    GameObject _wall = Instantiate(p_wall, t_walls);
                    _wall.transform.position = new Vector3(i, 0, j) + agentStartPos;
                    _wall.GetComponent<MeshRenderer>().material = m_wall;
                }
                else
                {
                    GameObject _floor = Instantiate(p_floor, t_floors);
                    _floor.transform.position = new Vector3(i, 0, j) + agentStartPos;
                    _floor.GetComponent<MeshRenderer>().material = (i + j) % 2 == 0 ? m_floor1 : m_floor2;
                }
            }
        }
    }

    public void MakeItem()
    {
        GameObject _item = Instantiate(p_item);
        item = _item.transform;
        ChangeItemPos();
    }

    public void ChangeItemPos()
    {
        do
        {
            item.position = new Vector3(Random.Range(0, floorSize), 0.3f, Random.Range(0, floorSize)) + agentStartPos;
        } while (item.position == new Vector3(head.position.x, 0.3f, head.position.z));
    }

    #endregion

}
