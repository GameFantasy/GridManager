using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour {

    public enum UserOperation
    {
        //设置释放点
        SpellPlaceSetting = 0,
        //设置范围集合
        RangeGridSetting = 1,
    };

    //定义格子类型数组，存储不同主题的格子
    public GameObject[] gridTypes;
    //定义选择的主题
    public int selType;
    //边长
    public int length;
    //x轴偏移量
    private float offsetX;
    //y轴偏移量
    private float offsetY;
    //z轴偏移量
    private float offsetZ;
   
    //记录用户当前的操作模式
    public UserOperation operation;
    //格子行的集合
    List<List<GameObject>> gridList = new List<List<GameObject>>();
    //记录施放地点
    Grid spellPlace=new Grid();
    //记录格子集合
    List<Grid> rangeGrids = new List<Grid>();
    // Use this for initialization
    void Start()
    {
        //判断单双
        bool isOdd=length % 2 == 0 ? false : true;
        //格子集合
        GameObject grids = new GameObject();
        grids.name = "grids";
        //获取选择的格子主题
        GameObject gridType = gridTypes[selType];
        //计算起始位置
        float startX = -length/2.0f;
        float startY = length/2.0f;
        float endX = length/2.0f;
        float endY = -length/2.0f;
        //设置摄像机
        GameObject.FindObjectOfType<Camera>().GetComponent<Camera>().orthographicSize=length/2.0f;
        //循环生成方块
        for (float i = startX;i < endX; i++)
        {
            //记录一行中每一列的格子
            List<GameObject> row = new List<GameObject>();
            for (float j = startY; j > endY; j--)
            {
                
                var obj=Instantiate(gridType,new Vector3(i,j,0),Quaternion.identity);
                obj.tag = "Grid";
                //将生成的方块加入格子集合
                obj.transform.parent = grids.transform;
                row.Add(obj);

            }
            //添加一行到格子集合
            gridList.Add(row);
        }
        //设置偏移
        offsetX = isOdd? length / 2 + 1.0f : length / 2 + 0.5f;
        offsetY = 0;
        offsetZ = 0;
        var x = grids.transform.position.x + offsetX+0.5f;
        var y = grids.transform.position.y + offsetY-0.5f;
        var z = grids.transform.position.z + offsetZ;
        grids.transform.localPosition = new Vector3(x, y, z);

    }

    // Update is called once per frame
    void Update () {
        
        //鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            if (GetClickGrid() != null)
            {
                for (int i = 0; i < gridList.Count; i++)
                {
                    for (int j = 0; j < gridList[i].Count; j++)
                    {
                        if (GetClickGrid() == gridList[i][j])
                        {
                            //判断用户所在操作模式
                            switch (operation)
                            {
                                //选择施法地点
                                case UserOperation.SpellPlaceSetting:
                                    //清空所有选择（包括改变材质和清除集合）
                                    foreach (var gridRow in gridList)
                                    {
                                        foreach (var grid in gridRow)
                                        {
                                            grid.GetComponent<Renderer>().material.color = Color.white;
                                        }
                                    }
                                    rangeGrids.Clear();
                                    //未选择
                                    if (spellPlace.x == -1 && spellPlace.y == -1)
                                    {
                                        //Debug.Log("选择");
                                        spellPlace.x = i;
                                        spellPlace.y = j;
                                        gridList[i][j].GetComponent<Renderer>().material.color = Color.blue;
                                    }
                                    else
                                    {
                                        //取消选择
                                        if (spellPlace.x == i && spellPlace.y == j)
                                        {
                                            //Debug.Log("取消选择");
                                            spellPlace = new Grid();
                                            gridList[i][j].GetComponent<Renderer>().material.color = Color.white;
                                        }
                                        //重新选择
                                        else
                                        {
                                            //Debug.Log("重新选择");
                                            gridList[spellPlace.x][spellPlace.y].GetComponent<Renderer>().material.color = Color.white;
                                            spellPlace.x = i;
                                            spellPlace.y = j;
                                            gridList[i][j].GetComponent<Renderer>().material.color = Color.blue;
                                        }
                                    }
                                   
                                    break;
                                //选择施法范围
                                case UserOperation.RangeGridSetting:
                                    //点击格子是施放格子
                                    //注解：这里有这样一个流程，需要选择施法地点才能攒则施法范围，在点击选择施法范围按钮时，会将已选择的施法地点加入集合，施法地点一定是施法范围集合的第一个元素
                                    if (spellPlace.x == i && spellPlace.y == j)
                                    {
                                        Debug.Log("该格子已包含在范围内");  
                                    }
                                    else
                                    {
                                        //记录是否在集合内
                                        bool isInRangeGrids=false;
                                        foreach (var grid in rangeGrids)
                                        {
                                            //点击格子在集合内，取消选择
                                            if (grid.x == i && grid.y == j)
                                            {
                                                //Debug.Log("取消选择");
                                                spellPlace = new Grid();
                                                gridList[i][j].GetComponent<Renderer>().material.color = Color.white;
                                                rangeGrids.Remove(grid);
                                                isInRangeGrids = true;
                                                break;
                                            }
                                        }
                                        //不包含在格子集合中
                                        if (!isInRangeGrids)
                                        {
                                            Grid newGrid=new Grid();
                                            newGrid.x = i;
                                            newGrid.y = j;
                                            gridList[i][j].GetComponent<Renderer>().material.color = Color.red;
                                            rangeGrids.Add(newGrid);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
    /// <summary>
    /// 获取点击的格子
    /// </summary>
    /// <returns></returns>
    private GameObject GetClickGrid()
    {
        GameObject obj = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            //划出射线，在scene视图中能看到由摄像机发射出的射线
            Debug.DrawLine(ray.origin, hitInfo.point);
            GameObject gameObj = hitInfo.collider.gameObject;
            //当射线碰撞目标的name包含Cube，执行拾取操作
            if (gameObj.tag == "Grid")
            {
                obj = gameObj;
            }
        }
        return obj;
    }
}
public class Grid
{
    public int x { get; set; }
    public int y { get; set; }
    //实例化后x=-1，y=-1，表示未选择状态
    public Grid()
    {
        this.x = -1;
        this.y = -1;
    }
}