using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������ ������ �ȵǳ�
// reset�ϸ� ù��° �������� �� �����ϰ� ù��° ������ �ٴڹؿ� �����


public class Player : MonoBehaviour
{
    [Header("�̵�")]
    public Vector3 startPos;
    public Vector3 nextPos;    // ���� �̵��ϴ� ��ġ

    // w-0, a-1, s-2, d-3
    public int curDir = 1;      // ���� �̵��ϴ� ���� (�̵��ϴµ��� ����)
    public int inputDir = 1;     // ������ �̵��� ���� (�Է�)

    public float moveTime;
    public float moveSpeed;

    [Space(20f)]
    [Header("������")]
    public Transform head;
    public List<Transform> tails;
    public int startTailCount = 3;

    [Space(20f)]
    [Header("������")]
    public GameObject p_tail;

    [Space(20f)]
    [Header("���̾�")]
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
        // �ð�++
        moveTime += Time.deltaTime * moveSpeed;

        // �Է� (���� �̵�����)
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
        if (curDir % 2 != inputDir % 2)  // �ڷ� �̵� �Ұ�
            curDir = inputDir;
        switch (curDir)
        {
            case 0: nextPos += Vector3.forward; break;
            case 1: nextPos += Vector3.left; break;
            case 2: nextPos += Vector3.back; break;
            case 3: nextPos += Vector3.right; break;
        }

        TryEatItem(nextPos);

        // ���� �̵�
        for (int i = tails.Count - 1; i > 0; i--)
        {
            tails[i].position = tails[i - 1].position;
        }
        tails[0].position = head.position;

        // �Ӹ� �̵�
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

        //���� �߰�
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

        //���� ���
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
