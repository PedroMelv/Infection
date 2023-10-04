using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CellType
{
    WALK,
    SPIN,
    STATIC
}

public class EnemyCell : Cell
{
    public bool activated = false;
    [SerializeField] private CellType cellType;
    private Vector3 targetPos;
    private float speed;
    private float startZ;
    private bool spinDirection;
    private Image cellImg;

    private void Awake()
    {
        cellImg = GetComponent<Image>();
    }

    private void Start()
    {
        if (cellType == CellType.STATIC) return;

        spinDirection = Random.value < 0.5f;

        if(cellType == CellType.WALK)
        {
            float startRandom = Random.Range(0f, 2f);
            InvokeRepeating(nameof(SetTarget), startRandom, 1.5f);
            startZ = this.GetComponent<RectTransform>().position.z;
        }
        else
        {
            speed = Random.Range(30f, 90f);
        }
    }

    private void Update()
    {
        cellImg.color = new Color(cellImg.color.r, cellImg.color.g, cellImg.color.b, (activated ? 1f : .33f));

        switch (cellType)
        {
            case CellType.WALK:
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -65f, 34f), Mathf.Clamp(transform.position.y, -38f, 38f), startZ);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                break;
            case CellType.SPIN:
                transform.Rotate(0f,0f,((spinDirection ? 1 : -1 ) * speed) * Time.deltaTime);
                break;
            default:
                break;
        }
       
    }

    private void SetTarget()
    {
        speed = Random.Range(5f, 30f);
        targetPos = transform.position + Random.insideUnitSphere * Random.Range(25f,50f);
        //targetPos.z = 0f;
    }

    public override void PointerEnter()
    {
        if(!activated) return;
        CellArea.i.RestartCell();
    }
}
