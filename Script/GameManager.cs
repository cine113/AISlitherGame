using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("������")]
    public GameObject p_floor;
    public GameObject p_wall;
    public GameObject p_item;

    [Header("���߸���")]
    public Material m_wall;
    public Material m_floor1;
    public Material m_floor2;

    [Space(20f)]
    [Header("Ʈ������")]
    public Transform t_floors;
    public Transform t_walls;
    public Transform t_camera;

    [Space(20f)]
    [Header("���� ����")]
    public int floorSize;
    public Vector3 cameraPos;   //floorSize�� �°� ī�޶� ��ġ ����

    [Space(20f)]
    [Header("���� ������Ʈ")]
    public Transform item;
    public PlayAgent player;


    void Start()
    {
        MakeItem();
        ResetGame();
    }

    void InitFloor()
    {
        // �ٴ��̶� �� �ı�
        int numOfChild = t_floors.childCount;
        for (int i = 0; i < numOfChild; i++)
            Destroy(t_floors.GetChild(i).gameObject);
        numOfChild = t_walls.childCount;
        for (int i = 0; i < numOfChild; i++)
            Destroy(t_walls.GetChild(i).gameObject);

        // ������ŭ ���� �ٴ�&�� �����
        for (int i = -1; i <= floorSize; i++)
        {
            for (int j = -1; j <= floorSize; j++)
            {
                if (i == -1 | i == floorSize | j == -1 | j == floorSize)
                {
                    GameObject _wall = Instantiate(p_wall, t_walls);
                    _wall.transform.position = new Vector3(i, 0, j);
                    _wall.GetComponent<MeshRenderer>().material = m_wall;
                }
                else
                {
                    GameObject _floor = Instantiate(p_floor, t_floors);
                    _floor.transform.position = new Vector3(i, 0, j);
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
        item.position = new Vector3(Random.Range(0, GameManager.Instance.floorSize), 0.3f, Random.Range(0, GameManager.Instance.floorSize));
    }

    public void SetCameraPos()
    {
        t_camera.position = new Vector3(floorSize / 2, Mathf.Sqrt(3 / 2 * floorSize * floorSize), floorSize / 2);
    }

    public void ResetGame()
    {
        player.item = item;
        player.floorSize = floorSize;

        InitFloor();
        ChangeItemPos();
        SetCameraPos();
        floorSize = Random.Range(10, 21);
    }
}
