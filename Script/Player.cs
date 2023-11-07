using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 꼬리가 삭제가 안되네
// reset하면 첫번째 꼬리빼고 다 삭제하고 첫번째 꼬리는 바닥밑에 숨기기


public class Player : MonoBehaviour
{
    [Header("이동")]
    public Vector3 startPos;
    public Vector3 nextPos;    // 다음 이동하는 위치

    // w-0, a-1, s-2, d-3
    public int curDir = 1;      // 현재 이동하는 방향 (이동하는동안 고정)
    public int inputDir = 1;     // 다음에 이동할 방향 (입력)

    public float moveTime;
    public float moveSpeed;

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

    void Start()
    {
        head.position = startPos;

        for (int i = 0; i < startTailCount - 1; i++)
        {
            AddTail();
        }
    }

    void Update()
    {
        // 시간++
        moveTime += Time.deltaTime * moveSpeed;

        // 입력 (다음 이동방향)
        if (Input.GetKeyDown(KeyCode.W))
        {
            inputDir = 0;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            inputDir = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            inputDir = 2;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            inputDir = 3;
        }

        if (moveTime > 0.5f)
        {
            moveTime = 0;
            Move();
        }

    }

    void Move()
    {
        if (curDir % 2 != inputDir % 2)  // 뒤로 이동 불가
            curDir = inputDir;
        switch (curDir)
        {
            case 0: nextPos += Vector3.forward; break;
            case 1: nextPos += Vector3.left; break;
            case 2: nextPos += Vector3.back; break;
            case 3: nextPos += Vector3.right; break;
        }

        TryEatItem(nextPos);

        // 꼬리 이동
        for (int i = tails.Count - 1; i > 0; i--)
        {
            tails[i].position = tails[i - 1].position;
        }
        tails[0].position = head.position;

        // 머리 이동
        head.position = nextPos;

        CheckCollideTail(nextPos);
    }


    void TryEatItem(Vector3 _pos)
    {
        Vector3 pos = new Vector3(_pos.x, 0, _pos.z);
        if (Physics.Raycast(pos, Vector3.up, out RaycastHit _item, 1f, l_item))
        {
            EatItem(_item.transform.gameObject);
        }
    }

    void EatItem(GameObject _item)
    {
        AddTail();
        GameManager.Instance.ChangeItemPos();
        moveSpeed += 0.1f;

        //점수 추가
    }

    bool CheckCollideTail(Vector3 _pos)
    {
        Vector3 pos = new Vector3(_pos.x, 1, _pos.z);
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit _tail, 1f, l_tail))
        {
            Die();
            return true;
        }
        return false;
    }

    void Die()
    {
        ResetGame();

        //점수 깎기
    }

    void ResetGame()
    {
        GameManager.Instance.ResetGame();

        head.position = startPos;
        nextPos = startPos;
        moveSpeed = 1f;

        for (int i = tails.Count - 1; i > 0; i--)
        {
            Destroy(tails[i].gameObject);
            tails.RemoveAt(i);
        }

        tails[0].position = new Vector3(0, -2, 0);

        for (int i = 0; i < startTailCount - 1; i++)
        {
            AddTail();
        }
    }

    void AddTail()
    {
        GameObject _tail = Instantiate(p_tail, transform);
        _tail.transform.position = tails[^1].position;
        tails.Add(_tail.transform);
    }

}
