using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
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
    public Transform t_camera;

    [Space(20f)]
    [Header("게임 설정")]
    public int floorSize;
    public Vector3 cameraPos;   //floorSize에 맞게 카메라 위치 조정

    [Space(20f)]
    [Header("게임 오브젝트")]
    public Transform item;
    public PlayAgent player;


    void Start()
    {
        MakeItem();
        ResetGame();
    }

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
